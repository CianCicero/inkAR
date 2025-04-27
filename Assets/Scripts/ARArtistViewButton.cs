using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple button to view the artist's profile from the AR scene
/// </summary>
public class ARArtistViewButton : MonoBehaviour
{
    [SerializeField] private Button viewArtistButton;
    [SerializeField] private string artistProfileSceneName = "ArtistProfileScene"; // Name of your artist profile scene
    
    private void Start()
    {
        if (viewArtistButton == null)
            viewArtistButton = GetComponent<Button>();
            
        if (viewArtistButton != null)
        {
            viewArtistButton.onClick.AddListener(OnViewArtistButtonClicked);
            
            // Check if we have an artist ID in PlayerPrefs
            string artistId = PlayerPrefs.GetString("CurrentArtistId", "");
            if (string.IsNullOrEmpty(artistId))
            {
                // No artist ID available, disable the button
                viewArtistButton.interactable = false;
                Debug.LogWarning("No artist ID found. View Artist button disabled.");
            }
            else
            {
                Debug.Log($"Artist ID found: {artistId}. View Artist button enabled.");
            }
        }
    }
    
    private void OnViewArtistButtonClicked()
    {
        // The artistId is already in PlayerPrefs, so we just need to load the artist profile scene
        SceneManager.LoadScene(artistProfileSceneName);
    }
    
    private void OnDestroy()
    {
        if (viewArtistButton != null)
        {
            viewArtistButton.onClick.RemoveListener(OnViewArtistButtonClicked);
        }
    }
}