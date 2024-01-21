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
    [SerializeField] private float swingTime;

    private GameObject sword;
    

    protected override void Awake()
    {
        base.Awake();

        sword = transform.Find("Sword").gameObject;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        swingTime = attackRechargeTime / 2;

        AttackBehaviour();
    }

    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null)
        {
            StartCoroutine(SwingSword());
            if (enemy.unitType == UnitType.Archer)
            {
                enemy.TakeDamage(2 * attackDamage);
            }
            else
            {
                enemy.TakeDamage(attackDamage);
            }
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

    private IEnumerator SwingSword()
    {
        float timer = 0f;
        Debug.Log("I am swinging sword");

        while (timer < swingTime / 2)
        {
            timer += Time.deltaTime;
            sword.transform.RotateAround(sword.transform.position, sword.transform.right, 90 * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingR : " + sword.transform.localEulerAngles.x);

            yield return null;
        }
        sword.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        Debug.Log("swingRL");
        while (timer < swingTime)
        {
            timer += Time.deltaTime;
            sword.transform.RotateAround(sword.transform.position, sword.transform.right, -90 * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingL : " + sword.transform.localEulerAngles.x);

            yield return null;
        }
        sword.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        
    }
}
