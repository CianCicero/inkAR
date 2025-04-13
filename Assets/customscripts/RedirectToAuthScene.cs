using UnityEngine;
using UnityEngine.SceneManagement; 

public class RedirectToAuthScene : MonoBehaviour
{
    public void OnRedirectButtonClick()
    {
        // Load the AuthScene
        SceneManager.LoadScene("AuthScene"); 
    }
}
