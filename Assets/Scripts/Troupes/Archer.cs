using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Archer : Troup
{
    [Header("Archer properties")]
    [SerializeField] private float invisibleTime;
    [SerializeField] private Material invisibleMaterial;

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
        Debug.Log("Archer special ability activated");

        isVisible = false;
        Material defaultMaterial = transform.Find("Capsule").gameObject.GetComponent<Renderer>().material;
        transform.Find("Capsule").gameObject.GetComponent<Renderer>().material = invisibleMaterial;

        float elapsedTime = 0f;
        while (elapsedTime < invisibleTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / invisibleTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        isVisible = true;
        transform.Find("Capsule").gameObject.GetComponent<Renderer>().material = defaultMaterial;

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }
}
