using UnityEngine;

/// <summary>
/// 拉枪模式。准星进入目标附近后开始采样；鼠标停下时判定一次过冲/欠冲。
/// </summary>
public class FlickTrainingSession
{
    /// <summary>准星进入该角距后开始计算（武器准星附近）。</summary>
    const float NearEnterDeg = 10f;
    /// <summary>停下时落在该角距内算干净停稳。</summary>
    const float OnTargetDeg = 1.4f;
    /// <summary>视角角速度低于此值视为鼠标停下（度/秒）。</summary>
    const float StopSpeedDegPerSec = 12f;
    /// <summary>连续停下多久才结算一次。</summary>
    const float StopHoldSeconds = 0.09f;
    /// <summary>进入附近后至少有过这么大的角速度，才允许“停下判定”（避免生成瞬间误判）。</summary>
    const float MinMotionNearDegPerSec = 25f;

    readonly FirstPersonShooter _player;
    readonly Transform _arenaRoot;
    readonly Vector3 _spawnCenter;
    readonly float _maxYawDeg;
    readonly float _maxPitchDeg;
    readonly float _targetLifetime;
    readonly float _respawnDelay;

    AimTarget _current;
    float _targetSpawnTime;
    float _nextSpawnTime;
    bool _firstShotTaken;

    bool _nearEntered;
    bool _leadLagSettled;
    bool _hadMotionNear;
    bool _crossedTarget;
    float _entrySignH;
    float _stopHold;
    float _prevSignedH;
    bool _hasPrevSigned;

    public FlickMetrics BatchMetrics { get; } = new FlickMetrics();
    public FlickMetrics SessionMetrics { get; } = new FlickMetrics();
    public int CompletedTargets { get; private set; }
    public bool IsRunning { get; private set; }
    public event System.Action OnTargetCompleted;

    public FlickTrainingSession(
        FirstPersonShooter player,
        Transform arenaRoot,
        Vector3 spawnCenter,
        float maxYawDeg = 16f,
        float maxPitchDeg = 9f,
        float targetLifetime = 1.6f,
        float respawnDelay = 0.28f)
    {
        _player = player;
        _arenaRoot = arenaRoot;
        _spawnCenter = spawnCenter;
        _maxYawDeg = maxYawDeg;
        _maxPitchDeg = maxPitchDeg;
        _targetLifetime = targetLifetime;
        _respawnDelay = respawnDelay;
    }

    public void Start()
    {
        IsRunning = true;
        _player.OnShot += HandleShot;
        _player.OnShotHit += HandleHit;
        _player.OnShotMiss += HandleMiss;
        SpawnTarget();
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _player.OnShot -= HandleShot;
        _player.OnShotHit -= HandleHit;
        _player.OnShotMiss -= HandleMiss;
        DestroyCurrent();
    }

    public void Tick(float dt)
    {
        if (!IsRunning) return;

        if (_current == null)
        {
            if (Time.time >= _nextSpawnTime)
                SpawnTarget();
            return;
        }

        SampleAndMaybeJudge(dt);

        if (Time.time - _targetSpawnTime >= _targetLifetime)
            EndCurrentTarget();
    }

    void SampleAndMaybeJudge(float dt)
    {
        if (_leadLagSettled || _current == null)
            return;

        Vector3 targetPos = _current.transform.position;
        float ang = _player.AngularErrorDegrees(targetPos);
        float signedH = _player.SignedHorizontalErrorDegrees(targetPos);
        float speed = _player.LastAngularSpeed;

        if (!_nearEntered)
        {
            if (ang > NearEnterDeg)
            {
                _prevSignedH = signedH;
                _hasPrevSigned = true;
                return;
            }

            // 准星进入目标附近：开始计算
            _nearEntered = true;
            _entrySignH = Mathf.Abs(signedH) > 0.15f ? Mathf.Sign(signedH) : (_hasPrevSigned ? Mathf.Sign(_prevSignedH) : 0f);
            _stopHold = 0f;
        }

        if (speed >= MinMotionNearDegPerSec)
            _hadMotionNear = true;

        if (_hasPrevSigned && _entrySignH != 0f)
        {
            bool crossed = Mathf.Sign(_prevSignedH) != Mathf.Sign(signedH) &&
                           Mathf.Abs(_prevSignedH) > 0.5f &&
                           Mathf.Abs(signedH) > 0.5f;
            if (crossed)
                _crossedTarget = true;
        }

        _prevSignedH = signedH;
        _hasPrevSigned = true;

        // 进附近后有过拉枪动作，且鼠标停下 → 判定一次
        if (!_hadMotionNear)
        {
            _stopHold = 0f;
            return;
        }

        if (speed <= StopSpeedDegPerSec)
        {
            _stopHold += dt;
            if (_stopHold >= StopHoldSeconds)
                ClassifyAtStop(ang, signedH);
        }
        else
        {
            _stopHold = 0f;
        }
    }

