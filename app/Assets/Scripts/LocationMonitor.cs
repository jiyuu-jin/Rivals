using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class LocationMonitor : MonoBehaviour
{
    private IEnumerator coroutine;

    ObjectSpawner m_ObjectSpawner;

    void Start()
    {
        coroutine = Routine();
        StartCoroutine(coroutine);

        if (m_ObjectSpawner == null)
#if UNITY_2023_1_OR_NEWER
            m_ObjectSpawner = FindAnyObjectByType<ObjectSpawner>();
#else
            m_ObjectSpawner = FindObjectOfType<ObjectSpawner>();
#endif

    }

    IEnumerator Routine()
    {
        Debug.Log("LocationMonitor: Starting");
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
            Debug.Log("Location not enabled on device or app does not have permission to access location");

        // Starts the location service.

        float desiredAccuracyInMeters = 10f;
        float updateDistanceInMeters = 10f;

        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);
        Debug.Log("Location service started");

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("status: " + Input.location.status);
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            List<int> knownTraps = new List<int>();
            while (true)
            {
                // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
                Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

                string json_body = "{ \"owner_username\": \"player1\", \"latitude\": " + Input.location.lastData.latitude + ", \"longitude\": " + Input.location.lastData.longitude + " }";
                using (UnityWebRequest www = UnityWebRequest.Post("http://10.1.9.21:3000/api/movement", json_body, "application/json"))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        Traps traps = JsonUtility.FromJson<Traps>(www.downloadHandler.text);
                        foreach (Trap trap in traps.traps)
                        {
                            if (!knownTraps.Contains(trap.id)) {
                                knownTraps.Add(trap.id);
                                Debug.Log("New Trap: " + trap.id + " " + trap.latitude + " " + trap.longitude);

                                // Instead of using AR raycast hit, spawn in front of player
                                Vector3 spawnPosition = GetGroundPositionInFrontOfPlayer();
                                if (spawnPosition != Vector3.zero) // Check if we found a valid ground position
                                {
                                    m_ObjectSpawner.TrySpawnObject(spawnPosition, Vector3.up, true);
                                }
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        // Stops the location service if there is no need to query location updates continuously.
        // Input.location.Stop();
    }

    // Add this method to calculate spawn position in front of player
    private Vector3 GetGroundPositionInFrontOfPlayer()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null) return Vector3.zero;

        // Get the forward direction from the camera
        Vector3 forward = playerCamera.transform.forward;

        // Project forward direction onto the horizontal plane (ignore Y component)
        forward.y = 0;
        forward.Normalize();

        // Set spawn distance (how far in front of player)
        float spawnDistance = 2.0f; // Adjust this value as needed

        // Calculate the target position in front of player
        Vector3 targetPosition = playerCamera.transform.position + (forward * spawnDistance);

        // Raycast downward from above the target position to find ground
        Vector3 rayStart = targetPosition + Vector3.up * 10f; // Start 10 units above
        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f, LayerMask.GetMask("Default")))
        {
            return hit.point;
        }

        // Fallback: if no ground found, use the target position at ground level
        return new Vector3(targetPosition.x, 0, targetPosition.z);
    }
}

[Serializable]
public class Traps
{
    [SerializeField] public List<Trap> traps;
}

[Serializable]
public class Trap
{
    [SerializeField] public int id;
    [SerializeField] public float latitude;
    [SerializeField] public float longitude;
}
