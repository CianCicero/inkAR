using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TattooLoader : MonoBehaviour
{
    public GameObject crabCubePrefab;  // Reference to the crab cube prefab
    public Material[] tattooMaterials; // Array of materials corresponding to tattoos

    void Start()
    {
        // Get the selected tattoo name from PlayerPrefs
        string selectedTattoo = PlayerPrefs.GetString("SelectedTattoo", "Tattoo1");  // Default to "Tattoo1" if no selection is made

        // Find the corresponding material for the selected tattoo
        foreach (Material material in tattooMaterials)
        {
            if (material.name == selectedTattoo)
            {
                // Update the material of the prefab before it is instantiated
                Renderer renderer = crabCubePrefab.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                }
                break;
            }
        }
    }
}
