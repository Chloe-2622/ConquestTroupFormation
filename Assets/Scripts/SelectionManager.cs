using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] GameObject[] gameObjects;

    private Dictionary<GameObject, bool> selections = new Dictionary<GameObject, bool>();
    private GameObject currentSelection;

    private bool isHolding = false;
    private Vector3 position_1;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isHolding == false)
        {
            position_1 = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            //if (Input.mousePosition - position_1)
        }
    }

    public void selectArea()
    {
        Debug.Log("Area selected");
    }

    public void selectUnit()
    {
        Debug.Log("Unit� s�lectionn�e");
    }








    public void completeDictionnary(GameObject selection)
    {
        selections.Add(selection, false);
    }

    public bool isSelected(GameObject selection)
    {
        if (selections.ContainsKey(selection))
        {
            return selections[selection];
        }
        else
        {
            Debug.LogWarning("L'objet n'est pas pr�sent dans le dictionnaire de s�lection.");
            return false;
        }
    }

    public void select(GameObject selection)
    {
        if (selections.ContainsKey(selection))
        {
            selections[selection] = true;
            Debug.Log(selection.ToString() + " a �t� s�lectionn�");
            if (currentSelection != null && currentSelection != selection)
            {
                selections[currentSelection] = false;
            }
            currentSelection = selection;
        }
        else
        {
            Debug.LogWarning("L'objet n'est pas pr�sent dans le dictionnaire de s�lection.");
        }
    }





}
