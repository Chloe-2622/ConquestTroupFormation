using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guerisseur : Troup
{
    [Header("Guerisseur properties")]
    [SerializeField] private float healAmount;
    [SerializeField] private float healRange;
    [SerializeField] private float healRechargeTime;
    [SerializeField] private float resurrectionRadius;
    [SerializeField] private LayerMask tombeMask;
    [SerializeField] private GameObject healEffect;
    [SerializeField] private GameObject healEffectSpawnPoint;
    [SerializeField] float healEffectSpeed;

    [SerializeField] private int IAminimumTroupToRevive;

    private IEnumerator healCoroutine;

    

    [Header("Debug Variables")]
    [SerializeField] private GameObject closestInjuredAllyDetected;
    [SerializeField] private bool isFollowingAlly;
    [SerializeField] private bool isHealing;
    [SerializeField] private GameObject currentFollowedAlly;
    [SerializeField] private GameObject currentHealedAlly;


    protected override void Awake()
    {
        base.Awake();

        IAminimumTroupToRevive = troupType == TroupType.Ally ? 1 : 3;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        HealBehaviour();

        if (troupType == TroupType.Enemy && currentFollowedTroup == null && currentAttackedTroup == null) { IAEnemy(); }
    }

    protected override void IAEnemy() 
    {

        StartCoroutine(SpecialAbility());

        if (health <= (maxHealth / 2) && specialAbilityDelay == 0)
        {
            IAminimumTroupToRevive = 1;
        }

        if (timeBeforeNextAction == 0f && currentFollowedTroup == null && currentAttackedTroup == null)
        {
            actionQueue.Enqueue(new FollowUnit(agent, RandomTroupInTeam(gameManager.getEnemies(), 10f).gameObject));

            timeBeforeNextAction = Random.Range(5f, 10f);
            StartCoroutine(IAactionCountdown());
        }
    }

    protected override IEnumerator Attack(Troup enemy)
    {
        yield return null;
    }

    private void HealBehaviour()
    {
        // Find all allies in detection range
        HashSet<GameObject> detectedInjuredAllies = new HashSet<GameObject>();

        if (troupType == TroupType.Ally)
        {
            HashSet<Troup> allies = GameManager.Instance.getAllies();
            float closestDistance = Mathf.Infinity;
            closestInjuredAllyDetected = null;
            foreach (Troup ally in allies)
            {
                float distance = Vector3.Distance(transform.position, ally.transform.position);

                if (ally != this && distance <= detectionRange && ally.GetComponent<Troup>().IsInjured())
                {
                    detectedInjuredAllies.Add(ally.gameObject);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInjuredAllyDetected = ally.gameObject;
                    }
                }
            }            
        } 
        else
        { 
            HashSet<Troup> enemies = GameManager.Instance.getEnemies();
            float closestDistance = Mathf.Infinity;
            closestInjuredAllyDetected = null;
            foreach (Troup enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (enemy != this && distance <= detectionRange && enemy.GetComponent<Troup>().IsInjured())
                {
                    detectedInjuredAllies.Add(enemy.gameObject);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInjuredAllyDetected = enemy.gameObject;
                    }
                }
            }
        }

        if ((currentFollowedAlly != closestInjuredAllyDetected && !isFollowingAlly) || (currentFollowedAlly != null && closestInjuredAllyDetected != null && currentFollowedAlly != closestInjuredAllyDetected))
        {
            isFollowingAlly = true;
            currentFollowedAlly = closestInjuredAllyDetected;

            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();

            Debug.Log("Getting closer to enemy : " + currentFollowedAlly);
            StopCoroutine(currentActionCoroutine);
            actionQueue.Enqueue(new MoveToUnit(agent, currentFollowedAlly, healRange));
            StartCoroutine(currentActionCoroutine);
        }
        if (currentFollowedAlly != null && Vector3.Distance(transform.position, currentFollowedAlly.transform.position) >= detectionRange)
        {
            currentFollowedAlly = null;
        }
        if (currentFollowedAlly == null)
        {
            isFollowingAlly = false;
        }


        HashSet<GameObject> inRangeAllies = new HashSet<GameObject>();
        if (troupType == TroupType.Ally)
        {
            HashSet<Troup> allies = GameManager.Instance.getAllies();
            foreach (var ally in allies)
            {
                if (ally != this && Vector3.Distance(transform.position, ally.transform.position) <= healRange && ally.IsInjured())
                {
                    inRangeAllies.Add(ally.gameObject);
                }
            }
        }
        else
        {
            HashSet<Troup> enemies = GameManager.Instance.getEnemies();
            foreach (var enemy in enemies)
            {
                if (enemy != this && Vector3.Distance(transform.position, enemy.transform.position) <= healRange && enemy.IsInjured())
                {
                    inRangeAllies.Add(enemy.gameObject);
                }
            }
        }

        float closestDistanceInRange = Mathf.Infinity;
        GameObject closestAllyInRange = null;

        foreach (GameObject ally in inRangeAllies)
        {
            float distance = Vector3.Distance(transform.position, ally.transform.position);

            if (distance <= closestDistanceInRange)
            {
                closestDistanceInRange = distance;
                closestAllyInRange = ally;
            }
        }

        Debug.Log("ClosestDistanceInRange : " + closestDistanceInRange);

        if (currentHealedAlly == null && !isFollowingOrders)
        {
            isHealing = false;
            if (healCoroutine != null)
            {
                StopCoroutine(healCoroutine);
            }

            if (closestAllyInRange != null)
            {
                currentHealedAlly = closestAllyInRange;

                actionQueue.Clear();
                agent.isStopped = true;
                agent.ResetPath();
                Debug.Log("Stopping path");

                StopCoroutine(currentActionCoroutine);
                actionQueue.Enqueue(new Standby());
                StartCoroutine(currentActionCoroutine);

                StartCoroutine(TurnTo(currentHealedAlly.transform.position));
                healCoroutine = Heal(currentHealedAlly.GetComponent<Troup>());
                StartCoroutine(healCoroutine);
                isHealing = true;
            }
        }

        if (closestAllyInRange == null)
        {
            currentHealedAlly = null;
            isHealing = false;
        }

        if (currentHealedAlly != null)
        {
            Vector3 targetPosition = currentHealedAlly.transform.position;
            targetPosition.y = transform.position.y;  // Keep the same y position as the object you are rotating

            // transform.LookAt(targetPosition);
        }

        if (currentHealedAlly != null && !currentHealedAlly.GetComponent<Troup>().IsInjured())
        {
            currentHealedAlly = null;
            currentFollowedAlly = null;
            isFollowingAlly = false;
        }

    }

    private IEnumerator Heal(Troup troup)
    {
        
        while (troup.gameObject != null || troup.IsInjured())
        {
            
            GameObject currentHealEffect = Instantiate(healEffect, healEffectSpawnPoint.transform);
            StartCoroutine(HealEffectAnimation(troup, currentHealEffect, healEffectSpawnPoint.transform.position, troup.gameObject.transform.position + new Vector3(0, 1f, 0), healEffectSpeed));
            
            yield return new WaitForSeconds(healRechargeTime);
        }
        yield return null;
    }

    private IEnumerator HealEffectAnimation(Troup target, GameObject effect, Vector3 spawnPoint, Vector3 endpoint, float healEffectSpeed)
    {
        effect.transform.position = spawnPoint;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / healEffectSpeed;
            effect.transform.position = Vector3.Lerp(spawnPoint, endpoint, t);
            yield return null;
        }

        effect.transform.position = endpoint;
        if (target != null)
        {
            target.Heal(healAmount);
        }
        
        Destroy(effect);

    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Guerisseur ability activated");

        Collider[] colliders = Physics.OverlapSphere(transform.position, resurrectionRadius, tombeMask);

        HashSet<Tombe> troupToRevive = new HashSet<Tombe>();

        foreach(Collider collider in colliders)
        {
            Debug.Log(collider);
            
            if (troupType == TroupType.Ally && collider.GetComponent<Tombe>().tombeTroupType == Tombe.TombeTroupType.Ally)
            {
                troupToRevive.Add(collider.GetComponent<Tombe>());
            }
            if (troupType == TroupType.Enemy && collider.GetComponent<Tombe>().tombeTroupType == Tombe.TombeTroupType.Enemy)
            {
                troupToRevive.Add(collider.GetComponent<Tombe>());
            }
        }

        if (troupToRevive.Count >= IAminimumTroupToRevive)
        {
            foreach (Tombe tombe in troupToRevive)
            {
                if (!tombe.HasRevived()) { tombe.Revive(); }
            }

            abilityBar.fillAmount = 0f;
            specialAbilityDelay = -1;
        }

        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, resurrectionRadius);
    }
}
