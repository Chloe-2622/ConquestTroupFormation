using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TempoPlayerController : MonoBehaviour
{
    [SerializeField] Camera camera2;
    [SerializeField] LayerMask layerMask;
    [SerializeField] SelectionManager selectionManager;

    // Start is called before the first frame update
    void Start()
    {
        selectionManager.completeDictionnary(transform.gameObject);
        //transform.GetChild(0).GetComponent<Animator>().SetFloat("ForwardSpeed", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (selectionManager.isSelected(transform.gameObject) && Input.GetMouseButtonDown(0)) // 
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
