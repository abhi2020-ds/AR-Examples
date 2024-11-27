using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTracker : MonoBehaviour
{
    // Reference to AR tracked image manager component
    private ARTrackedImageManager _trackedImagesManager;

    // List of prefabs to instantiate - these should be named the same
    // as their corresponding 2D images in the reference image library 
    public GameObject[] ArPrefabs;

    // Keep dictionary array of created prefabs
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new Dictionary<string, GameObject>();

    // Singleton pattern for managing ARSession persistence across scenes
    private static bool arSessionInitialized = false;
    private ARSession _arSession;

    void Awake()
    {
        // Cache a reference to the Tracked Image Manager component
        _arSession = FindObjectOfType<ARSession>();
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();

        if (_arSession != null && !arSessionInitialized)
        {
            // Ensure AR session persists across scene transitions
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
            arSessionInitialized = true;  // Set flag to avoid multiple calls to DontDestroyOnLoad
        }
        else if (_arSession == null)
        {
            Debug.LogError("ARSession not found in the scene.");
        }
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
            foreach (var curPrefab in ArPrefabs)
            {
                // Check whether this prefab matches the tracked image name, and that
                // the prefab hasn't already been created
                if (string.Compare(curPrefab.name, imageName, StringComparison.OrdinalIgnoreCase) == 0
                    && !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    // Instantiate the prefab, parenting it to the ARTrackedImage
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    // Add the created prefab to our array
                    _instantiatedPrefabs[imageName] = newPrefab;
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

    // This function will be called whenever a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ensure that the AR session is still running after the scene is loaded
        if (_arSession != null && !_arSession.enabled)
        {
            _arSession.enabled = true;
        }

        // Optionally, re-initialize or re-enable any AR components if needed
        // For example, you can also check if AR session was initialized properly.
        // This is a good place to ensure AR functionality is properly set up after scene load.
    }

    void Start()
    {
        // Subscribe to the scene loaded event to re-initialize AR session if needed
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnApplicationQuit()
    {
        // Unsubscribe from the scene change event to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}

