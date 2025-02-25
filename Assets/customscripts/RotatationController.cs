using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class RotationController : MonoBehaviour
{
    public Slider rotationSlider;  // Slider to control rotation
    private GameObject crabInstance;  // The dynamically spawned crab_cube instance
    private ARTrackedImageManager arTrackedImageManager;  // AR Image Manager
    private ARTrackedImage arTrackedImage;  // ARTrackedImage component
    private Quaternion initialRotation;  // Initial rotation of the object
    private float currentRotationOffset = 0f;  // Current user-defined rotation offset
    private bool isAdjusting = false;  // Flag to track if the user is adjusting rotation

    void Start()
    {
        rotationSlider.onValueChanged.AddListener(UpdateRotation);
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void Update()
    {
        // Dynamically find the crab instance if it hasn't been found already
        if (crabInstance == null)
        {
            crabInstance = GameObject.FindWithTag("CrabCube");
            if (crabInstance != null)
            {
                // Store the initial rotation when the object is first found
                initialRotation = crabInstance.transform.rotation;

                // Find the ARTrackedImage component
                arTrackedImage = crabInstance.GetComponent<ARTrackedImage>();
            }
        }

        // Apply the current rotation offset to the crab instance
        if (crabInstance != null && arTrackedImage != null)
        {
            crabInstance.transform.rotation = initialRotation * Quaternion.Euler(0, currentRotationOffset, 0);
        }
    }

    void UpdateRotation(float value)
    {
        // Calculate the new rotation offset based on the slider's value
        currentRotationOffset = value;

        if (crabInstance != null && arTrackedImage != null)
        {
            // Update the crabInstance's rotation based on the user input
            crabInstance.transform.rotation = initialRotation * Quaternion.Euler(0, currentRotationOffset, 0);

            // Update the information of the tracked image
            // Display updated rotation or other related data
            var canvas = arTrackedImage.GetComponentInChildren<Canvas>();
            var text = canvas.GetComponentInChildren<Text>();
            text.text = $"Rotation: {currentRotationOffset}°";  // Show current rotation in the UI
        }
    }

    public void OnSliderReleased()
    {
        // Once the user finishes adjusting the slider, lock the new rotation into the AR Image Tracker
        if (crabInstance != null && arTrackedImage != null)
        {
            // Set AR tracker rotation to the adjusted value as the new "0" rotation
            arTrackedImage.transform.rotation = crabInstance.transform.rotation;
            isAdjusting = false; // Reset the adjusting flag
        }
    }
}
