using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private float borderSize = 50f;

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

        if (Input.GetKeyDown(KeyCode.Escape)) { isPaused = !isPaused; }

        if (Input.GetKey(KeyCode.LeftShift)) { currentSpeed = 2 * defaultSpeed; } 
        else { currentSpeed = defaultSpeed; }

        float dx = 0f;
        float dz = 0f;

        if (Input.mousePosition.x > cameraObject.pixelWidth - borderSize)
        {
            dx++;
            dz--;
        }
        if (Input.mousePosition.x < borderSize)
        {
            dx--;
            dz++;
        }
        if (Input.mousePosition.y > cameraObject.pixelHeight - borderSize)
        {
            dx++;
            dz++;
        }
        if (Input.mousePosition.y < borderSize)
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
