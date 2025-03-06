using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    public static bool IsFirebaseInitialized = false;

    void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                IsFirebaseInitialized = true;
                Debug.Log("Firebase Initialized");
            }
            else
            {
                Debug.LogError("Firebase Initialization failed: " + task.Exception);
            }
        });
    }
}
