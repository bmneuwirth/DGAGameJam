using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Basic Movement")]
    private CharacterController playerController;
    public Camera playerCamera;
    [SerializeField] private float speed = 12f;
    public float gravity = 9.8f;
    
    public float lookSpeed = 2.0f;
    public float lookXLimit = 90.0f;
    float rotationX = 0;

    private bool isCrouched = false;
    private float standingCamHeight;
    private float crouchHeight = 0.6f;
    private float crouchSpeed = 8f;

    void Start()
    {
        playerController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        standingCamHeight = playerCamera.transform.localPosition.y;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouched = !isCrouched;
        }


        float targetHeight = isCrouched ? crouchHeight : standingCamHeight;
        Vector3 curPos = playerCamera.transform.localPosition;
        float lerpedHeight = Mathf.Lerp(curPos.y, targetHeight, crouchSpeed * Time.deltaTime);
        curPos.y = lerpedHeight;
        playerCamera.transform.localPosition = curPos;
        playerController.height = lerpedHeight;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x * speed + transform.forward * z * speed;

        if (!playerController.isGrounded)
        {
            move.y -= gravity;
        }
        Debug.Log(move);
        playerController.Move(move * Time.deltaTime);

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        Debug.Log(playerController.isGrounded);
    }
}
