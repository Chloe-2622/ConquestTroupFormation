using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public abstract class Troup : MonoBehaviour
{

    [Header("General stats")]
    [SerializeField] protected float movingSpeed;
    [SerializeField] protected float health;
    [SerializeField] protected float armor;

    [Header("Attack stats")]
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float attackRechargeTime;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float specialAttackRechargeTime;

    public enum TroupType { Ally, Ennemy }

    [SerializeField] protected TroupType troupType;
    [SerializeField] protected Camera camera1;

    public static HashSet<Troup> Allies = new HashSet<Troup>();
    public static HashSet<Troup> Ennemies = new HashSet<Troup>();

    [SerializeField] protected TextMeshProUGUI TroupSelectionPopUp;
    [SerializeField] protected TextMeshProUGUI PlaceSelectionPopUp;
    [SerializeField] protected TextMeshProUGUI PatrolSelectionPopUp1;
    [SerializeField] protected TextMeshProUGUI PatrolSelectionPopUp2;

    private Transform SelectionCircle;
    private Queue<IAction> actionQueue = new Queue<IAction>();

    private bool isActionQueueCoroutineRunning;

    [SerializeField] protected SelectionManager selectionManager;

    protected NavMeshAgent agent;


    protected virtual void Awake() 
    {
        if (troupType == TroupType.Ally)
        {
            Allies.Add(this);
            selectionManager.completeDictionnary(transform.gameObject);
        }
        if (troupType == TroupType.Ennemy)
        {
            Ennemies.Add(this);
        }

        TroupSelectionPopUp.enabled = false;
        PlaceSelectionPopUp.enabled = false;
        agent = GetComponent<NavMeshAgent>();

        SelectionCircle = transform.Find("SelectionCircle");

        AddAction(new Standby());
        StartCoroutine(ExecuteActionQueue());

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Debug.Log(selectionManager.isSelected(transform.gameObject));

        if (selectionManager.isSelected(this.gameObject))
        {
            SelectionCircle.GetComponent<MeshRenderer>().enabled = true;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(PlaceSelection());                
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(PatrolSelection());
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AddAction(new Standby());
            }

        } else
        {
            SelectionCircle.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    protected IEnumerator PlaceSelection()
    {
        PlaceSelectionPopUp.enabled = true;
        bool hasSelected = false;

        Debug.Log("Enabling Place Selection");

        while (!hasSelected)
        {
            PlaceSelectionPopUp.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);
            // Debug.Log("Est enabled : " + PlaceSelectionPopUp.enabled);

            if (Input.GetMouseButton(0))
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
            }
            
            yield return null;
        }

        
    }

    protected IEnumerator PatrolSelection()
    {
        PatrolSelectionPopUp1.enabled = true;
        bool hasSelectedFistPos = false;
        bool hasSelectedSecondPos = false;

        Vector3 firstPos = new Vector3();
        Vector3 secondPos = new Vector3();

        Debug.Log("Enabling Patrol Selection");

        while (!hasSelectedFistPos)
        {
            PatrolSelectionPopUp1.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Debug.Log("Target position clicked : " + hit.point);
                    firstPos = hit.point;
                }

                hasSelectedFistPos = true;
                PatrolSelectionPopUp1.enabled = false;
            }

            yield return null;
        }

        PatrolSelectionPopUp2.enabled = true;

        while (!hasSelectedSecondPos)
        {
            PatrolSelectionPopUp2.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Debug.Log("Target position clicked : " + hit.point);
                    secondPos = hit.point;
                }

                hasSelectedSecondPos = true;
                PatrolSelectionPopUp2.enabled = false;
            }

            yield return null;
        }

        Debug.Log("Starting patroling between " + firstPos + " and " + secondPos);
        AddAction(new Patrol(agent, firstPos, secondPos));
    }

    protected void AddAction(IAction action)
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            actionQueue.Enqueue(action);

        } else
        {
            actionQueue.Clear();
            isActionQueueCoroutineRunning = false;
            GetComponent<NavMeshAgent>().isStopped = true;
            StopAllCoroutines();
            // StopAllCoroutines(ExecuteActionQueue());
            Debug.Log("Stopping Troup");

            actionQueue.Enqueue(action);

            Debug.Log("Actions en queue après 2 : " + actionQueue.Count);

            Debug.Log("Starting new ExecuteActionQueue Coroutine");
            StartCoroutine(ExecuteActionQueue());
        }
    }

    // public abstract void Attack();

    public virtual void Follow()
    {
        TroupSelectionPopUp.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 10, Input.mousePosition.z);
        TroupSelectionPopUp.enabled = true;
    }

    public virtual Troup FindNearestEnnemy()
    {
        Vector3 pos = transform.position;
        float dist = float.PositiveInfinity;
        Troup targ = null;

        if (troupType == TroupType.Ally)
        {
            foreach (var ennemy in Ennemies)
            {
                var d = (pos - ennemy.transform.position).sqrMagnitude;
                if (d < dist)
                {
                    targ = ennemy;
                    dist = d;
                }
            }
        }
        if (troupType == TroupType.Ennemy)
        {
            foreach (var ally in Allies)
            {
                var d = (pos - ally.transform.position).sqrMagnitude;
                if (d < dist)
                {
                    targ = ally;
                    dist = d;
                }
            }
        }

        return targ;
    }

    protected interface IAction
    {
        bool IsActionComplete { get; set; }
        bool IsStandBy { get; set; }
        public void Execute();
    }

    protected class Standby : IAction
    {
        public bool IsActionComplete { get; set; }
        public bool IsStandBy { get; set; }

        public void Execute()
        {
            Debug.Log("Standby pendant 1 seconde");
        }

        public IEnumerator StandbyWait()
        {
            yield return new WaitForSeconds(1f);
            if (IsStandBy) { IsActionComplete = true; }
        }
    }

    protected class Patrol : IAction
    {
        public bool IsActionComplete { get; set; }
        public bool IsStandBy { get; set; }
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

            // Debug.Log("Distance initiale : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));

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
        public bool IsStandBy { get; set; }
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
            Vector3 startingPosition = navMeshAgent.transform.position;

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(targetPosition);

            Debug.Log("Distance initiale : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));

            while (Vector3.Distance(navMeshAgent.transform.position, targetPosition) >= 1f)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                yield return null;
            }

            IsActionComplete = true;

            Debug.Log("Movement complete");
        }
    }

    protected IEnumerator ExecuteActionQueue()
    {
        Debug.Log("Start Action Queue");
        isActionQueueCoroutineRunning = true;

        while (actionQueue.Count > 0)
        {
            IAction currentAction = actionQueue.Dequeue();

            Debug.Log("Treating action : " + currentAction);

            if (currentAction is MoveToPosition moveToPosition)
            {
                currentAction.IsStandBy = false;
                StartCoroutine(moveToPosition.GoToPosition());
            }
            if (currentAction is Patrol patrol)
            {
                currentAction.IsStandBy = false;
                StartCoroutine(patrol.StartPatroling());
            }
            if (currentAction is Standby standby)
            {
                currentAction.IsStandBy = true;
                StartCoroutine(standby.StandbyWait());
            }

            yield return new WaitUntil(() => currentAction.IsActionComplete);

            Debug.Log("Actions en queue : " + actionQueue.Count);

            

            if (currentAction is Patrol && actionQueue.Count == 0)
            {
                Debug.Log("Action en cours : " + currentAction);
                currentAction.IsActionComplete = false;
                actionQueue.Enqueue(currentAction);
            }
            if (currentAction is not Patrol && actionQueue.Count == 0)
            {
                actionQueue.Enqueue(new Standby());
            }

            Debug.Log("Action terminée");
            Debug.Log("Actions en queue après : " + actionQueue.Count);

        }

        Debug.Log("Execute Action Queue Coroutine ended");

        isActionQueueCoroutineRunning = false;
        yield break;
        
    }
}