    /// <summary>
    /// 鼠标停下瞬间：停在中心附近=干净；越过中心=过冲；仍在进入侧=欠冲。
    /// </summary>
    void ClassifyAtStop(float ang, float signedH)
    {
        if (_leadLagSettled) return;

        if (ang <= OnTargetDeg)
        {
            SettleLeadLag(overshoot: false, undershoot: false);
            return;
        }

        bool oppositeSide = _entrySignH != 0f &&
                            Mathf.Abs(signedH) > OnTargetDeg * 0.5f &&
                            Mathf.Sign(signedH) != _entrySignH;

        if (_crossedTarget || oppositeSide)
            SettleLeadLag(overshoot: true);
        else
            SettleLeadLag(overshoot: false, undershoot: true);
    }

    void ClassifyFallback()
    {
        if (_leadLagSettled || _current == null) return;

        // 超时/命中时若还没因停下结算：用当前准星位置按同样规则判一次
        if (!_nearEntered || !_hadMotionNear)
        {
            SettleLeadLag(overshoot: false, undershoot: false);
            return;
        }

        Vector3 targetPos = _current.transform.position;
        ClassifyAtStop(
            _player.AngularErrorDegrees(targetPos),
            _player.SignedHorizontalErrorDegrees(targetPos));
    }

    void SettleLeadLag(bool overshoot, bool undershoot = false)
    {
        if (_leadLagSettled) return;
        _leadLagSettled = true;

        if (overshoot)
        {
            BatchMetrics.overshootEvents++;
            SessionMetrics.overshootEvents++;
        }
        else if (undershoot)
        {
            BatchMetrics.undershootEvents++;
            SessionMetrics.undershootEvents++;
        }
        else
        {
            BatchMetrics.settleEvents++;
            SessionMetrics.settleEvents++;
        }
    }

    void EndCurrentTarget()
    {
        if (_current == null) return;

        ClassifyFallback();
        DestroyCurrent();
        CompletedTargets++;
        _nextSpawnTime = Time.time + _respawnDelay;
        OnTargetCompleted?.Invoke();
    }

    void HandleShot()
    {
        if (_current == null) return;
        BatchMetrics.shots++;
        SessionMetrics.shots++;
    }

    void HandleHit(RaycastHit hit)
    {
        if (_current == null) return;
        var target = hit.collider.GetComponentInParent<AimTarget>();
        if (target != _current) return;

        BatchMetrics.hits++;
        SessionMetrics.hits++;
        if (!_firstShotTaken)
        {
            BatchMetrics.firstShotHits++;
            SessionMetrics.firstShotHits++;
            _firstShotTaken = true;
        }

        EndCurrentTarget();
    }

    void HandleMiss()
    {
        if (_current == null || _firstShotTaken) return;
        _firstShotTaken = true;
    }

    void SpawnTarget()
    {
        DestroyCurrent();
        BatchMetrics.targetsPresented++;
        SessionMetrics.targetsPresented++;

        Vector3 pos = PickVisibleSpawnPosition();
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "FlickTarget";
        go.transform.SetParent(_arenaRoot, true);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.14f;
        UnityEngine.Object.Destroy(go.GetComponent<Collider>());
        var col = go.AddComponent<SphereCollider>();
        col.radius = 0.5f;

        _current = go.AddComponent<AimTarget>();
        _current.Configure(new Color(1f, 0.35f, 0.2f), 0.14f);

        _targetSpawnTime = Time.time;
        _firstShotTaken = false;
        _nearEntered = false;
        _leadLagSettled = false;
        _hadMotionNear = false;
        _crossedTarget = false;
        _entrySignH = 0f;
        _stopHold = 0f;
        _hasPrevSigned = false;
    }

    Vector3 PickVisibleSpawnPosition()
    {
        Transform cam = _player.playerCamera != null
            ? _player.playerCamera.transform
            : _player.transform;

        Vector3 origin = _player.LookOrigin;
        float dist = Vector3.Distance(origin, _spawnCenter);
        dist = Mathf.Clamp(dist, 9f, 13f);

        for (int i = 0; i < 10; i++)
        {
            float yaw = Random.Range(-_maxYawDeg, _maxYawDeg);
            float pitch = Random.Range(-_maxPitchDeg, _maxPitchDeg);

            if (Mathf.Abs(yaw) < 4f && Mathf.Abs(pitch) < 3f)
            {
                yaw = (Random.value < 0.5f ? -1f : 1f) * Random.Range(5f, _maxYawDeg);
                pitch = Random.Range(-_maxPitchDeg, _maxPitchDeg);
            }

            Vector3 dir = Quaternion.AngleAxis(yaw, cam.up) *
                          Quaternion.AngleAxis(pitch, cam.right) *
                          cam.forward;
            Vector3 candidate = origin + dir.normalized * dist;
            candidate.y = Mathf.Clamp(candidate.y, _spawnCenter.y - 1.1f, _spawnCenter.y + 1.6f);
            candidate.z = Mathf.Clamp(candidate.z, _spawnCenter.z - 1.2f, _spawnCenter.z + 1.2f);

            float ang = Vector3.Angle(cam.forward, candidate - origin);
            if (ang < 32f || i == 9)
                return candidate;
        }

        return _spawnCenter + cam.right * 1.5f;
    }

    void DestroyCurrent()
    {
        if (_current != null)
        {
            UnityEngine.Object.Destroy(_current.gameObject);
            _current = null;
        }
    }
}
