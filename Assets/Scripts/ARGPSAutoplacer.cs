using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARGPSAutoplacer : MonoBehaviour
{
    public ARGPSGameObjectsDatapack[] ObjectsToSpawn;
    [Tooltip("Distance from object coordinates, where the object spawn is triggered.")]
    public float SpawnDistance = 6;
    [Tooltip("Distance from object coordinates, where the object destroy is triggered.")]
    public float DestroyDistance = 20;

    public ARPlaneManager MyPlaneManager;
    public Transform ARCamera;
    public Material[] PlanesMaterials;

    List<ARGPSGameObjectsDatapack> spawnedObjects = new List<ARGPSGameObjectsDatapack>();
    List<int> currentlySpawnedObjectInidices = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Controllo tutti gli oggetti DA SPAWNARE, se mi trovo nel range di qualcuno, lo istanzio... 
        for (int i = 0; i < ObjectsToSpawn.Length; i++)
        {
            //Iniziamo controllando se non ho già creato questo oggetto, nel momento in cui lo genero, il suo indice finisce in questa lista, finchè non lo distruggo...
            if (!currentlySpawnedObjectInidices.Contains(i))
            {
                // Genero la posizione 3D
                UnityGPSDatapack coordinateDatapack = GPSManager.Singleton.GetCoordinatesInfo(ObjectsToSpawn[i].GetGPSCoordinatesAsVector2());
                // Ottengo la mia distanza, come al solito
                float currentObjectDistance = Vector3.Distance(GPSManager.Singleton.MyPosition3D, coordinateDatapack.Target3DPosition);
                // Se è abbastanza vicino, genero un istanza
                if (currentObjectDistance < SpawnDistance)
                {
                    //La vecchia funzione di generazione di oggetti tramite posizione gps, ho aggiunto un int per tenere traccia del tipo di oggetto
                    PlaceGPSObject(ObjectsToSpawn[i],i);
                    //Aggiungo l'indice di questo oggetto che ho creato ad una lista, in questo modo evito di crearne altri finche non lo distruggo
                    currentlySpawnedObjectInidices.Add(i);
                }
            }     
        }

        // Controllo tutti gli oggetti PRECEDENTEMENTE SPAWNATI, se qualcuno di essi esce dal range, lo distruggo.
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            // Genero la posizione 3D
            UnityGPSDatapack coordinateDatapack = GPSManager.Singleton.GetCoordinatesInfo(spawnedObjects[i].GetGPSCoordinatesAsVector2());
            // Ottengo la mia distanza, come al solito
            float currentObjectDistance = Vector3.Distance(GPSManager.Singleton.MyPosition3D, coordinateDatapack.Target3DPosition);
            // Se è troppo lontano lo distruggo
            if (currentObjectDistance > DestroyDistance)
            {
                // Distruggo la sua istanza gameobject in scena
                Destroy(spawnedObjects[i].GPSGameobject);
                // Lo tolgo dalla lista degli indici di oggetti creati in scena, in modo di poterlo ricreare in futuro
                currentlySpawnedObjectInidices.Remove(spawnedObjects[i].ObjectTypeIndex);
                // Elimino il suo slot dalla lista degli oggetti spawnati
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    void PlaceGPSObject(ARGPSGameObjectsDatapack objectToPlace, int objectTypeIndex)
    {
        // Otteniamo la posizione sulla y del piano più in basso

        float lowestPlaneY = float.MaxValue;

        foreach (var currentPlane in MyPlaneManager.trackables)
        {
            if (currentPlane.center.y < lowestPlaneY)
            {
                lowestPlaneY = currentPlane.center.y;

            }
            // Nascondo tutti i piani, tramite materiale in modo da gestire i presenti e i futuri senza interrompere la generazione.
            for (int i = 0; i < PlanesMaterials.Length; i++)
            {
                PlanesMaterials[i].color = new Color(0, 0, 0, 0);
            }
        }


        // Creiamo un oggetto vuoto nella nostra posizione AR
        GameObject pivotGameobject = new GameObject("PlacerPivot");
        // Diamo a questo oggetto la posizione della nostra camera AR
        pivotGameobject.transform.position = new Vector3(ARCamera.position.x, lowestPlaneY, ARCamera.position.z);
        // Proiettiamo la nostra rotazione sul pavimento 
        Vector3 projectedForward = Vector3.ProjectOnPlane(pivotGameobject.transform.forward, Vector3.up);
        Quaternion projectedRotation = Quaternion.LookRotation(projectedForward);
        pivotGameobject.transform.rotation = projectedRotation;

        // Converto la stringa di Maps in un vector2
        Vector2 coordinatePoint = objectToPlace.GetGPSCoordinatesAsVector2();
        // Genero Posizione 3D e Heading e le metto in un datapack
        UnityGPSDatapack coordinateDatapack = GPSManager.Singleton.GetCoordinatesInfo(coordinatePoint);
        // Ottengo la mia distanza dal target attuale, non mi serve altro attualmente
        float distanceTowardsTheTarget = Vector3.Distance(GPSManager.Singleton.MyPosition3D, coordinateDatapack.Target3DPosition);
        print("SAMDEBUG AutoPlacer " + distanceTowardsTheTarget);
        // Istanzio una copia dell'oggetto
        GameObject instantiatedGameobject = Instantiate(objectToPlace.GPSGameobject);
        // Lo Parento al pivot
        instantiatedGameobject.transform.parent = pivotGameobject.transform;
        // Azzero la sua rotazione (qui posso fare diverse cose,anche ad esempio orientarlo verso di me)
        instantiatedGameobject.transform.rotation = Quaternion.identity;
        // Lo metto di fronte a me, ma alla corretta distanza calcolata dal gps.
        instantiatedGameobject.transform.localPosition = new Vector3(0, 0, distanceTowardsTheTarget);
        // Ruoto il pivot tramite i dati dalla bussola
        pivotGameobject.transform.Rotate(new Vector3(0, coordinateDatapack.TargetHeading, 0));
        // Aggiungo l'anchor per migliorare il comportamento in AR
        instantiatedGameobject.AddComponent<ARAnchor>();
        // S-Parento l'oggetto instanziato
        instantiatedGameobject.transform.parent = null;
        // Aggiungo l'istanza dell'oggetto appena creato e posizionato ad una lista, per tenerne traccia
        spawnedObjects.Add(new ARGPSGameObjectsDatapack(instantiatedGameobject, objectToPlace.GPSCoordinates,objectTypeIndex));
        // Distruggo il gruppo, non serve più
        Destroy(pivotGameobject);
    }
}

//Una classe utile per definire pacchetti di dati riferiti ad un oggetto
[System.Serializable]
public class ARGPSGameObjectsDatapack {
    public GameObject GPSGameobject;
    public string GPSCoordinates;
    public int ObjectTypeIndex = 0;
    public string PointDescription;


    public ARGPSGameObjectsDatapack()
    {
        GPSGameobject = null;
        GPSCoordinates = "";
    }

    public ARGPSGameObjectsDatapack(GameObject inGPSGameobject, string inGpsCoordinates)
    {
        GPSGameobject = inGPSGameobject;
        GPSCoordinates = inGpsCoordinates;
    }

    public ARGPSGameObjectsDatapack(GameObject inGPSGameobject, string inGpsCoordinates, int inIndex)
    {
        GPSGameobject = inGPSGameobject;
        GPSCoordinates = inGpsCoordinates;
        ObjectTypeIndex = inIndex;
    }

    public Vector2 GetGPSCoordinatesAsVector2()
    {
        return GPSManager.MapsCoordinateToVector2(GPSCoordinates);
    }
}
