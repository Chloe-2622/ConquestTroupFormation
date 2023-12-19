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
