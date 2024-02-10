using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCrounch : MonoBehaviour
{
    private CharacterController playerController;
    public Transform playerCamera;
    [SerializeField] private float crounchSpeed, standingHeight, crounchHeight;
    private bool isCrounching = false;
    private float standingCameraHeight = 0.6f;

    private void Start()
    {
        playerController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrounching = !isCrounching;
        }

        if (isCrounching)
        {
            playerController.height = playerController.height - crounchSpeed * Time.deltaTime;
            if (playerController.height <= crounchHeight)
            {

                playerController.height = crounchHeight;
            }
            AdjustCameraHeight();
        }

        if (!isCrounching)
        {
            playerController.height = playerController.height + crounchSpeed * Time.deltaTime;
            if (playerController.height >= standingHeight )
            {
                playerController.height = standingHeight;
            }
            AdjustCameraHeight();
        }
    }

    private void AdjustCameraHeight()
    {
        playerCamera.position = new Vector3(playerCamera.position.x, standingCameraHeight - (standingHeight - playerController.height), playerCamera.position.z);
    }
}
