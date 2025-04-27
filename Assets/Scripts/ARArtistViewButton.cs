using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ARArtistViewButton : MonoBehaviour
{
    [SerializeField] private Button viewArtistButton;
    [SerializeField] private string artistProfileSceneName = "ArtistProfileScene";
    
    private void Start()
    {
        if (viewArtistButton == null)
            viewArtistButton = GetComponent<Button>();
            
        if (viewArtistButton != null)
        {
            viewArtistButton.onClick.AddListener(OnViewArtistButtonClicked);
            
            string artistId = PlayerPrefs.GetString("CurrentArtistId", "");
            if (string.IsNullOrEmpty(artistId))
            {
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
        // The artistId is already in PlayerPrefs
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