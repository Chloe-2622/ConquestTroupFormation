using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Porte_etendard : Troup
{

    [Header("------------------ Porte étendard ------------------ ")]
    [Header("General stats")]
    [SerializeField] private float damageBoost;
    [SerializeField] private float attackSpeedBoost;
    [SerializeField] private float zoneRadius;
    [SerializeField] private float IAminimumTroupToBoost;

    [Header("Special ability parameters")]
    [SerializeField] private GameObject etendardPrefab;

    // Private variables
    private bool isBoosting;
    HashSet<Troup> troupToBoost = new HashSet<Troup>();


    // Main Functions ---------------------------------------------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();

        if (troupType == TroupType.Enemy) { IAminimumTroupToBoost = 3; }
    }

    protected override void Update()
    {
        base.Update();

        BoostBehaviour();
        if (troupType == TroupType.Enemy && currentFollowedTroup == null && currentAttackedTroup == null) { IAEnemy(); }
    }

    private void OnDestroy()
    {
        foreach (Troup troup in troupToBoost)
        {
            troup.ActivateBoostParticle(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
    }

    // Attack and ability -----------------------------------------------------------------------------------------
    protected override IEnumerator Attack(Troup enemy)
    {
        // Le porte étendard n'attaque pas
        yield return null;
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
                    // troup.ActivateBoostParticle(true);
                }
            }
            else
            {
                if (troupToBoost.Contains(troup))
                {
                    troupToBoost.Remove(troup);
                    troup.AddDamage(-damageBoost);
                    troup.ChangeAttackSpeed(1 / attackSpeedBoost);
                    // troup.ActivateBoostParticle(false);
                }
            }

            if (!troup.IsBoosted())
            {
                troup.ActivateBoostParticle(troupToBoost.Contains(troup));
            }

        }


    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Porte étendard special ability activated");

        GameObject etendard = Instantiate(etendardPrefab, transform.position + transform.forward + - .2f * transform.up, Quaternion.identity, null);
        etendard.GetComponent<Etendard>().isPlaced = true;

        isBoosting = true;

        abilityBar.fillAmount = 0f;
        specialAbilityDelay = -1;

        yield return null;
    }

    // IA Enemy ---------------------------------------------------------------------------------------------------
    protected override void IAEnemy() 
    {
        bool canBoost = false;

        float totalHealth = 0f;
        float totalMaxHealth = 0f;
        HashSet<Troup> troupToCheck = troupType == TroupType.Ally ? GameManager.Instance.getAllies() : GameManager.Instance.getEnemies();
        Collider[] detectedColliders = Physics.OverlapSphere(transform.position, zoneRadius, troupMask);
        foreach (Collider detectedCollider in detectedColliders)
        {
            Troup detectedTroup = detectedCollider.GetComponent<Troup>();
            if (detectedTroup != null && detectedTroup.gameObject != gameObject && detectedTroup.troupType == TroupType.Enemy)
            {
                troupToCheck.Add(detectedTroup);
                totalHealth += detectedTroup.getHealth();
                totalMaxHealth += detectedTroup.getMaxHealth();
            }
        }

        if (troupToCheck.Count >= IAminimumTroupToBoost) { canBoost = totalHealth < .5f * totalMaxHealth; }

        if (!isBoosting && canBoost) { StartCoroutine(SpecialAbility()); }

        if (health <= (maxHealth / 2) && specialAbilityDelay == 0)
        {
            IAminimumTroupToBoost = 1;
        }

        Vector3 center = new Vector3(0, 0, 0);
        int count = 0;

        HashSet<Troup> enemies = gameManager.getEnemies();

        foreach (Troup troup in enemies)
        {
            if (troup != null && Vector3.Distance(transform.position, troup.transform.position) < 10) { center += troup.transform.position; count++; }

        }
        center /= count;

        actionQueue.Enqueue(new MoveToPosition(agent, center, positionThreshold));
    }

}
