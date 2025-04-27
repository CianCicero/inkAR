using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections;
using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using Firebase;


public class UploadUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RawImage previewImage;
    [SerializeField] private Button selectImageButton;
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField tagsField;
    [SerializeField] private TMP_InputField descriptionField;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button cancelButton;
    
    // References to managers
    private AuthManager authManager;
    private FirebaseAuthManager firebaseAuthManager;
    
    // Firebase references
    private FirebaseStorage storage;
    private FirebaseFirestore firestore;
    private StorageReference storageRef;
    
    // Tattoo image data
    private Texture2D tattooTexture;
    
    private void Awake()
    {
        // Get manager references
        authManager = FindObjectOfType<AuthManager>();
        firebaseAuthManager = FindObjectOfType<FirebaseAuthManager>();
    }
    
    private void Start()
    {
        InitializeFirebase();
        
        selectImageButton.onClick.AddListener(OnSelectImageButtonClick);
        uploadButton.onClick.AddListener(OnUploadButtonClick);
        cancelButton.onClick.AddListener(OnBackButtonClick);
        
        ResetForm();
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
    
    private void ResetForm()
    {
        previewImage.texture = null;
        nameField.text = "";
        tagsField.text = "";
        descriptionField.text = "";
        statusText.text = "";
        
        // Reset progress
        progressSlider.value = 0;
        
        // Enable buttons
        uploadButton.interactable = true;
        selectImageButton.interactable = true;
    }
    
    private void OnSelectImageButtonClick()
    {
        #if UNITY_EDITOR
        // Mock tattoo selection for testing in editor
        StartCoroutine(MockSelectImage());
        #else
        // Use native file picker on mobile
        NativeGallery.GetImageFromGallery((path) => {
            if (!string.IsNullOrEmpty(path))
            {
                LoadImageFromPath(path);
            }
        }, "Select Tattoo Image");
        #endif
    }
    
    private IEnumerator MockSelectImage()
    {
        // Create a test texture
        int size = 512;
        tattooTexture = new Texture2D(size, size);
        
        // Generate a simple pattern
        Color[] colors = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (float)x / size;
                float normalizedY = (float)y / size;
                
                float dist = Vector2.Distance(new Vector2(normalizedX, normalizedY), new Vector2(0.5f, 0.5f));
                
                if (dist < 0.4f)
                {
                    colors[y * size + x] = new Color(0.1f, 0.1f, 0.1f, 1);
                }
                else if (dist < 0.42f)
                {
                    colors[y * size + x] = new Color(0.05f, 0.05f, 0.05f, 1);
                }
                else
                {
                    // Transparent background
                    colors[y * size + x] = new Color(0, 0, 0, 0);
                }
            }
        }
        
        tattooTexture.SetPixels(colors);
        tattooTexture.Apply();
        
        // Display the texture
        previewImage.texture = tattooTexture;
        
        if (string.IsNullOrEmpty(nameField.text))
        {
            nameField.text = "Sample Tattoo";
        }
        
        yield return null;
    }
    
    private void LoadImageFromPath(string path)
    {
        // Load image from file
        byte[] fileData = File.ReadAllBytes(path);
        
        // Create texture from data
        tattooTexture = new Texture2D(2, 2);
        if (tattooTexture.LoadImage(fileData))
        {
            // Display the texture
            previewImage.texture = tattooTexture;
            
            // Suggest a name based on filename
            if (string.IsNullOrEmpty(nameField.text))
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                nameField.text = fileName;
            }
        }
        else
        {
            statusText.text = "Failed to load image";
        }
    }
    
    private void OnUploadButtonClick()
    {
        // Validate input
        if (previewImage.texture == null)
        {
            statusText.text = "Please select an image first";
            return;
        }
        
        if (string.IsNullOrEmpty(nameField.text))
        {
            statusText.text = "Please enter a name for the tattoo";
            return;
        }
        
        string artistName = firebaseAuthManager.DisplayName;
        if (string.IsNullOrEmpty(artistName))
        {
            statusText.text = "Artist name not available";
            return;
        }
        
        // Disable buttons during upload
        uploadButton.interactable = false;
        selectImageButton.interactable = false;
        
        // Start the upload process
        UploadTattooAsync(nameField.text, artistName, tagsField.text).ContinueWith(task =>
        {
            // Return to main thread for UI updates
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (task.IsFaulted)
                {
                    statusText.text = "Upload failed: " + task.Exception.Message;
                    Debug.LogError("Upload failed: " + task.Exception);
                    
                    // Re-enable buttons
                    uploadButton.interactable = true;
                    selectImageButton.interactable = true;
                }
                else
                {
                    // Show complete
                    progressSlider.value = 1.0f;
                    statusText.text = "Upload completed successfully!";

                    ResetForm();
                    
                    // Go back to profile after delay
                    StartCoroutine(ReturnToProfile(1.5f));
                }
            });
        });
    }
    
    private IEnumerator ReturnToProfile(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnBackButtonClick();
    }
    
    private async Task UploadTattooAsync(string tattooName, string artistName, string tags)
    {
        try
        {
            // Simulate progress
            UpdateProgress(0.1f);
            
            // Convert texture to PNG bytes
            byte[] imageBytes = tattooTexture.EncodeToPNG();
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new Exception("Failed to encode texture to PNG.");
            }
            
            UpdateProgress(0.3f);
            
            string fileName = $"{tattooName}_{artistName}_{DateTime.UtcNow.Ticks}.png";
            StorageReference imageRef = storageRef.Child(fileName);
            
            var metadata = new MetadataChange
            {
                ContentType = "image/png"
            };
            
            UpdateProgress(0.5f);
            
            // Upload the image with metadata
            await imageRef.PutBytesAsync(imageBytes, metadata);
            Debug.Log("Image uploaded to Firebase Storage.");
            
            UpdateProgress(0.7f);
            
            // Get the download URL
            string imageURL = (await imageRef.GetDownloadUrlAsync()).ToString();
            Debug.Log("Download URL: " + imageURL);
            
            UpdateProgress(0.8f);
            
            // Upload metadata to Firestore
            await UploadMetadataAsync(tattooName, artistName, tags, descriptionField.text, imageURL);
            Debug.Log("Metadata uploaded to Firestore.");
            
            UpdateProgress(1.0f);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error during upload: " + ex);
            throw;
        }
    }
    
    private void UpdateProgress(float progress)
    {
        // Return to main thread to update UI
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            progressSlider.value = progress;
        });
    }
    
    private async Task UploadMetadataAsync(string tattooName, string artistName, string tags, string description, string imageURL)
    {
    // Process tags
    string[] tagArray = !string.IsNullOrEmpty(tags) ? tags.Split(',') : new string[0];
    
    // Trim whitespace from tags
    for (int i = 0; i < tagArray.Length; i++)
    {
        tagArray[i] = tagArray[i].Trim();
    }
    
    // Get current user info
    string artistId = firebaseAuthManager.UserId;
    
    Dictionary<string, object> tattooMetadata = new Dictionary<string, object>
    {
        { "tattooName", tattooName },
        { "artistName", artistName },
        { "artistId", artistId },
        { "tags", tagArray },
        { "imageURL", imageURL }
    };
    
    // Add to Firestore
    DocumentReference docRef = firestore.Collection("tattoometa").Document();
    await docRef.SetAsync(tattooMetadata);
    
    Debug.Log($"Tattoo metadata uploaded with ID: {docRef.Id}");
    }
    
    private void OnBackButtonClick()
    {
        // Return to artist profile panel
        authManager.ShowArtistProfilePanel();
    }
}