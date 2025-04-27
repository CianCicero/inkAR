using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions; 
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;  
using System;
using UnityEngine.InputSystem; 

[System.Serializable]
public class TattooData
{
    public string tattooName;  
    public string imageURL;    
    public string artistName;
    public string artistId;  
    public string[] tags;
    
    // Helper method to check if the tattoo matches the search query
    public bool MatchesSearch(string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return true; // If no search query, all items match
            
        searchQuery = searchQuery.ToLower();
        
        // Check if any attribute contains the search query
        return tattooName.ToLower().Contains(searchQuery) ||
               artistName.ToLower().Contains(searchQuery) ||
               tags.Any(tag => tag.ToLower().Contains(searchQuery));
    }
}

public class DownloadAndApply : MonoBehaviour
{
    public GameObject crabCubePrefab;
    public Button buttonPrefab;              // Prefab with both Image and Text components
    public Transform buttonContainer;        
    public List<TattooData> tattoos;
    
    [Header("Pagination Settings")]
    public int itemsPerPage = 10;
    public Button nextPageButton;
    public Button prevPageButton;
    public TextMeshProUGUI pageIndicator;
    
    [Header("Search Settings")]
    public TMP_InputField searchInputField;
    public Button searchButton;
    public Button clearSearchButton;
    
    private Renderer cubeRenderer;
    private FirebaseFirestore db;
    private int currentPage = 0;
    private List<TattooData> filteredTattoos = new List<TattooData>();
    private string currentSearchQuery = "";
    private List<GameObject> instantiatedButtons = new List<GameObject>();

    void Start()
    {
        // Initialize Firebase
        InitializeFirebase();
        cubeRenderer = crabCubePrefab.GetComponent<Renderer>();
        
        // Set up pagination and search UI event listeners
        SetupUIControls();
    }
    
