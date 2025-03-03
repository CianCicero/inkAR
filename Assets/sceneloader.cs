using UnityEngine;
using UnityEngine.SceneManagement;

public class Sceneloader : MonoBehaviour
{
    public void LoadTattooSelectionScene()
    {
        SceneManager.LoadScene("TattooSelectionScene");
    }
}
