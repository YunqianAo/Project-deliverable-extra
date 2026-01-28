using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con UI
using System.Collections;

public class CanvasManager2 : MonoBehaviour
{
    public GameObject logoCanvas;       // Arrastra tu canvas LOGO aquí
    public GameObject titleCanvas;      // Arrastra tu canvas TITLE aquí
    // AudioSource para reproducir audio en TITLE

    public float fadeDuration = 1f;     // Duración del desvanecimiento

    private void Start()
    {
        StartCoroutine(ShowLogoAndFade());
    }

    private IEnumerator ShowLogoAndFade()
    {
        // Mostrar el logo
        titleCanvas.SetActive(false); // Asegúrate de que TITLE esté desactivado inicialmente
        logoCanvas.SetActive(true);

        // Esperar hasta que se presione la tecla Espacio
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null; // Esperar un frame y volver a comprobar
        }

        // Obtener o agregar el CanvasGroup para manejar el fade
        CanvasGroup logoGroup = logoCanvas.GetComponent<CanvasGroup>();
        if (logoGroup == null)
        {
            logoGroup = logoCanvas.AddComponent<CanvasGroup>();
        }

        // Fade out
        float startTime = Time.time;
        while (Time.time < startTime + fadeDuration)
        {
            float alpha = 1 - ((Time.time - startTime) / fadeDuration);
            logoGroup.alpha = alpha;
            yield return null;
        }
        logoGroup.alpha = 0;

        // Cambiar a TITLE
        logoCanvas.SetActive(false);
        titleCanvas.SetActive(true); // Solo se activa el canvas TITLE
    }
}
