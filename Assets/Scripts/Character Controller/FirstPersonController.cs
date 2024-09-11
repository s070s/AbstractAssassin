using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float lookXLimit = 90f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float crouchSpeed = 5f;

    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private Camera playerCamera;

    private float verticalVelocity = 0f;
    private float xRotation = 0f;
    private bool isCrouching = false;
    private Vector3 moveDirection = Vector3.zero;
    private float startingHeight;
    private Vector3 startingCameraHeight = new Vector3(0f, 1.4f, 0.3f);

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("No camera found in children. Please assign a camera to the playerCamera field.");
                return;
            }
        }

        // Store the starting height of the character controller
        startingHeight = controller.height;

        // Parent the camera to this GameObject if it's not already
        if (playerCamera.transform.parent != transform)
        {
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = startingCameraHeight;
            playerCamera.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        HandleJump();
        HandleCrouch();
        HandleSprint();
    }

    private void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveHorizontal + transform.forward * moveVertical;
        moveDirection = move.normalized * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0f, mouseX, 0f);
    }

    private void HandleJump()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        moveDirection.y = verticalVelocity;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isCrouching)
        {
            StartCoroutine(DoCrouch(crouchHeight));
            isCrouching = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && isCrouching)
        {
            StartCoroutine(DoCrouch(startingHeight));
            isCrouching = false;
        }
    }

    private IEnumerator DoCrouch(float targetHeight)
    {
        float time = 0;
        float startHeight = controller.height;
        Vector3 startCameraPosition = startingCameraHeight;
        Vector3 targetCameraPosition = new Vector3(0f, targetHeight / 2f, 0f);

        while (time < 1)
        {
            controller.height = Mathf.Lerp(startHeight, targetHeight, time);
            controller.center = Vector3.up * controller.height / 2f;
            playerCamera.transform.localPosition = Vector3.Lerp(startCameraPosition, targetCameraPosition, time);

            time += Time.deltaTime * crouchSpeed;
            yield return null;
        }
        if (targetHeight == crouchHeight)
        {
            controller.height = targetHeight;
            controller.center = Vector3.zero;
            playerCamera.transform.localPosition = targetCameraPosition;
        }
        else
        {
            controller.height = targetHeight;
            controller.center = Vector3.zero;
            playerCamera.transform.localPosition = startingCameraHeight;
        }

    }

    private void HandleSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            walkSpeed = sprintSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            walkSpeed = 5f;
        }
    }
}