using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public abstract class Troup : MonoBehaviour
{

    /* 
     TODO : 

                Carte avec terrain mieux, plus belle
        -DONE-  Couronne (modèle + récupérable + qui flotte)
        -DONE-  Roi (comportement des unités si Roi)
                Modèle des unités
                Animation basique des unités
                Ecran menu principal (blender avec mise en scène non contractuelle des modèles)
                Finir les unités : 
	    -DONE-      - Catapulte
	    -DONE-      - Porte-étendard
	    -DONE-      - Porte-bouclier
                Capa spécial bélier
                Catapulte avec queue (si le temps)   
                SFX basiques

    */


    [Header("General stats")]
    [SerializeField] public TroupType troupType;
    [SerializeField] public UnitType unitType;
    [SerializeField] protected bool isSelected;
    [SerializeField] protected bool hasCrown = false;
    [SerializeField] protected int goldCost;
    [SerializeField] protected float movingSpeed;
    [SerializeField] protected float health;
    [SerializeField] protected float armor;
    [SerializeField] protected float detectionRange;
    [SerializeField] protected float positionThreshold;

    [Header("Attack stats")]
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackRechargeTime;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float specialAbilityRechargeTime;

    // Private variables ------------------------------------------------------------------------------------------
    [Header("Debug Variables")]
    [SerializeField] private bool isChosingPatrol;
    [SerializeField] private bool isFollowingEnemy;
    [SerializeField] private bool isAttackingEnemy;
    [SerializeField] private bool isPatroling;
    [SerializeField] protected GameObject currentAttackedTroup;
    [SerializeField] private GameObject currentFollowedTroup;
    [SerializeField] protected float specialAbilityDelay = 0f;
    [SerializeField] protected bool isVisible = true;
    [SerializeField] protected bool isAddedWhenAwake = false;
    [SerializeField] public Vector3 moveTargetDestination;
    private bool isPlayingCircleAnim;
    private bool isChosingPlacement;
    private bool isChosingFollow;
    protected bool isFollowingOrders;
    private bool hasSpawnedTombe;
    private bool isMovingToKing;
    private bool isBoosted;
    private float maxHealth;
    protected IEnumerator currentActionCoroutine;
    private IEnumerator attackCoroutine;

    // Scene objects
    protected Camera camera1;
    protected SelectionManager selectionManager;
    protected GameObject selectionArrow;
    protected GameObject tombe;
    protected Transform SelectionCircle;
    protected Transform SelectionArrow;
    protected GameObject FirstPatrolPoint;
    protected GameObject SecondPatrolPoint;
    protected GameObject SelectionParticleCircle;
    protected GameObject BoostParticle;
    protected GameObject ArmorBoostParticle;
    protected GameObject QueueUI;
    protected NavMeshAgent agent;
    protected LayerMask troupMask;

    // UI elements
    protected TextMeshProUGUI PlaceSelectionPopUp;
    protected TextMeshProUGUI PatrolSelectionPopUp1;
    protected TextMeshProUGUI PatrolSelectionPopUp2;
    protected TextMeshProUGUI FollowSelectionPopUp;
    protected Image healthBar;
    protected Image abilityBar;
    protected GameObject unitBar;
    protected GameObject Bars;


    private GameManager gameManager;

    // Troup types ------------------------------------------------------------------------------------------------
    public enum TroupType { Ally, Enemy }
    public enum UnitType
    {
        Null, Combattant, Archer, Cavalier, Guerisseur, Catapulte, Porte_bouclier, Porte_etendard, Batisseur, Belier
    }

    // Action Queue -----------------------------------------------------------------------------------------------
    protected Queue<IAction> actionQueue = new Queue<IAction>();

    // Main Functions ---------------------------------------------------------------------------------------------

    protected virtual void Awake() 
    {
        if (troupType == TroupType.Ally)
        {
            QueueUI = transform.Find("Canvas").Find("Queue").gameObject;
        }

        // Setup and show Health Bar
        maxHealth = health;

        // Agent setup
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movingSpeed;

        // GameManager variable setup
        gameManager = GameManager.Instance;

        Debug.Log("GameManager of " + troupType + " " + unitType + " = " + gameManager);
        camera1 = gameManager.mainCamera;
        selectionManager = gameManager.selectionManager;
        SelectionArrow = gameManager.selectionArrow;
        tombe = gameManager.tombe;
        PlaceSelectionPopUp = gameManager.PlaceSelectionPopUp;
        PatrolSelectionPopUp1 = gameManager.PatrolSelectionPopUp1;
        PatrolSelectionPopUp2 = gameManager.PatrolSelectionPopUp2;
        FollowSelectionPopUp = gameManager.FollowSelectionPopUp;
        FirstPatrolPoint = Instantiate(gameManager.FirstPatrolPointPrefab, gameManager.PatrolingCircles.transform);
        SecondPatrolPoint = Instantiate(gameManager.SecondPatrolPointPrefab, gameManager.PatrolingCircles.transform);
        SelectionParticleCircle = Instantiate(gameManager.SelectionParticleCirclePrefab, gameManager.SelectionParticleCircles.transform);
        BoostParticle = Instantiate(gameManager.BoostParticleEffectPrefab, gameManager.BoostParticles.transform);
        ArmorBoostParticle = Instantiate(gameManager.ArmorBoostParticleEffectPrefab, gameManager.BoostParticles.transform);

        unitBar = Instantiate(gameManager.unitBarsPrefab, gameManager.UI.bars.transform);
        healthBar = unitBar.transform.GetChild(0).GetComponent<Image>();
        abilityBar = unitBar.transform.GetChild(1).GetComponent<Image>();

        troupMask = gameManager.troupMask;

        // Start Action Queue
        currentActionCoroutine = ExecuteActionQueue();
        AddAction(new Standby());
        StartCoroutine(currentActionCoroutine);

        if (troupType == TroupType.Enemy)
        {
            gameManager.addEnemy(this);
        }
        if (isAddedWhenAwake)
        {
            Debug.Log("!! Unit added");
            addToGroup();
        }
    }

    // Ally or Enemy
    public void addToGroup()
    {
        if (troupType == TroupType.Ally)
        {
            gameManager.addAlly(this);
            selectionManager.completeDictionnary(transform.gameObject);
        }
        if (troupType == TroupType.Enemy)
        {
            gameManager.addEnemy(this);
        }
    }

    public void OnDisable()
    {
        if (healthBar != null)
        {
            healthBar.enabled = false;
        }
        if (abilityBar != null)
        {
            abilityBar.enabled = false;
        }
        if (BoostParticle != null)
        {
            BoostParticle.SetActive(false);
        }
        if (ArmorBoostParticle != null)
        {
            ArmorBoostParticle.SetActive(false);
        }
        if (SelectionParticleCircle != null)
        {
            GameObject.Destroy(SelectionParticleCircle);
        }
        
    }

    // Get stats
    public int getCost() { return goldCost; }
    public float getHealth() { return health; }
    public float getArmor() { return armor; }
    public float getSpeed() { return movingSpeed; }
    public float getAttack() { return attackDamage; }
    public float getAttackSpeed() { return attackRechargeTime; }
    public float getAttackRange() { return attackRange; }
    public float getAbilityRecharge() { return specialAbilityRechargeTime; }
    public bool isKing() { return hasCrown; }

    // Update
    protected virtual void Update()
    {
        if (isFollowingOrders)
        {
            isFollowingEnemy = false;
            isAttackingEnemy = false;
        }

        if (troupType == TroupType.Ally)
        {
            if (transform.Find("Crown").gameObject.activeSelf == true)
            {
                hasCrown = true;
                gameManager.isCrownCollected = true;
                gameManager.king = gameObject;
            }
        }

        if (BoostParticle != null)
        {
            if (BoostParticle.activeSelf)
            {
                BoostParticle.transform.position = transform.position;
            }
            if (ArmorBoostParticle.activeSelf)
            {
                ArmorBoostParticle.transform.position = transform.position;
            }
        }        

        SelectedBehaviour();

        HealthBarControl();
        AbilityBarControl();

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

    }

    protected virtual void SelectedBehaviour()
    {
        isSelected = selectionManager.isSelected(this.gameObject);

        if (selectionManager.isSelected(this.gameObject))
        {
            SelectionParticleCircle.SetActive(true);
            if (!isPlayingCircleAnim)
            {
                SelectionParticleCircle.GetComponent<ParticleSystem>().Play();
                isPlayingCircleAnim = true;
            }

            SelectionParticleCircle.transform.position = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);


            if (troupType == TroupType.Ally)
            {
                if (selectionManager.numberOfSelected() == 1)
                {
                    QueueUI.SetActive(true);
                }
                else
                {
                    QueueUI.SetActive(false);
                }
            }



            if (!isChosingPatrol)
            {
                if (isPatroling)
                {
                    FirstPatrolPoint.GetComponent<Renderer>().enabled = true;
                    SecondPatrolPoint.GetComponent<Renderer>().enabled = true;
                }
                else
                {
                    FirstPatrolPoint.GetComponent<Renderer>().enabled = false;
                    SecondPatrolPoint.GetComponent<Renderer>().enabled = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
            {
                StopCoroutine(PlaceSelection());
                StopCoroutine(PatrolSelection());
                StopCoroutine(FollowSelection());
                PlaceSelectionPopUp.enabled = false;
                PatrolSelectionPopUp1.enabled = false;
                PatrolSelectionPopUp2.enabled = false;
                FollowSelectionPopUp.enabled = false;
                isChosingPlacement = false;
                isChosingPatrol = false;
                isChosingFollow = false;
            }

            // Placement input
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                isChosingPlacement = true;
                StartCoroutine(PlaceSelection());
            }

            // Patrol input
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                isChosingPatrol = true;
                StartCoroutine(PatrolSelection());
            }

            // Follow input
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                isChosingFollow = true;
                StartCoroutine(FollowSelection());
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AddAction(new Standby());
            }

            if (Input.GetKeyDown(KeyCode.F) && specialAbilityDelay == 0)
            {
                StartCoroutine(SpecialAbility());
                specialAbilityDelay = -1f;
            }

        }
        else
        {
            FirstPatrolPoint.GetComponent<Renderer>().enabled = false;
            SecondPatrolPoint.GetComponent<Renderer>().enabled = false;
            SelectionParticleCircle.SetActive(false);
            SelectionParticleCircle.GetComponent<ParticleSystem>().Stop();
            isPlayingCircleAnim = false;

            if (troupType == TroupType.Ally)
            {
                QueueUI.SetActive(false);
            }
        }
    }

    // Selection Coroutines ---------------------------------------------------------------------------------------
    protected IEnumerator PlaceSelection()
    {
        PlaceSelectionPopUp.enabled = true;
        bool hasSelected = false;

        Debug.Log("Enabling Place Selection");

        while (!hasSelected && isChosingPlacement)
        {
            //PlaceSelectionPopUp.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);
            SelectionArrow.GetComponent<MeshRenderer>().enabled = true;
            // selectionArrow.transform.LookAt(camera1.transform.position);

            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                SelectionArrow.transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);

                if (Input.GetMouseButton(0))
                {
                    float selectionCount = selectionManager.numberOfSelected();
                    Vector3 targetPosition = new Vector3(0, 0, 0);

                    if (selectionCount == 1)
                    {
                        targetPosition = hit.point;
                    } else if (selectionCount > 1)
                    {
                        Vector3 center = new Vector3(0, 0, 0);

                        foreach (GameObject troup in selectionManager.getCurrentSelection())
                        {
                            center += troup.transform.position / selectionCount;
                        }

                        NavMeshPath navMeshPath = new NavMeshPath();
                        agent.CalculatePath(hit.point + transform.position - center, navMeshPath);

                        if (navMeshPath.status == NavMeshPathStatus.PathComplete)
                        {
                            targetPosition = hit.point + transform.position - center;
                        } else
                        {
                            NavMeshHit closestHit;

                            
                            if (NavMesh.SamplePosition(hit.point, out closestHit, 10, 1))
                            {
                                targetPosition = closestHit.position;
                            } else
                            {
                                closestHit.position = hit.point;
                            }                            
                        }
                    }

                    Debug.Log("Target position clicked : " + hit.point);
                    AddAction(new MoveToPosition(agent,targetPosition , positionThreshold));
                    hasSelected = true;
                    SelectionArrow.GetComponent<MeshRenderer>().enabled = false;
                    PlaceSelectionPopUp.enabled = false;

                }
            }

            /* if (Input.GetMouseButton(0))
            {
                Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Debug.Log("Target position clicked : " + hit.point);
                    AddAction(new MoveToPosition(agent, hit.point));
                }

                hasSelected = true;
                PlaceSelectionPopUp.enabled = false;
            } */
            
            yield return null;
        }

        
    }

    protected IEnumerator FollowSelection()
    {
        FollowSelectionPopUp.enabled = true;
        bool hasSelected = false;

        GameObject unitToFollow = new GameObject();

        Debug.Log("Enabling Follow Selection");

        while (!hasSelected && isChosingFollow)
        {
            // FollowSelectionPopUp.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);
            // Debug.Log("Est enabled : " + PlaceSelectionPopUp.enabled);

            Debug.Log("Unit Selection");
            Ray ray_1 = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit_1;
            Physics.Raycast(ray_1, out hit_1, Mathf.Infinity);
            float minDistance = 1f;
            GameObject nearestObject = null;
            foreach (GameObject selectionableObject in selectionManager.getDictionnary().Keys)
            {
                float distance = Vector3.Distance(hit_1.point, selectionableObject.transform.position);
                if (selectionableObject.transform.Find("FollowingArrow") != null)
                {
                    selectionableObject.transform.Find("FollowingArrow").GetComponent<Renderer>().enabled = false;
                }
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestObject = selectionableObject;
                    nearestObject.transform.Find("FollowingArrow").GetComponent<Renderer>().enabled = true;
                }
            }
            

            if (Input.GetMouseButton(0) && nearestObject != null)
            {
                hasSelected = true;
                unitToFollow = nearestObject;
                nearestObject.transform.Find("FollowingArrow").GetComponent<Renderer>().enabled = false;
            }


            yield return null;
        }

        if (isChosingFollow)
        {
            AddAction(new FollowUnit(agent, unitToFollow));
        }
        FollowSelectionPopUp.enabled = false;

    }

    protected IEnumerator PatrolSelection()
    {
        PatrolSelectionPopUp1.enabled = true;
        bool hasSelectedFistPos = false;
        bool hasSelectedSecondPos = false;

        Vector3 firstPos = new Vector3();
        Vector3 secondPos = new Vector3();

        FirstPatrolPoint.GetComponent<Renderer>().enabled = true;

        Debug.Log("Enabling Patrol Selection");

        while (!hasSelectedFistPos && isChosingPatrol)
        {
            // PatrolSelectionPopUp1.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);

            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Target position clicked : " + hit.point);
                firstPos = hit.point;
                Debug.Log("firstPos : " + firstPos);
                FirstPatrolPoint.transform.position= new Vector3(firstPos.x, firstPos.y + 0.1f, firstPos.z);
            }

            if (Input.GetMouseButtonDown(0))
            {
                hasSelectedFistPos = true;
                PatrolSelectionPopUp1.enabled = false;
            }

            yield return null;
        }
        
        if (isChosingPatrol) 
        { 
            PatrolSelectionPopUp2.enabled = true;
            SecondPatrolPoint.GetComponent<Renderer>().enabled = true;
        }

        while (!hasSelectedSecondPos && isChosingPatrol)
        {
            // PatrolSelectionPopUp2.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);

            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Target position clicked : " + hit.point);
                secondPos = hit.point;
                SecondPatrolPoint.transform.position = new Vector3(secondPos.x, secondPos.y + 0.1f, secondPos.z);
            }

            if (Input.GetMouseButtonDown(0))
            {
                hasSelectedSecondPos = true;
                PatrolSelectionPopUp2.enabled = false;
            }

            yield return null;
        }

        
        if (isChosingPatrol) {
            Debug.Log("Starting patroling between " + firstPos + " and " + secondPos);
            AddAction(new Patrol(agent, firstPos, secondPos));
            isChosingPatrol = false;
        }
    }

    // Misc -------------------------------------------------------------------------------------------------------
    protected IEnumerator UpdateHealthBar()
    {
        float targetFillAmount = health / maxHealth;
        float t = 0f;
        Debug.Log("Je heal de " + healthBar.fillAmount + " à " + targetFillAmount);
        while (t < 1f && healthBar.fillAmount != targetFillAmount)
        {
            t += Time.deltaTime / 0.5f; // 0.5f est la dur�e de la transition en secondes, ajustez selon vos besoins
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFillAmount, t);
            Debug.Log("Je passe le fillAmount à : " + healthBar.fillAmount);
            yield return null;
        }

        healthBar.fillAmount = targetFillAmount;
    }

    protected void HealthBarControl()
    {
        // Health Bar control
        float normalizedHealth = health / maxHealth;
        healthBar.enabled = !gameManager.isInPause();

        Vector3 healthBarPosition = camera1.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + agent.height, transform.position.z));
        healthBar.transform.position = new Vector3(healthBarPosition.x, healthBarPosition.y, healthBarPosition.z);

        if (normalizedHealth >= .75)
        {
            healthBar.color = Color.green;
        }
        if (normalizedHealth >= .5 && normalizedHealth <= .75)
        {
            healthBar.color = Color.yellow;
        }
        if (normalizedHealth >= .25 && normalizedHealth <= .5)
        {
            healthBar.color = new Color(1.0f, 0.5f, 0.0f);
        }
        if (normalizedHealth <= .25)
        {
            healthBar.color = Color.red;
        }
    }

    protected void AbilityBarControl()
    {
        // Ability Bar control
        // float normalizedHealth = health / maxHealth;
        abilityBar.enabled = !gameManager.isInPause();

        Vector3 abilityBarPosition = camera1.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + agent.height, transform.position.z));
        abilityBar.transform.position = new Vector3(abilityBarPosition.x, abilityBarPosition.y - 5.5f, abilityBarPosition.z);

        /* if (normalizedHealth >= .75)
        {
            healthBar.color = Color.green;
        }
        if (normalizedHealth >= .5 && normalizedHealth <= .75)
        {
            healthBar.color = Color.yellow;
        }
        if (normalizedHealth >= .25 && normalizedHealth <= .5)
        {
            healthBar.color = new Color(1.0f, 0.5f, 0.0f);
        }
        if (normalizedHealth <= .25)
        {
            healthBar.color = Color.red;
        } */
}

    public bool IsInjured()
    {
        // Debug.Log("max health = " + maxHealth + "et health = " + health);
        return health < maxHealth;
    }

    public bool IsBoosted()
    {
        return isBoosted;
    }

    public void ActivateBoostParticle(bool activate)
    {
        BoostParticle.SetActive(activate);
        isBoosted = activate;
    }

    public void ActivateArmorBoostParticle(bool activate)
    {
        ArmorBoostParticle.SetActive(activate);
    }

    private void OnDrawGizmos()
    {
        if (selectionManager != null && selectionManager.isSelected(this.gameObject))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    // Attack and ability -----------------------------------------------------------------------------------------
    protected void AttackBehaviour()
    {
        if (troupType == TroupType.Enemy && gameManager.isCrownCollected)
        {
            GameObject king = gameManager.king;
            
            if (king != null && king.GetComponent<Troup>().isVisible && !isMovingToKing)
            {
                isMovingToKing = true;

                actionQueue.Clear();
                agent.isStopped = true;
                agent.ResetPath();

                // Debug.Log("Getting closer to enemy : " + king);
                StopCoroutine(currentActionCoroutine);
                actionQueue.Enqueue(new MoveToUnit(agent, king, attackRange));
                StartCoroutine(currentActionCoroutine);
            }
        }

        
        // Find all enemies in detection range
        HashSet<GameObject> detectedEnemies = new HashSet<GameObject>();
        if (troupType == TroupType.Ally)
        {
            HashSet<Troup> enemies = gameManager.getEnemies();
            foreach (Troup enemie in enemies)
            {
                if (enemie.isVisible && Vector3.Distance(transform.position, enemie.transform.position) <= detectionRange)
                {
                    detectedEnemies.Add(enemie.gameObject);
                }
            }
        } else
        {
            HashSet<Troup> allies = gameManager.getAllies();
            foreach (Troup ally in allies)
            {
                if (ally.isVisible && Vector3.Distance(transform.position, ally.transform.position) <= detectionRange)
                {
                    detectedEnemies.Add(ally.gameObject);
                }
            }
        }


        // Find the closest enemy
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in detectedEnemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        // Follow closestEnemy until it's in range

        if (closestEnemy != null && !isFollowingEnemy && !isFollowingOrders)
        {
            isFollowingEnemy = true;
            currentFollowedTroup = closestEnemy;
            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();

            Debug.Log("Getting closer to enemy : " + closestEnemy);
            StopCoroutine(currentActionCoroutine);
            actionQueue.Enqueue(new MoveToUnit(agent, closestEnemy, attackRange));
            StartCoroutine(currentActionCoroutine);

            
        }
        if (currentFollowedTroup != null && Vector3.Distance(transform.position, currentFollowedTroup.transform.position) >= detectionRange)
        {
            currentFollowedTroup = null;
        }
        if (currentFollowedTroup == null)
        {
            isFollowingEnemy = false;
        }
        if ((currentFollowedTroup != null && !currentFollowedTroup.GetComponent<Troup>().isVisible) || (currentAttackedTroup != null && !currentAttackedTroup.GetComponent<Troup>().isVisible))
        {
            isFollowingEnemy = false;
            isAttackingEnemy = false;
            currentAttackedTroup = null;
            currentFollowedTroup = null;
            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();
            StopCoroutine(currentActionCoroutine);
            actionQueue.Enqueue(new Standby());
            StartCoroutine(currentActionCoroutine);
        }

        
        

        HashSet<GameObject> inRangeEnemies = new HashSet<GameObject>();
        if (troupType == TroupType.Ally)
        {
            HashSet<Troup> enemies = gameManager.getEnemies();
            foreach (var enemy in enemies)
            {
                if (enemy.isVisible && Vector3.Distance(transform.position, enemy.transform.position) <= attackRange)
                {
                    inRangeEnemies.Add(enemy.gameObject);
                }
            }
        }
        else
        {
            HashSet<Troup>  allies = gameManager.getAllies();
            foreach (var ally in allies)
            {
                if (ally.isVisible && Vector3.Distance(transform.position, ally.transform.position) <= attackRange)
                {
                    inRangeEnemies.Add(ally.gameObject);
                }
            }
        }

        float closestDistanceInRange = Mathf.Infinity;
        GameObject closestEnemyInRange = null;

        foreach (GameObject enemy in inRangeEnemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= closestDistanceInRange)
            {
                closestDistanceInRange = distance;
                closestEnemyInRange = enemy;
            }
        }

        if (currentAttackedTroup == null && !isFollowingOrders)
        {
            isAttackingEnemy = false;
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }
            
            if (closestEnemyInRange != null) 
            {
                currentAttackedTroup = closestEnemyInRange;

                actionQueue.Clear();
                agent.isStopped = true;
                agent.ResetPath();
                Debug.Log("Je lance une nouvelle attaque contre + " + currentAttackedTroup);

                StopCoroutine(currentActionCoroutine);
                actionQueue.Enqueue(new Standby());
                StartCoroutine(currentActionCoroutine);

                attackCoroutine = Attack(currentAttackedTroup.GetComponent<Troup>());
                StartCoroutine(attackCoroutine);
                isAttackingEnemy = true;
            }
        }

        if (troupType == TroupType.Enemy && currentAttackedTroup == null && currentFollowedTroup != null)
        {
            isFollowingEnemy = false;
        }

        /* if (currentAttackedTroup == null && closestEnemyInRange != null && !isAttackingEnemy)
        {
            currentAttackedTroup = closestEemyInRange;
            StartCoroutine(Attack(currentAttackedTroup.GetComponent<Troup>()));
            isAttackingEnemy = true;
        } */
        if (closestEnemyInRange == null)
        {
            currentAttackedTroup = null;
            isAttackingEnemy = false;
        }

        if (currentAttackedTroup != null)
        {
            Vector3 targetPosition = currentAttackedTroup.transform.position;
            targetPosition.y = transform.position.y;  // Keep the same y position as the object you are rotating

            transform.LookAt(targetPosition);
        }

    }

    protected abstract IEnumerator Attack(Troup enemy);

    protected abstract IEnumerator SpecialAbility();

    protected IEnumerator SpecialAbilityCountdown()
    {
        float currentDelay = specialAbilityDelay;

        while (currentDelay > 0)
        {
            currentDelay -= Time.deltaTime;
            specialAbilityDelay -= Time.deltaTime;
            abilityBar.fillAmount = 1 - currentDelay / specialAbilityRechargeTime;

            yield return null;
        }

        specialAbilityDelay = 0f;

        abilityBar.fillAmount = 1;
    }

    public virtual void TakeDamage(float damage)
    {
        float beforeHealth = health;

        health -= Mathf.Max(damage - armor, 0);

        float newHealth = health;
        Debug.Log("J'ai pris dégat : " + damage);
        Debug.Log("J'ai " + health + " vie");

        if (beforeHealth * newHealth <= 0 && !hasSpawnedTombe)
        {
            if (troupType == TroupType.Ally) { gameManager.removeAlly(this); }
            if (troupType == TroupType.Enemy) { gameManager.removeEnemy(this);  }
            GameObject tombeMort = Instantiate(tombe, transform.position, transform.rotation, null);
            hasSpawnedTombe = true;
            // if (troupType == TroupType.Enemy) { tombeMort.transform.position += Vector3.up * 3; }
            tombeMort.GetComponent<Tombe>().SetUnitType((Tombe.TombeUnitType)unitType);
            tombeMort.GetComponent<Tombe>().SetTroupType((Tombe.TombeTroupType)troupType);
            if (hasCrown)
            {
                gameManager.kingIsDead();
            }
            Destroy(SelectionParticleCircle);
            Destroy(BoostParticle);
            Destroy(ArmorBoostParticle);
            Destroy(FirstPatrolPoint);
            Destroy(SecondPatrolPoint);
            selectionManager.removeObject(gameObject);
            Destroy(gameObject);
        } else
        {
            StartCoroutine(UpdateHealthBar());
        }
    }

    public virtual void AddDamage(float damage)
    {
        attackDamage += damage;
        if (damage < 0)
        {
            isBoosted = false;
        }
    }

    public virtual void AddArmor(float armorCount)
    {
        armor += armorCount;
    }

    public virtual void ChangeAttackSpeed(float multiplier)
    {
        attackRechargeTime *= multiplier;
    }

    public virtual void Heal(float healAmount)
    {
        health = Mathf.Min(maxHealth, health + healAmount);
        StartCoroutine(UpdateHealthBar());
    }

    // IAction Interface ------------------------------------------------------------------------------------------
    protected interface IAction
    {
        bool IsActionComplete { get; set; }
        // bool IsStandBy { get; set; }
        public void Execute();
    }

    protected void AddAction(IAction action)
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            actionQueue.Enqueue(action);

        }
        else
        {
            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();
            StopCoroutine(currentActionCoroutine);
            Debug.Log("Stopping Troup");

            actionQueue.Enqueue(action);

            Debug.Log("Actions en queue apr�s 2 : " + actionQueue.Count);

            Debug.Log("Starting new ExecuteActionQueue Coroutine");
            StartCoroutine(currentActionCoroutine);
        }
    }

    protected class Standby : IAction
    {
        public bool IsActionComplete { get; set; }

        public void Execute()
        {
            Debug.Log("Standby pendant 1 seconde");
        }

        public IEnumerator StandbyWait()
        {
            yield return new WaitForSeconds(1f);
            IsActionComplete = true;
        }
    }

    protected class Patrol : IAction
    {
        public bool IsActionComplete { get; set; }
        // public bool IsStandBy { get; set; }
        private Vector3 firstPosition;
        private Vector3 secondPosition;
        private NavMeshAgent navMeshAgent;

        public Patrol(NavMeshAgent agent, Vector3 firstPos, Vector3 secondPos)
        {
            navMeshAgent = agent;
            firstPosition = firstPos;
            secondPosition = secondPos;
        }
        public void Execute()
        {
            Debug.Log("Patroling executed between position " + firstPosition + " and " + secondPosition);
        }
        public IEnumerator StartPatroling()
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(firstPosition);

            Debug.Log("Distance initiale : ");

            while (Vector3.Distance(navMeshAgent.transform.position, firstPosition) >= 1f)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                yield return null;
            }

            navMeshAgent.SetDestination(secondPosition);

            while (Vector3.Distance(navMeshAgent.transform.position, secondPosition) >= 1f)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                yield return null;
            }

            IsActionComplete = true;

            Debug.Log("Movement complete");
        }
    }

    protected class MoveToPosition : IAction
    {
        public bool IsActionComplete { get; set; }
        // public bool IsStandBy { get; set; }
        private Vector3 targetPosition;
        private NavMeshAgent navMeshAgent;
        float positionThreshold;

        public MoveToPosition(NavMeshAgent agent, Vector3 position, float apositionThreshold)
        {
            navMeshAgent = agent;
            targetPosition = position;
            positionThreshold = apositionThreshold;
        }

        public void Execute()
        {
            Debug.Log("Move Action executed to position: " + targetPosition);
        }

        public IEnumerator GoToPosition()
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(targetPosition);

            Debug.Log("Distance initiale : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));

            navMeshAgent.transform.GetComponent<Troup>().moveTargetDestination = targetPosition;

            while (!navMeshAgent.isStopped && Vector2.Distance(new Vector2(navMeshAgent.transform.position.x, navMeshAgent.transform.position.z), new Vector2(targetPosition.x, targetPosition.z)) >= positionThreshold)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                Debug.Log("Moving to destination : " + targetPosition + " and current Position : " + navMeshAgent.transform.position);
                yield return null;
            }



            Debug.Log("I arrived at destination ! My position is " + navMeshAgent.transform.position + " and destination is " + targetPosition);

            if (Vector2.Distance(new Vector2(navMeshAgent.transform.position.x, navMeshAgent.transform.position.z), new Vector2(navMeshAgent.transform.GetComponent<Troup>().moveTargetDestination.x, navMeshAgent.transform.GetComponent<Troup>().moveTargetDestination.z)) <= positionThreshold)
            {
                navMeshAgent.isStopped = true;
            }


            
            IsActionComplete = true;

            Debug.Log("Movement complete");
        }
    }

    protected class FollowUnit : IAction
    {
        public bool IsActionComplete { get; set; }
        public GameObject unitToFollow;
        private NavMeshAgent navMeshAgent;

        public FollowUnit(NavMeshAgent agent, GameObject unit)
        {
            navMeshAgent = agent;
            unitToFollow = unit;
        }

        public void Execute()
        {
            Debug.Log("Following Unit : " + unitToFollow);
        }

        public IEnumerator StartFollowing()
        {
            Vector3 targetPosition = new Vector3();
            if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }

            // if (unitToFollow != null)

            Debug.Log("Following unit at position : " + targetPosition);
            
            // navMeshAgent.SetDestination(targetPosition);

            while (unitToFollow != null && Vector3.Distance(navMeshAgent.transform.position, targetPosition) >= 1f)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                navMeshAgent.isStopped = false;
                if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }
                // Debug.Log("Going to " + targetPosition);
                navMeshAgent.SetDestination(targetPosition);

                yield return null;
            }

            navMeshAgent.ResetPath();
            IsActionComplete = true;

            Debug.Log("Movement complete");
        }
    }

    protected class MoveToUnit : IAction
    {
        public bool IsActionComplete { get; set; }
        public GameObject unitToFollow;
        private NavMeshAgent navMeshAgent;
        private float range;

        public MoveToUnit(NavMeshAgent agent, GameObject unit, float attackRange)
        {
            navMeshAgent = agent;
            unitToFollow = unit;
            range = attackRange;
        }

        public void Execute()
        {
            Debug.Log("Moving to Unit : " + unitToFollow);
        }

        public IEnumerator StartFollowing()
        {
            Vector3 targetPosition = new Vector3();
            if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }

            // if (unitToFollow != null)

            Debug.Log("Following unit at position : " + targetPosition);

            // navMeshAgent.SetDestination(targetPosition);

            while (unitToFollow != null && navMeshAgent != null && Vector3.Distance(navMeshAgent.transform.position, targetPosition) >= range && unitToFollow.GetComponent<Troup>().isVisible)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                navMeshAgent.isStopped = false;
                if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }
                // Debug.Log("Going to enemy " + targetPosition);
                navMeshAgent.SetDestination(targetPosition);

                yield return null;
            }

            navMeshAgent.SetDestination(navMeshAgent.transform.position);
            navMeshAgent.isStopped = true;
            // navMeshAgent.ResetPath();
            IsActionComplete = true;

            Debug.Log("Movement complete");
        }
    }

    // Action Queue execution -------------------------------------------------------------------------------------
    protected IEnumerator ExecuteActionQueue()
    {
        Debug.Log("Start Action Queue");
        bool hasToStop = false;

        while (actionQueue.Count > 0 && !hasToStop)
        {
            IAction currentAction = actionQueue.Dequeue();

            isFollowingOrders = false;

            // Affichage des actions de la queue
            if (troupType == TroupType.Ally)
            {
                QueueUI.GetComponent<TextMeshProUGUI>().text = "Current action: " + (currentAction.ToString().StartsWith("Troup+") ? currentAction.ToString().Substring("Troup+".Length) : currentAction.ToString());

                string queueText = "";

                foreach (IAction action in actionQueue)
                {
                    Debug.Log("Action " + action + " ajout�e � la liste");
                    queueText += "\n" + (action.ToString().StartsWith("Troup+") ? action.ToString().Substring("Troup+".Length) : action.ToString());
                }
                QueueUI.GetComponent<TextMeshProUGUI>().text += "\n Enqueued action: ";
                QueueUI.GetComponent<TextMeshProUGUI>().text += queueText;
            }

            Debug.Log("Treating action : " + currentAction);

            isPatroling = false;
            Debug.Log("isPatroling false");

            if (currentAction is MoveToPosition moveToPosition)
            {
                isFollowingOrders = true;
                StartCoroutine(moveToPosition.GoToPosition());
            }
            if (currentAction is Patrol patrol)
            {
                StartCoroutine(patrol.StartPatroling());
                isPatroling = true;
                Debug.Log("isPatroling true");
            }
            if (currentAction is Standby standby)
            {
                StartCoroutine(standby.StandbyWait());
            }
            if (currentAction is FollowUnit followUnit)
            {
                isFollowingOrders = true;
                StartCoroutine(followUnit.StartFollowing());
            }
            if (currentAction is MoveToUnit moveToUnit)
            {
                StartCoroutine(moveToUnit.StartFollowing());
            }

            yield return new WaitUntil(() => currentAction.IsActionComplete);

            Debug.Log("Actions en queue : " + actionQueue.Count);

            if (actionQueue.Count == 0)
            {
                if (currentAction is Patrol)
                {
                    Debug.Log("Action en cours : " + currentAction);
                    currentAction.IsActionComplete = false;
                    actionQueue.Enqueue(currentAction);
                }
                else if (currentAction is FollowUnit)
                {
                    Debug.Log("Action en cours : " + currentAction);
                    currentAction.IsActionComplete = false;
                    actionQueue.Enqueue(new Standby());
                    actionQueue.Enqueue(currentAction);
                }
                else if (!isPatroling)
                {
                    Debug.Log("Ajout du standby");
                    actionQueue.Enqueue(new Standby());
                }
            }

            

            Debug.Log("Action termin�e");
            Debug.Log("Actions en queue apr�s : " + actionQueue.Count);

        }

        Debug.Log("Execute Action Queue Coroutine ended");

        yield break;
        
    }
}