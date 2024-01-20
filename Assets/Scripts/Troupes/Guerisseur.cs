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
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        HealBehaviour();
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
                if (ally != this && Vector3.Distance(transform.position, ally.transform.position) <= healRange)
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
                if (enemy != this && Vector3.Distance(transform.position, enemy.transform.position) <= healRange)
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
            transform.LookAt(currentHealedAlly.transform.position);
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
        
        while (troup.gameObject == null || troup.IsInjured())
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
        target.Heal(healAmount);
        Destroy(effect);

    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Guerisseur ability activated");

        Collider[] colliders = Physics.OverlapSphere(transform.position, resurrectionRadius, tombeMask);
        

        foreach(Collider collider in colliders)
        {
            Debug.Log(collider);
            
            if (troupType == TroupType.Ally && collider.GetComponent<Tombe>().tombeTroupType == Tombe.TombeTroupType.Ally)
            {
                collider.GetComponent<Tombe>().Revive();
            }
            if (troupType == TroupType.Enemy && collider.GetComponent<Tombe>().tombeTroupType == Tombe.TombeTroupType.Enemy)
            {
                collider.GetComponent<Tombe>().Revive();
            }
        }

        if (colliders.Length != 0)
        {
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
