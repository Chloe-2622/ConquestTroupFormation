using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{

    [Header("Boulder stats")]
    [SerializeField] private bool oneShot;
    [SerializeField] private float damage;
    [SerializeField] private float hitRadius;
    public BoulderTroupType boulderTroupType;

    HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    public enum BoulderTroupType { Ally, Enemy }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, hitRadius);

        foreach (Collider collider in colliders)
        {
            Troup troup = collider.GetComponent<Troup>();

            // Debug.Log("Touchéééé : " + collider.gameObject);

            if (boulderTroupType == BoulderTroupType.Ally)
            {
                if (troup != null && troup.troupType == Troup.TroupType.Enemy)
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
                            Debug.Log("Touchéééé : " + collider.gameObject);
                            troup.TakeDamage(damage);
                        }
                    }
                }
            } else
            {
                if (troup != null && troup.troupType == Troup.TroupType.Ally)
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
                            Debug.Log("Touchéééé : " + collider.gameObject);
                            troup.TakeDamage(damage);
                        }
                    }
                }
            }

            
        }
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
