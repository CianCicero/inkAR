using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using System.Collections;

/// <summary>
/// Manager for Firebase Authentication operations
/// </summary>
public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase variables
    private FirebaseAuth auth;
    private FirebaseUser user;
    
    // Delegate for authentication callbacks
    public delegate void AuthCallback(bool success, string errorMessage);
    
    // Property to check if user is logged in
    public bool IsLoggedIn => user != null;
    
    // Property to get current user ID
    public string UserId => user?.UserId;
    
    // Property to get user display name
    public string DisplayName => user?.DisplayName;
    
    // Property to get user email
    public string Email => user?.Email;
    
    // Initialize Firebase
    private void Start()
    {
        StartCoroutine(InitializeFirebase());
    }
    
    private IEnumerator InitializeFirebase()
    {
        // Wait until Firebase is ready
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        
        yield return new WaitUntil(() => dependencyTask.IsCompleted);
        
        DependencyStatus dependencyStatus = dependencyTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            // Initialize Firebase Auth
            auth = FirebaseAuth.DefaultInstance;
            
            // Set up auth state changed callback
            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
            
            Debug.Log("Firebase Auth initialized successfully");
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
        }
    }
    
    // Handle auth state changes
    private void AuthStateChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != user)
        {
            bool wasLoggedIn = user != null;
            user = auth.CurrentUser;
            bool isLoggedIn = user != null;
            
            if (wasLoggedIn != isLoggedIn)
            {
                if (isLoggedIn)
                {
                    Debug.Log($"User logged in: {user.DisplayName} ({user.UserId})");
                    
                    // Notify other components about successful login
                    AuthManager authManager = FindObjectOfType<AuthManager>();
                    if (authManager != null)
                    {
                        authManager.OnUserLoggedIn(user.DisplayName, user.UserId);
                    }
                }
                else
                {
                    Debug.Log("User logged out");
                    
                    // Notify other components about logout
                    AuthManager authManager = FindObjectOfType<AuthManager>();
                    if (authManager != null)
                    {
                        authManager.OnUserLoggedOut();
                    }
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Remove the callback when the component is destroyed
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
        }
    }
    
    // Sign in with email and password
    public void SignInWithEmailPassword(string email, string password, AuthCallback callback)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            // Return to the main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (task.IsCanceled)
                {
                    callback(false, "Login was cancelled.");
                    return;
                }
                
                if (task.IsFaulted)
                {
                    // Get the first exception since that's the most relevant
                    callback(false, GetErrorMessage(task.Exception));
                    return;
                }
                
                callback(true, null);
            });
        });
    }
    
    // Register with email and password
    public void RegisterWithEmailPassword(string email, string password, string displayName, AuthCallback callback)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            // Return to the main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (task.IsCanceled)
                {
                    callback(false, "Registration was cancelled.");
                    return;
                }
                
                if (task.IsFaulted)
                {
                    callback(false, GetErrorMessage(task.Exception));
                    return;
                }
                
                // Registration successful
                FirebaseUser newUser = auth.CurrentUser; 
                
                UserProfile profile = new UserProfile
                {
                    DisplayName = displayName
                };
                
                newUser.UpdateUserProfileAsync(profile).ContinueWith(profileTask => {
                    // Return to the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() => {
                        if (profileTask.IsCanceled || profileTask.IsFaulted)
                        {
                            Debug.LogWarning("Setting display name failed, but user was created.");
                        }
                        
                        callback(true, null);
                    });
                });
            });
        });
    }
    
    // Sign out
    public void SignOut()
    {
        auth.SignOut();
    }
    
    // Helper method to extract error message from Firebase exception and display a user-friendly message
    private string GetErrorMessage(Exception exception)
    {
        if (exception == null)
        {
            return "Unknown error";
        }
        
        FirebaseException firebaseException = exception.GetBaseException() as FirebaseException;
        if (firebaseException != null)
        {
            AuthError authError = (AuthError)firebaseException.ErrorCode;
            switch (authError)
            {
                case AuthError.MissingEmail:
                    return "Email is missing";
                case AuthError.MissingPassword:
                    return "Password is missing";
                case AuthError.WeakPassword:
                    return "Password is too weak";
                case AuthError.InvalidEmail:
                    return "Email is invalid";
                case AuthError.UserNotFound:
                    return "Account not found";
                case AuthError.WrongPassword:
                    return "Incorrect password";
                case AuthError.EmailAlreadyInUse:
                    return "Email is already in use";
                case AuthError.NetworkRequestFailed:
                    return "Network error. Check your connection";
                default:
                    return firebaseException.Message;
            }
        }
        
        return exception.Message;
    }
}