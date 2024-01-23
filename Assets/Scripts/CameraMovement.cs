using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Customisation")]
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 2f;
    //[SerializeField] private float borderSize = 50f;

    [Header("Input System")]
    [SerializeField] private InputActionReference movement;
    [SerializeField] private InputActionReference sprint;
    [SerializeField] private InputActionReference zoom;

    private Camera cameraObject;
    private bool isMoving;

    // Start is called before the first frame update
    void Start()
    {
        cameraObject = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        movement.action.Enable(); // Activer l'action d'entr�e lorsque le script est activ�
        sprint.action.Enable();
        zoom.action.Enable();

        movement.action.started += startCameraMove;
        movement.action.canceled += stopCameraMove;
        zoom.action.started += zoomCamera;
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        movement.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
        sprint.action.Disable();
        zoom.action.Disable();

        movement.action.started -= startCameraMove;
        movement.action.canceled -= stopCameraMove;
        zoom.action.started -= zoomCamera;
    }

    public void startCameraMove(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isInPause()) { return; }
        isMoving = true;
    }

    public void stopCameraMove(InputAction.CallbackContext context)
    {
        isMoving = false;
    }

    public void Update()
    {
        if ( isMoving ) 
        {
            float currentSpeed;
            if (sprint.action.inProgress)
            {
                currentSpeed = sprintMultiplier * defaultSpeed;
            }
            else { currentSpeed = defaultSpeed; }

            float dx = 0f;
            float dz = 0f;
            Vector2 movementInput = movement.action.ReadValue<Vector2>();

            if (movementInput.x > 0)
            {
                dx++;
                dz--;
            }
            if (movementInput.x < 0)
            {
                dx--;
                dz++;
            }
            if (movementInput.y > 0)
            {
                dx++;
                dz++;
            }
            if (movementInput.y < 0)
            {
                dx--;
                dz--;
            }

            GameManager.Instance.troupPurchase.refreshPreview();
            cameraObject.transform.Translate(new Vector3(dx * currentSpeed * Time.deltaTime, 0, dz * currentSpeed * Time.deltaTime), Space.World);
        }
    }

    public void zoomCamera(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isInPause() ) { return; }

        Vector2 zoomVector = zoom.action.ReadValue<Vector2>();
        zoomVector.Normalize();

        // Adjust the speed of scrolling by multiplying with a factor if needed
        float scrollSpeed = transform.position.y / 10;

        // Calculate the new position
        Vector3 newPosition = transform.position + (transform.forward * zoomVector.y * scrollSpeed);

        // Clamp the y component between 6 and 30
        newPosition.y = Mathf.Clamp(newPosition.y, 6f, 30f);

        // Physics.Raycast(transform.position, Vector3.down, 5f);

        if (Physics.Raycast(transform.position, Vector3.down, 5f))
        {
            if (zoomVector.y < 0)
            {
                transform.position = newPosition;
            }
        } else if (newPosition.y == 30f)
        {
            if (zoomVector.y > 0)
            {
                transform.position = newPosition;
            }
        } else
        {
            transform.position = newPosition;
        }
    }






    /*
    // Update is called once per frame
    void Update()
    {

        float currentSpeed;
        if (sprint.action.inProgress) {
            currentSpeed = sprintMultiplier * defaultSpeed;}
        else { currentSpeed = defaultSpeed; }

        float dx = 0f;
        float dz = 0f;
        
        Vector2 movementInput = movement.action.ReadValue<Vector2>();

        if (movementInput.x > 0)
        {
            dx++;
            dz--;
        }
        if (movementInput.x < 0)
        {
            dx--;
            dz++;
        }
        if (movementInput.y > 0)
        {
            dx++;
            dz++;
        }
        if (movementInput.y < 0)
        {
            dx--;
            dz--;
        }

        cameraObject.transform.Translate(new Vector3(dx * currentSpeed * Time.deltaTime, 0, dz * currentSpeed * Time.deltaTime), Space.World);
    }
    */
}
