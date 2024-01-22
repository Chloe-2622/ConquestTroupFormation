using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Batisseur : Troup
{
    [Header("Batisseur properties")]
    [SerializeField] private float swingTime;
    [SerializeField] private float wallRechargeTime;

    private GameObject hammer;
    

    protected override void Awake()
    {
        base.Awake();

        hammer = transform.Find("Hammer").gameObject;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        swingTime = attackRechargeTime / 2;

        AttackBehaviour();
        BuildBehaviour();
    }

    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null)
        {
            StartCoroutine(SwingHammer());
            enemy.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected void BuildBehaviour()
    {
        // A Ecrire
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Batisseur special ability activated");

        float elapsedTime = 0f;
        while (elapsedTime < wallRechargeTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / wallRechargeTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }

    private IEnumerator SwingHammer()
    {
        float timer = 0f;
        Debug.Log("I am swinging hammer");

        while (timer < swingTime / 2)
        {
            timer += Time.deltaTime;
            hammer.transform.RotateAround(hammer.transform.position, hammer.transform.right, 90 * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingR : " + hammer.transform.localEulerAngles.x);

            yield return null;
        }
        hammer.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        Debug.Log("swingRL");
        while (timer < swingTime)
        {
            timer += Time.deltaTime;
            hammer.transform.RotateAround(hammer.transform.position, hammer.transform.right, -90 * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingL : " + hammer.transform.localEulerAngles.x);

            yield return null;
        }
        hammer.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        
    }
}
