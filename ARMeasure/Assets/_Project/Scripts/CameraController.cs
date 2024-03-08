using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityGoogleDrive;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public FileUploader m_fileUploader;
    public Measurement m_measurement;
    public RawImage m_rawImage;
    Camera mainCamera;
    public string imagePath = "Assets/CapturedImage.png";
    Texture2D image;
    private void Start()
    {
         m_fileUploader = GetComponent<FileUploader>();
         mainCamera = GetComponent<Camera>();
    }
    private void Update()
    {
        if (m_measurement.wallPoints.Count == 0)
        {
            Debug.LogWarning("No wallPoints assigned to CameraController. Please assign wallPoints.");
            return;
        }

        // Find the center point of all wallPoints
        Vector3 centerPoint = CalculateCenterPoint();

        // Set camera position above and at the center of wallPoints
        transform.position = new Vector3(centerPoint.x, CalculateCameraHeight(), centerPoint.z);

        // Set camera rotation to look straight down (90 degrees)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Set camera to orthographic projection
        Camera.main.orthographic = true;

        // Adjust camera orthographic size to ensure all wallPoints are visible
        AdjustCameraSize(centerPoint);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(UploadImageToDrive());
        }
    }

    private Vector3 CalculateCenterPoint()
    {
        Vector3 centerPoint = Vector3.zero;

        foreach (GameObject point in m_measurement.wallPoints)
        {
            centerPoint += point.transform.position;
        }

        centerPoint /= m_measurement.wallPoints.Count;

        return centerPoint;
    }

    private float CalculateCameraHeight()
    {
        // You can adjust this value to set the desired height above the wallPoints
        return 10f;
    }

    private void AdjustCameraSize(Vector3 centerPoint)
    {


        if (mainCamera == null)
        {
             mainCamera = GetComponent<Camera>();
            Debug.LogError("Camera component not found on the camera GameObject.");
            return;
        }

        // Calculate the distance from the center point to the furthest wallPoint
        float maxDistance = 0f;

        foreach (GameObject point in m_measurement.wallPoints)
        {
            float distance = Vector3.Distance(centerPoint, point.transform.position);

            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        // Set the orthographic size to ensure all wallPoints are visible
        float requiredSize = maxDistance / Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        mainCamera.orthographicSize = requiredSize;
    }

    private void CaptureImage()
    {
        // Ensure that the target texture is not null
        if (mainCamera.targetTexture == null)
        {
            Debug.LogError("Target texture is null. Please assign a render texture to the camera's target texture.");
            return;
        }

        // Get the texture from the camera's target texture
        RenderTexture.active = mainCamera.targetTexture;
        Texture2D rawTexture = new Texture2D(mainCamera.targetTexture.width, mainCamera.targetTexture.height);
        rawTexture.ReadPixels(new Rect(0, 0, mainCamera.targetTexture.width, mainCamera.targetTexture.height), 0, 0);
        rawTexture.Apply();
        RenderTexture.active = null;

        // Create a new Texture2D and copy the camera's target texture into it
        image = new Texture2D(rawTexture.width, rawTexture.height);
        image.SetPixels(rawTexture.GetPixels());
        image.Apply();

        // Convert the Texture2D to JPEG format and save it to the specified path
        byte[] bytes = image.EncodeToJPG();
        System.IO.File.WriteAllBytes(imagePath, bytes);
        Debug.Log("Image captured and saved to: " + imagePath);

      //  StartCoroutine(UploadImageToDrive());

    }


    public void CallFileUpload()
    {
        StartCoroutine(UploadImageToDrive());
    }

    IEnumerator UploadImageToDrive()
    {
        // Get the texture from the camera's target texture
        RenderTexture.active = mainCamera.targetTexture;
        Texture2D rawTexture = new Texture2D(mainCamera.targetTexture.width, mainCamera.targetTexture.height);
        rawTexture.ReadPixels(new Rect(0, 0, mainCamera.targetTexture.width, mainCamera.targetTexture.height), 0, 0);
        rawTexture.Apply();
        RenderTexture.active = null;

        // Convert the Texture2D to JPEG format
        byte[] content = rawTexture.EncodeToJPG();

        // Create a File object with the image data
        var file = new UnityGoogleDrive.Data.File()
        {
            Name = "CapturedImage.jpg",
            Content = content
        };

        // Upload the file to Google Drive
        var request = GoogleDriveFiles.Create(file);
        request.Fields = new List<string> { "id" };
        yield return request.Send();

        // Check for errors and log the response
        if (request.IsError)
        {
            Debug.LogError("Error uploading file to Google Drive: " + request.ErrorMessage);
        }
        else
        {
            Debug.Log("File uploaded successfully. ID: " + request.ResponseData.Id);
            m_fileUploader.UploadDone();
        }
    }

 


}
