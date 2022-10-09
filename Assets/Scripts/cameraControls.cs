using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControls : MonoBehaviour
{
    [SerializeField] float horSens;
    [SerializeField] float vertSens;
    [SerializeField] float lookVertMax;
    [SerializeField] float lookVertMin;
    [SerializeField] bool invert;

    float xRotation;
    // Start is called before the first frame update
    void Start()
    {
        //lock cursor to middle and make invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        //camera input
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * horSens;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * vertSens;
        if (invert)
        {
            xRotation -= mouseY;
        }
        else
        {
            xRotation += mouseY;
        }

        //clamp xRotation
        xRotation = Mathf.Clamp(xRotation, lookVertMin, lookVertMax);
        //rotate camera
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        //rotate player
        transform.parent.Rotate(Vector3.up * mouseX);

    }

   
}
