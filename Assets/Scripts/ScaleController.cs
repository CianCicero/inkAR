using UnityEngine;
using UnityEngine.UI;

public class ScaleController : MonoBehaviour
{
    public Slider scaleSlider; 
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
        Vector3 currentScale = crabInstance.transform.localScale;
        crabInstance.transform.localScale = new Vector3(value, currentScale.y, value);
    }
}

}
