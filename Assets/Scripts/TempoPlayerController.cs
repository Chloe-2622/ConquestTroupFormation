using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class TempoPlayerController : MonoBehaviour
{
    [SerializeField] Camera camera2;
    [SerializeField] LayerMask layerMask;
    [SerializeField] SelectionManager selectionManager;

    [SerializeField] private InputActionReference goTo;

    private bool isMoving = false;
    private Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        selectionManager.completeDictionnary(transform.gameObject);
        //transform.GetChild(0).GetComponent<Animator>().SetFloat("ForwardSpeed", 0f);
    }

    private void OnEnable()
    {
        goTo.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        goTo.action.started += OnInputStarted; // S'active à la pression initiale des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        goTo.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        goTo.action.started -= OnInputStarted;
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        if (selectionManager.isSelected(transform.gameObject))
        {
            Ray ray = camera2.ScreenPointToRay(goTo.action.ReadValue<Vector2>());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                GetComponent<NavMeshAgent>().SetDestination(hit.point);
                destination = hit.point;
                isMoving = true;
            }
        }
    }

    public void Update()
    {
        if(isMoving)
        {
            Debug.DrawLine(transform.position, destination, Color.green);
        }
        if(Vector3.Distance(transform.position, destination) < 1)
        {
            isMoving = false;
        }
    }
}
