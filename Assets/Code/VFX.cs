using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(AudioSource))]
public class VFXSoundPlayer : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private AudioClip sound;
    [SerializeField] private Vector3 effectOffset;
    [SerializeField] private float effectLifetime = 2f;
    [Range(0f, 1f)][SerializeField] private float soundVolume = 1f;

    [Header("Play Options")]
    [SerializeField] private bool playOnStart;
    [SerializeField] private bool playOnCollision;
    [SerializeField] private bool playOnTrigger;

    [Header("Keyboard")]
    [SerializeField] private bool playOnKeyPress;
    [SerializeField] private KeyCode keyToPress = KeyCode.F;

    private AudioSource audioSource;
    private float nextPlayTime = -1f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (playOnStart) PlayEffect();
    }

    private void Update()
    {
        if (!playOnKeyPress || keyToPress == KeyCode.None) return;

        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var key = Keyboard.current.FindKeyOnCurrentKeyboardLayout(keyToPress.ToString());
            pressed = key != null && key.wasPressedThisFrame;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(keyToPress);
#endif
        if (pressed) PlayEffect();
    }

    public void PlayEffect()
    {
        if (Time.time < nextPlayTime) return;
        nextPlayTime = Time.time + 0.05f;

        if (vfxPrefab != null)
        {
            GameObject effect = Instantiate(vfxPrefab, transform.position + effectOffset, transform.rotation);
            effectLifetime = Mathf.Max(0.1f, effectLifetime);
            Destroy(effect, effectLifetime);
        }

        if (sound != null) audioSource.PlayOneShot(sound, soundVolume);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (playOnCollision) PlayEffect();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playOnTrigger) PlayEffect();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (playOnCollision) PlayEffect();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnTrigger) PlayEffect();
    }

    private void OnValidate()
    {
        effectLifetime = Mathf.Max(0.1f, effectLifetime);
        soundVolume = Mathf.Clamp01(soundVolume);
    }
}