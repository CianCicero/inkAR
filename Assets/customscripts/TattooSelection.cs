using UnityEngine;
using UnityEngine.SceneManagement;

public class TattooSelection : MonoBehaviour
{
    // Method to be called when a button is pressed
    public void SelectTattoo(string tattooName)
    {
        // Store the selected tattoo in PlayerPrefs
        PlayerPrefs.SetString("SelectedTattoo", tattooName);

        // Load the AR scene
        SceneManager.LoadScene("imageTrackingTattoo");
    }
}
