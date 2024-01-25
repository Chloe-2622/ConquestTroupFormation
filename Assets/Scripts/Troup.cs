using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;
using static UnityEngine.UI.Image;
using Unity.VisualScripting;

public abstract class Troup : MonoBehaviour
{
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
    [SerializeField] protected float defaultAttackRange;
    [SerializeField] protected float specialAbilityRechargeTime;

    [Header("Damages")]
    [SerializeField] protected float showTimeDamages;

    // Private variables ------------------------------------------------------------------------------------------
    [Header("Debug Variables")]
    [SerializeField] private bool isChosingPatrol;
    [SerializeField] protected bool isFollowingEnemy;
    [SerializeField] protected bool isAttackingEnemy;
    [SerializeField] private bool isPatroling;
    [SerializeField] protected GameObject currentAttackedTroup;
    [SerializeField] protected GameObject currentFollowedTroup;
    [SerializeField] protected float specialAbilityDelay = 0f;
    [SerializeField] protected bool isVisible = true;
    [SerializeField] protected bool isAddedWhenAwake = false;
    [SerializeField] public Vector3 moveTargetDestination;
    [SerializeField] protected float timeBeforeNextAction;
    [SerializeField] protected float turnTime;
    protected Vector3 defaultPosition;
    private Wall wallComponent;
    private bool isPlayingCircleAnim;
    private bool isChosingPlacement;
    private bool isChosingFollow;
    public bool isPlacingWall;
    protected bool isFollowingOrders;
    private bool hasSpawnedTombe;
    [SerializeField] private bool isMovingToKing;
    private bool isBoosted;
    protected float maxHealth;
    private float attackRange;
    protected IEnumerator currentActionCoroutine;
    private IEnumerator attackCoroutine;
    private IEnumerator goToPosition;


    HashSet<GameObject> detectedEnemies = new HashSet<GameObject>();
    HashSet<GameObject> inRangeEnemies = new HashSet<GameObject>();

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
    protected LayerMask floorMask;
    protected LayerMask troupMask;
    protected LayerMask wallMask;

    // UI elements
    protected TextMeshProUGUI PlaceSelectionPopUp;
    protected TextMeshProUGUI PatrolSelectionPopUp1;
    protected TextMeshProUGUI PatrolSelectionPopUp2;
    protected TextMeshProUGUI FollowSelectionPopUp;
    protected Image healthBar;
    protected Image abilityBar;
    protected TextMeshProUGUI damageText;
    protected GameObject unitBar;
    protected GameObject Bars;


    protected GameManager gameManager;

    // Troup types ------------------------------------------------------------------------------------------------
    public enum TroupType { Ally, Enemy }
    public enum UnitType
    {
        Null, Combattant, Archer, Cavalier, Guerisseur, Catapulte, Porte_bouclier, Porte_etendard, Batisseur, Belier, Mur
    }

    // Action Queue -----------------------------------------------------------------------------------------------
    protected Queue<IAction> actionQueue = new Queue<IAction>();

    // Main Functions ---------------------------------------------------------------------------------------------

    protected virtual void Awake() 
    {
        turnTime = .3f;

        if (troupType == TroupType.Ally && unitType != UnitType.Mur)
        {
            QueueUI = transform.Find("Canvas").Find("Queue").gameObject;
        }

        defaultPosition = transform.position;

        // Setup and show Health Bar
        maxHealth = health;

        // Agent setup
        if (unitType != UnitType.Mur)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = movingSpeed;
            attackRange = defaultAttackRange + agent.radius;
        }
        

        // Setup range
        
        
        // Debug.Log("agent radius " + agent.radius + " , radius = " + attackRange + " , default = " + defaultAttackRange);

        // GameManager variable setup
        gameManager = GameManager.Instance;

        // Debug.Log("GameManager of " + troupType + " " + unitType + " = " + gameManager);
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

        unitBar = Instantiate(gameManager.unitBarsPrefab, gameManager.eventSystem.GetComponent<InGameUI>().bars.transform);
        healthBar = unitBar.transform.GetChild(0).GetComponent<Image>();
        abilityBar = unitBar.transform.GetChild(1).GetComponent<Image>();
        damageText = unitBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        troupMask = gameManager.troupMask;
        floorMask = gameManager.floorMask;

