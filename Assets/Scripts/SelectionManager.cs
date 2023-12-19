using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] Camera shootingCamera;
    [SerializeField] int areaSelectionLimit = 1;
    [SerializeField] int unitSelectionDistanceLimit = 1;

    [Header("Input System")]
    [SerializeField] private InputActionReference selectionAction;


    private Dictionary<GameObject, bool> selectionableObjects = new Dictionary<GameObject, bool>();
    private List<GameObject> nextSelections = new List<GameObject>();
    private List<GameObject> currentSelections = new List<GameObject>();


    private bool isHolding = false;
    private Vector2 position_1;
    private Vector2 position_2;


    // On s'abonne aux évènements du Event System
    private void OnEnable()
    {
        selectionAction.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        selectionAction.action.started += OnInputStarted; // S'active à la pression initiale des touches
        selectionAction.action.canceled += OnInputCanceled; // S'active au relachement des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        selectionAction.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        selectionAction.action.started -= OnInputStarted;
        selectionAction.action.canceled -= OnInputCanceled;
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        position_1 = selectionAction.action.ReadValue<Vector2>();
        isHolding = true;
    }

    public void OnInputCanceled(InputAction.CallbackContext context)
    {
        position_2 = selectionAction.action.ReadValue<Vector2>();
        Debug.Log(Vector3.Distance(position_1, position_2));
        if (Vector3.Distance(position_1, position_2) < areaSelectionLimit)
        {
            selectUnit();
        }
        else
        {
            selectArea();
        }
        isHolding = false;
    }

    // Sactive en permanence, pour afficher la zone sélectionner
    private void Update()
    {
        if (isHolding)
        {
            drawSquare();
        }
    }

    // Sélectionne l'unité la plus proche 
    public void selectUnit()
    {
        Debug.Log("Unit Selected");
        Ray ray_1 = shootingCamera.ScreenPointToRay(position_1);
        RaycastHit hit_1;
        Physics.Raycast(ray_1, out hit_1, Mathf.Infinity);
        float minDistance = unitSelectionDistanceLimit;
        GameObject nearestObject = null;
        foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {
            float distance = Vector3.Distance(hit_1.point, selectionableObject.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestObject = selectionableObject;
            }
        }
        select(new List<GameObject> { nearestObject });
    }

    // Détermine les unités qui se situent dans la zone sélectionnée
    public void selectArea()
    {
        Debug.Log("Area selected");
        Ray ray_1 = shootingCamera.ScreenPointToRay(position_1);
        RaycastHit hit_1;
        Ray ray_2 = shootingCamera.ScreenPointToRay(position_2);
        RaycastHit hit_2;

        Physics.Raycast(ray_1, out hit_1, Mathf.Infinity);
        Physics.Raycast(ray_2, out hit_2, Mathf.Infinity);

        nextSelections = new List<GameObject>();
        foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {
            if (isInArea(hit_1.point, hit_2.point, selectionableObject.transform.position))
            {
                nextSelections.Add(selectionableObject);
                Debug.Log(selectionableObject.name);
            }
        }
        select(nextSelections);
    }

    // Vérifie si l'unité en position unitPosition est dans le carré délimité par point_1 et point_2
    public bool isInArea(Vector3 point_1, Vector3 point_2, Vector3 unitPosition)
    {
        bool isIn = false;

        if ((point_1.x < unitPosition.x && unitPosition.x  < point_2.x) || (point_2.x < unitPosition.x && unitPosition.x < point_1.x))
        {
            if ((point_1.z < unitPosition.z && unitPosition.z < point_2.z) || (point_2.z < unitPosition.z && unitPosition.z < point_1.z))
            {
                isIn = true;
            }
        }
        return isIn;
    }
    
    // Sélectionne une liste d'éléments voulus
    public void select(List<GameObject> selectionedList)
    {
        resetSelection();
        foreach (GameObject nextSelection in nextSelections)
        {
            if (selectionableObjects.ContainsKey(nextSelection))
            {
                selectionableObjects[nextSelection] = true;
                currentSelections.Add(nextSelection);
                Debug.Log(nextSelection.ToString() + " a été sélectionné");
            }
            else
            {
                Debug.LogWarning("L'objet n'est pas présent dans le dictionnaire de sélection.");
            }
        }
    }

    // Dessine la zone de sélection lorsque le click est maintenue
    public void drawSquare()
    {
        Vector2 currentPosition = selectionAction.action.ReadValue<Vector2>();
        Ray ray_1 = shootingCamera.ScreenPointToRay(position_1);
        RaycastHit hit_1;
        Ray ray_2 = shootingCamera.ScreenPointToRay(new Vector2(currentPosition.x, position_1.y));
        RaycastHit hit_2;
        Ray ray_3 = shootingCamera.ScreenPointToRay(currentPosition);
        RaycastHit hit_3;
        Ray ray_4 = shootingCamera.ScreenPointToRay(new Vector2(position_1.x, currentPosition.y));
        RaycastHit hit_4;

        Physics.Raycast(ray_1, out hit_1, Mathf.Infinity);
        Physics.Raycast(ray_2, out hit_2, Mathf.Infinity);
        Physics.Raycast(ray_3, out hit_3, Mathf.Infinity);
        Physics.Raycast(ray_4, out hit_4, Mathf.Infinity);
        
        Debug.DrawLine(hit_1.point, hit_2.point, Color.red);
        Debug.DrawLine(hit_2.point, hit_3.point, Color.red);
        Debug.DrawLine(hit_3.point, hit_4.point, Color.red);
        Debug.DrawLine(hit_4.point, hit_1.point, Color.red);
    }

    // Désélectionne tous les éléments précédements sélectionnés
    private void resetSelection()
    {
        foreach (GameObject currentSelection in currentSelections)
        {
            selectionableObjects[currentSelection] = false;
        }
        currentSelections = new List<GameObject>();
    }


    public void completeDictionnary(GameObject selection)
    {
        selectionableObjects.Add(selection, false);
    }

    public bool isSelected(GameObject selection)
    {
        if (selectionableObjects.ContainsKey(selection))
        {
            return selectionableObjects[selection];
        }
        else
        {
            Debug.LogWarning("L'objet n'est pas présent dans le dictionnaire de sélection.");
            return false;
        }
    }
}