    void SetupUIControls()
    {
        // Set up search functionality
        if (searchButton != null)
            searchButton.onClick.AddListener(PerformSearch);
            
        if (searchInputField != null)
            searchInputField.onEndEdit.AddListener(OnSearchInputEnd);
            
        if (clearSearchButton != null)
            clearSearchButton.onClick.AddListener(ClearSearch);
            
        // Set up pagination functionality
        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);
            
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(PreviousPage);
    }
    
    private void OnSearchInputEnd(string value)
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame || 
             UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame))
        {
            PerformSearch();
        }
    }
    
    public void PerformSearch()
    {
        currentSearchQuery = searchInputField.text;
        currentPage = 0; // Reset to first page when searching
        FilterAndDisplayTattoos();
    }
    
    public void ClearSearch()
    {
        searchInputField.text = "";
        currentSearchQuery = "";
        currentPage = 0;
        FilterAndDisplayTattoos();
    }
    
    public void NextPage()
    {
        int totalPages = Mathf.CeilToInt((float)filteredTattoos.Count / itemsPerPage);
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            DisplayCurrentPage();
            UpdatePageControls();
        }
    }
    
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayCurrentPage();
            UpdatePageControls(); 
        }
    }

    // Initialize Firebase
    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance; 
            Debug.Log("Firebase Initialized");

            FetchTattooData();
        });
    }

    // Fetch tattoo data from Firestore
    void FetchTattooData()
    {
        if (db == null)
        {
            Debug.LogError("Firestore is not initialized!");
            return;
        }

        db.Collection("tattoometa").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to fetch tattoo data from Firestore");
            }
            else
            {
                QuerySnapshot snapshot = task.Result;
                List<DocumentSnapshot> documentList = snapshot.Documents.ToList();
                Debug.Log($"Found {documentList.Count} tattoos in Firestore");

                tattoos = new List<TattooData>();

                // Iterate through each document and add to tattoos list
                foreach (DocumentSnapshot document in documentList)
                {
                    try
                    {
                        var tattoo = new TattooData
                        {
                            tattooName = document.GetValue<string>("tattooName"),
                            imageURL = document.GetValue<string>("imageURL"),
                            artistName = document.GetValue<string>("artistName"),
                            artistId = document.GetValue<string>("artistId"), // Get the artist ID
                            tags = document.GetValue<List<string>>("tags")?.ToArray() ?? new string[0]
                        };

                        tattoos.Add(tattoo);
                        Debug.Log($"Tattoo name: {tattoo.tattooName}, Artist: {tattoo.artistName}, Artist ID: {tattoo.artistId}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error converting document to TattooData: {ex.Message}");
                    }
                }

                // After fetching data, filter and display tattoos with pagination
                FilterAndDisplayTattoos();
            }
        });
    }
    
    // Filter tattoos based on search query and update UI
    void FilterAndDisplayTattoos()
    {
        // Filter tattoos based on search query
        filteredTattoos = tattoos.Where(tattoo => tattoo.MatchesSearch(currentSearchQuery)).ToList();
        
        // Update page controls
        UpdatePageControls();
        
        // Display current page
        DisplayCurrentPage();
    }
    
    void UpdatePageControls()
    {
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)filteredTattoos.Count / itemsPerPage));
        
        // Update page indicator
        if (pageIndicator != null)
        {
            pageIndicator.text = $"{currentPage + 1} / {totalPages}";
            // Force refresh the UI
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageIndicator.rectTransform);
        }
            
        // Handle pagination buttons
        if (nextPageButton != null)
        {
            bool canGoNext = (currentPage < totalPages - 1);
            nextPageButton.interactable = canGoNext;
            
            if (nextPageButton.gameObject.activeSelf != canGoNext)
                nextPageButton.gameObject.SetActive(canGoNext);
        }
            
        if (prevPageButton != null)
        {
            bool canGoPrev = (currentPage > 0);
            prevPageButton.interactable = canGoPrev;
            
            if (prevPageButton.gameObject.activeSelf != canGoPrev)
                prevPageButton.gameObject.SetActive(canGoPrev);
        }
        
        // Force immediate UI update
        Canvas.ForceUpdateCanvases();
    }
    
    // Display the current page of tattoos
    void DisplayCurrentPage()
    {
        // Clear existing buttons
        ClearButtonContainer();
        
        // Calculate the current page items
        int startIndex = currentPage * itemsPerPage;
        int itemsToShow = Mathf.Min(itemsPerPage, filteredTattoos.Count - startIndex);
        
        // Create buttons for the current page
        for (int i = 0; i < itemsToShow; i++)
        {
            int tattooIndex = startIndex + i;
            if (tattooIndex < filteredTattoos.Count)
            {
                CreateTattooButton(filteredTattoos[tattooIndex]);
            }
        }
        
        // Update the page controls to reflect the current page
        UpdatePageControls();
        
        // Log for debugging
        Debug.Log($"Displaying page {currentPage + 1} with {itemsToShow} items starting at index {startIndex}");
    }
    
    // Clear all buttons from the container
    void ClearButtonContainer()
    {
        foreach (GameObject buttonObj in instantiatedButtons)
        {
            Destroy(buttonObj);
        }
        instantiatedButtons.Clear();
    }
    
    // Create a button for a specific tattoo
    void CreateTattooButton(TattooData tattoo)
    {
        Debug.Log($"Creating button for tattoo: {tattoo.tattooName} with image URL: {tattoo.imageURL}, Artist ID: {tattoo.artistId}");

        Button button = Instantiate(buttonPrefab, buttonContainer); // Instantiate the button
        instantiatedButtons.Add(button.gameObject);
        
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();   // Get the Text component
        RawImage buttonRawImage = button.GetComponentInChildren<RawImage>();  // Get the RawImage component

        if (buttonText != null)
        {
            buttonText.text = tattoo.tattooName;  // Set the text for the button
        }
        else
        {
            Debug.LogError("Button prefab does not contain a Text component.");
        }

        if (buttonRawImage != null)
        {
            StartCoroutine(LoadAndDisplayTattooImage(tattoo.imageURL, buttonRawImage));  // Load and apply image to button
        }
        else
        {
            Debug.LogError("Button prefab does not contain a RawImage component.");
        }

        // Pass the artist ID along with the image URL when the button is clicked
        button.onClick.AddListener(() => LoadTattooFromURL(tattoo.imageURL, tattoo.artistId, tattoo.tattooName));
    }

    // Load and display tattoo image on button's RawImage
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

    // Load tattoo texture to apply on the AR object and store artist ID for later use
    public void LoadTattooFromURL(string imageURL, string artistId, string tattooName)
    {
        // Store data in PlayerPrefs for use in the AR scene
        PlayerPrefs.SetString("CurrentArtistId", artistId);
        PlayerPrefs.SetString("SelectedTattooURL", imageURL);
        PlayerPrefs.SetString("SelectedTattooName", tattooName);
        PlayerPrefs.Save();
        
        Debug.Log($"Stored artist ID in PlayerPrefs: {artistId}");
        
        StartCoroutine(LoadAndApplyTexture(imageURL));
    }

    // Apply the texture to the AR object
    private IEnumerator LoadAndApplyTexture(string imageURL)
    {
        if (cubeRenderer == null)
        {
            Debug.LogError("Cube Renderer not assigned!");
            yield break;
        }

        Debug.Log($"Loading texture from URL: {imageURL}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            cubeRenderer.sharedMaterial.mainTexture = texture;

            Debug.Log("Texture applied to cube.");
            SceneManager.LoadScene("imageTrackingTattoo");
        }
    }
}