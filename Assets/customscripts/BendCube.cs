using UnityEngine;
using UnityEngine.UI;

public class BendCube : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] originalVertices;
    public Slider bendSlider;  // Reference to the Slider
    public float bendSpeed = 1.0f;  // Speed of the bending effect

    private float bendAmount = 1.0f;  // Default bend amount
    private GameObject crabCube;  // Reference to the object with the "CrabCube" tag

    void Start()
    {
        // Find the object with the "CrabCube" tag
        crabCube = GameObject.FindGameObjectWithTag("CrabCube");

        if (crabCube != null)
        {
            // Get the Mesh component of the CrabCube object
            mesh = crabCube.GetComponent<MeshFilter>().mesh;
            
            // Store the original vertices of the cube
            originalVertices = mesh.vertices;

            Debug.Log("Mesh found and vertices stored.");
        }
        else
        {
            Debug.LogError("No object with the CrabCube tag found.");
        }

        // Add listener to the slider to adjust bendAmount in real time
        if (bendSlider != null)
        {
            bendSlider.onValueChanged.AddListener(UpdateBendAmount);
        }
    }

    void Update()
    {
        if (mesh != null)
        {
            BendObject();
        }
    }

    // Update the bendAmount based on the slider value
    void UpdateBendAmount(float value)
    {
        bendAmount = value;  // The slider value will control the bend amount
        Debug.Log("Bend amount updated: " + bendAmount);  // Log the bend amount to ensure the slider works
    }

    void BendObject()
    {
        if (mesh == null) return;

        // Create a copy of the original vertices
        Vector3[] vertices = (Vector3[])originalVertices.Clone();

        for (int i = 0; i < vertices.Length; i++)
        {
            // Apply a bend effect based on the x position of the vertex
            // Bend the vertices along the Z-axis
            float bendFactor = Mathf.Sin(vertices[i].x * bendSpeed) * bendAmount;
            vertices[i].z += bendFactor; // Apply bend effect to Z-axis
        }

        // Apply the modified vertices back to the mesh
        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // Recalculate normals to update lighting

        // Debug the bending effect
        Debug.Log("Vertices modified, mesh updated.");
    }
}
