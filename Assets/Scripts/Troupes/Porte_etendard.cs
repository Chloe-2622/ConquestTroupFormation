using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Porte_etendard : Troup
{
    [Header("Combattant properties")]
    [SerializeField] private float damageBoost;
    [SerializeField] private float attackSpeedBoost;
    [SerializeField] private float zoneRadius;
    [SerializeField] private GameObject etendardPrefab;

    HashSet<Troup> troupToBoost = new HashSet<Troup>();

    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        BoostBehaviour();
    }

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

    private void BoostBehaviour()
    {
        

        HashSet<Troup> troupToCheck = troupType == TroupType.Ally ? GameManager.Instance.getAllies() : GameManager.Instance.getEnemies();
        foreach (Troup troup in troupToCheck)
        {
            if (Vector3.Distance(transform.position, troup.transform.position) <= zoneRadius)
            {
                if (!troupToBoost.Contains(troup))
                {
                    troupToBoost.Add(troup);
                    troup.AddDamage(damageBoost);
                    troup.ChangeAttackSpeed(attackSpeedBoost);
                    troup.ActivateBoostParticle(true);
                }
            }
            else
            {
                if (troupToBoost.Contains(troup))
                {
                    troupToBoost.Remove(troup);
                    troup.AddDamage(-damageBoost);
                    troup.ChangeAttackSpeed(1 / attackSpeedBoost);
                    troup.ActivateBoostParticle(false);
                }
            }
        }

    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Porte étendard special ability activated");

        GameObject etendard = Instantiate(etendardPrefab, transform.position + transform.forward + - .2f * transform.up, Quaternion.identity, null);
        etendard.GetComponent<Etendard>().isPlaced = true;

        specialAbilityDelay = -1;

        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
    }
}
