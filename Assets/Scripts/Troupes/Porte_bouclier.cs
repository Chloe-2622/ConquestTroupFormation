using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Porte_bouclier : Troup
{
    [Header("Combattant properties")]
    [SerializeField] private float armureBoost;
    [SerializeField] private float boostTime;
    [SerializeField] private float boostRadius;

    HashSet<Troup> troupToBoost = new HashSet<Troup>();

    protected override void Awake()
    {
        base.Awake();

        positionThreshold = 2f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        AttackBehaviour();
        if (troupType == TroupType.Enemy && currentFollowedTroup == null && currentAttackedTroup == null) { IAEnemy(); }
    }

    protected override void IAEnemy() { }

    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null)
        {
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
        Debug.Log("Porte_etendard special ability activated");

        troupToBoost = new HashSet<Troup>();

        HashSet<Troup> troupToCheck = troupType == TroupType.Ally ? GameManager.Instance.getAllies() : GameManager.Instance.getEnemies();
        foreach (Troup troup in troupToCheck)
        {
            if (Vector3.Distance(transform.position, troup.transform.position) <= boostRadius && troup != this)
            {
                troupToBoost.Add(troup);
                Debug.Log("Adding troup : " + troup);
                troup.AddArmor(armureBoost);
                troup.ActivateArmorBoostParticle(true);
            }
        }

        float elapsedTime = 0f;
        while (elapsedTime < boostTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / boostTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        foreach (Troup troup in troupToBoost)
        {
            troup.AddArmor(-armureBoost);
            troup.ActivateArmorBoostParticle(false);
            Debug.Log("Desaction de l'armur pour : " + troup.gameObject);
        }

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }
}