        isPlacingWall = false;
        if (unitType == Troup.UnitType.Mur)
        {
            wallComponent = GetComponent<Wall>();
        }

        // Start Action Queue
        currentActionCoroutine = ExecuteActionQueue();
        AddAction(new Standby());
        StartCoroutine(currentActionCoroutine);

        timeBeforeNextAction = Random.Range(0f, 5f);
        StartCoroutine(IAactionCountdown());

        if (troupType == TroupType.Enemy)
        {
            addToGroup();
        }
        if (isAddedWhenAwake)
        {
            Debug.Log("--- Unit added");
            addToGroup();
        }
    }

    // Ally or Enemy
    public virtual void addToGroup()
    {
        if (troupType == TroupType.Ally)
        {
            gameManager.addAlly(this);
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
    public float getMaxHealth() { return maxHealth; }
    public float getMaxSpeed() { return movingSpeed; }
    public bool isKing() { return hasCrown; }
    public bool IsInjured() { return health < maxHealth; }
    public bool IsBoosted() { return isBoosted; }

    // Update
    protected virtual void Update()
    {
        // Debug.Log("start------------------------------------");
        foreach (GameObject enemy in inRangeEnemies)
        {
            // Debug.Log(enemy);
        }
        // Debug.Log("end------------------------------------");
        isSelected = selectionManager.isSelected(this.gameObject);

        
        /*

        if (currentFollowedTroup != null)
        {
            Troup currentFollowedTroupComponent = currentFollowedTroup.GetComponent<Troup>();
            attackRange = (currentFollowedTroupComponent.unitType == UnitType.Cavalier || currentFollowedTroupComponent.unitType == UnitType.Catapulte || currentFollowedTroupComponent.unitType == UnitType.Belier || currentFollowedTroupComponent.unitType == UnitType.Porte_bouclier) ? defaultAttackRange + 1.1f : defaultAttackRange;
        }
        if (currentAttackedTroup != null)
        {
            Troup currentAttackedTroupComponent = currentAttackedTroup.GetComponent<Troup>();
            attackRange = (currentAttackedTroupComponent.unitType == UnitType.Cavalier || currentAttackedTroupComponent.unitType == UnitType.Catapulte || currentAttackedTroupComponent.unitType == UnitType.Belier || currentAttackedTroupComponent.unitType == UnitType.Porte_bouclier) ? defaultAttackRange + 1.1f : defaultAttackRange;
        } */

        if (isFollowingOrders)
        {
            isFollowingEnemy = false;
            isAttackingEnemy = false;
            currentFollowedTroup = null;
            currentAttackedTroup = null;
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

        if (selectionManager.isSelected(this.gameObject))
        {
            SelectionParticleCircle.SetActive(true);
            if (!isPlayingCircleAnim)
            {
                SelectionParticleCircle.GetComponent<ParticleSystem>().Play();
                isPlayingCircleAnim = true;
            }
        }
        else
        {
            SelectionParticleCircle.SetActive(false);
            SelectionParticleCircle.GetComponent<ParticleSystem>().Stop();
            isPlayingCircleAnim = false;
        }

        if (selectionManager.getCurrentSelection().Count > 1 && gameManager.isFormationShapeForced)
        {
            agent.speed = FormationSpeed();
        } else
        {
            agent.speed = movingSpeed;
        }

        SelectionParticleCircle.transform.position = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);

        SelectedBehaviour();

        HealthBarControl();
        AbilityBarControl();

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

    }

    protected virtual void SelectedBehaviour()
    {
        if (!gameManager.hasGameStarted()) { return; }

        if (selectionManager.isSelected(this.gameObject))
        {
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

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.F))
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
            if (Input.GetKeyDown(KeyCode.Alpha1) && !isPlacingWall)
            {
                isChosingPlacement = true;
                StartCoroutine(PlaceSelection());
            }

            // Patrol input
            if (Input.GetKeyDown(KeyCode.Alpha2) && !isPlacingWall)
            {
                isChosingPatrol = true;
                StartCoroutine(PatrolSelection());
            }

            // Follow input
            if (Input.GetKeyDown(KeyCode.Alpha3) && !isPlacingWall)
            {
                isChosingFollow = true;
                StartCoroutine(FollowSelection());
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) && !isPlacingWall)
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

        // Debug.Log("Enabling Place Selection");

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
                    // Debug.Log("c'est tard0");
                    float selectionCount = selectionManager.numberOfSelected();
                    // Debug.Log("c'est tard1");
                    Vector3 targetPosition = new Vector3(0, 0, 0);

                    if (selectionCount == 1)
                    {
                        targetPosition = hit.point;
                    } else if (selectionCount > 1)
                    {
                        Vector3 center = new Vector3(0, 0, 0);

                        foreach (GameObject troup in selectionManager.getCurrentSelection())
                        {
                            if (troup != null) { center += troup.transform.position / selectionCount; }
                            
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

                    // Debug.Log("Target position clicked : " + hit.point);
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
                    // Debug.Log("Target position clicked : " + hit.point);
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

        // Debug.Log("Enabling Follow Selection");

        while (!hasSelected && isChosingFollow)
        {
            // FollowSelectionPopUp.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);
            // Debug.Log("Est enabled : " + PlaceSelectionPopUp.enabled);

            // Debug.Log("Unit Selection");
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

        // Debug.Log("Enabling Patrol Selection");

        while (!hasSelectedFistPos && isChosingPatrol)
        {
            // PatrolSelectionPopUp1.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);

            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Debug.Log("Target position clicked : " + hit.point);
                // Debug.Log("firstPos : " + firstPos);
                
            }

            FirstPatrolPoint.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);

            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("c'est tard0");
                float selectionCount = selectionManager.numberOfSelected();
                // Debug.Log("c'est tard1");

                if (selectionCount == 1)
                {
                    firstPos = hit.point;
                }
                else if (selectionCount > 1)
                {
                    Vector3 center = new Vector3(0, 0, 0);

                    foreach (GameObject troup in selectionManager.getCurrentSelection())
                    {
                        if (troup != null) { center += troup.transform.position / selectionCount; }

                    }

                    NavMeshPath navMeshPath = new NavMeshPath();
                    agent.CalculatePath(hit.point + transform.position - center, navMeshPath);

                    if (navMeshPath.status == NavMeshPathStatus.PathComplete)
                    {
                        firstPos = hit.point + transform.position - center;
                    }
                    else
                    {
                        NavMeshHit closestHit;


                        if (NavMesh.SamplePosition(hit.point, out closestHit, 10, 1))
                        {
                            firstPos = closestHit.position;
                        }
                        else
                        {
                            closestHit.position = hit.point;
                        }
                    }
                }

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
                // Debug.Log("Target position clicked : " + hit.point);
                SecondPatrolPoint.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("c'est tard0");
                float selectionCount = selectionManager.numberOfSelected();
                // Debug.Log("c'est tard1");

                if (selectionCount == 1)
                {
                    secondPos = hit.point;
                }
                else if (selectionCount > 1)
                {
                    Vector3 center = new Vector3(0, 0, 0);

                    foreach (GameObject troup in selectionManager.getCurrentSelection())
                    {
                        if (troup != null) { center += troup.transform.position / selectionCount; }

                    }

                    NavMeshPath navMeshPath = new NavMeshPath();
                    agent.CalculatePath(hit.point + transform.position - center, navMeshPath);

                    if (navMeshPath.status == NavMeshPathStatus.PathComplete)
                    {
                        secondPos = hit.point + transform.position - center;
                    }
                    else
                    {
                        NavMeshHit closestHit;


                        if (NavMesh.SamplePosition(hit.point, out closestHit, 10, 1))
                        {
                            secondPos = closestHit.position;
                        }
                        else
                        {
                            closestHit.position = hit.point;
                        }
                    }
                }

                hasSelectedSecondPos = true;
                PatrolSelectionPopUp2.enabled = false;
            }

            yield return null;
        }

        
        if (isChosingPatrol) {
            // Debug.Log("Starting patroling between " + firstPos + " and " + secondPos);
            AddAction(new Patrol(agent, firstPos, secondPos));
            isChosingPatrol = false;
        }
    }

    // Misc -------------------------------------------------------------------------------------------------------
    protected IEnumerator UpdateHealthBar()
    {
        float targetFillAmount = health / maxHealth;
        float t = 0f;
        // Debug.Log("Je heal de " + healthBar.fillAmount + " à " + targetFillAmount);
        while (t < 1f && healthBar.fillAmount != targetFillAmount)
        {
            t += Time.deltaTime / 0.5f; // 0.5f est la dur�e de la transition en secondes, ajustez selon vos besoins
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFillAmount, t);
            // Debug.Log("Je passe le fillAmount à : " + healthBar.fillAmount);
            yield return null;
        }

        healthBar.fillAmount = targetFillAmount;
    }

    protected void HealthBarControl()
    {
        // Health Bar control
        float normalizedHealth = health / maxHealth;
        healthBar.enabled = !gameManager.isInPause();

        if (unitType == UnitType.Mur)
        {
            Vector3 healthBarPosition = camera1.WorldToScreenPoint(wallComponent.getHealthPosition());
            healthBar.transform.position = healthBarPosition;
        }
        else
        {
            Vector3 healthBarPosition = camera1.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + agent.height, transform.position.z));
            healthBar.transform.position = healthBarPosition;
        }

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
}

    public void ActivateBoostParticle(bool activate)
    {
        BoostParticle.SetActive(activate);
        isBoosted = activate;
    }

    public void ActivateArmorBoostParticle(bool activate)
    {
        if (ArmorBoostParticle != null) { ArmorBoostParticle.SetActive(activate); }
    }

    public Vector3 RandomVectorInFlatCircle(Vector3 center, float circleRadius)
    {
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);
        float randomRadius = Mathf.Sqrt(Random.Range(0f, 1f)) * circleRadius;

        float x = center.x + randomRadius * Mathf.Cos(randomAngle);
        float z = center.z + randomRadius * Mathf.Sin(randomAngle);

        float y = transform.position.y;

        return new Vector3(x, y, z);
    }

    public Troup RandomTroupInTeam(HashSet<Troup> team)
    {
        if (team == null || team.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, team.Count);
        int currentIndex = 0;

        foreach (Troup troup in team)
        {
            if (currentIndex == randomIndex)
            {
                return troup;
            }

            currentIndex++;
        }

        return null;
    }

    public IEnumerator IAactionCountdown()
    {
        while (timeBeforeNextAction > 0)
        {
            timeBeforeNextAction -= Time.deltaTime;
            yield return null;
        }
        timeBeforeNextAction = 0f;
    }

    public float FormationSpeed()
    {
        float maxSpeed = Mathf.Infinity;

        List<GameObject> selection = selectionManager.getCurrentSelection();

        foreach (GameObject troup in selection)
        {
            if (troup != null)
            {
                float troupSpeed = troup.GetComponent<Troup>().getMaxSpeed();
                if (troupSpeed < maxSpeed)
                {
                    maxSpeed = troupSpeed;
                }
            }
            
        }

        return maxSpeed;
    }

    public IEnumerator TurnTo(Vector3 targetPosition)
    {
        // Calculate the rotation needed to face the target position
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

        // Time elapsed
        float elapsedTime = 0f;

        while (elapsedTime < turnTime)
        {
            // Interpolate between start and target rotations
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / turnTime);

            // Update the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the final rotation is the target rotation
        transform.rotation = targetRotation;
    }

    private void OnDrawGizmos()
    {
        if (selectionManager != null && selectionManager.isSelected(this.gameObject))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            //Gizmos.color = Color.red;
            //Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // Attack and ability -----------------------------------------------------------------------------------------
    protected void AttackBehaviour()
    {
        if (!gameManager.hasGameStarted()) { return;  }

        // If King is present, attack it instead
        if (hasCrown) { return; }

        if (troupType == TroupType.Enemy && gameManager.isCrownCollected && gameManager.king != null)
        {
            AttackKingBehaviour();
            return;
        }
        // Debug.Log(gameObject + " - 1 - King is not present");

        // Find all enemies in detection range
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, troupMask);
        foreach (Collider collider in colliders)
        {
            if (!detectedEnemies.Contains(collider.gameObject))
            {
                Troup detectedTroup = collider.GetComponent<Troup>();
                if (detectedTroup.troupType != troupType && detectedTroup.isVisible) { detectedEnemies.Add(detectedTroup.gameObject); }
            }
        }

        // Find the closest enemy in detection range
        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        HashSet<GameObject> excludedDetectedEnemies = new HashSet<GameObject>();
        foreach (GameObject enemy in detectedEnemies)
        {
            if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) > detectionRange || (enemy != null && !enemy.GetComponent<Troup>().isVisible))
            {
                excludedDetectedEnemies.Add(enemy);
                continue;
            }
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        detectedEnemies.ExceptWith(excludedDetectedEnemies);
        // Debug.Log(gameObject + " - 2 - Closest enemy " + closestEnemy);

        // Follow closestEnemy until it's in range
        if (closestEnemy != null && !isFollowingEnemy && !isFollowingOrders)
        {
            isFollowingEnemy = true;
            currentFollowedTroup = closestEnemy;
            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();

            // Debug.Log(gameObject + " -3 - Getting closer to enemy : " + closestEnemy);
            StopCoroutine(currentActionCoroutine);
            actionQueue.Enqueue(new MoveToUnit(agent, closestEnemy, attackRange));
            StartCoroutine(currentActionCoroutine);
        }

        // Clear currentFollowedTroup if it's too far or die
        if (currentFollowedTroup != null && Vector3.Distance(transform.position, currentFollowedTroup.transform.position) - detectionRange > 0 || currentFollowedTroup != closestEnemy)
        {
            // Debug.Log(gameObject + " - 4 - Current followed troup " + currentFollowedTroup + " is too far");
            currentFollowedTroup = null;
        }
        if (currentFollowedTroup == null)
        {
            isFollowingEnemy = false;
        }

        // Clear currentFollowedTroup and currentAttackedTroup if it becomes invisible
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

        // Find all enemies in attack range
        colliders = Physics.OverlapSphere(transform.position, attackRange, troupMask);
        foreach (Collider collider in colliders)
        {
            // Debug.Log(gameObject + " - 5 - I have detected " + collider.gameObject + " in attackRange.");
            if (!inRangeEnemies.Contains(collider.gameObject))
            {
                Troup detectedTroup = collider.GetComponent<Troup>();
                if (detectedTroup.troupType != troupType && detectedTroup.isVisible) { inRangeEnemies.Add(detectedTroup.gameObject); }
            }
        }

        // Find closest enemy in attack range
        float closestDistanceInRange = Mathf.Infinity;
        GameObject closestEnemyInRange = null;
        HashSet<GameObject> excludedInRangeEnemies = new HashSet<GameObject>();
        foreach (GameObject enemy in inRangeEnemies)
        {
            // Debug.Log(gameObject + " - 6 - Enemis in range : " + enemy);

            if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) - attackRange - enemy.GetComponent<NavMeshAgent>().radius > 0)
            {
                // Debug.Log("Excluded enemy because distance = " + Vector3.Distance(transform.position, enemy.transform.position) + " and distance to check = " + (attackRange + enemy.GetComponent<NavMeshAgent>().radius + .2f));
                excludedInRangeEnemies.Add(enemy);
                continue;
            }
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance <= closestDistanceInRange)
                {
                    closestDistanceInRange = distance;
                    closestEnemyInRange = enemy;
                }
            }
        }
        inRangeEnemies.ExceptWith(excludedInRangeEnemies);
        // Debug.Log(gameObject + " - 7 - Closest enemy in range : " + closestEnemyInRange);

        // Starts attacking closest enemy in range
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
                // Debug.Log("Je lance une nouvelle attaque contre + " + currentAttackedTroup);

                StopCoroutine(currentActionCoroutine);
                actionQueue.Enqueue(new Standby());
                StartCoroutine(currentActionCoroutine);

                StartCoroutine(TurnTo(currentAttackedTroup.transform.position));
                attackCoroutine = Attack(currentAttackedTroup.GetComponent<Troup>());
                StartCoroutine(attackCoroutine);
                isAttackingEnemy = true;
            }
        }

        // Clear current attacked troup if no ennemies are in range (and therefore current attacked troup is not in range)
        if (closestEnemyInRange == null && currentAttackedTroup != null && currentAttackedTroup.GetComponent<Troup>().unitType != Troup.UnitType.Mur)
        {
            // Debug.Log("BBBBBB");
            currentAttackedTroup = null;
            isAttackingEnemy = false;
        }

        // Look at current attacked troup
        if (currentAttackedTroup != null)
        {
            if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(currentAttackedTroup.transform.position - transform.position)) > 10)
            {
                StartCoroutine(TurnTo(currentAttackedTroup.transform.position));
            }

        }
    }

    protected void AttackKingBehaviour()
    {
        GameObject king = gameManager.king;

        // Sets king as closest enemy
        GameObject closestEnemy = king;

        // Follow closestEnemy until it's in range
        if (closestEnemy != null && !isFollowingEnemy && !isFollowingOrders)
        {
            isFollowingEnemy = true;
            currentFollowedTroup = closestEnemy;
            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();

            // Debug.Log("Getting closer to enemy : " + closestEnemy);
            StopCoroutine(currentActionCoroutine);
            actionQueue.Enqueue(new MoveToUnit(agent, closestEnemy, attackRange));
            StartCoroutine(currentActionCoroutine);
        }

        // Clear currentFollowedTroup if it's too far or die
        // Debug.Log("Distance to king = " + Vector3.Distance(transform.position, currentFollowedTroup.transform.position) + " and attackrange = " + attackRange);
        if (currentFollowedTroup != null && Vector3.Distance(transform.position, currentFollowedTroup.transform.position) - attackRange - king.GetComponent<NavMeshAgent>().radius > 0)
        {
            currentFollowedTroup = null;
        }
        if (currentFollowedTroup == null)
        {
            isFollowingEnemy = false;
        }

        // Clear currentFollowedTroup and currentAttackedTroup if it becomes invisible
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

        // Sets king as closest enemy in range if in range
        GameObject closestEnemyInRange = null;
        if (Vector3.Distance(transform.position, king.transform.position) - attackRange - king.GetComponent<NavMeshAgent>().radius < 0)
        {
            closestEnemyInRange = king;
        }

        // Starts attacking closest enemy in range
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
                // Debug.Log("Je lance une nouvelle attaque contre + " + currentAttackedTroup);

                StopCoroutine(currentActionCoroutine);
                actionQueue.Enqueue(new Standby());
                StartCoroutine(currentActionCoroutine);

                attackCoroutine = Attack(currentAttackedTroup.GetComponent<Troup>());
                StartCoroutine(attackCoroutine);
                isAttackingEnemy = true;
            }
        }

        Debug.Log("--- " + (currentAttackedTroup.GetComponent<Troup>().unitType != Troup.UnitType.Mur));
        // Clear current attacked troup if no ennemies are in range (and therefore current attacked troup is not in range)
        if (closestEnemyInRange == null && currentAttackedTroup.GetComponent<Troup>().unitType != Troup.UnitType.Mur)
        {
            currentAttackedTroup = null;
            isAttackingEnemy = false;
        }

        // Look at current attacked troup
        if (currentAttackedTroup != null)
        {
            Vector3 targetPosition = currentAttackedTroup.transform.position;
            targetPosition.y = transform.position.y;

            transform.LookAt(targetPosition);
        }
    }

    public void BecomesKing()
    {
        actionQueue.Clear();
        agent.isStopped = true;
        agent.ResetPath();
        StopCoroutine(currentActionCoroutine);
        actionQueue.Enqueue(new Standby());
        StartCoroutine(currentActionCoroutine);
    }

    protected abstract IEnumerator Attack(Troup enemy);

    protected abstract void IAEnemy();

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
        StartCoroutine(ShowDamages(Mathf.Max(damage - armor, 0)));

        ParticleSystem damageParticle = Instantiate(gameManager.DamageParticlePrefab.GetComponent<ParticleSystem>(), transform.position, Quaternion.identity);
        damageParticle.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        damageParticle.Play();
        Destroy(damageParticle.gameObject, damageParticle.main.duration);

        float newHealth = health;
        // Debug.Log("J'ai pris dégat : " + damage);
        // Debug.Log("J'ai " + health + " vie");

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
            // selectionManager.removeObject(gameObject);
            Destroy(gameObject);
        } else
        {
            StartCoroutine(UpdateHealthBar());
        }
    }


    public IEnumerator ShowDamages(float damages)
    {
        if ( damages < 0f )
        {
            damageText.text = "- " + damages.ToString();
        }
        yield return new WaitForSeconds(showTimeDamages);
        damageText.text = "";
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
        if (unitType == Troup.UnitType.Mur) { return; }

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
            // Debug.Log("Stopping Troup");

            actionQueue.Enqueue(action);

            // Debug.Log("Actions en queue apr�s 2 : " + actionQueue.Count);

            // Debug.Log("Starting new ExecuteActionQueue Coroutine");
            StartCoroutine(currentActionCoroutine);
        }
    }

    protected class Standby : IAction
    {
        public bool IsActionComplete { get; set; }

        public void Execute()
        {
            // Debug.Log("Standby pendant 1 seconde");
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
            // Debug.Log("Patroling executed between position " + firstPosition + " and " + secondPosition);
        }
        public IEnumerator StartPatroling()
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(firstPosition);

            // Debug.Log("Distance initiale : ");

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

            // Debug.Log("Movement complete");
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
            // Debug.Log("Move Action executed to position: " + targetPosition);
        }

        public IEnumerator GoToPosition()
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(targetPosition);

            // Debug.Log("Distance initiale : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));

            navMeshAgent.transform.GetComponent<Troup>().moveTargetDestination = targetPosition;

            yield return new WaitWhile(() => navMeshAgent.path.status == NavMeshPathStatus.PathComplete && !navMeshAgent.isStopped && Vector3.Distance(navMeshAgent.transform.position, targetPosition) > positionThreshold);

            if (navMeshAgent.path.status != NavMeshPathStatus.PathComplete)
            {
                NavMeshHit navMeshHit;
                if (NavMesh.Raycast(navMeshAgent.transform.position, targetPosition, out navMeshHit, NavMesh.AllAreas))
                {
                    Vector3 wallPosition = navMeshHit.position;
                    navMeshAgent.SetDestination(wallPosition);
                    Troup troup = navMeshAgent.GetComponent<Troup>();

                    yield return new WaitWhile(() => !navMeshAgent.isStopped && Vector3.Distance(navMeshAgent.transform.position, wallPosition) > troup.attackRange);
                    

                    //Debug.DrawRay(navMeshAgent.transform.position, navMeshAgent.transform.forward, Color.red, 10f);
                    RaycastHit hit_wall;
                    if (Physics.Raycast(navMeshAgent.transform.position, navMeshAgent.transform.forward, out hit_wall, Mathf.Infinity))
                    {
                        Wall wallToAttack = hit_wall.transform.parent.GetComponent<Wall>();

                        if (troup.troupType != wallToAttack.troupType)
                        {
                            troup.attackCoroutine = troup.Attack(wallToAttack);
                            troup.AddAction(new Standby());
                            troup.currentAttackedTroup = wallToAttack.gameObject;
                            troup.StartCoroutine(troup.attackCoroutine);

                            yield return new WaitWhile(() => wallToAttack != null);

                            troup.actionQueue.Enqueue(new MoveToPosition(navMeshAgent, targetPosition, positionThreshold));
                        }
                    }
                }
            }
            
            Debug.Log("I arrived at destination ! My position is " + navMeshAgent.transform.position + " and destination is " + targetPosition);

            if (Vector2.Distance(new Vector2(navMeshAgent.transform.position.x, navMeshAgent.transform.position.z), new Vector2(navMeshAgent.transform.GetComponent<Troup>().moveTargetDestination.x, navMeshAgent.transform.GetComponent<Troup>().moveTargetDestination.z)) <= positionThreshold)
            {
                navMeshAgent.isStopped = true;
            }
            
            IsActionComplete = true;

            // Debug.Log("Movement complete");
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
            // Debug.Log("Following Unit : " + unitToFollow);
        }

        public IEnumerator StartFollowing()
        {
            Vector3 targetPosition = new Vector3();
            if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }

            // if (unitToFollow != null)

            // Debug.Log("Following unit at position : " + targetPosition);
            
            // navMeshAgent.SetDestination(targetPosition);

            while (unitToFollow != null && Vector3.Distance(navMeshAgent.transform.position, targetPosition) >= unitToFollow.transform.GetComponent<Troup>().positionThreshold)
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

            // Debug.Log("Movement complete");
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
            // Debug.Log("Moving to Unit : " + unitToFollow);
        }

        public IEnumerator StartFollowing()
        {
            Vector3 targetPosition = new Vector3();
            if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }

            // if (unitToFollow != null)

            

            // navMeshAgent.SetDestination(targetPosition);

            Troup unitTypeToFollow = unitToFollow.GetComponent<Troup>();
            float distanceToCheck = (unitTypeToFollow.agent != null) ? range + unitTypeToFollow.agent.radius : range;
            //(unitTypeToFollow == UnitType.Cavalier || unitTypeToFollow == UnitType.Catapulte || unitTypeToFollow == UnitType.Belier) ? range : range;

            // Debug.Log("Following unit at position : " + targetPosition + " et distanceToCheck : " + distanceToCheck + " et disance = " + Vector2.Distance(new Vector2(navMeshAgent.transform.position.x, navMeshAgent.transform.position.z), new Vector2(targetPosition.x, targetPosition.z)));

            Debug.Log("!!! " + distanceToCheck);
            while (unitToFollow != null && navMeshAgent != null && Vector2.Distance(new Vector2(navMeshAgent.transform.position.x, navMeshAgent.transform.position.z), new Vector2(targetPosition.x, targetPosition.z)) - distanceToCheck > 0 && unitToFollow.GetComponent<Troup>().isVisible)
            {
                Debug.Log("!!! Dans le while");

                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                navMeshAgent.isStopped = false;
                if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }
                // Debug.Log(navMeshAgent.gameObject + " Going to enemy " + targetPosition + " et distance = " + Vector2.Distance(new Vector2(navMeshAgent.transform.position.x, navMeshAgent.transform.position.z), new Vector2(targetPosition.x, targetPosition.z)) + " et distance à checker = " + distanceToCheck);
                navMeshAgent.SetDestination(targetPosition);

                yield return null;
            }

            navMeshAgent.SetDestination(navMeshAgent.transform.position);
            navMeshAgent.isStopped = true;
            // navMeshAgent.ResetPath();
            IsActionComplete = true;

            // Debug.Log("Movement complete");
        }
    }

    // Action Queue execution -------------------------------------------------------------------------------------
    protected IEnumerator ExecuteActionQueue()
    {
        // Debug.Log("Start Action Queue");
        bool hasToStop = false;

        while (actionQueue.Count > 0 && !hasToStop)
        {
            IAction currentAction = actionQueue.Dequeue();

            isFollowingOrders = false;

            // Affichage des actions de la queue
            if (troupType == TroupType.Ally)
            {
                if (QueueUI != null)
                {
                    QueueUI.GetComponent<TextMeshProUGUI>().text = "Current action: " + (currentAction.ToString().StartsWith("Troup+") ? currentAction.ToString().Substring("Troup+".Length) : currentAction.ToString());

                    string queueText = "";

                    foreach (IAction action in actionQueue)
                    {
                        // Debug.Log("Action " + action + " ajout�e � la liste");
                        queueText += "\n" + (action.ToString().StartsWith("Troup+") ? action.ToString().Substring("Troup+".Length) : action.ToString());
                    }
                    QueueUI.GetComponent<TextMeshProUGUI>().text += "\n Enqueued action: ";
                    QueueUI.GetComponent<TextMeshProUGUI>().text += queueText;
                }
            }

            // Debug.Log("Treating action : " + currentAction);

            isPatroling = false;
            // Debug.Log("isPatroling false");

            if (currentAction is MoveToPosition moveToPosition)
            {
                isFollowingOrders = true;
                StartCoroutine(moveToPosition.GoToPosition());
            }
            if (currentAction is Patrol patrol)
            {
                StartCoroutine(patrol.StartPatroling());
                isPatroling = true;
                // Debug.Log("isPatroling true");
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

            // Debug.Log("Actions en queue : " + actionQueue.Count);

            if (actionQueue.Count == 0)
            {
                if (currentAction is Patrol)
                {
                    // Debug.Log("Action en cours : " + currentAction);
                    currentAction.IsActionComplete = false;
                    actionQueue.Enqueue(currentAction);
                }
                else if (currentAction is FollowUnit)
                {
                    // Debug.Log("Action en cours : " + currentAction);
                    currentAction.IsActionComplete = false;
                    actionQueue.Enqueue(new Standby());
                    actionQueue.Enqueue(currentAction);
                }
                else if (!isPatroling)
                {
                    // Debug.Log("Ajout du standby");
                    actionQueue.Enqueue(new Standby());
                }
            }

            

            // Debug.Log("Action termin�e");
            // Debug.Log("Actions en queue apr�s : " + actionQueue.Count);

        }

        // Debug.Log("Execute Action Queue Coroutine ended");

        yield break;
        
    }
}