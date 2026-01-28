using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class UIButtonSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [Header("Audio Clips")]
    public AudioClip hoverSound;  // Sonido al pasar el ratón
    public AudioClip clickSound;  // Sonido al hacer clic

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Método que se ejecuta al hacer clic en el botón
    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(clickSound);
    }

    // Método que se ejecuta al pasar el ratón sobre el botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(hoverSound);
    }

    // Método para reproducir sonidos
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
