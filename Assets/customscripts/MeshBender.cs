using UnityEngine;

public class MeshBender : MonoBehaviour
{
    public string objectTag = "CrabCube";  // Set this in Unity
    public float bendAmount = 0.5f;

    private GameObject targetObject;
    private Mesh originalMesh;
    private Mesh deformedMesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;

    void Start()
    {
        // Find the spawned object using the tag
        targetObject = GameObject.FindGameObjectWithTag(objectTag);
        if (targetObject == null)
        {
            Debug.LogError("Object with tag " + objectTag + " not found!");
            return;
        }

        // Get the mesh
        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found on " + targetObject.name);
            return;
        }

        originalMesh = meshFilter.mesh;
        deformedMesh = Instantiate(originalMesh);
        meshFilter.mesh = deformedMesh;

        // Store original vertices
        originalVertices = originalMesh.vertices;
        modifiedVertices = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        if (targetObject != null)
        {
            ApplyBend();
        }
    }

    void ApplyBend()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 v = originalVertices[i];

            // Apply a simple bending effect
            float offset = v.z * bendAmount;
            v.x += Mathf.Sin(offset) * bendAmount;

            modifiedVertices[i] = v;
        }

        // Apply new vertex positions
        deformedMesh.vertices = modifiedVertices;
        deformedMesh.RecalculateNormals();
        deformedMesh.RecalculateBounds();
    }
}
