using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TroupPurchase : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] private InputActionReference placeUnitAction;
    [SerializeField] private InputActionReference showPlacementAction;
    [SerializeField] private InputActionReference removeUnitAction;
    [SerializeField] private InputActionReference removeSelectionAction;

    [Header("Placement Options")]
    [SerializeField] private float minDistanceBetweenUnits;

    [Header("Layers")]
    [SerializeField] private LayerMask floorLayerMask;

    private List<GameObject> unitPrefabs;
    private Troup.UnitType currentSelectedUnitType;

    private GameManager gameManager;
    private GameObject preview;
    private Troup previewTroupComponent;
    private Vector3 lastPosition;
    private bool isOnUI;




    private void OnEnable()
    {
        placeUnitAction.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        showPlacementAction.action.Enable();
        removeUnitAction.action.Enable();
        removeSelectionAction.action.Enable();
        placeUnitAction.action.performed += placeUnit; // S'active lorque la valeur de ReadValue change
        showPlacementAction.action.performed += showPlacement;
        removeUnitAction.action.performed += removeUnit;
        removeSelectionAction.action.started += removeSelection;
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        Debug.Log("Troup Purchase Disabled");
        GameObject.Destroy(preview);

        placeUnitAction.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
        showPlacementAction.action.Disable();
        removeUnitAction.action.Disable();
        removeSelectionAction.action.Disable();
        placeUnitAction.action.performed += placeUnit;
        showPlacementAction.action.performed -= showPlacement;
        removeUnitAction.action.performed += removeUnit;
        removeSelectionAction.action.started += removeSelection;
    }

    public void Start()
    {
        gameManager = GameManager.Instance;
        unitPrefabs = gameManager.getUnitPrefabs();
    }


    private void showPlacement(InputAction.CallbackContext context)
    {
        Debug.Log(gameManager.getAllies().Count);

        // Si la souris est sur l'UI, on ne fait rien
        if (isOnUI) { return; }

        // Si on a pas s�lectionn� de troupe, on ne fait rien
        if (currentSelectedUnitType == Troup.UnitType.Null) { return; }

        // On regarde le point du niveau touch� par notre souris
        Ray ray = gameManager.mainCamera.ScreenPointToRay(context.action.ReadValue<Vector2>());
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayerMask))
        {
            // On cherche le point le plus proche sur le NavMesh
            NavMeshHit closestHit;

            if (NavMesh.SamplePosition(hit.point, out closestHit, 10, 1))
            {
                // Si la preview existe et est du bon type, on la d�place vers la position point�e
                if (preview != null && previewTroupComponent.unitType == currentSelectedUnitType)
                {
                    preview.transform.position = closestHit.position;
                }
                // Sinon on (re)cr�e la preview
                else
                {
                    GameObject.Destroy(preview);
                    preview = Instantiate(unitPrefabs[(int)currentSelectedUnitType - 1], closestHit.position, new Quaternion(0, 0, 0, 0));
                    previewTroupComponent = preview.GetComponent<Troup>();
                    previewTroupComponent.enabled = false;
                }

                // On sauvegarde la derni�re position valide
                lastPosition = closestHit.position;
            }
            else
            {
                Debug.Log("Couldn't find near NavMesh");
            }
        }
    }   

    private void placeUnit(InputAction.CallbackContext context)
    {
        // Si la souris est sur l'UI, le click peut �tre celui d'un bouton
        if (isOnUI) { return; }

        // Si on a pas s�lectionn� de troupe, on ne fait rien
        if (currentSelectedUnitType == Troup.UnitType.Null) { return; }

        // Si l'unit� est trop pr�s d'une unit� d�j� plac�e, il ne se passe rien
        HashSet<Troup> allies = gameManager.getAllies();
        Debug.Log(allies.Count);
        foreach (Troup ally in allies)
        {
            Debug.Log(ally);
            Debug.Log(ally.gameObject);
            Debug.Log(ally.gameObject.transform.position);
            Debug.Log(lastPosition);
            if (Vector3.Distance(ally.gameObject.transform.position, lastPosition) < minDistanceBetweenUnits) { return; }
        }

        GameObject newUnit = Instantiate(unitPrefabs[(int)currentSelectedUnitType - 1], lastPosition, new Quaternion(0, 0, 0, 0));
        gameManager.addAlly(newUnit.GetComponent<Troup>());
    }

    private void removeUnit(InputAction.CallbackContext context)
    {

    }

    private void removeSelection(InputAction.CallbackContext context)
    {

    }



    // is Cursor on UI
    public void set_isOnUI(bool newValue) { isOnUI = newValue; }

    // Current Troup Selection
    public Troup.UnitType getCurrentSelectedTroupType() { return currentSelectedUnitType; }
    public void setCurrentSelectedTroupType(Troup.UnitType selectedTroupType) { currentSelectedUnitType = selectedTroupType; }
}
