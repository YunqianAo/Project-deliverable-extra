using UnityEngine;
using UnityEngine.UI; // Necesario para trabajar con UI
using System.Collections;

public class CanvasManager : MonoBehaviour
{
    public GameObject logoCanvas;       // Arrastra tu canvas LOGO aquÅE
    public GameObject titleCanvas;      // Arrastra tu canvas TITLE aquÅE
    // AudioSource para reproducir audio en TITLE

    public float fadeDuration = 1f;     // DuraciÛn del desvanecimiento

    private void Start()
    {
        StartCoroutine(ShowLogoAndFade());
    }

    private IEnumerator ShowLogoAndFade()
    {
        // Mostrar el logo
        titleCanvas.SetActive(true); // Aseg˙rate de que TITLE estÅEdesactivado inicialmente
        logoCanvas.SetActive(true);
       
        // Esperar 3 segundos
        yield return new WaitForSeconds(2f);
      
        // Desvanecer el canvas LOGO
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
        Destroy(logoCanvas);
        titleCanvas.SetActive(true); // Solo se activa el canvas TITLE

    }

   

}
