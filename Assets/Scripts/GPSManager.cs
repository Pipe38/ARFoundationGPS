using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Singleton;

    public float GPSDataUpdateFrequency = 0.1f;
    [Space(20)]

    //La mia posizione attuale, non è necessario vederla nell'inspector, tanto non è modificabile, ma è importante che sia accessibile da altri scripts.
    [HideInInspector]
    public Vector2 MyPosition;
    //La parte di definizione della coordinata di arrivo è stata spostata per supportare l'utilizzo tramite script esterno. Guarda la funzione GetCoordinatesInfo
    //public Vector2 TargetPosition = new Vector2(45.44599709547845f, 9.173861936741822f);

    [Space(20)]
    public Vector3 MyPosition3D;
    public Vector3 TargetPosition3D;
    public float CurrentHeadingToNorth;
    public float CurrentHeadingToTarget;

    [Space(20)]
    public float PositionalAccuracy = -1;
    public float CompassAccuracy = -1;


    public TMP_Text MyDebugText;

    private void OnEnable()
    {
        if (Singleton!= null)
        {
            Destroy(this);
        }
        Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GPSStartEnumerator());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public UnityGPSDatapack GetCoordinatesInfo(Vector2 worldPosition)
    {
        Vector3 targetPosition3D = GPSEncoder.GPSToUCS(worldPosition);
        float currentHeadingToTarget = (CurrentHeadingToNorth - CalculateBearing(MyPosition, worldPosition));
        return new UnityGPSDatapack(targetPosition3D, currentHeadingToTarget);
    }

    IEnumerator GPSStartEnumerator()
    {
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
        {
            MyDebugText.text = "Location Not Enabled";
            Debug.LogError("Location not enabled on device or app does not have permission to access location");
            yield return new WaitUntil(() => Input.location.isEnabledByUser);

        }

        // Starts the location service.
        Input.location.Start(5,1);
        Input.compass.enabled = true;

        // Waits until the location service initializes
        int maxWait = 20;
        MyDebugText.text = "Initializing...";
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            MyDebugText.text = "Timed out";
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            MyDebugText.text = "Unable to determine device location";
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            while (true)
            {
                // Updating my position
                MyPosition = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
                // Updating my heading through the compass
                CurrentHeadingToNorth = Input.compass.trueHeading;
                //CurrentHeadingToTarget = (CurrentHeadingToNorth - CalculateBearing(MyPosition, TargetPosition));
                // Convert GPS Position to 3D Unity position
                MyPosition3D = GPSEncoder.GPSToUCS(MyPosition);
                //TargetPosition3D = GPSEncoder.GPSToUCS(TargetPosition);
                PositionalAccuracy = Input.location.lastData.horizontalAccuracy;
                CompassAccuracy = Input.compass.headingAccuracy;
                // Updating debug data
                if (MyDebugText != null)
                {
                    MyDebugText.text = "Location: Lat:" + Input.location.lastData.latitude + " Lon:" + Input.location.lastData.longitude + " PosPrec:" + Input.location.lastData.horizontalAccuracy + " HeadPrec:" + Input.compass.headingAccuracy;
                    //MyDebugText.text = "Location: Lat:" + Input.location.lastData.latitude + " Lon:" + Input.location.lastData.longitude + " Alt:" + Input.location.lastData.altitude + " PosPrec:" + Input.location.lastData.horizontalAccuracy + " HeadPrec:" + Input.compass.headingAccuracy + " Time:" + Input.location.lastData.timestamp + " Dst:" + Vector3.Distance(MyPosition3D, TargetPosition3D) + " Az:" + CalculateBearing(MyPosition, TargetPosition) + " Az Diff:" + CurrentHeadingToTarget;
                }
                yield return new WaitForSeconds(GPSDataUpdateFrequency); 
            }
        }
    }

    public float CalculateBearing(Vector2 pos1, Vector2 pos2)
    {
        // Converti le latitudini e longitudini in radianti
        float lat1 = pos1.x * Mathf.Deg2Rad;
        float lat2 = pos2.x * Mathf.Deg2Rad;
        float lon1 = pos1.y * Mathf.Deg2Rad;
        float lon2 = pos2.y * Mathf.Deg2Rad;

        // Calcola la differenza di longitudine
        float dLon = lon2 - lon1;

        // Calcola l'azimut
        float y = Mathf.Sin(dLon) * Mathf.Cos(lat2);
        float x = Mathf.Cos(lat1) * Mathf.Sin(lat2) - Mathf.Sin(lat1) * Mathf.Cos(lat2) * Mathf.Cos(dLon);
        float bearingRadians = Mathf.Atan2(y, x);

        // Converti l'azimut in gradi
        float bearingDegrees = bearingRadians * Mathf.Rad2Deg;

        // Normalizza l'angolo
        bearingDegrees = (bearingDegrees + 360) % 360;

        return bearingDegrees;
    }

    //Essendo statica e pubblica, questa funzione può essere utilizzata ovunque senza fare riferimento a questa istanza (vedi ARGPSPlacer e Autoplacer)
    public static Vector2 MapsCoordinateToVector2(string inputString)
    {
        string[] vectorParts = inputString.Split(',');
        float latitudeFloat = float.Parse(vectorParts[0], CultureInfo.InvariantCulture);
        float longitudeFloat = float.Parse(vectorParts[1], CultureInfo.InvariantCulture);
        return new Vector2(latitudeFloat, longitudeFloat);
    }
}


[System.Serializable]
public class UnityGPSDatapack
{
    public Vector3 Target3DPosition;
    public float TargetHeading;

    public UnityGPSDatapack(Vector3 inTarget3DPosition, float inTarget3DHeading)
    {
        Target3DPosition = inTarget3DPosition;
        TargetHeading = inTarget3DHeading;
    }

    public UnityGPSDatapack()
    {
        Target3DPosition = Vector3.zero;
        TargetHeading = 0;
    }
}
