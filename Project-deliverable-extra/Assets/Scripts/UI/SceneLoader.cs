using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGameScene()
    {
        // Cambia a la escena del juego
        SceneManager.LoadScene("ServerTest");
    }
}
