using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Runtime.InteropServices.WindowsRuntime;

public class SelectionManager : MonoBehaviour
{
    [Header("Selection Parameters")]
    [SerializeField] Camera shootingCamera;
    [SerializeField] int areaSelectionLimit = 1;
    [SerializeField] int unitSelectionDistanceLimit = 1;

    [Header("Selection Representation")]
    [SerializeField] private UnityEngine.UI.Image SquareBarTop;
    [SerializeField] private UnityEngine.UI.Image SquareBarRight;
    [SerializeField] private UnityEngine.UI.Image SquareBarBottom;
    [SerializeField] private UnityEngine.UI.Image SquareBarLeft;

    [Header("Input System")]
    [SerializeField] private InputActionReference selectionAction;


    private Dictionary<GameObject, bool> selectionableObjects = new Dictionary<GameObject, bool>();
    private List<GameObject> currentSelections = new List<GameObject>();


    private bool isHolding = false;
    private Vector2 lastPosition;
    private Vector2 position_1;
    private Vector2 position_2;

    public UnityEvent newSelection;

    private void Awake()
    {
        if (newSelection == null)
            newSelection = new UnityEvent();
    }

        // On s'abonne aux évènements du Event System
    private void OnEnable()
    {
        selectionAction.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        selectionAction.action.started += OnInputStarted; // S'active à la pression initiale des touches
        selectionAction.action.performed += OnInputPerformed; // S'active lorque la valeur de ReadValue change
        selectionAction.action.canceled += OnInputCanceled; // S'active au relachement des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        selectionAction.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        selectionAction.action.started -= OnInputStarted;
        selectionAction.action.performed -= OnInputPerformed;
        selectionAction.action.canceled -= OnInputCanceled;
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isInPause()) { return; }
        position_1 = context.action.ReadValue<Vector2>();
        isHolding = true;      
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isInPause()) { return; }
        lastPosition = context.action.ReadValue<Vector2>();
    }


    public void OnInputCanceled(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.isInPause()) { return; }
        position_2 = lastPosition;

        //Debug.Log("position 1 " + position_1.ToString());
        //Debug.Log("position 2 " + position_2.ToString());
        //Debug.Log("distance " + Vector2.Distance(position_1, position_2).ToString());

        if (Vector2.Distance(position_1, position_2) < areaSelectionLimit)
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
        SquareBarTop.enabled = isHolding;
        SquareBarRight.enabled = isHolding;
        SquareBarBottom.enabled = isHolding;
        SquareBarLeft.enabled = isHolding;

        if (isHolding)
        {
            drawSquare();
        }
    }

    // Sélectionne l'unité la plus proche 
    public void selectUnit()
    {
        RemoveNullKeys(selectionableObjects);
        Debug.Log("Clé Unit Selection");
        /* foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {
            if (selectionableObject == null)
            {
                selectionableObjects.Remove(selectionableObject);
            }
            
        } */
        foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {

            Debug.Log("clé : " + selectionableObject + " et value : " + selectionableObjects[selectionableObject]);

        }
        Ray ray_1 = shootingCamera.ScreenPointToRay(position_1);
        RaycastHit hit_1;
        Physics.Raycast(ray_1, out hit_1, Mathf.Infinity);
        float minDistance = unitSelectionDistanceLimit;
        GameObject nearestObject = null;
        foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {
            Debug.Log("Point 1 : " + hit_1);
            Debug.Log("Point selObj : " + selectionableObject);
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
        Debug.Log("Area Selection");
        Ray ray_1 = shootingCamera.ScreenPointToRay(position_1);
        RaycastHit hit_1;
        Ray ray_2 = shootingCamera.ScreenPointToRay(new Vector2(position_2.x, position_1.y));
        RaycastHit hit_2;
        Ray ray_3 = shootingCamera.ScreenPointToRay(position_2);
        RaycastHit hit_3;
        Ray ray_4 = shootingCamera.ScreenPointToRay(new Vector2(position_1.x, position_2.y));
        RaycastHit hit_4;

        Physics.Raycast(ray_1, out hit_1, Mathf.Infinity);
        Physics.Raycast(ray_2, out hit_2, Mathf.Infinity);
        Physics.Raycast(ray_3, out hit_3, Mathf.Infinity);
        Physics.Raycast(ray_4, out hit_4, Mathf.Infinity);

        List<GameObject> nextSelections = new List<GameObject>();
        foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {
            if (isInArea(new Vector3[] { hit_1.point, hit_2.point, hit_3.point, hit_4.point }, selectionableObject.transform.position))
            {
                nextSelections.Add(selectionableObject);
            }
        }
        select(nextSelections);
    }

    // Vérifie si l'unité en position unitPosition est dans le carré délimité par point_1 et point_2
    public bool isInArea(Vector3[] polygon, Vector3 unitPosition)
    {
        bool isIn = false;

        float a = Vector3.Cross(polygon[1] - polygon[0], unitPosition - polygon[0]).y;
        float b = Vector3.Cross(polygon[2] - polygon[1], unitPosition - polygon[1]).y;
        float c = Vector3.Cross(polygon[3] - polygon[2], unitPosition - polygon[2]).y;
        float d = Vector3.Cross(polygon[0] - polygon[3], unitPosition - polygon[3]).y;


        if (a*b > 0 && a*c > 0 && a*d > 0)
        {
            isIn = true;
        }

        return isIn;
    }
    
    // Sélectionne une liste d'éléments voulus
    public void select(List<GameObject> nextSelections)
    {
        resetSelection();
        foreach (GameObject nextSelection in nextSelections)
        {
            if (nextSelection != null)
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
        newSelection.Invoke();
    }

    // Dessine la zone de sélection lorsque le click est maintenue
    public void drawSquare()
    {
        Vector2 currentPosition = selectionAction.action.ReadValue<Vector2>();
        /* Ray ray_1 = shootingCamera.ScreenPointToRay(position_1);
        RaycastHit hit_1;
        Ray ray_2 = shootingCamera.ScreenPointToRay(new Vector2(currentPosition.x, position_1.y));
        RaycastHit hit_2;
        Ray ray_3 = shootingCamera.ScreenPointToRay(currentPosition);
        RaycastHit hit_3;
        Ray ray_4 = shootingCamera.ScreenPointToRay(new Vector2(position_1.x, currentPosition.y));
        RaycastHit hit_4; */

        /* Physics.Raycast(ray_1, out hit_1, Mathf.Infinity);
        Physics.Raycast(ray_2, out hit_2, Mathf.Infinity);
        Physics.Raycast(ray_3, out hit_3, Mathf.Infinity);
        Physics.Raycast(ray_4, out hit_4, Mathf.Infinity); */

        

        SquareBarTop.transform.position = new Vector2((position_1.x + currentPosition.x) / 2, position_1.y);
        SquareBarTop.transform.localScale = new Vector3((currentPosition.x - position_1.x) / 20, SquareBarTop.transform.localScale.y, SquareBarTop.transform.localScale.z);

        SquareBarRight.transform.position = new Vector2(currentPosition.x, (position_1.y + currentPosition.y) / 2);
        SquareBarRight.transform.localScale = new Vector3((currentPosition.y - position_1.y) / 20, SquareBarRight.transform.localScale.y, SquareBarRight.transform.localScale.z);

        SquareBarBottom.transform.position = new Vector2((position_1.x + currentPosition.x) / 2, currentPosition.y);
        SquareBarBottom.transform.localScale = new Vector3((currentPosition.x - position_1.x) / 20, SquareBarTop.transform.localScale.y, SquareBarTop.transform.localScale.z);

        SquareBarLeft.transform.position = new Vector2(position_1.x, (position_1.y + currentPosition.y) / 2);
        SquareBarLeft.transform.localScale = new Vector3((currentPosition.y - position_1.y) / 20, SquareBarLeft.transform.localScale.y, SquareBarLeft.transform.localScale.z);



        /*  Debug.DrawLine(hit_1.point, hit_2.point, Color.red);
        Debug.DrawLine(hit_2.point, hit_3.point, Color.red);
        Debug.DrawLine(hit_3.point, hit_4.point, Color.red);
        Debug.DrawLine(hit_4.point, hit_1.point, Color.red);
        */
    }

    // Désélectionne tous les éléments précédements sélectionnés
    private void resetSelection()
    {
        foreach (GameObject currentSelection in currentSelections)
        {
            if (currentSelection != null) { selectionableObjects[currentSelection] = false; }
        }
        currentSelections = new List<GameObject>();
    }

    public Dictionary<GameObject, bool> getDictionnary()
    {
        return selectionableObjects;
    }

    public List<GameObject> getCurrentSelection()
    {
        return currentSelections;
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
            // Debug.LogWarning("L'objet n'est pas présent dans le dictionnaire de sélection.");
            return false;
        }
    }

    public void removeObject(GameObject obj)
    {
        selectionableObjects[obj] = false;
        selectionableObjects.Remove(obj);
    }

    static void RemoveNullKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        // Create a list to store keys with null values
        List<TKey> keysToRemove = new List<TKey>();

        // Find keys with null values and add them to the list
        foreach (var kvp in dictionary)
        {
            if (kvp.Key == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        // Remove entries with null keys
        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
    }

    public int numberOfSelected()
    {
        int count = 0;
        foreach (var (unit,selection) in selectionableObjects)
        {
            if (selection) { count++; }
        }

        return count;
    }
}
