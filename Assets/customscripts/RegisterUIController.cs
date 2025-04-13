using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;

/// <summary>
/// Handles UI interactions for the register panel
/// </summary>
public class RegisterUIController : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField artistNameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField confirmPasswordField;

    [Header("Buttons")]
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backButton;

    [Header("Status Text")]
    [SerializeField] private TextMeshProUGUI statusText;

    // Firebase Authentication Manager
    private FirebaseAuthManager authManager;

    private void Awake()
    {
        authManager = FindObjectOfType<FirebaseAuthManager>();
    }

    
    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        backButton.onClick.AddListener(OnBackButtonClick);
        
        statusText.text = "";
        
        emailField.onSelect.AddListener((_) => statusText.text = "");
        artistNameField.onSelect.AddListener((_) => statusText.text = "");
        passwordField.onSelect.AddListener((_) => statusText.text = "");
        confirmPasswordField.onSelect.AddListener((_) => statusText.text = "");
    }
    
    private void OnRegisterButtonClick()
    {
        // Validate input
        if (string.IsNullOrEmpty(emailField.text))
        {
            statusText.text = "Email is required";
            return;
        }
        
        if (string.IsNullOrEmpty(artistNameField.text))
        {
            statusText.text = "Artist name is required";
            return;
        }
        
        if (string.IsNullOrEmpty(passwordField.text))
        {
            statusText.text = "Password is required";
            return;
        }
        
        if (passwordField.text != confirmPasswordField.text)
        {
            statusText.text = "Passwords don't match";
            return;
        }
        
        if (passwordField.text.Length < 6)
        {
            statusText.text = "Password must be at least 6 characters";
            return;
        }
        
        // Disable the register button to prevent multiple attempts
        registerButton.interactable = false;
        statusText.text = "Creating account...";
        
        // Call the auth manager to register new user
        authManager.RegisterWithEmailPassword(emailField.text, passwordField.text, artistNameField.text, OnRegisterCompleted);
    }
    
    private void OnRegisterCompleted(bool success, string errorMessage)
    {
        // Re-enable the register button
        registerButton.interactable = true;
        
        if (success)
        {
            statusText.text = "";
            
            ShowArtistProfilePanel();
        }
        else
        {
            statusText.text = errorMessage;
        }
    }
    
    private void OnBackButtonClick()
    {
        // Hide register panel and show login panel
        gameObject.SetActive(false);
        transform.parent.Find("LoginPanel").gameObject.SetActive(true);
    }
    
    private void ShowArtistProfilePanel()
    {
        // Hide register panel and show artist profile panel
        gameObject.SetActive(false);
        transform.parent.Find("ArtistProfilePanel").gameObject.SetActive(true);
    }
}