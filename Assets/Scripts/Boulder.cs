using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Boulder : MonoBehaviour
{

    [Header("Boulder stats")]
    [SerializeField] private bool oneShot;
    [SerializeField] private float damage;
    [SerializeField] private float hitRadius;
    [SerializeField] private float touchGroundThreshold;
    public Troup.TroupType boulderType;

    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
    private LayerMask floorMask;

    public void OnEnable()
    {
        floorMask = GameManager.Instance.floorMask;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, hitRadius);

        foreach (Collider collider in colliders)
        {
            Troup troup = collider.GetComponent<Troup>();
            if (troup != null && troup.troupType != boulderType)
            {
                if (oneShot)
                {
                    troup.TakeDamage(Mathf.Infinity);
                }
                else
                {
                    if (!hitObjects.Contains(collider.gameObject))
                    {
                        hitObjects.Add(collider.gameObject);
                        Debug.Log("**** Touchéééé : " + collider.gameObject);
                        troup.TakeDamage(damage);
                    }
                }
            }
        }

        Ray ray = new Ray(transform.position, -Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
        {
            if (Vector3.Distance(hit.point, transform.position) < touchGroundThreshold) { GameObject.Destroy(gameObject); }
        }
        else { GameObject.Destroy(gameObject); }
    }

    public void OneShotMode(bool activate)
    {
        oneShot = activate;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
