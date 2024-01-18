using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guerisseur : Troup
{
    [Header("Guerisseur properties")]
    [SerializeField] private float resurrectionRadius;
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

    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= attackRange)
        {
            enemy.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Guerisseur ability activated");

        Collider[] colliders = Physics.OverlapSphere(transform.position, resurrectionRadius, tombeMask);
        

        foreach(Collider collider in colliders)
        {
            Debug.Log(collider);
            
            if (collider.GetComponent<Tombe>().tombeTroupType == Tombe.TombeTroupType.Ally)
            {
                collider.GetComponent<Tombe>().Revive();
            }
        }

        if (colliders.Length != 0)
        {
            abilityBar.fillAmount = 0f;
            specialAbilityDelay = Mathf.Infinity;
        }

        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, resurrectionRadius);
    }
}
