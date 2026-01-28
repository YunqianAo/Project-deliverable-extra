using System.Collections;
using UnityEngine;
using TMPro; // Importante si usas TextMeshPro

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent; // Referencia al componente TextMeshPro
    public float typingSpeed = 0.05f; // Velocidad de escritura en segundos por letra
    public AudioSource typingSound; // Sonido que se reproducirá mientras se escribe

    private string fullText; // Texto completo
    private string currentText = ""; // Texto que se va mostrando

    void Start()
    {
        // Asegúrate de que el texto esté vacío al inicio
        fullText = textComponent.text;
        textComponent.text = "";

        // Inicia la animación
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            currentText += fullText[i];
            textComponent.text = currentText;

            // Reproduce el sonido si está configurado
            if (typingSound != null && !typingSound.isPlaying)
            {
                typingSound.Play();
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
