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

[System.Serializable]
public class TattooData
{
    public string tattooName;  
    public string imageURL;    
    public string artistName;  
    public string[] tags;     
}

public class DownloadAndApply : MonoBehaviour
{
    public GameObject crabCubePrefab;
    public Button buttonPrefab;              // Prefab with both Image and Text components
    public Transform buttonContainer;        
    public List<TattooData> tattoos;        

    private Renderer cubeRenderer;
    private FirebaseFirestore db;

    void Start()
    {
        // Initialize Firebase
        InitializeFirebase();
        cubeRenderer = crabCubePrefab.GetComponent<Renderer>(); 
    }

    // Initialize Firebase
    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance; // Ensure using the default Firestore instance
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
                            tags = document.GetValue<List<string>>("tags").ToArray()
                        };

                        tattoos.Add(tattoo);
                        Debug.Log($"Tattoo name: {tattoo.tattooName}, Image URL: {tattoo.imageURL}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error converting document to TattooData: {ex.Message}");
                    }
                }

                // After fetching data, create the buttons
                CreateButtons();
            }
        });
    }

    // Dynamically create buttons with URLs from TattooData
    void CreateButtons()
    {
        foreach (TattooData tattoo in tattoos)
        {
            Debug.Log($"Creating button for tattoo: {tattoo.tattooName} with image URL: {tattoo.imageURL}");

            Button button = Instantiate(buttonPrefab, buttonContainer); // Instantiate the button
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

            button.onClick.AddListener(() => LoadTattooFromURL(tattoo.imageURL));  // Set URL for button click
        }
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

    // Load tattoo texture to apply on the AR object
    public void LoadTattooFromURL(string imageURL)
    {
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
