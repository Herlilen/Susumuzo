using UnityEngine;

/// <summary>
/// 跟枪模式：左右平移。采集连续「滞后角」，再用它做稳定调参。
/// </summary>
public class TrackTrainingSession
{
    const float ErrorDeadzoneDeg = 0.35f;
    const float VelocityEps = 0.35f; // 掉头附近速度很小时不采样，避免污染

    readonly FirstPersonShooter _player;
    readonly Transform _arenaRoot;
    readonly Vector3 _center;
    readonly float _halfWidth;
    readonly float _speed;
    readonly Vector3 _axis;

    AimTarget _target;
    float _lateral;
    float _dir = 1f;
    Vector3 _prevTargetPos;
    bool _hasPrevTargetPos;

    public TrackMetrics Metrics { get; } = new TrackMetrics();
    public bool IsRunning { get; private set; }
    public float TimeLeft { get; private set; }

    public TrackTrainingSession(
        FirstPersonShooter player,
        Transform arenaRoot,
        Vector3 center,
        float halfWidth = 4.5f,
        float speed = 2.5f,
        float sessionSeconds = 30f)
    {
        _player = player;
        _arenaRoot = arenaRoot;
        _center = center;
        _halfWidth = halfWidth;
        _speed = speed;
        _axis = Vector3.right;
        TimeLeft = sessionSeconds;
    }

    public void Start()
    {
        IsRunning = true;
        SpawnTarget();
        _player.OnShotHit += HandleHit;
    }

    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        _player.OnShotHit -= HandleHit;
        if (_target != null)
        {
            Object.Destroy(_target.gameObject);
            _target = null;
        }
    }

    public void Tick(float dt)
    {
        if (!IsRunning || _target == null) return;

        TimeLeft -= dt;
        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            Stop();
            return;
        }

        _lateral += _dir * _speed * dt;
        if (_lateral >= _halfWidth)
        {
            _lateral = _halfWidth;
            _dir = -1f;
        }
        else if (_lateral <= -_halfWidth)
        {
            _lateral = -_halfWidth;
            _dir = 1f;
        }

        Vector3 pos = _center + _axis * _lateral;
        _target.transform.position = pos;

        Metrics.totalSeconds += dt;
        if (_player.IsLookingAt(_target.GetComponent<Collider>()))
            Metrics.onTargetSeconds += dt;

        SampleLeadLag(pos, dt);

        _prevTargetPos = pos;
        _hasPrevTargetPos = true;
    }

    void SampleLeadLag(Vector3 targetPos, float dt)
    {
        if (!_hasPrevTargetPos)
            return;

        // 用真实运动轴速度（世界 X），不受镜头晃动影响
        float axisVel = Vector3.Dot((targetPos - _prevTargetPos) / Mathf.Max(dt, 0.0001f), _axis);
        if (Mathf.Abs(axisVel) < VelocityEps)
            return;

        float hErr = _player.SignedHorizontalErrorDegrees(targetPos);
        // + = 准星落后于目标运动方向（欠冲）；- = 超前（过冲）
        float signedLag = hErr * Mathf.Sign(axisVel);

        Metrics.signedLagSum += signedLag;
        Metrics.lagSamples++;

        if (signedLag > ErrorDeadzoneDeg)
            Metrics.undershootSeconds += dt;
        else if (signedLag < -ErrorDeadzoneDeg)
            Metrics.overshootSeconds += dt;
        else
            Metrics.alignedSeconds += dt;
    }

    void HandleHit(RaycastHit hit)
    {
    }

    void SpawnTarget()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "TrackTarget_HorizontalOnly";
        go.transform.SetParent(_arenaRoot, true);

        _lateral = -_halfWidth;
        _dir = 1f;
        go.transform.position = _center + _axis * _lateral;
        go.transform.localScale = Vector3.one * 0.32f;

        _target = go.AddComponent<AimTarget>();
        _target.Configure(new Color(0.25f, 0.75f, 1f), 0.32f);
        _hasPrevTargetPos = false;
    }
}
