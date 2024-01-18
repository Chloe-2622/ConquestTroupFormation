using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cavalier : Troup
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

    protected override IEnumerator Attack(Troup ennemy)
    {
        while (ennemy != null)
        {
            ennemy.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Cavalier special ability activated");
        yield return null;
    }
}