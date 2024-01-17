using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public abstract class Troup : MonoBehaviour
{

    [Header("General stats")]
    [SerializeField] protected TroupType troupType;
    [SerializeField] protected UnitType unitType;
    [SerializeField] protected bool isSelected;
    [SerializeField] protected bool hasCrown = false;
    [SerializeField] protected float movingSpeed;
    [SerializeField] protected float health;
    [SerializeField] protected float armor;
    [SerializeField] protected float detectionRange;

    [Header("Attack stats")]
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackRechargeTime;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float specialAbilityRechargeTime;
    [SerializeField] protected float specialAbilityDelay = 0f;

    [Header("Scene objects")]
    [SerializeField] protected Camera camera1;
    [SerializeField] protected SelectionManager selectionManager;
    [SerializeField] protected Image healthBar;
    [SerializeField] protected Image flecheUI;
    [SerializeField] protected GameObject selectionArrow;
    [SerializeField] protected GameObject tombe;
    [SerializeField] protected Transform SelectionCircle;
    [SerializeField] protected Transform SelectionArrow;
    [SerializeField] protected GameObject FirstPatrolPoint;
    [SerializeField] protected GameObject SecondPatrolPoint;
    [SerializeField] protected GameObject SelectionParticleCircle;
    [SerializeField] protected GameObject QueueUI;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected LayerMask troupMask;


    [Header("Text PopUps")]
    [SerializeField] protected TextMeshProUGUI TroupSelectionPopUp;
    [SerializeField] protected TextMeshProUGUI PlaceSelectionPopUp;
    [SerializeField] protected TextMeshProUGUI PatrolSelectionPopUp1;
    [SerializeField] protected TextMeshProUGUI PatrolSelectionPopUp2;
    [SerializeField] protected TextMeshProUGUI FollowSelectionPopUp;

    // Allies and Ennemis dictionnary -----------------------------------------------------------------------------
    public static HashSet<Troup> Allies = new HashSet<Troup>();
    public static HashSet<Troup> Ennemies = new HashSet<Troup>();

    // Troup types ------------------------------------------------------------------------------------------------
    public enum TroupType { Ally, Ennemy }
    public enum UnitType
    {
        Combattant, Archer, Cavalier, Guerisseur, Catapulte, Porte_bouclier, Porte_etendard, Batisseur, Belier
    }

    // Action Queue -----------------------------------------------------------------------------------------------
    private Queue<IAction> actionQueue = new Queue<IAction>();

    // Private variables ------------------------------------------------------------------------------------------
    private bool isChosingPlacement;
    [SerializeField] private bool isChosingPatrol;
    private bool isChosingFollow;
    [SerializeField] private bool isFollowingEnnemy;
    [SerializeField] private bool isAttackingEnnemy;
    private bool hasSpawnedTombe;
    private float maxHealth;
    [SerializeField] private bool isPatroling;
    private Vector3 FirstPatrolPosCo;
    private Vector3 SecondPatrolPosCo;
    [SerializeField] private GameObject currentAttackedTroup;
    [SerializeField] private GameObject currentFollowedTroup;
    private bool enqueuingNewAction;
    private bool isPlayingCircleAnim;
    private IEnumerator currentActionCoroutine;
    private IEnumerator attackCoroutine;

    // TODO : Cleanup variables + Utiliser le GameManager et le Awake pour ne pas avoir de Serialized Field � modifier � la mano
    // TODO : Capa sp�cial Combattant


    // Awake
    protected virtual void Awake() 
    {
        // Children variable setup
        SelectionCircle = transform.Find("SelectionCircle");
        healthBar = transform.Find("Canvas").Find("Vie").GetComponent<Image>();

        if (troupType == TroupType.Ally)
        {
            flecheUI = transform.Find("Canvas").Find("Fleche").GetComponent<Image>();
            QueueUI = transform.Find("Canvas").Find("Queue").gameObject;
        }

        // Setup and show Health Bar
        maxHealth = health;
        healthBar.enabled = true;

        // Agent setup
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movingSpeed;

        // GameManager variable setup
        Debug.Log("GameManager of " + troupType + " " + unitType + " = " + GameManager.Instance);
        camera1 = GameManager.Instance.mainCamera;
        selectionManager = GameManager.Instance.selectionManager;
        SelectionArrow = GameManager.Instance.selectionArrow;
        tombe = GameManager.Instance.tombe;
        TroupSelectionPopUp = GameManager.Instance.TroupSelectionPopUp;
        PlaceSelectionPopUp = GameManager.Instance.PlaceSelectionPopUp;
        PatrolSelectionPopUp1 = GameManager.Instance.PatrolSelectionPopUp1;
        PatrolSelectionPopUp2 = GameManager.Instance.PatrolSelectionPopUp2;
        FollowSelectionPopUp = GameManager.Instance.FollowSelectionPopUp;
        FirstPatrolPoint = Instantiate(GameManager.Instance.FirstPatrolPointPrefab, GameManager.Instance.PatrolingCircles.transform);
        SecondPatrolPoint = Instantiate(GameManager.Instance.SecondPatrolPointPrefab, GameManager.Instance.PatrolingCircles.transform);
        SelectionParticleCircle = Instantiate(GameManager.Instance.SelectionParticleCirclePrefab, GameManager.Instance.SelectionParticleCircles.transform);
        troupMask = GameManager.Instance.troupMask;

        // Ally or Ennemy
        if (troupType == TroupType.Ally)
        {
            Allies.Add(this);
            selectionManager.completeDictionnary(transform.gameObject);
        }
        if (troupType == TroupType.Ennemy)
        {
            Ennemies.Add(this);
        }

        // Start Action Queue
        currentActionCoroutine = ExecuteActionQueue();
        AddAction(new Standby());
        StartCoroutine(currentActionCoroutine);
    }

    // Update
    protected virtual void Update()
    {
        SelectedBehaviour();

        HealthBarControl();
    }

    // SelectedBehaviour
    protected virtual void SelectedBehaviour()
    {
        isSelected = selectionManager.isSelected(this.gameObject);

        if (selectionManager.isSelected(this.gameObject))
        {
            // SelectionCircle.GetComponent<MeshRenderer>().enabled = true;
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
                    // FirstPatrolPos.transform.position = FirstPatrolPosCo;
                    // SecondPatrolPos.transform.position = SecondPatrolPosCo;

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

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (specialAbilityDelay == 0f)
                {
                    StartCoroutine(SpecialAbility());
                    specialAbilityDelay = specialAbilityRechargeTime;
                    StartCoroutine(SpecialAbilityCountdown());
                }
                if (specialAbilityDelay == -1f)
                {
                    StartCoroutine(SpecialAbility());
                    specialAbilityDelay = Mathf.Infinity;
                }
            }

        }
        else
        {
            SelectionCircle.GetComponent<MeshRenderer>().enabled = false;
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

            // flecheUI.enabled = true;
            
            // Debug.Log("Est enabled : " + PlaceSelectionPopUp.enabled);

            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                SelectionArrow.transform.position = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);
                flecheUI.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y + 15);

                if (Input.GetMouseButton(0))
                {
                    Debug.Log("Target position clicked : " + hit.point);
                    AddAction(new MoveToPosition(agent, hit.point));
                    hasSelected = true;
                    flecheUI.enabled = false;
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
                FirstPatrolPosCo = new Vector3(firstPos.x, firstPos.y + 0.1f, firstPos.z);
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
                SecondPatrolPosCo = new Vector3(secondPos.x, secondPos.y + 0.1f, secondPos.z);
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
        while (t < 1f && healthBar.fillAmount != targetFillAmount)
        {
            t += Time.deltaTime / 0.5f; // 0.5f est la dur�e de la transition en secondes, ajustez selon vos besoins
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFillAmount, t);
            Debug.Log("Je passe le fillAmount � : " + healthBar.fillAmount);
            yield return null;
        }

        healthBar.fillAmount = targetFillAmount;
    }

    protected void HealthBarControl()
    {
        // Health Bar control
        float normalizedHealth = health / maxHealth;
        healthBar.enabled = !GameManager.Instance.isInPause();

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
        
        // Find all ennemies in detection range
        HashSet<GameObject> detectedEnnemies = new HashSet<GameObject>();
        if (troupType == TroupType.Ally)
        {
            foreach(Troup ennemy in Ennemies)
            {
                if (Vector3.Distance(transform.position, ennemy.transform.position) <= detectionRange)
                {
                    detectedEnnemies.Add(ennemy.gameObject);
                }
            }
        } else
        {
            foreach (Troup ally in Allies)
            {
                if (Vector3.Distance(transform.position, ally.transform.position) <= detectionRange)
                {
                    detectedEnnemies.Add(ally.gameObject);
                }
            }
        }


        // Find the closest ennemy
        float closestDistance = Mathf.Infinity;
        GameObject closestEnnemy = null;

        foreach (GameObject ennemy in detectedEnnemies)
        {
            float distance = Vector3.Distance(transform.position, ennemy.transform.position);

            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestEnnemy = ennemy;
            }
        }

        // Follow closestEnnemy until it's in range

        if (closestEnnemy != null && !isFollowingEnnemy)
        {
            isFollowingEnnemy = true;
            currentFollowedTroup = closestEnnemy;
            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();

            Debug.Log("Getting closer to ennemy : " + closestEnnemy);
            StopCoroutine(currentActionCoroutine);
            actionQueue.Enqueue(new MoveToEnnemy(agent, closestEnnemy, attackRange));
            StartCoroutine(currentActionCoroutine);

            
        }
        if (currentFollowedTroup != null && Vector3.Distance(transform.position, currentFollowedTroup.transform.position) >= detectionRange)
        {
            currentFollowedTroup = null;
        }
        if (currentFollowedTroup == null)
        {
            isFollowingEnnemy = false;
        }

        
        

        HashSet<GameObject> inRangeEnnemies = new HashSet<GameObject>();
        if (troupType == TroupType.Ally)
        {
            foreach (Troup ennemy in Ennemies)
            {
                if (Vector3.Distance(transform.position, ennemy.transform.position) <= attackRange)
                {
                    inRangeEnnemies.Add(ennemy.gameObject);
                }
            }
        }
        else
        {
            foreach (Troup ally in Allies)
            {
                if (Vector3.Distance(transform.position, ally.transform.position) <= attackRange)
                {
                    inRangeEnnemies.Add(ally.gameObject);
                }
            }
        }

        float closestDistanceInRange = Mathf.Infinity;
        GameObject closestEnnemyInRange = null;

        foreach (GameObject ennemy in inRangeEnnemies)
        {
            float distance = Vector3.Distance(transform.position, ennemy.transform.position);

            if (distance <= closestDistanceInRange)
            {
                closestDistanceInRange = distance;
                closestEnnemyInRange = ennemy;
            }
        }

        if (currentAttackedTroup == null)
        {
            isAttackingEnnemy = false;
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }
            
            if (closestEnnemyInRange != null) 
            {
                currentAttackedTroup = closestEnnemyInRange;

                actionQueue.Clear();
                agent.isStopped = true;
                agent.ResetPath();
                Debug.Log("Stopping path");

                StopCoroutine(currentActionCoroutine);
                actionQueue.Enqueue(new Standby());
                StartCoroutine(currentActionCoroutine);

                attackCoroutine = Attack(currentAttackedTroup.GetComponent<Troup>());
                StartCoroutine(attackCoroutine);
                isAttackingEnnemy = true;
            }
        }

        if (troupType == TroupType.Ennemy && currentAttackedTroup == null && currentFollowedTroup != null)
        {
            isFollowingEnnemy = false;
        }

        /* if (currentAttackedTroup == null && closestEnnemyInRange != null && !isAttackingEnnemy)
        {
            currentAttackedTroup = closestEnnemyInRange;
            StartCoroutine(Attack(currentAttackedTroup.GetComponent<Troup>()));
            isAttackingEnnemy = true;
        } */
        if (closestEnnemyInRange == null)
        {
            currentAttackedTroup = null;
            isAttackingEnnemy = false;
        }

    }

    protected abstract IEnumerator Attack(Troup ennemy);

    protected abstract IEnumerator SpecialAbility();

    protected IEnumerator SpecialAbilityCountdown()
    {
        while (specialAbilityDelay > 0)
        {
            specialAbilityDelay--;
            yield return new WaitForSeconds(1f);
        }
    }

    public virtual void TakeDamage(float damage)
    {
        float beforeHealth = health;

        health -= damage;

        float newHealth = health;

        Debug.Log("J'ai " + health + " vie");

        if (beforeHealth * newHealth <= 0 && !hasSpawnedTombe)
        {
            if (troupType == TroupType.Ally) { Allies.Remove(this); }
            if (troupType == TroupType.Ennemy) { Ennemies.Remove(this); }
            GameObject tombeMort = Instantiate(tombe, transform.position, transform.rotation, null);
            hasSpawnedTombe = true;
            // if (troupType == TroupType.Ennemy) { tombeMort.transform.position += Vector3.up * 3; }
            tombeMort.GetComponent<Tombe>().SetUnitType((Tombe.TombeUnitType)unitType);
            tombeMort.GetComponent<Tombe>().SetTroupType((Tombe.TombeTroupType)troupType);
            Destroy(SelectionParticleCircle);
            Destroy(FirstPatrolPoint);
            Destroy(SecondPatrolPoint);
            selectionManager.removeObject(gameObject);
            Destroy(gameObject);
        } else
        {
            StartCoroutine(UpdateHealthBar());
        }
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

        public MoveToPosition(NavMeshAgent agent, Vector3 position)
        {
            navMeshAgent = agent;
            targetPosition = position;
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

            while (Vector3.Distance(navMeshAgent.transform.position, targetPosition) >= 1f)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                yield return null;
            }

            navMeshAgent.isStopped = true;
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

    protected class MoveToEnnemy : IAction
    {
        public bool IsActionComplete { get; set; }
        public GameObject unitToFollow;
        private NavMeshAgent navMeshAgent;
        private float range;

        public MoveToEnnemy(NavMeshAgent agent, GameObject unit, float attackRange)
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

            while (unitToFollow != null && Vector3.Distance(navMeshAgent.transform.position, targetPosition) >= range)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                navMeshAgent.isStopped = false;
                if (unitToFollow != null) { targetPosition = unitToFollow.transform.position; }
                // Debug.Log("Going to ennemy " + targetPosition);
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
                StartCoroutine(followUnit.StartFollowing());
            }
            if (currentAction is MoveToEnnemy moveToEnnemy)
            {
                StartCoroutine(moveToEnnemy.StartFollowing());
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