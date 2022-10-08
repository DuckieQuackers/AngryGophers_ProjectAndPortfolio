using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControls : MonoBehaviour
{
    [SerializeField] int sensHort;
    [SerializeField] int sensVert;

    [SerializeField] int lockVertMin;
    [SerializeField] int lockVertMax;

    [SerializeField] bool invert;

    float xRotation;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    void LateUpdate()
    {
        //get input
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensHort;
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensVert;

        if (invert)
        {
            xRotation += mouseY;
        }
        else
        {
            xRotation -= mouseY;
        }
        

        //clamp camera rotation
        xRotation = Mathf.Clamp(xRotation, lockVertMin, lockVertMax);

        //rotate camera on x-axis
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        //rotate player
        transform.parent.Rotate(Vector3.up * mouseX);

    }

   
}
