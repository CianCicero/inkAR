using UnityEngine;
using UnityEngine.UI; // Import the UI namespace for working with sliders

public class MeshBender : MonoBehaviour
{
    private GameObject crabCubePrefab; // Reference to the CrabCube prefab
    private Mesh deformingMesh;
    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;

    // Bend multiplier to control the intensity of the deformation
    public float bendMultiplier = 0.5f;

    // Reference to the slider for controlling the bending amount
    public Slider bendSlider;

    void Start()
    {
        // Find the CrabCube object in the scene by tag
        crabCubePrefab = GameObject.FindGameObjectWithTag("CrabCube");
        if (crabCubePrefab != null)
        {
            // Retrieve the MeshFilter component from the CrabCube prefab and access its mesh
            MeshFilter meshFilter = crabCubePrefab.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                deformingMesh = meshFilter.mesh;
                originalVertices = deformingMesh.vertices;
                displacedVertices = new Vector3[originalVertices.Length];

                // Initialize displaced vertices to the original ones
                for (int i = 0; i < originalVertices.Length; i++)
                {
                    displacedVertices[i] = originalVertices[i];
                }
            }
            else
            {
                Debug.LogError("No MeshFilter found on the CrabCube prefab.");
            }
        }
        else
        {
            Debug.LogError("No object with the tag 'CrabCube' found in the scene.");
        }

        // Make sure the slider is assigned and add a listener to update the deformation
        if (bendSlider != null)
        {
            bendSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        else
        {
            Debug.LogError("BendSlider is not assigned.");
        }
    }

    // Method to apply deformation logic (e.g., bending) based on slider input
    public void ApplyDeformation(float deformationAmount)
    {
        // Apply deformation logic here based on the slider value (0-1)
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i] + new Vector3(0, Mathf.Sin(i * deformationAmount) * bendMultiplier, 0);
        }

        // Update the mesh with the new deformed vertices
        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    // Called when the slider value changes
    private void OnSliderValueChanged(float value)
    {
        ApplyDeformation(value); // Apply the deformation based on slider value
    }
}
