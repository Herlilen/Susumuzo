using UnityEngine;

public class AimTarget : MonoBehaviour
{
    public System.Action<AimTarget, Vector3> OnHit;

    Renderer _renderer;
    Material _mat;
    Color _baseColor = new Color(0.95f, 0.35f, 0.25f);
    Color _flashColor = new Color(1f, 0.9f, 0.2f);
    float _flashUntil;

    public void Configure(Color color, float scale = 1f)
    {
        _baseColor = color;
        transform.localScale = Vector3.one * scale;
        EnsureMaterial();
        ApplyColor(_baseColor);
    }

    public void RegisterHit(Vector3 point)
    {
        _flashUntil = Time.time + 0.08f;
        ApplyColor(_flashColor);
        OnHit?.Invoke(this, point);
    }

    void Update()
    {
        if (_flashUntil > 0f && Time.time >= _flashUntil)
        {
            _flashUntil = 0f;
            ApplyColor(_baseColor);
        }
    }

    void EnsureMaterial()
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();
        if (_renderer == null) return;

        if (_mat == null)
        {
            _mat = RuntimeGraphics.CreateColored(_baseColor);
            _renderer.sharedMaterial = _mat;
        }
    }

    void ApplyColor(Color c)
    {
        EnsureMaterial();
        if (_mat == null) return;
        RuntimeGraphics.ApplyColor(_mat, c);
    }

    void OnDestroy()
    {
        if (_mat != null)
            Destroy(_mat);
    }
}
