// See https://youtu.be/gpaq5bAjya8  for accompanying tutorial and usage!

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTracker : MonoBehaviour
{
    // Reference to AR tracked image manager component
    private ARTrackedImageManager _trackedImagesManager;

    // List of prefabs to instantiate - these should be named the same
    // as their corresponding 2D images in the reference image library 
    public GameObject[] ArPrefabs;

    public Vector3 offset; // Offset from the tracked image's position

    public Transform cameraTransform; // Reference to the camera's transform
    public float distanceFromCamera = 1.0f; // Distance from the camera

    public Vector3 positionOffset = new Vector3(0, 0, 0); // Offset from the image target's position
    public Vector3 rotationOffset = new Vector3(0, 0, 0); // Offset for rotation adjustment


    // Keep dictionary array of created prefabs
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Cache a reference to the Tracked Image Manager component
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        // Attach event handler when tracked images change
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        // Remove event handler
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // Event Handler
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {

        // Loop through all new tracked images that have been detected
        foreach (var trackedImage in eventArgs.added)
        {
            // Get the name of the reference image
            var imageName = trackedImage.referenceImage.name;
            // Now loop over the array of prefabs

            Vector3 imagePosition = trackedImage.transform.position + positionOffset;
            Quaternion imageRotation = trackedImage.transform.rotation * Quaternion.Euler(rotationOffset); ;

            // Get the size of the tracked image
            Vector2 imageSize = trackedImage.size;

            // Calculate the center of the tracked image
            Vector3 centerPosition = imagePosition + trackedImage.transform.forward * imageSize.y * 0.5f;

            // Get the position of the image
            // Adjust the y-coordinate of the prefab's position to match the y-coordinate of the image
            //Vector3 centerPosition = new Vector3(transform.position.x, imagePosition.y, transform.position.z);

            foreach (var curPrefab in ArPrefabs)
            {
                // Check whether this prefab matches the tracked image name, and that
                // the prefab hasn't already been created
                if (string.Compare(curPrefab.name, imageName, StringComparison.OrdinalIgnoreCase) == 0
                    && !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    // Instantiate the prefab, parenting it to the ARTrackedImage
                    var newPrefab = Instantiate(curPrefab, centerPosition, imageRotation);
                    // Add the created prefab to our array

                    // Ensure the prefab faces the camera
                    transform.LookAt(cameraTransform);

                    _instantiatedPrefabs[imageName] = newPrefab;
                }

                else
                {
                    // Update the position of the instantiated prefab to match the center of the tracked image
                    curPrefab.transform.position = centerPosition;
                    curPrefab.transform.rotation = imageRotation;

                    // Ensure the prefab faces the camera
                    transform.LookAt(cameraTransform);

                }


            }
        }

        // For all prefabs that have been created so far, set them active or not depending
        // on whether their corresponding image is currently being tracked
        foreach (var trackedImage in eventArgs.updated)
        {
            _instantiatedPrefabs[trackedImage.referenceImage.name]
                .SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }

        // If the AR subsystem has given up looking for a tracked image
        foreach (var trackedImage in eventArgs.removed)
        {
            // Destroy its prefab
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);
            // Also remove the instance from our array
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            // Or, simply set the prefab instance to inactive
            //_instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(false);
        }
    }
}