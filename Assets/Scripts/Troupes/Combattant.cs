using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Combattant : Troup
{

    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        AttackBehaviour();
    }

    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null)
        {
            enemy.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Combattant special ability activated");
        yield return null;
    }
}
