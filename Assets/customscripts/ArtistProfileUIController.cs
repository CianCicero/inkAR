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
    
    [Header("Settings")]
    [SerializeField] private string mainSceneName;
    
    [Header("Dependencies")]
    [SerializeField] private AuthManager authManager;
    
    private void Start()
    {
        if (authManager == null)
        {
            authManager = FindObjectOfType<AuthManager>();
            if (authManager == null)
            {
                Debug.LogError("AuthManager not found in scene! Please add the AuthManager prefab.");
                return;
            }
        }
        
        // Set up button listeners
        uploadButton.onClick.AddListener(OnUploadButtonClicked);
        myTattoosButton.onClick.AddListener(OnMyTattoosButtonClicked);
        arButton.onClick.AddListener(OnARButtonClicked);
        logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        
        // Subscribe to auth events
        authManager.OnLogoutSuccess += HandleLogoutSuccess;
        
        // Set welcome text
        if (AuthManager.Instance.IsLoggedIn)
        {
            string artistName = AuthManager.Instance.DisplayName;
            welcomeText.text = $"Welcome, {artistName}!";
        }
    }
    
    private void OnEnable()
    {
        // Update welcome text whenever panel is shown
        if (AuthManager.Instance.IsLoggedIn)
        {
            string artistName = AuthManager.Instance.DisplayName;
            welcomeText.text = $"Welcome, {artistName}!";
        }
    }
    
    private void OnUploadButtonClicked()
    {
        // Show upload panel
        if (uploadPanel != null)
        {
            uploadPanel.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Upload panel not assigned!");
        }
    }
    
    private void OnMyTattoosButtonClicked()
    {
        Debug.Log("My Tattoos feature not implemented yet");
    }
    
    private void OnARButtonClicked()
    {
        // Load the main AR scene
        SceneManager.LoadScene(mainSceneName);
    }
    
    private void OnLogoutButtonClicked()
    {
        // Log out the user
        authManager.LogoutArtist();
    }
    
    private void HandleLogoutSuccess()
    {
        // Show login panel
        gameObject.SetActive(false);
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Login panel not assigned!");
        }
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