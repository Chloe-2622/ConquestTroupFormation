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
    [SerializeField] private float IAminimumTroupToBoost;
    [SerializeField] private float bashTime;

    [SerializeField] private bool isBoosting;
    Vector3 shieldDefaultPosition;

    private GameObject shield;

    HashSet<Troup> troupToBoost = new HashSet<Troup>();

    protected override void Awake()
    {
        base.Awake();

        timeBeforeNextAction = 0f;
        shield = transform.Find("Shield").gameObject;
        

        if (troupType == TroupType.Enemy) { IAminimumTroupToBoost = 5; }
        
        positionThreshold = 2f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        AttackBehaviour();
        if (troupType == TroupType.Enemy) { IAEnemy(); }
    }

    protected override void IAEnemy() 
    {
        bool canBoost = false;

        float totalHealth = 0f;
        float totalMaxHealth = 0f;
        HashSet<Troup> troupToCheck = troupType == TroupType.Ally ? GameManager.Instance.getAllies() : GameManager.Instance.getEnemies();
        foreach (Troup troup in troupToCheck)
        {
            totalHealth += troup.getHealth();
            totalMaxHealth += troup.getMaxHealth();
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
            shieldDefaultPosition = transform.position + transform.forward * 0.306f;
            StartCoroutine(BashShield());
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
                
            }
        }

        if (troupToBoost.Count >= IAminimumTroupToBoost)
        {
            foreach (Troup troup in troupToBoost) {
                troup.AddArmor(armureBoost);
                troup.ActivateArmorBoostParticle(true);
            }

            isBoosting = true;

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

            isBoosting = false;

            troupToBoost = new HashSet<Troup>();

            specialAbilityDelay = specialAbilityRechargeTime;
            StartCoroutine(SpecialAbilityCountdown());
        }
    }

    private IEnumerator BashShield()
    {
        float t = 0f;
        shield.transform.position = shieldDefaultPosition;
        while (t < 1f)
        {
            t += Time.deltaTime / (bashTime / 2);
            shield.transform.position = Vector3.Lerp(shieldDefaultPosition, shieldDefaultPosition + .3f * transform.forward, t);
            yield return null;
        }
        shield.transform.position = shieldDefaultPosition + .3f * transform.forward;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (bashTime / 2);
            shield.transform.position = Vector3.Lerp(shieldDefaultPosition + .3f * transform.forward, shieldDefaultPosition, t);
            yield return null;
        }
        shield.transform.position = shieldDefaultPosition;
    }

    private void OnDestroy()
    {
        foreach (Troup troup in troupToBoost)
        {
            troup.AddArmor(-armureBoost);
            troup.ActivateArmorBoostParticle(false);
            Debug.Log("Desaction de l'armur pour : " + troup.gameObject);
        }
    }
}
