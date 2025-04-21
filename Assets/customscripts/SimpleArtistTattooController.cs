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

public class SimpleArtistTattooController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private GameObject tattooItemPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private TextMeshProUGUI noTattoosText;
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private GameObject artistProfilePanel;

    // Firebase references
    private FirebaseFirestore db;
    private FirebaseStorage storage;
    
    // Keep track of instantiated items for cleanup
    private List<GameObject> tattooItems = new List<GameObject>();
    
    void Start()
    {
        // Set up button listener
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => {
                gameObject.SetActive(false);
                if (artistProfilePanel != null)
                {
                    artistProfilePanel.SetActive(true);
                }
            });
        }
        
        // Set up refresh button listener
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(() => {
                LoadArtistTattoos();
            });
        }
        
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            db = FirebaseFirestore.DefaultInstance;
            storage = FirebaseStorage.DefaultInstance;
            Debug.Log("Firebase initialized in SimpleArtistTattoos");
            
            // Load tattoos after Firebase is initialized
            LoadArtistTattoos();
        });
    }
    
    void OnEnable()
    {
        // When panel becomes active, load tattoos if Firebase is ready
        if (db != null)
        {
            LoadArtistTattoos();
        }
    }
    
    public void LoadArtistTattoos()
    {
        // Show loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        // Clear any previous tattoo items
        ClearTattooItems();
        
        // Get current user ID
        string currentUserId = "123456"; // Default for testing
        
        // If we have a Firebase Auth manager, get the actual user ID
        FirebaseAuthManager authManager = FindObjectOfType<FirebaseAuthManager>();
        if (authManager != null && authManager.IsLoggedIn)
        {
            currentUserId = authManager.UserId;
        }
        
        // Query Firestore for this artist's tattoos
        db.Collection("tattoometa")
            .WhereEqualTo("artistId", currentUserId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Error loading tattoos: {task.Exception}");
                    if (loadingIndicator != null) loadingIndicator.SetActive(false);
                    return;
                }
                
                var querySnapshot = task.Result;
                
                // Show "no tattoos" message if needed
                if (querySnapshot.Count == 0)
                {
                    if (noTattoosText != null) noTattoosText.gameObject.SetActive(true);
                    if (loadingIndicator != null) loadingIndicator.SetActive(false);
                    return;
                }
                
                // Hide "no tattoos" message
                if (noTattoosText != null) noTattoosText.gameObject.SetActive(false);
                
                // Create an item for each tattoo
                foreach (var document in querySnapshot.Documents)
                {
                    try
                    {
                        // Create tattoo item
                        CreateTattooItem(
                            documentId: document.Id,
                            name: document.GetValue<string>("tattooName"),
                            imageUrl: document.GetValue<string>("imageURL"),
                            artistName: document.GetValue<string>("artistName")
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error creating tattoo item: {ex.Message}");
                    }
                }
                
                // Hide loading indicator
                if (loadingIndicator != null) loadingIndicator.SetActive(false);
            });
    }
    
    private void CreateTattooItem(string documentId, string name, string imageUrl, string artistName)
    {
        if (tattooItemPrefab == null || gridLayout == null)
        {
            Debug.LogError("Missing references. Check tattooItemPrefab and gridLayout.");
            return;
        }
        
        // Instantiate the prefab as a child of the grid layout
        GameObject item = Instantiate(tattooItemPrefab, gridLayout.transform);
        tattooItems.Add(item);
        
        // Load the tattoo image
        RawImage image = item.GetComponentInChildren<RawImage>();
        if (image != null && !string.IsNullOrEmpty(imageUrl))
        {
            StartCoroutine(LoadTattooImage(imageUrl, image));
        }
        
        // Set up delete button
        Button deleteButton = item.GetComponentInChildren<Button>();
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => DeleteTattoo(documentId, imageUrl));
        }
    }
    
    private IEnumerator LoadTattooImage(string imageUrl, RawImage targetImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                targetImage.texture = texture;
            }
            else
            {
                Debug.LogError($"Failed to load image: {request.error}");
            }
        }
    }
    
    private void DeleteTattoo(string documentId, string imageUrl)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        // Delete document from Firestore
        db.Collection("tattoometa").Document(documentId)
            .DeleteAsync()
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Error deleting tattoo: {task.Exception}");
                }
                else
                {
                    Debug.Log($"Tattoo deleted successfully");
                    
                    // Try to delete image from storage
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            StorageReference imageRef = storage.GetReferenceFromUrl(imageUrl);
                            imageRef.DeleteAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Could not delete image: {ex.Message}");
                        }
                    }
                    
                    // Reload the tattoos
                    LoadArtistTattoos();
                }
            });
    }
    
    private void ClearTattooItems()
    {
        foreach (var item in tattooItems)
        {
            Destroy(item);
        }
        tattooItems.Clear();
        
        if (noTattoosText != null)
        {
            noTattoosText.gameObject.SetActive(false);
        }
    }
    
    // Public method to reload tattoos (can be called from outside this script)
    public void RefreshTattoos()
    {
        LoadArtistTattoos();
    }
}