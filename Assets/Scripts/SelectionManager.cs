using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] Camera shootingCamera;
    [SerializeField] int areaSelectionLimit = 1;

    private Dictionary<GameObject, bool> selectionableObjects = new Dictionary<GameObject, bool>();
    private List<GameObject> nextSelections = new List<GameObject>();
    private List<GameObject> currentSelections = new List<GameObject>();

    private bool isHolding = false;
    private Vector3 position_1;
    private Vector3 position_2;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isHolding == false)
        {
            position_1 = Input.mousePosition;
            isHolding = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            position_2 = Input.mousePosition;
            Debug.Log(Vector3.Distance(position_1, position_2));            
            if (Vector3.Distance(position_1, position_2) < areaSelectionLimit)
            {
                selectUnit();
            }
            else
            {
                selectArea();
            }
            isHolding= false;
        }

        if (isHolding == true)
        {
            drawSquare();
        }
    }

    public void selectUnit()
    {


        Debug.Log("Unité sélectionnée");
    }

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

        /*if ()
        {
            GetComponent<NavMeshAgent>().SetDestination(hit.point);
            Debug.DrawLine(transform.position, hit.point);
        }*/
    }

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


    public void drawSquare()
    {
        Vector3 currentPosition = Input.mousePosition;
        Ray ray_1 = shootingCamera.ScreenPointToRay(position_1);
        RaycastHit hit_1;
        Ray ray_2 = shootingCamera.ScreenPointToRay(new Vector3(currentPosition.x, position_1.y, 0));
        RaycastHit hit_2;
        Ray ray_3 = shootingCamera.ScreenPointToRay(currentPosition);
        RaycastHit hit_3;
        Ray ray_4 = shootingCamera.ScreenPointToRay(new Vector3(position_1.x, currentPosition.y, 0));
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

    private void resetSelection()
    {
        foreach (GameObject currentSelection in currentSelections)
        {
            selectionableObjects[currentSelection] = false;
        }
        currentSelections = new List<GameObject>();
    }

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

    /*
    public void select(GameObject selection)
    {
        if (selectionableObjects.ContainsKey(selection))
        {
            selectionableObjects[selection] = true;
            Debug.Log(selection.ToString() + " a été sélectionné");
            if (currentSelection != null && currentSelection != selection)
            {
                selectionableObjects[currentSelection] = false;
            }
            currentSelection = selection;
        }
        else
        {
            Debug.LogWarning("L'objet n'est pas présent dans le dictionnaire de sélection.");
        }
    }*/

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
