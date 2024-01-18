using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cavalier : Troup
{
    [Header("Cavalier properties")]
    [SerializeField] private float chargeTime;
    [SerializeField] private float chargeSpeed;

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

        agent.speed = chargeSpeed;

        float elapsedTime = 0f;
        while (elapsedTime < chargeTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / chargeTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        agent.speed = movingSpeed;

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }
}
