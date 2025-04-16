using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ArtistProfileUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI welcomeText;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button myTattoosButton;
    [SerializeField] private Button arButton;
    [SerializeField] private Button logoutButton;
    
    [Header("Panel References")]
    [SerializeField] private GameObject uploadPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject myTattoosPanel;
    
    [Header("Settings")]
    [SerializeField] private string mainSceneName = "MainARScene";
    
    private AuthManager authManager;
    
    private void Start()
    {
        // Find auth manager instance
        authManager = AuthManager.Instance;
        
        if (authManager == null)
        {
            Debug.LogError("AuthManager instance not found!");
            return;
        }
        
        // Set up button listeners
        uploadButton.onClick.AddListener(OnUploadButtonClicked);
        myTattoosButton.onClick.AddListener(OnMyTattoosButtonClicked);
        arButton.onClick.AddListener(OnARButtonClicked);
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        
        // Subscribe to auth events
        authManager.OnLogoutSuccess += HandleLogoutSuccess;
        
        // Set welcome text
        if (authManager.IsLoggedIn)
        {
            welcomeText.text = $"Welcome, {authManager.DisplayName}!";
        }
    }
    
    private void OnEnable()
    {
        // Update welcome text whenever panel is shown
        if (authManager != null && authManager.IsLoggedIn)
        {
            welcomeText.text = $"Welcome, {authManager.DisplayName}!";
        }
    }
    
    private void OnUploadButtonClicked()
    {
        // Show upload panel
        authManager.ShowUploadPanel();
    }
    
    private void OnMyTattoosButtonClicked()
    {
        // Show My Tattoos panel
        if (myTattoosPanel != null)
        {
            myTattoosPanel.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("My Tattoos panel not assigned!");
        }
    }
    
    private void OnARButtonClicked()
    {
        // Go to main AR scene
        authManager.GoToARScene();
    }
    
    private void OnLogoutButtonClicked()
    {
        // Log out the user
        authManager.LogoutArtist();
    }
    
    private void HandleLogoutSuccess()
    {
        // The AuthManager will handle showing the login panel
        Debug.Log("Logout successful");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (authManager != null)
        {
            authManager.OnLogoutSuccess -= HandleLogoutSuccess;
        }
    }
}