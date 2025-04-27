using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine.Networking;
using System.Linq;
using System;

[System.Serializable]
public class ArtistTattooItem
{
    public string tattooName;
    public string imageURL;
    public string artistName;
    public string[] tags;
    public string documentId; // Store Firestore document ID
}

public class MyTattoosUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform tattooContainer; // Parent transform for tattoo items
    [SerializeField] private GameObject tattooItemPrefab; // Prefab for each tattoo item
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI noTattoosText;
    [SerializeField] private Button refreshButton;
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("Panel References")]
    [SerializeField] private GameObject artistProfilePanel;
    
    // Firebase references
    private FirebaseFirestore db;
    private FirebaseStorage storage;
    
    private AuthManager authManager;
    private List<GameObject> instantiatedItems = new List<GameObject>();
    private List<ArtistTattooItem> artistTattoos = new List<ArtistTattooItem>();
    
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
        backButton.onClick.AddListener(OnBackButtonClicked);
        refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        
        // Initialize Firebase
        InitializeFirebase();
    }
    
    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            storage = FirebaseStorage.DefaultInstance;
            Debug.Log("Firebase initialized in MyTattoosUI");
        });
    }
    
    private void OnEnable()
    {
        // Load tattoos when panel becomes active
        if (db != null)
        {
            LoadTattoos();
        }
        else
        {
            // Firebase might not be initialized yet
            StartCoroutine(WaitForFirebaseAndLoad());
        }
    }
    
    private IEnumerator WaitForFirebaseAndLoad()
    {
        // Wait for Firebase to initialize
        yield return new WaitUntil(() => db != null);
        LoadTattoos();
    }
    
    private void OnBackButtonClicked()
    {
        // Go back to artist profile panel
        gameObject.SetActive(false);
        if (artistProfilePanel != null)
        {
            artistProfilePanel.SetActive(true);
        }
        else
        {
            // Try using AuthManager to show profile panel
            authManager.ShowArtistProfilePanel();
        }
    }
    
    private void OnRefreshButtonClicked()
    {
        // Reload tattoos
        LoadTattoos();
    }
    
    private void LoadTattoos()
    {
        // Show loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        // Clear existing items
        ClearTattooItems();

        FirebaseAuthManager firebaseAuthManager = FindObjectOfType<FirebaseAuthManager>();

        string currentArtistID = "123456";

        // Query Firestore for tattoos belonging to the current artist
        if (firebaseAuthManager != null && firebaseAuthManager.IsLoggedIn)
        {
            currentArtistID = firebaseAuthManager.UserId;
        }
        
        db.Collection("tattoometa")
            .WhereEqualTo("artistId", currentArtistID)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to fetch artist tattoos: " + task.Exception);
                    if (loadingIndicator != null)
                    {
                        loadingIndicator.SetActive(false);
                    }
                    return;
                }
                
                QuerySnapshot snapshot = task.Result;
                List<DocumentSnapshot> documents = snapshot.Documents.ToList();
                Debug.Log($"Found {documents.Count} tattoos for artist {currentArtistID}");
                
                artistTattoos.Clear();
                
                foreach (DocumentSnapshot document in documents)
                {
                    try
                    {
                        var tattoo = new ArtistTattooItem
                        {
                            tattooName = document.GetValue<string>("tattooName"),
                            imageURL = document.GetValue<string>("imageURL"),
                            artistName = document.GetValue<string>("artistName"),
                            tags = document.GetValue<List<string>>("tags")?.ToArray() ?? new string[0],
                            documentId = document.Id // Store the document ID for deletion
                        };
                        
                        artistTattoos.Add(tattoo);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error converting document to ArtistTattooItem: {ex.Message}");
                    }
                }
                
                // Display tattoos or show "no tattoos" message
                if (artistTattoos.Count == 0)
                {
                    if (noTattoosText != null)
                    {
                        noTattoosText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (noTattoosText != null)
                    {
                        noTattoosText.gameObject.SetActive(false);
                    }
                    
                    foreach (var tattoo in artistTattoos)
                    {
                        CreateTattooItem(tattoo);
                    }
                }
                
                // Hide loading indicator
                if (loadingIndicator != null)
                {
                    loadingIndicator.SetActive(false);
                }
            });
    }
    
    private void CreateTattooItem(ArtistTattooItem tattooData)
    {
        if (tattooItemPrefab == null || tattooContainer == null)
        {
            Debug.LogError("Tattoo item prefab or container not assigned!");
            return;
        }
        
        GameObject item = Instantiate(tattooItemPrefab, tattooContainer);
        instantiatedItems.Add(item);
        
        // Find components in the prefab
        TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        RawImage thumbnailImage = item.GetComponentInChildren<RawImage>();
        Button editButton = item.transform.Find("EditButton")?.GetComponent<Button>();
        Button deleteButton = item.transform.Find("DeleteButton")?.GetComponent<Button>();
        
        // Set up data in the UI - simplified to match DownloadAndApply approach
        if (nameText != null)
        {
            nameText.text = tattooData.tattooName;
        }
        else
        {
            Debug.LogError("Tattoo item prefab does not contain a TextMeshProUGUI component.");
        }
        
        // Load and display the thumbnail image
        if (thumbnailImage != null && !string.IsNullOrEmpty(tattooData.imageURL))
        {
            StartCoroutine(LoadAndDisplayTattooImage(tattooData.imageURL, thumbnailImage));
        }
        else
        {
            Debug.LogError("Tattoo item prefab does not contain a RawImage component.");
        }
        
        // Set up button actions
        if (editButton != null)
        {
            editButton.onClick.AddListener(() => OnEditTattooClicked(tattooData));
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => OnDeleteTattooClicked(tattooData));
        }
    }
    
    private IEnumerator LoadAndDisplayTattooImage(string imageURL, RawImage rawImage)
    {
        Debug.Log($"Loading image from URL: {imageURL}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            rawImage.texture = texture;  // Set the image texture to the RawImage component

            Debug.Log("Image loaded successfully.");
        }
    }
    
    private void OnEditTattooClicked(ArtistTattooItem tattooData)
    {
        Debug.Log($"Edit tattoo: {tattooData.tattooName}");
    }
    
    private void OnDeleteTattooClicked(ArtistTattooItem tattooData)
    {
        Debug.Log($"Delete tattoo: {tattooData.tattooName}");
        
        DeleteTattoo(tattooData);
    }
    
    private void DeleteTattoo(ArtistTattooItem tattooData)
    {
        if (db == null)
        {
            Debug.LogError("Firestore not initialized");
            return;
        }
        
        if (string.IsNullOrEmpty(tattooData.documentId))
        {
            Debug.LogError("Cannot delete tattoo: Document ID is missing");
            return;
        }
        
        // Show loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        // Delete the Firestore document
        db.Collection("tattoometa").Document(tattooData.documentId)
            .DeleteAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to delete tattoo: " + task.Exception);
                }
                else
                {
                    Debug.Log($"Tattoo '{tattooData.tattooName}' deleted successfully");
                    
                    if (!string.IsNullOrEmpty(tattooData.imageURL))
                    {
                        try {
                            // Try to get a reference to the storage object
                            StorageReference imageRef = storage.GetReferenceFromUrl(tattooData.imageURL);
                            
                            // Delete the file
                            imageRef.DeleteAsync().ContinueWithOnMainThread(deleteTask =>
                            {
                                if (deleteTask.IsFaulted || deleteTask.IsCanceled)
                                {
                                    Debug.LogWarning("Failed to delete tattoo image: " + deleteTask.Exception);
                                }
                                else
                                {
                                    Debug.Log("Tattoo image deleted successfully");
                                }
                            });
                        }
                        catch (Exception ex) {
                            Debug.LogWarning($"Could not delete image from storage: {ex.Message}");
                        }
                    }
                    
                    // Refresh the list
                    LoadTattoos();
                }
                
                // Hide loading indicator
                if (loadingIndicator != null)
                {
                    loadingIndicator.SetActive(false);
                }
            });
    }
    
    private void ClearTattooItems()
    {
        foreach (var item in instantiatedItems)
        {
            Destroy(item);
        }
        instantiatedItems.Clear();
        
        // Hide "no tattoos" message until we know the list is empty
        if (noTattoosText != null)
        {
            noTattoosText.gameObject.SetActive(false);
        }
    }
}