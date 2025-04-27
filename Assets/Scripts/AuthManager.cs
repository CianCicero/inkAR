using System; 
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages authentication state and UI panel visibility throughout the app
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    
    // User information
    private string userId;
    private string displayName;
    
    // Properties
    public bool IsLoggedIn => !string.IsNullOrEmpty(userId);
    public string UserId => userId;
    public string DisplayName => displayName;
    
    // UI Panels
    private GameObject loginPanel;
    private GameObject registerPanel;
    private GameObject artistProfilePanel;
    private GameObject uploadPanel;

    // Event triggered on successful logout
    public event Action OnLogoutSuccess;
    
    // Initialize singleton instance
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Find all panels
        Transform canvasTransform = FindObjectOfType<Canvas>().transform;
        
        loginPanel = canvasTransform.Find("LoginPanel").gameObject;
        registerPanel = canvasTransform.Find("RegisterPanel").gameObject;
        artistProfilePanel = canvasTransform.Find("ArtistProfilePanel").gameObject;
        uploadPanel = canvasTransform.Find("UploadPanel").gameObject;
    }
    
    private void Start()
    {
        // By default, show the login panel and hide others
        ShowLoginPanel();
    }
    
    // Called by FirebaseAuthManager when a user logs in
    public void OnUserLoggedIn(string name, string id)
    {
        displayName = name;
        userId = id;
        
        // Update UI to show the artist name
        if (artistProfilePanel != null)
        {
            Transform welcomeTransform = artistProfilePanel.transform.Find("WelcomeText");
            if (welcomeTransform != null)
            {
                TextMeshProUGUI welcomeText = welcomeTransform.GetComponent<TextMeshProUGUI>();
                if (welcomeText != null)
                {
                    welcomeText.text = $"Welcome, {displayName}!";
                }
            }
        }
        
        // Show the artist profile panel
        ShowArtistProfilePanel();
        
        // Save auth state if needed
        SaveAuthState();
    }
    
    // Called by FirebaseAuthManager when a user logs out
    public void OnUserLoggedOut()
    {
        displayName = null;
        userId = null;
        
        // Show the login panel
        ShowLoginPanel();
        
        // Clear saved auth state
        ClearAuthState();

        // Trigger the OnLogoutSuccess event
        OnLogoutSuccess?.Invoke();
    }
    
    // Show login panel and hide others
    public void ShowLoginPanel()
    {
        SetActivePanel(loginPanel);
    }
    
    // Show register panel and hide others
    public void ShowRegisterPanel()
    {
        SetActivePanel(registerPanel);
    }
    
    // Show artist profile panel and hide others
    public void ShowArtistProfilePanel()
    {
        SetActivePanel(artistProfilePanel);
    }
    
    // Show upload panel and hide others
    public void ShowUploadPanel()
    {
        SetActivePanel(uploadPanel);
    }
    
    // Helper method to set active panel
    private void SetActivePanel(GameObject activePanel)
    {
        if (loginPanel != null) loginPanel.SetActive(loginPanel == activePanel);
        if (registerPanel != null) registerPanel.SetActive(registerPanel == activePanel);
        if (artistProfilePanel != null) artistProfilePanel.SetActive(artistProfilePanel == activePanel);
        if (uploadPanel != null) uploadPanel.SetActive(uploadPanel == activePanel);
    }
    
    // Go to AR scene
    public void GoToARScene()
    {
        Debug.Log("Navigate to AR Scene");
    }
    
    // Save authentication state
    private void SaveAuthState()
    {
        // Save minimal auth info to PlayerPrefs
        PlayerPrefs.SetString("UserId", userId);
        PlayerPrefs.SetString("DisplayName", displayName);
        PlayerPrefs.Save();
    }
    
    // Clear saved authentication state
    private void ClearAuthState()
    {
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.DeleteKey("DisplayName");
        PlayerPrefs.Save();
    }
    
    // Load saved authentication state
    private void LoadAuthState()
    {
        if (PlayerPrefs.HasKey("UserId"))
        {
            userId = PlayerPrefs.GetString("UserId");
            displayName = PlayerPrefs.GetString("DisplayName");
            
            // Check with Firebase if this session is still valid
            FirebaseAuthManager firebaseAuthManager = FindObjectOfType<FirebaseAuthManager>();
            if (firebaseAuthManager != null && firebaseAuthManager.IsLoggedIn)
            {
                // Session is valid, show artist profile
                OnUserLoggedIn(displayName, userId);
            }
            else
            {
                // Session is invalid, clear state and show login
                ClearAuthState();
                ShowLoginPanel();
            }
        }
    }

    public void LogoutArtist()
    {
        // Call the OnUserLoggedOut method to handle logout logic
        OnUserLoggedOut();
    }
}