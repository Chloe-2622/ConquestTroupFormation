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
    [SerializeField] private float IAminimumTroupToBoost;
    [SerializeField] private GameObject etendardPrefab;

    [SerializeField] private bool isBoosting;

    HashSet<Troup> troupToBoost = new HashSet<Troup>();

    protected override void Awake()
    {
        base.Awake();

        if (troupType == TroupType.Enemy) { IAminimumTroupToBoost = 3; }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        BoostBehaviour();
        if (troupType == TroupType.Enemy && currentFollowedTroup == null && currentAttackedTroup == null) { IAEnemy(); }
    }

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

        float minX = Mathf.Infinity;
        foreach (Troup ally in gameManager.getEnemies())
        {
            if (ally.unitType != UnitType.Porte_bouclier && ally.unitType != UnitType.Guerisseur) { minX = Mathf.Min(minX, ally.transform.position.x); }
        }

        if (timeBeforeNextAction == 0f && currentFollowedTroup == null && currentAttackedTroup == null)
        {
            int nextActionIndex = Random.Range(0, 2);

            if (nextActionIndex == 0)
            {
                actionQueue.Enqueue(new MoveToPosition(agent, new Vector3(minX - 5f, transform.position.y, gameManager.CrownPosition.transform.position.z + Random.Range(-10f, 10f)), positionThreshold));
            }
            else
            {
                Vector3 pos1 = new Vector3(minX - 5f, transform.position.y, gameManager.CrownPosition.transform.position.z + Random.Range(-10f, 10f));
                Vector3 pos2 = new Vector3(minX - 5f, transform.position.y, gameManager.CrownPosition.transform.position.z + Random.Range(-10f, 10f));
                actionQueue.Enqueue(new Patrol(agent, pos1, pos2));
            }

            timeBeforeNextAction = Random.Range(5f, 10f);
            StartCoroutine(IAactionCountdown());
        }
    }

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

        abilityBar.fillAmount = 0f;
        specialAbilityDelay = -1;

        yield return null;
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
}
