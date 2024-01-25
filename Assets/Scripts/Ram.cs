using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ram : MonoBehaviour
{
    [Header("Ram properties")]
    [SerializeField] private float ramDamage;
    [SerializeField] private float ramHitRadius;
    [SerializeField] private float wallFactor;
 
    [HideInInspector] public Troup.TroupType ramType;
    [HideInInspector] public bool isLaunch;

    private LayerMask wallMask;
    private LayerMask troupMask;

    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    public void OnEnable()
    {
        isLaunch = false;
        wallMask = GameManager.Instance.wallMask;
        troupMask = GameManager.Instance.troupMask;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLaunch) { return; }

        Collider[] troupColliders = Physics.OverlapSphere(transform.position, ramHitRadius, troupMask);

        foreach (Collider collider in troupColliders)
        {
            Troup troup = collider.GetComponent<Troup>();
            if (troup != null && troup.troupType != ramType)
            {
                Debug.Log("!!!! is troup");
                if (!hitObjects.Contains(collider.gameObject))
                {
                    hitObjects.Add(collider.gameObject);
                    Debug.Log("**** Touchéééé : " + collider.gameObject);
                    troup.TakeDamage(ramDamage);
                }
            }
        }

        Collider[] wallColliders = Physics.OverlapSphere(transform.position, ramHitRadius, wallMask);

        foreach (Collider collider in wallColliders)
        {
            Wall wall = collider.transform.parent.GetComponent<Wall>();
            if (wall != null && wall.troupType != ramType)
            {
                if (!hitObjects.Contains(collider.gameObject))
                {
                    hitObjects.Add(collider.gameObject);
                    Debug.Log("**** Touchéééé : " + collider.gameObject);
                    wall.TakeDamage(ramDamage * wallFactor);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, ramHitRadius);
    }
}
