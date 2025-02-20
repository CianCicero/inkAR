using UnityEngine;
using UnityEngine.UI;

public class ScaleController : MonoBehaviour
{
    public Slider scaleSlider;  // Assign in Inspector
    private GameObject crabInstance; // The dynamically spawned crab_cube instance

    void Start()
    {
        scaleSlider.onValueChanged.AddListener(UpdateScale);
    }

    void Update()
    {
        if (crabInstance == null) 
        {
            // Find the spawned object dynamically
            crabInstance = GameObject.FindWithTag("CrabCube"); 
        }
    }

    void UpdateScale(float value)
    {
        if (crabInstance != null)
        {
            crabInstance.transform.localScale = Vector3.one * value;
        }
    }
}
