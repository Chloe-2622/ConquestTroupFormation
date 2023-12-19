using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Combattant : Troup
{

    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Target position clicked : " + hit.point);
                Debug.DrawLine(transform.position, hit.point);
                AddAction(new MoveToPosition(agent, hit.point));
            }
        }
    }

    private void OnMouseOver()
    {
        Follow();
    }

    private void OnMouseExit()
    {
        // Standby();
    }
}
