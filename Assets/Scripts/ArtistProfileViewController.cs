using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;

public class ArtistProfileViewController : MonoBehaviour
{
    [Header("Artist Info")]
    [SerializeField] private TextMeshProUGUI artistNameText;
    [SerializeField] private TextMeshProUGUI artistEmailText;
    [SerializeField] private Button contactButton;
    
    [Header("Tattoo Display")]
    [SerializeField] private GameObject tattooButtonPrefab;
    [SerializeField] private Transform tattooContainer;
    [SerializeField] private TextMeshProUGUI noTattoosText;
    
    [Header("Navigation")]
    [SerializeField] private string returnToSceneName = "imageTrackingTattoo";
    
    [Header("Loading")]
    [SerializeField] private GameObject loadingIndicator;
    
    // Firebase references
    private FirebaseFirestore db;
    
    // Current artist being viewed
    private string currentArtistId;
    
    // Keep track of instantiated items for cleanup
    private List<GameObject> tattooItems = new List<GameObject>();
    
    private void Start()
    {
        
        // Set up Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => 
        {
            db = FirebaseFirestore.DefaultInstance;
            
            // Get artist ID from PlayerPrefs
            string artistId = PlayerPrefs.GetString("CurrentArtistId", "");
            if (!string.IsNullOrEmpty(artistId))
            {
                LoadArtistProfile(artistId);
            }
            else
            {
                Debug.LogError("No artist ID found in PlayerPrefs");
                if (artistNameText != null)
                    artistNameText.text = "Error: Artist not found";
                
                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);
            }
        });
    }
    
    public void LoadArtistProfile(string artistId)
    {
        currentArtistId = artistId;
        
        // Show loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        // Clear any existing tattoo items
        ClearTattooItems();
        
        // Load artist data
        LoadArtistInfo(artistId);
        
        // Load artist's tattoos
        LoadArtistTattoos(artistId);
    }
    
    private void LoadArtistInfo(string artistId)
    {
        // Try to get artist info from users collection
        db.Collection("users").Document(artistId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task => 
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Error loading artist info: {task.Exception}");
                    TryGetArtistInfoFromTattoos(artistId);
                    return;
                }
                
                DocumentSnapshot document = task.Result;
                if (document.Exists)
                {
                    // Get the artist name
                    string name = document.GetValue<string>("displayName") ?? "Unknown Artist";
                    
                    // Check email privacy setting
                    bool showPublicEmail = document.ContainsField("showPublicEmail") && document.GetValue<bool>("showPublicEmail");
                    string email = "";
                    
                    if (showPublicEmail)
                    {
                        email = document.GetValue<string>("publicEmail") ?? 
                               document.GetValue<string>("email") ?? 
                               "No email available";
                    }
                    else
                    {
                        email = "Artist has chosen not to share their email";
                    }
                    
                    UpdateArtistUI(name, email);
                }
                else
                {
                    TryGetArtistInfoFromTattoos(artistId);
                }
            });
    }
    
    private void TryGetArtistInfoFromTattoos(string artistId)
    {
        // Fallback: try to get artist name from their tattoos
        db.Collection("tattoometa")
            .WhereEqualTo("artistId", artistId)
            .Limit(1)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task => 
            {
                if (task.IsFaulted || ((Task<QuerySnapshot>)task).Result.Count == 0)
                {
                    UpdateArtistUI("Unknown Artist", "Contact info unavailable");
                    return;
                }
                
                DocumentSnapshot document = ((Task<QuerySnapshot>)task).Result.Documents.FirstOrDefault();
                string artistName = document.GetValue<string>("artistName") ?? "Unknown Artist";
                
                UpdateArtistUI(artistName, "Contact info unavailable");
            });
    }
    
    private void UpdateArtistUI(string name, string email)
    {
        // Update name display
        if (artistNameText != null)
        {
            artistNameText.text = $"{name}'s collection";
        }
        
        // Update email display
        if (artistEmailText != null)
        {
            artistEmailText.text = email;
        }
        
        // Set up contact button
        if (contactButton != null)
        {
            bool canContact = !string.IsNullOrEmpty(email) && 
                             email != "Contact info unavailable" && 
                             email != "Artist has chosen not to share their email";
                             
            contactButton.interactable = canContact;
            
        if (canContact)
            {
               contactButton.onClick.RemoveAllListeners();
               contactButton.onClick.AddListener(() => {
                // Create email intent
                string subject = "Tattoo Inquiry";
                string body = $"Hi {name},\n\nI saw your design in the InkAR app and I'm interested in booking a session. I have attached the placement reference to this email!\n\n";

                // Encode the parameters
                string encodedSubject = UnityWebRequest.EscapeURL(subject).Replace("+", "%20");
                string encodedBody = UnityWebRequest.EscapeURL(body).Replace("+", "%20");

                // Open the mail app
                Application.OpenURL($"mailto:{email}?subject={encodedSubject}&body={encodedBody}");
               });
            }
        }
    }
    
    private void LoadArtistTattoos(string artistId)
    {
        // Query for tattoos by this artist
        db.Collection("tattoometa")
            .WhereEqualTo("artistId", artistId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task => 
            {
                // Hide loading indicator
                if (loadingIndicator != null)
                {
                    loadingIndicator.SetActive(false);
                }
                
                if (task.IsFaulted)
                {
                    Debug.LogError($"Error loading tattoos: {task.Exception}");
                    return;
                }
                
                QuerySnapshot snapshot = task.Result;
                
                // Show message if no tattoos found
                if (snapshot.Count == 0)
                {
                    if (noTattoosText != null)
                    {
                        noTattoosText.gameObject.SetActive(true);
                    }
                    return;
                }
                
                // Hide no tattoos message
                if (noTattoosText != null)
                {
                    noTattoosText.gameObject.SetActive(false);
                }
                
                // Create tattoo items
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    try
                    {
                        string tattooName = document.GetValue<string>("tattooName") ?? "Untitled";
                        string imageURL = document.GetValue<string>("imageURL") ?? "";
                        
                        if (!string.IsNullOrEmpty(imageURL))
                        {
                            CreateTattooItem(document.Id, tattooName, imageURL);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error creating tattoo item: {ex.Message}");
                    }
                }
            });
    }
    
    private void CreateTattooItem(string tattooId, string tattooName, string imageURL)
    {
        if (tattooButtonPrefab == null || tattooContainer == null)
        {
            Debug.LogError("Missing prefab or container references");
            return;
        }
        
        // Create the tattoo item
        GameObject item = Instantiate(tattooButtonPrefab, tattooContainer);
        tattooItems.Add(item);
        
        // Find the Canvas and then get the RawImage inside it
        Canvas canvas = item.GetComponentInChildren<Canvas>();
        RawImage image = null;
        
        if (canvas != null)
        {
            // Get the RawImage from the Canvas children
            image = canvas.GetComponentInChildren<RawImage>();
        }
        else
        {
            // Fallback: try to find RawImage directly
            image = item.GetComponentInChildren<RawImage>();
        }
        
        // Load the image
        if (image != null && !string.IsNullOrEmpty(imageURL))
        {
            StartCoroutine(LoadImage(imageURL, image));
        }
        else
        {
            Debug.LogError($"Couldn't find RawImage component in tattoo button prefab");
        }
        
        // Set up the button click
        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => {
                // Store data for AR scene
                PlayerPrefs.SetString("SelectedTattooURL", imageURL);
                PlayerPrefs.SetString("SelectedTattooName", tattooName);
                PlayerPrefs.SetString("CurrentArtistId", currentArtistId);
                PlayerPrefs.Save();
                
                Debug.Log($"Selected tattoo: {tattooName}, opening AR scene");
                
                // Load AR scene
                SceneManager.LoadScene("imageTrackingTattoo");
            });
        }
        else
        {
            Debug.LogError($"Couldn't find Button component on tattoo prefab");
        }
    }
    
    private IEnumerator LoadImage(string imageURL, RawImage targetImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageURL))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                targetImage.texture = texture;
            }
            else
            {
                Debug.LogError($"Failed to load image: {request.error}");
            }
        }
    }
    
    private void ClearTattooItems()
    {
        foreach (GameObject item in tattooItems)
        {
            Destroy(item);
        }
        
        tattooItems.Clear();
        
        if (noTattoosText != null)
        {
            noTattoosText.gameObject.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (contactButton != null)
        {
            contactButton.onClick.RemoveAllListeners();
        }
    }
}