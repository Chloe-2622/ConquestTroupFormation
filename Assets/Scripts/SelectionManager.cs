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
    private List<GameObject> currentSelections = new List<GameObject>();


    private bool isHolding = false;
    private Vector2 lastPosition;
    private Vector2 position_1;
    private Vector2 position_2;


    // On s'abonne aux �v�nements du Event System
    private void OnEnable()
    {
        selectionAction.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        selectionAction.action.started += OnInputStarted; // S'active � la pression initiale des touches
        selectionAction.action.performed += OnInputPerformed; // S'active lorque la valeur de ReadValue change
        selectionAction.action.canceled += OnInputCanceled; // S'active au relachement des touches
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        selectionAction.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
        selectionAction.action.started -= OnInputStarted;
        selectionAction.action.performed -= OnInputPerformed;
        selectionAction.action.canceled -= OnInputCanceled;
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        position_1 = context.action.ReadValue<Vector2>();
        isHolding = true;
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        lastPosition = context.action.ReadValue<Vector2>();
    }


    public void OnInputCanceled(InputAction.CallbackContext context)
    {
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

    // Sactive en permanence, pour afficher la zone s�lectionner
    private void Update()
    {
        if (isHolding)
        {
            drawSquare();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            foreach (GameObject selectionableObject in selectionableObjects.Keys)
            {
                Debug.Log("cl� : " + selectionableObject + " et value : " + selectionableObjects[selectionableObject]);
            }
        }
    }

    // S�lectionne l'unit� la plus proche 
    public void selectUnit()
    {
        RemoveNullKeys(selectionableObjects);
        Debug.Log("Cl� Unit Selection");
        /* foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {
            if (selectionableObject == null)
            {
                selectionableObjects.Remove(selectionableObject);
            }
            
        } */
        foreach (GameObject selectionableObject in selectionableObjects.Keys)
        {

            Debug.Log("cl� : " + selectionableObject + " et value : " + selectionableObjects[selectionableObject]);

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
        if ((System.Object)nearestObject != null) { select(new List<GameObject> { nearestObject }); }
        
    }

    // D�termine les unit�s qui se situent dans la zone s�lectionn�e
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

    // V�rifie si l'unit� en position unitPosition est dans le carr� d�limit� par point_1 et point_2
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
    
    // S�lectionne une liste d'�l�ments voulus
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
                    Debug.Log(nextSelection.ToString() + " a �t� s�lectionn�");
                }
                else
                {
                    Debug.LogWarning("L'objet n'est pas pr�sent dans le dictionnaire de s�lection.");
                }
            }
        }
    }

    // Dessine la zone de s�lection lorsque le click est maintenue
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

    // D�s�lectionne tous les �l�ments pr�c�dements s�lectionn�s
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
            // Debug.LogWarning("L'objet n'est pas pr�sent dans le dictionnaire de s�lection.");
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
}
