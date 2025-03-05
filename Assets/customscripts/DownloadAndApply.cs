using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class TattooData
{
    public string tattooName;  // Name of the tattoo
    public string imageUrl;    // URL of the tattoo image
}

public class DownloadAndApply : MonoBehaviour
{
    public GameObject crabCubePrefab;
    public Button buttonPrefab;              // Prefab with both Image and Text components
    public Transform buttonContainer;        // Where buttons will be instantiated
    public List<TattooData> tattoos;        // List of TattooData objects with names and image URLs

    private Renderer cubeRenderer;

    void Start()
    {
        cubeRenderer = crabCubePrefab.GetComponent<Renderer>();

        // Dynamically create buttons with URLs from TattooData
        foreach (TattooData tattoo in tattoos)
        {
            Button button = Instantiate(buttonPrefab, buttonContainer); // Instantiate the button
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();   // Get the Text component in the button
            RawImage buttonRawImage = button.GetComponentInChildren<RawImage>();  // Get the RawImage component in the button
            
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
                StartCoroutine(LoadAndDisplayTattooImage(tattoo.imageUrl, buttonRawImage));  // Load and apply image to the button
            }
            else
            {
                Debug.LogError("Button prefab does not contain a RawImage component.");
            }

            button.onClick.AddListener(() => LoadTattooFromURL(tattoo.imageUrl));  // Set URL for button click
        }
    }

    // Load tattoo texture from the URL and apply to RawImage
    private IEnumerator LoadAndDisplayTattooImage(string imageUrl, RawImage rawImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            rawImage.texture = texture;  // Set the image texture to the RawImage component
        }
    }

    public void LoadTattooFromURL(string imageUrl)
    {
        StartCoroutine(LoadAndApplyTexture(imageUrl));
    }

    private IEnumerator LoadAndApplyTexture(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            cubeRenderer.sharedMaterial.mainTexture = texture;

            Debug.Log("Texture applied, loading AR scene...");
            SceneManager.LoadScene("imageTrackingTattoo");
        }
    }
}
