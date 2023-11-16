using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStandbyTime : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
