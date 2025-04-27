using UnityEngine;
using UnityEngine.UI;

public class TattooColorToggle : MonoBehaviour
{
    public Button colorToggleButton;  // Button to toggle the color
    public GameObject tattooCube;     // The cube with the tattoo

    private Material tattooMaterial;  // Material of the cube
    private bool isBlack = true;      // Track if the tattoo is currently black or white

    void Start()
    {
        // Ensure that the tattooCube is assigned in the inspector
        if (tattooCube != null)
        {
            // Get the material of the tattoo cube's renderer
            tattooMaterial = tattooCube.GetComponent<Renderer>().material;
        }
        else
        {
            Debug.LogError("Tattoo Cube is not assigned!");
        }

        // Set up the button listener to toggle the color
        if (colorToggleButton != null)
        {
            colorToggleButton.onClick.AddListener(ToggleColor);
        }
        else
        {
            Debug.LogError("Color Toggle Button is not assigned!");
        }
    }

    // Function to toggle the color
    void ToggleColor()
    {
        if (tattooMaterial != null)
        {
            // Change color between black and white based on the current state
            if (isBlack)
            {
                tattooMaterial.SetColor("_Color", Color.white); // Change to white
            }
            else
            {
                tattooMaterial.SetColor("_Color", Color.black); // Change to black
            }

            // Toggle the state
            isBlack = !isBlack;
        }
        else
        {
            Debug.LogError("Tattoo Material is missing!");
        }
    }
}
