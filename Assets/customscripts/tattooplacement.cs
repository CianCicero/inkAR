using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class PlaceObjectOnTap : MonoBehaviour
{
    public GameObject tattooPrefab; // The tattoo (or image) to place
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();  // For storing raycast hits

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // Check if the screen is tapped
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            // Perform raycasting to detect where the touch is
            if (raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                // Get the point where the touch hit the detected plane
                Pose hitPose = hits[0].pose;

                // Place the tattoo prefab at the detected position
                PlaceTattoo(hitPose);
            }
        }
    }

    void PlaceTattoo(Pose pose)
    {
        // Instantiate the tattoo prefab at the position where the raycast hit
        Instantiate(tattooPrefab, pose.position, pose.rotation);
    }
}
