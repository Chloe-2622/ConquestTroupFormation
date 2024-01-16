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
    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        cameraObject = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        movement.action.Enable(); // Activer l'action d'entrée lorsque le script est activé
        sprint.action.Enable();
        zoom.action.Enable();

        zoom.action.started += zoomCamera;
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        movement.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        sprint.action.Disable();
        zoom.action.Disable();

        zoom.action.started -= zoomCamera;
    }


    public void zoomCamera(InputAction.CallbackContext context)
    {
        Vector2 zoomVector = zoom.action.ReadValue<Vector2>();
        zoomVector.Normalize();

        // Adjust the speed of scrolling by multiplying with a factor if needed
        float scrollSpeed = transform.position.y / 10;

        // Calculate the new position
        Vector3 newPosition = transform.position + (transform.forward * zoomVector.y * scrollSpeed);
        Debug.Log(newPosition);

        // Clamp the y component between 6 and 30
        newPosition.y = Mathf.Clamp(newPosition.y, 6f, 30f);

        // Apply the new position
        if ((newPosition.y == 6f && zoomVector.y > 0) || (newPosition.y == 30f && zoomVector.y < 0))
        {
            // Scroll is not allowed at the current position, do not update the position
        }
        else
        {
            // Apply the new position
            transform.position = newPosition;
        }
    }

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

        if (!isPaused)
        {
            cameraObject.transform.Translate(new Vector3(dx * currentSpeed * Time.deltaTime, 0, dz * currentSpeed * Time.deltaTime), Space.World);
        }
    }
}
