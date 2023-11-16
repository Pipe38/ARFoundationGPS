using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class ARGPSPlacer : MonoBehaviour
{
    public string[] CoordinatesPoints;

    public ARPlaneManager MyPlaneManager;
    public Transform ARCamera;
    public GameObject ObjectToSpawn;

    GameObject instantiatedGameobject;
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlaceGPSObject()
    {
        // Otteniamo la posizione sulla y del piano più in basso

        float lowestPlaneY = float.MaxValue;

        foreach (var currentPlane in MyPlaneManager.trackables)
        {
            if (currentPlane.center.y < lowestPlaneY)
            {
                lowestPlaneY = currentPlane.center.y;

            }
            //currentPlane.gameObject.SetActive(false);
        }


        //Creiamo un oggetto vuoto nella nostra posizione AR
        GameObject pivotGameobject = new GameObject("PlacerPivot");
        //Diamo a questo oggetto la posizione della nostra camera AR
        pivotGameobject.transform.position = new Vector3(ARCamera.position.x, lowestPlaneY, ARCamera.position.z);
        //Proiettiamo la nostra rotazione sul pavimento 
        Vector3 projectedForward = Vector3.ProjectOnPlane(pivotGameobject.transform.forward, Vector3.up);
        Quaternion projectedRotation = Quaternion.LookRotation(projectedForward);
        pivotGameobject.transform.rotation = projectedRotation;

        // Ottengo la mia distanza dal target attuale, non mi serve altro attualmente
        float distanceTowardsTheTarget = Vector3.Distance(GPSManager.Singleton.MyPosition3D, GPSManager.Singleton.TargetPosition3D);
        print("SAMDEBUG SINGLE " + distanceTowardsTheTarget);
        instantiatedGameobject = Instantiate(ObjectToSpawn);
        instantiatedGameobject.transform.parent = pivotGameobject.transform;
        instantiatedGameobject.transform.rotation = Quaternion.identity;
        instantiatedGameobject.transform.localPosition = new Vector3(0, 0, distanceTowardsTheTarget);

        pivotGameobject.transform.Rotate(new Vector3(0, GPSManager.Singleton.CurrentHeadingToTarget,0));

        instantiatedGameobject.AddComponent<ARAnchor>();

        instantiatedGameobject.transform.parent = null;
        Destroy(pivotGameobject);
    }


    public void PlaceGPSObjects()
    {
        // Otteniamo la posizione sulla y del piano più in basso

        float lowestPlaneY = float.MaxValue;
        foreach (var currentPlane in MyPlaneManager.trackables)
        {
            if (currentPlane.center.y < lowestPlaneY)
            {
                lowestPlaneY = currentPlane.center.y;

            }
            //currentPlane.gameObject.SetActive(false);
        }


        //Creiamo un oggetto vuoto nella nostra posizione AR
        GameObject pivotGameobject = new GameObject("PlacerPivot");
        //Diamo a questo oggetto la posizione della nostra camera AR
        pivotGameobject.transform.position = new Vector3(ARCamera.position.x, lowestPlaneY, ARCamera.position.z);
        //Proiettiamo la nostra rotazione sul pavimento 
        Vector3 projectedForward = Vector3.ProjectOnPlane(pivotGameobject.transform.forward, Vector3.up);
        Quaternion projectedRotation = Quaternion.LookRotation(projectedForward);
        pivotGameobject.transform.rotation = projectedRotation;

        for (int i = 0; i < CoordinatesPoints.Length; i++)
        {
            Vector2 coordinatePoint = GPSManager.MapsCoordinateToVector2(CoordinatesPoints[i]);

            // Ottengo la mia distanza dal target attuale, non mi serve altro attualmente
            UnityGPSDatapack coordinateDatapack = GPSManager.Singleton.GetCoordinatesInfo(coordinatePoint);

            float distanceTowardsTheTarget = Vector3.Distance(GPSManager.Singleton.MyPosition3D, coordinateDatapack.Target3DPosition);
            print("SAMDEBUG LOOP " + distanceTowardsTheTarget);
            instantiatedGameobject = Instantiate(ObjectToSpawn);
            instantiatedGameobject.transform.parent = pivotGameobject.transform;
            instantiatedGameobject.transform.rotation = Quaternion.identity;
            instantiatedGameobject.transform.localPosition = new Vector3(0, 0, distanceTowardsTheTarget);

            pivotGameobject.transform.Rotate(new Vector3(0, coordinateDatapack.TargetHeading, 0));

            instantiatedGameobject.AddComponent<ARAnchor>();

            instantiatedGameobject.transform.parent = null;
        }

        
        Destroy(pivotGameobject);
    }
}
