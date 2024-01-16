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
    }

    protected override IEnumerator Attack(Troup ennemy)
    {
        while (ennemy != null)
        {
            ennemy.TakeDamage(attackDamage);
            if (ennemy.getHealth() == 0) { Debug.Log("Ennemie tu�"); }
            Debug.Log("Attacking ennemy with " + ennemy.getHealth());
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Combattant special ability activated");
        yield return null;
    }
}
