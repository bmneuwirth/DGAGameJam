using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform player;
    [SerializeField] private float mouseSensitivity = 100f;
    float cameraVerticalRotation = 0f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {

        float inputX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float inputY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the Camera around its X axis

        cameraVerticalRotation -= inputY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90f);  // Player cannot look behind them
        transform.localEulerAngles = Vector3.right * cameraVerticalRotation;


        // Rotate the Player and the Camera around its Y axis

        player.Rotate(Vector3.up * inputX);

    }
}
