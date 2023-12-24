using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Customisation")]
    [SerializeField] private float defaultSpeed = 10f;
    //[SerializeField] private float borderSize = 50f;

    [Header("Input System")]
    [SerializeField] private InputActionReference movement;
    [SerializeField] private InputActionReference sprint;

    private Camera cameraObject;
    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        cameraObject = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        float currentSpeed;
        if (sprint.action.inProgress) { currentSpeed = 2 * defaultSpeed;}
        else { currentSpeed = defaultSpeed; }

        float dx = 0f;
        float dz = 0f;
        
        Vector2 movementInput = movement.action.ReadValue<Vector2>();

        if (movementInput.x > 0 || Input.GetKey(KeyCode.D))
        {
            dx++;
            dz--;
        }
        if (movementInput.x < 0 || Input.GetKey(KeyCode.A))
        {
            dx--;
            dz++;
        }
        if (movementInput.y > 0 || Input.GetKey(KeyCode.W))
        {
            dx++;
            dz++;
        }
        if (movementInput.y < 0 || Input.GetKey(KeyCode.S))
        {
            dx--;
            dz--;
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            // Adjust the speed of scrolling by multiplying with a factor if needed
            float scrollSpeed = transform.position.y / 10;

            // Calculate the new position
            Vector3 newPosition = transform.position + (transform.forward * Input.mouseScrollDelta.y * scrollSpeed);

            // Clamp the y component between 6 and 30
            newPosition.y = Mathf.Clamp(newPosition.y, 6f, 30f);

            // Apply the new position
            if ((newPosition.y == 6f && Input.mouseScrollDelta.y > 0) || (newPosition.y == 30f && Input.mouseScrollDelta.y < 0))
            {
                // Scroll is not allowed at the current position, do not update the position
            }
            else
            {
                // Apply the new position
                transform.position = newPosition;
            }
        }

        if (!isPaused)
        {
            cameraObject.transform.Translate(new Vector3(dx * currentSpeed * Time.deltaTime, 0, dz * currentSpeed * Time.deltaTime), Space.World);
        }
    }
}
