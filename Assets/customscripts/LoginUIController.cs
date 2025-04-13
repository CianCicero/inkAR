using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using System;

/// <summary>
/// Handles UI interactions for the login panel
/// </summary>
public class LoginUIController : MonoBehaviour
{
    // References to UI elements
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private TextMeshProUGUI statusText;
    
    // Reference to the authentication manager
    private FirebaseAuthManager authManager;
    
    private void Awake()
    { 
        // Get auth manager reference
        authManager = FindObjectOfType<FirebaseAuthManager>();
    }
    
    private void Start()
    {
        // Set up button listeners
        loginButton.onClick.AddListener(OnLoginButtonClick);
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        
        // Clear status text
        statusText.text = "";
        
        // Set up input field events
        emailField.onSelect.AddListener((_) => statusText.text = "");
        passwordField.onSelect.AddListener((_) => statusText.text = "");
    }
    
    private void OnLoginButtonClick()
    {
        // Validate input
        if (string.IsNullOrEmpty(emailField.text))
        {
            statusText.text = "Email is required";
            return;
        }
        
        if (string.IsNullOrEmpty(passwordField.text))
        {
            statusText.text = "Password is required";
            return;
        }
        
        // Disable the login button to prevent multiple attempts
        loginButton.interactable = false;
        statusText.text = "Signing in...";
        
        // Call the auth manager to sign in
        authManager.SignInWithEmailPassword(emailField.text, passwordField.text, OnSignInCompleted);
    }
    
    private void OnSignInCompleted(bool success, string errorMessage)
    {
        // Re-enable the login button
        loginButton.interactable = true;
        
        if (success)
        {
            // Clear the status text
            statusText.text = "";
            
            // Show the artist profile panel
            ShowArtistProfilePanel();
        }
        else
        {
            // Show error message
            statusText.text = errorMessage;
        }
    }
    
    private void OnRegisterButtonClick()
    {
        // Hide login panel and show register panel
        gameObject.SetActive(false);
        transform.parent.Find("RegisterPanel").gameObject.SetActive(true);
    }
    
    private void ShowArtistProfilePanel()
    {
        // Hide login panel and show artist profile panel
        gameObject.SetActive(false);
        transform.parent.Find("ArtistProfilePanel").gameObject.SetActive(true);
    }
}