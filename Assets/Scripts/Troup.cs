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
    [SerializeField] public UnitType unitType;
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
    [SerializeField] protected GameObject QueueUI;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected LayerMask troupMask;


    [Header("Text PopUps")]
    [SerializeField] protected TextMeshProUGUI TroupSelectionPopUp;
    [SerializeField] protected TextMeshProUGUI PlaceSelectionPopUp;
    [SerializeField] protected TextMeshProUGUI PatrolSelectionPopUp1;
    [SerializeField] protected TextMeshProUGUI PatrolSelectionPopUp2;
    [SerializeField] protected TextMeshProUGUI FollowSelectionPopUp;

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
    private bool isFollowingEnnemy;
    private bool isAttackingEnnemy;
    private bool hasSpawnedTombe;
    private float maxHealth;
    [SerializeField] private bool isPatroling;
    [SerializeField] private Vector3 FirstPatrolPosCo;
    [SerializeField] private Vector3 SecondPatrolPosCo;
    private Troup currentAttackedTroup;
    private bool enqueuingNewAction;
    private IEnumerator currentActionCoroutine;

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
        troupMask = GameManager.Instance.troupMask;

        // Ally or Ennemy
        if (troupType == TroupType.Ally)
        {
            GameManager.Instance.addAlly(this);
            selectionManager.completeDictionnary(transform.gameObject);
        }
        if (troupType == TroupType.Ennemy)
        {
            GameManager.Instance.addEnemy(this);
        }

        // Start Action Queue
        currentActionCoroutine = ExecuteActionQueue();
        AddAction(new Standby());
        StartCoroutine(currentActionCoroutine);
    }

    // Update
    protected virtual void Update()
    {
        // Special Ability
        /* if (specialAbilityDelay > 0)
        {
            specialAbilityDelay--;
        } */

        healthBar.enabled = !GameManager.Instance.isInPause();

        isSelected = selectionManager.isSelected(this.gameObject);

        if (selectionManager.isSelected(this.gameObject))
        {
            SelectionCircle.GetComponent<MeshRenderer>().enabled = true;
            
            
            if (troupType == TroupType.Ally)
            {
                if (selectionManager.numberOfSelected() == 1)
                {
                    QueueUI.SetActive(true);

                    string queueText = "";

                    /* foreach (IAction action in actionQueue)
                    {
                        Debug.Log("Action " + action + " ajoutée à la liste");
                        queueText += action.ToString();
                    } */

                    // QueueUI.GetComponent<TextMeshProUGUI>().text = "Action Queue:" + queueText;
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

        } else
        {
            SelectionCircle.GetComponent<MeshRenderer>().enabled = false;
            FirstPatrolPoint.GetComponent<Renderer>().enabled = false;
            SecondPatrolPoint.GetComponent<Renderer>().enabled = false;

            if (troupType == TroupType.Ally)
            {
                QueueUI.SetActive(false);
            }
        }

        Troup nearestEnnemy = FindNearestEnnemy();

        
        
        

        // Health Bar control

        float normalizedHealth = health / maxHealth;

        // healthBar.fillAmount = normalizedHealth;
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

        if (currentAttackedTroup == null)
        {
            isAttackingEnnemy = false;
        }

        // Find Nearby ennemy and attack them if in attack range distance
        if (nearestEnnemy != null)
        {
            

            if (!isFollowingEnnemy)
            {
                isFollowingEnnemy = true;
                actionQueue.Clear();
                agent.isStopped = true;
                agent.ResetPath();
                // StopAllCoroutines();
                StopCoroutine(ExecuteActionQueue());

                actionQueue.Enqueue(new FollowUnit(agent, nearestEnnemy.gameObject));

                StartCoroutine(ExecuteActionQueue());
            }

            if (Vector3.Distance(transform.position, nearestEnnemy.transform.position) <= attackRange)
            {
                if (!isAttackingEnnemy)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    // StopAllCoroutines();
                    StopCoroutine(ExecuteActionQueue());
                    actionQueue.Enqueue(new Standby());
                    StartCoroutine(ExecuteActionQueue());
                    
                    isAttackingEnnemy = true;
                    Debug.Log("Started attacking");
                    currentAttackedTroup = nearestEnnemy;
                    StartCoroutine(Attack(nearestEnnemy));
                }

            }
            else
            {
                isAttackingEnnemy = false;
                agent.isStopped = false;
                currentAttackedTroup = null;
                StopCoroutine(Attack(nearestEnnemy));
            }

        } else
        {
            isFollowingEnnemy = false;
            agent.isStopped = false;
        }

        AttackBehaviour();
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
            t += Time.deltaTime / 0.5f; // 0.5f est la durée de la transition en secondes, ajustez selon vos besoins
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, targetFillAmount, t);
            Debug.Log("Je passe le fillAmount à : " + healthBar.fillAmount);
            yield return null;
        }

        healthBar.fillAmount = targetFillAmount;
    }

    public float getHealth()
    {
        return health;
    }

    protected void showArrows()
    {

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
    public virtual Troup FindNearestEnnemy()
    {
        Vector3 pos = transform.position;
        float dist = detectionRange;
        Troup targ = null;

        if (troupType == TroupType.Ally)
        {
            HashSet<Troup> enemies = GameManager.Instance.getEnemies();
            foreach (var ennemy in enemies)
            {
                var d = Vector3.Distance(pos, ennemy.transform.position); // (pos - ennemy.transform.position).sqrMagnitude;
                if (d < dist)
                {
                    targ = ennemy;
                    dist = d;
                }
            }
        }
        if (troupType == TroupType.Ennemy)
        {
            HashSet<Troup>  allies = GameManager.Instance.getAllies();
            foreach (var ally in allies)
            {
                var d = Vector3.Distance(pos, ally.transform.position); //  (pos - ally.transform.position).sqrMagnitude;
                if (d < dist)
                {
                    targ = ally;
                    dist = d;
                }
            }
        }

        // Debug.Log("Nearest ennemy is far by : " + dist);

        return targ;
    }

    protected void AttackBehaviour()
    {
        Collider[] detectedEnnemies = Physics.OverlapSphere(transform.position, detectionRange, troupMask);
        foreach(Collider ennemie in detectedEnnemies)
        {
            if (ennemie.gameObject.GetComponent<Troup>().troupType == TroupType.Ennemy)
            {
                Debug.Log("Collider ennemie : " + ennemie.gameObject);
            }
            
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
            if (troupType == TroupType.Ally) { GameManager.Instance.removeAlly(this); }
            if (troupType == TroupType.Ennemy) { GameManager.Instance.removeEnemy(this);  }
            GameObject tombeMort = Instantiate(tombe, transform.position, transform.rotation, null);
            hasSpawnedTombe = true;
            // if (troupType == TroupType.Ennemy) { tombeMort.transform.position += Vector3.up * 3; }
            tombeMort.GetComponent<Tombe>().SetUnitType((Tombe.TombeUnitType)unitType);
            tombeMort.GetComponent<Tombe>().SetTroupType((Tombe.TombeTroupType)troupType);
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

            Debug.Log("Actions en queue après 2 : " + actionQueue.Count);

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
                
                navMeshAgent.SetDestination(targetPosition);

                yield return null;
            }

            navMeshAgent.ResetPath();
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
                    Debug.Log("Action " + action + " ajoutée à la liste");
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

            

            Debug.Log("Action terminée");
            Debug.Log("Actions en queue après : " + actionQueue.Count);

        }

        Debug.Log("Execute Action Queue Coroutine ended");

        yield break;
        
    }
}