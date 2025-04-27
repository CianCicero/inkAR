using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TattooLoader : MonoBehaviour
{
    public GameObject crabCubePrefab;  // Reference to the crab cube prefab
    public Texture2D[] tattooTextures; // Array of textures corresponding to tattoos

    void Start()
    {
        // Get the selected tattoo name from PlayerPrefs
        string selectedTattoo = PlayerPrefs.GetString("SelectedTattoo", "Tattoo1");  // Default to "Tattoo1" if no selection is made

        // Find the corresponding texture for the selected tattoo
        foreach (Texture2D texture in tattooTextures)
        {
            if (texture.name == selectedTattoo)
            {
                // Apply the texture to the prefab's material
                ApplyTextureToTattoo(texture);
                break;
            }
        }
    }

    // Method to apply the selected texture to the tattoo object's material
    void ApplyTextureToTattoo(Texture2D texture)
    {
        Renderer renderer = crabCubePrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Assuming the material already exists on the prefab, we apply the new texture to it
            renderer.material.mainTexture = texture;
        }
    }
}
