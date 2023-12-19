using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TempoPlayerController : MonoBehaviour
{
    [SerializeField] Camera camera2;
    [SerializeField] LayerMask layerMask;
    [SerializeField] SelectionManager selectionManager;

    [SerializeField] private InputActionReference goTo;

    // Start is called before the first frame update
    void Start()
    {
        selectionManager.completeDictionnary(transform.gameObject);
        //transform.GetChild(0).GetComponent<Animator>().SetFloat("ForwardSpeed", 0f);
    }

    private void OnEnable()
    {
        selection_tmp.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        selection_tmp.action.started += OnInputStarted; // S'active à la pression initiale des touches
        selection_tmp.action.canceled += OnInputCanceled; // S'active au relachement des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        selection_tmp.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        selection_tmp.action.started -= OnInputStarted;
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        position_1 = selection_tmp.action.ReadValue<Vector2>();
        isHolding = true;
    }


















    // Update is called once per frame
    void Update()
    {
        if (selectionManager.isSelected(transform.gameObject))
        {
            Ray ray = camera2.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.Log("Click !");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                GetComponent<NavMeshAgent>().SetDestination(hit.point);
                Debug.DrawLine(transform.position, hit.point);
            }
        }
    }
}
