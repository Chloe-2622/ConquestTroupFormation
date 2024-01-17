using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guerisseur : Troup
{

    [SerializeField] private float radius;
    [SerializeField] private LayerMask tombeMask;

    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override IEnumerator Attack(Troup ennemy)
    {
        while (ennemy != null && Vector3.Distance(transform.position, ennemy.transform.position) <= attackRange)
        {
            ennemy.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Guerisseur ability activated");

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, tombeMask);
        

        foreach(Collider collider in colliders)
        {
            Debug.Log(collider);
            
            if (collider.GetComponent<Tombe>().tombeTroupType == Tombe.TombeTroupType.Ally)
            {
                collider.GetComponent<Tombe>().Revive();
            }
        }

        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
