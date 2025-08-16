using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;
using UnityEngine.Networking;

public class LocationMonitor : MonoBehaviour
{
    private IEnumerator coroutine;

    void Start()
    {
        coroutine = Routine();
        StartCoroutine(coroutine);
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
            while (true)
            {
                // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
                Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

                string json_body = "{ \"owner_username\": \"player1\", \"latitude\": " + Input.location.lastData.latitude + ", \"longitude\": " + Input.location.lastData.longitude + " }";
                using (UnityWebRequest www = UnityWebRequest.Post("http://10.178.193.68:3000/api/movement", json_body, "application/json"))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(www.error);
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        // Stops the location service if there is no need to query location updates continuously.
        // Input.location.Stop();
    }
}
