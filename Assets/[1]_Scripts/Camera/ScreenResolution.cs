using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenResolution : MonoBehaviour
{
    Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();

        var h = Screen.height;
        var w = Screen.width;
        var aspect = h / w;

        if (aspect >= 1.65f && aspect <= 1.8f)
        {
            cam.orthographicSize = 5f;
        }
        else if (aspect > 1.8f)
        {
            cam.orthographicSize = 5.7f;
        }        
    }
}
