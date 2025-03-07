using Firebase;
using Firebase.Storage;
using Firebase.Firestore;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;

public class Upload : MonoBehaviour
{
    public TMP_InputField tattooNameInput;
    public TMP_InputField tagsInput;
    public TMP_InputField artistInput;
    public Button uploadButton;

    private FirebaseStorage storage;
    private FirebaseFirestore firestore;
    private StorageReference storageRef;

    void Start()
    {
        // Initialize Firebase
        InitializeFirebase();

        // Set up button listener
        uploadButton.onClick.AddListener(OnUploadButtonClicked);
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
                return;
            }

            FirebaseApp app = FirebaseApp.DefaultInstance;
            storage = FirebaseStorage.GetInstance(app);
            firestore = FirebaseFirestore.GetInstance(app);
            storageRef = storage.GetReference("tattoos");

            Debug.Log("Firebase initialized successfully.");
        });
    }

    private void OnUploadButtonClicked()
    {
        string tattooName = tattooNameInput.text;
        string artistName = artistInput.text;
        string tags = tagsInput.text;

        if (string.IsNullOrEmpty(tattooName) || string.IsNullOrEmpty(artistName))
        {
            Debug.LogError("Please fill all fields.");
            return;
        }

        // Start the upload process
        UploadTattooAsync(tattooName, artistName, tags).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Upload failed: " + task.Exception);
            }
            else
            {
                Debug.Log("Upload completed successfully.");
            }
        });
    }

    private async Task UploadTattooAsync(string tattooName, string artistName, string tags)
    {
        // Load the predefined texture
        Texture2D texture = LoadPredefinedTexture();
        if (texture == null)
        {
            Debug.LogError("Failed to load predefined texture.");
            return;
        }

        // Convert texture to PNG bytes
        byte[] imageBytes = texture.EncodeToPNG();
        if (imageBytes == null || imageBytes.Length == 0)
        {
            Debug.LogError("Failed to encode texture to PNG.");
            return;
        }

        // Upload image to Firebase Storage
        string fileName = $"{tattooName}_{artistName}.png"; // Combine tattooName and artistName for the filename
        StorageReference imageRef = storageRef.Child(fileName);

        // Set metadata for the file (MIME)
        var metadata = new MetadataChange
        {
            ContentType = "image/png"
        };

        try
        {
            // Upload the image with metadata
            await imageRef.PutBytesAsync(imageBytes, metadata);
            Debug.Log("Image uploaded to Firebase Storage.");

            // Get the download URL
            string imageURL = (await imageRef.GetDownloadUrlAsync()).ToString();
            Debug.Log("Download URL: " + imageURL);

            // Upload metadata to Firestore
            await UploadMetadataAsync(tattooName, artistName, tags, imageURL);
            Debug.Log("Metadata uploaded to Firestore.");

            // Reset UI
            ResetUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error during upload: " + ex);
        }
    }

    private async Task UploadMetadataAsync(string tattooName, string artistName, string tags, string imageURL)
    {
        // Create metadata dictionary
        Dictionary<string, object> tattooMetadata = new Dictionary<string, object>
        {
            { "tattooName", tattooName },
            { "artistName", artistName },
            { "tags", tags.Split(',') },
            { "imageURL", imageURL }
        };

        // Add metadata to Firestore in the tattoometa collection
        DocumentReference docRef = firestore.Collection("tattoometa").Document();
        await docRef.SetAsync(tattooMetadata);
    }

    private Texture2D LoadPredefinedTexture()
    {
        // Load the texture from the Resources folder
        Texture2D texture = Resources.Load<Texture2D>("celt");

        if (texture == null)
        {
            Debug.LogError("Predefined texture not found. Ensure the texture is in the Resources folder and named correctly.");
        }

        return texture;
    }

    private void ResetUI()
    {
        tattooNameInput.text = "";
        artistInput.text = "";
        tagsInput.text = "";
    }
}