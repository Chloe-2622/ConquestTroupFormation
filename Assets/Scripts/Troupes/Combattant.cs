using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Combattant : Troup
{
    [Header("Combattant properties")]
    [SerializeField] private float enragedTime;
    [SerializeField] private float enragedSpeed;
    [SerializeField] private float enragedAttackRechargeTime;

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
        agent.speed = enragedSpeed;

        float defaultRechargeTime = attackRechargeTime;
        attackRechargeTime = enragedAttackRechargeTime;

        float elapsedTime = 0f;
        while (elapsedTime < enragedTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / enragedTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        agent.speed = movingSpeed;
        attackRechargeTime = defaultRechargeTime;
        Debug.Log("Combattant special ability ended");

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }
}
