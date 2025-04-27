using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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
        uploadButton.onClick.AddListener(() => StartCoroutine(OnUploadButtonClicked()));
        myTattoosButton.onClick.AddListener(() => StartCoroutine(OnMyTattoosButtonClicked()));
        arButton.onClick.AddListener(() => StartCoroutine(OnARButtonClicked()));
        logoutButton.onClick.AddListener(() => StartCoroutine(OnLogoutButtonClicked()));
        
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

        ResetButtonSizes();


    }

    public void ResetButtonSizes()
    {
        // Reset button sizes to default
        uploadButton.transform.localScale = Vector3.one;
        myTattoosButton.transform.localScale = Vector3.one;
        arButton.transform.localScale = Vector3.one;
        logoutButton.transform.localScale = Vector3.one;
    }
    
    private IEnumerator OnUploadButtonClicked()
    {
        // Wait for 0.4 seconds
        yield return new WaitForSeconds(0.4f);

        // Show upload panel
        authManager.ShowUploadPanel();
    }
    
    private IEnumerator OnMyTattoosButtonClicked()
    {
        // Wait for 0.4 seconds
        yield return new WaitForSeconds(0.4f);

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
    
    private IEnumerator OnARButtonClicked()
    {
        // Wait for 0.4 seconds
        yield return new WaitForSeconds(0.4f);

        // Go to main AR scene
        authManager.GoToARScene();
    }
    
    private IEnumerator OnLogoutButtonClicked()
    {
        // Wait for 0.4 seconds
        yield return new WaitForSeconds(0.4f);

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
