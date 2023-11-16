using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestGPS : MonoBehaviour
{
    public TMP_Text Status;

    public Vector2 MyCoordinates;
    public Vector2 TargetCoordinates = new Vector2(45.81511541925334f, 9.224227653511278f);



    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GPSPositionRequest());
    }

        

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GPSPositionRequest()
    {
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location not enabled on device or app does not have permission to access location");
            Status.text = "Location not enabled";
            yield return new WaitUntil( ()=> Input.location.isEnabledByUser);
        }

        // Starts the location service.
        Input.location.Start(5);

        // Waits until the location service initializes
        int maxWait = 20;
        Status.text = "Initializing...";

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Status.text = "Timed out";
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Status.text = "Unable to determine device location";
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            while (true)
            {
                // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
                Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
                MyCoordinates = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
                Vector3 myCoord3d = GPSEncoder.GPSToUCS(MyCoordinates);
                Vector3 targetCoord3d = GPSEncoder.GPSToUCS(TargetCoordinates);
                Status.text = "Location: Lat:" + Input.location.lastData.latitude + " Lon:" + Input.location.lastData.longitude + " Alt:" + Input.location.lastData.altitude + " Acc:" + Input.location.lastData.horizontalAccuracy + " Time:" + Input.location.lastData.timestamp + " Dist" + Vector3.Distance(myCoord3d,targetCoord3d);
                yield return new WaitForSeconds(1);
            }
            
        }


        yield return null;
    }


}
