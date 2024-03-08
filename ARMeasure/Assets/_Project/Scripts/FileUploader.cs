using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileUploader : MonoBehaviour
{

    [SerializeField] private CameraController m_cameraController;
    [SerializeField] private GameObject sendPanel,fileSendMsg,closeApp;
    void Start()
    {
        sendPanel.SetActive(false);
        fileSendMsg.SetActive(false);
        closeApp.SetActive(false);
        m_cameraController = GetComponent<CameraController>();
    }



    public void DoneScane()
    {
        sendPanel.SetActive(true);
    }
    public void SendFile()
    {
        sendPanel.SetActive(true);
        m_cameraController.CallFileUpload();
    }

    


    public void ResStart()
    {
        Application.LoadLevel(1);
    }

    

    public void UploadDone()
    {
        StartCoroutine(ShowMessage());
    }

    IEnumerator ShowMessage()
    {
        yield return new WaitForSeconds(0.5f);
        sendPanel.SetActive(false);
        fileSendMsg.SetActive(true);

        yield return new WaitForSeconds(3);

        fileSendMsg.SetActive(false);
        closeApp.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

}
