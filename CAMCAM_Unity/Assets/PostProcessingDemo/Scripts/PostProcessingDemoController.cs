using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kasa.PostProcessingDemo
{
    /// <summary>
    /// Adds a little motion to the comparison scene so temporal CRT artifacts and
    /// ASCII sampling are easy to judge in Play Mode.
    /// </summary>
    public sealed class PostProcessingDemoController : MonoBehaviour
    {
        [SerializeField] private Transform[] rotatingSubjects;
        [SerializeField] private Light accentLight;
        [SerializeField] private float rotationSpeed = 28f;
        [Header("Filter switching")]
        [SerializeField] private RawImage output;
        [SerializeField] private Text modeLabel;
        [SerializeField] private Text detailLabel;
        [SerializeField] private RenderTexture normalSource;
        [SerializeField] private RenderTexture asciiSource;
        [SerializeField] private RenderTexture crtSource;
        [SerializeField] private Material asciiMaterial;
        [SerializeField] private Material crtMaterial;

        private int currentMode = -1;

        private void Start()
        {
            SetMode(0);
        }

        private void Update()
        {
            if (Pressed(1))
                SetMode(0);
            else if (Pressed(2))
                SetMode(1);
            else if (Pressed(3))
                SetMode(2);

            float direction = 1f;
            foreach (Transform subject in rotatingSubjects)
            {
                if (subject != null)
                {
                    subject.Rotate(0f, rotationSpeed * direction * Time.deltaTime, 0f, Space.World);
                    direction *= -1f;
                }
            }

            if (accentLight != null)
            {
                float hue = Mathf.Repeat(Time.time * 0.06f, 1f);
                accentLight.color = Color.HSVToRGB(hue, 0.7f, 1f);
            }
        }

        private void SetMode(int mode)
        {
            if (mode == currentMode || output == null)
                return;

            currentMode = mode;
            switch (mode)
            {
                case 0:
                    output.texture = normalSource;
                    output.material = null;
                    SetLabels("01  NORMAL", "Unprocessed reference");
                    break;
                case 1:
                    output.texture = asciiSource;
                    output.material = asciiMaterial;
                    SetLabels("02  ASCII", "Letter glyphs + quantized color blocks · 1920×1080 source");
                    break;
                default:
                    output.texture = crtSource;
                    output.material = crtMaterial;
                    SetLabels("03  CRT / RETRO", "320×180 · curvature · scanlines · chromatic aberration");
                    break;
            }
        }

        private void SetLabels(string title, string details)
        {
            if (modeLabel != null)
                modeLabel.text = title;
            if (detailLabel != null)
                detailLabel.text = details;
        }

        private static bool Pressed(int number)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return false;
            return number switch
            {
                1 => keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame,
                2 => keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame,
                3 => keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame,
                _ => false
            };
#else
            return number switch
            {
                1 => Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1),
                2 => Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2),
                3 => Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3),
                _ => false
            };
#endif
        }
    }
}
