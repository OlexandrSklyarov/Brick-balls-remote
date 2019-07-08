using UnityEngine;


public class CameraScreenScale : MonoBehaviour
{
    [SerializeField]
    private float screenHeight = 1920f;
    [SerializeField]
    private float screenWidth = 1080f;
    [SerializeField]
    private float targetAspect = 9f / 16f;
    [SerializeField]
    private float orthographicSize;
    private Camera mainCamera;    

    void Start()
    {
        mainCamera = GetComponent<Camera>();
         // Initialize variables        
        orthographicSize = mainCamera.orthographicSize;

        // Calculating ortographic width
        float orthoWidth = orthographicSize / screenHeight * screenWidth;
        // Setting aspect ration
        orthoWidth = orthoWidth / (targetAspect / mainCamera.aspect);
        // Setting Size
        Camera.main.orthographicSize = (orthoWidth / Screen.width * Screen.height);
    }  
}
