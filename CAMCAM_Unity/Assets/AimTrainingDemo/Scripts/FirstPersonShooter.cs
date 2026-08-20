using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonShooter : MonoBehaviour
{
    public SensitivityProfile sensitivity = new SensitivityProfile();
    public Camera playerCamera;
    public float maxPitch = 89f;
    public float fireCooldown = 0.12f;
    public float aimRayDistance = 80f;
    public LayerMask hitMask = ~0;

    public bool AdsHeld { get; private set; }
    public bool FirePressedThisFrame { get; private set; }
    public Vector3 LookOrigin => playerCamera != null ? playerCamera.transform.position : transform.position;
    public Vector3 LookDirection => playerCamera != null ? playerCamera.transform.forward : transform.forward;
    public float LastYawDelta { get; private set; }
    public float LastPitchDelta { get; private set; }
    public float LastAngularSpeed { get; private set; }

    public System.Action<RaycastHit> OnShotHit;
    public System.Action OnShotMiss;
    public System.Action OnShot;

    float _yaw;
    float _pitch;
    float _nextFireTime;
    bool _inputEnabled = true;

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        if (!enabled)
        {
            AdsHeld = false;
            FirePressedThisFrame = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void TeleportLook(Vector3 worldPos, Vector3 lookPoint)
    {
        transform.position = worldPos;
        Vector3 dir = (lookPoint - worldPos).normalized;
        _yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        _pitch = -Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;
        ApplyRotation();
    }

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        Vector3 e = transform.eulerAngles;
        _yaw = e.y;
        _pitch = 0f;
    }

    void OnEnable()
    {
        SetInputEnabled(true);
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        FirePressedThisFrame = false;
        LastYawDelta = 0f;
        LastPitchDelta = 0f;
        LastAngularSpeed = 0f;

        if (!_inputEnabled || Mouse.current == null)
            return;

        AdsHeld = Mouse.current.rightButton.isPressed;

        Vector2 delta = Mouse.current.delta.ReadValue();
        float accel = 1f + sensitivity.acceleration * Mathf.Clamp01(delta.magnitude / 40f);
        float ads = AdsHeld ? sensitivity.adsMultiplier : 1f;
        float yawDelta = delta.x * 0.05f * sensitivity.horizontal * ads * accel;
        float pitchDelta = -delta.y * 0.05f * sensitivity.vertical * ads * accel;

        _yaw += yawDelta;
        _pitch = Mathf.Clamp(_pitch + pitchDelta, -maxPitch, maxPitch);
        ApplyRotation();

        LastYawDelta = yawDelta;
        LastPitchDelta = pitchDelta;
        LastAngularSpeed = new Vector2(yawDelta, pitchDelta).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        bool wantsFire = Mouse.current.leftButton.wasPressedThisFrame;
        if (wantsFire && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + fireCooldown;
            FirePressedThisFrame = true;
            Fire();
        }
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    void Fire()
    {
        OnShot?.Invoke();
        Ray ray = new Ray(LookOrigin, LookDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, aimRayDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            var target = hit.collider.GetComponentInParent<AimTarget>();
            if (target != null)
            {
                target.RegisterHit(hit.point);
                OnShotHit?.Invoke(hit);
                return;
            }
        }

        OnShotMiss?.Invoke();
    }

    public bool IsLookingAt(Collider col, float maxDistance = -1f)
    {
        if (col == null) return false;
        float dist = maxDistance > 0f ? maxDistance : aimRayDistance;
        Ray ray = new Ray(LookOrigin, LookDirection);
        if (!Physics.Raycast(ray, out RaycastHit hit, dist, hitMask, QueryTriggerInteraction.Ignore))
            return false;
        return hit.collider == col || hit.collider.transform.IsChildOf(col.transform);
    }

    public float SignedHorizontalErrorDegrees(Vector3 worldPoint)
    {
        Vector3 to = worldPoint - LookOrigin;
        to.y = 0f;
        Vector3 fwd = LookDirection;
        fwd.y = 0f;
        if (to.sqrMagnitude < 0.0001f || fwd.sqrMagnitude < 0.0001f)
            return 0f;
        return Vector3.SignedAngle(fwd.normalized, to.normalized, Vector3.up);
    }

    public float AngularErrorDegrees(Vector3 worldPoint)
    {
        Vector3 to = (worldPoint - LookOrigin).normalized;
        return Vector3.Angle(LookDirection, to);
    }
}
