using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class TestRaycast : MonoBehaviour
{
    
    public ARRaycastManager RaycastManager;
    public Transform ObjectToSpawn;

    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    // Start is called before the first frame update
    void Start()
    {
        RaycastManager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if(RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
                {
                        GameObject instantiatedCube = Instantiate(ObjectToSpawn, m_Hits[m_Hits.Count -1].pose.position, m_Hits[m_Hits.Count -1].pose.rotation).gameObject;
                    instantiatedCube.AddComponent<ARAnchor>();
                }
            }
        }
    }
}
