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


    private Queue<IAction> actionQueue = new Queue<IAction>();

    private bool isActionQueueCoroutineRunning;

    // Start is called before the first frame update
    void Awake() 
    {
        if (troupType == TroupType.Ally)
        {
            Allies.Add(this);
        }
        if (troupType == TroupType.Ally)
        {
            Ennemies.Add(this);
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void AddAction(IAction action)
    {
        actionQueue.Enqueue(action);

        Debug.Log(isActionQueueCoroutineRunning);

        if (!isActionQueueCoroutineRunning)
        {
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

    // protected IEnumerator patrol();

    protected interface IAction
    {
        bool IsActionComplete { get; }
        public void Execute();
    }

    protected class MoveToPosition : IAction
    {
        public bool IsActionComplete { get; private set; }
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

            navMeshAgent.SetDestination(targetPosition);

            Debug.Log("Distance initiale : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));

            while (Vector3.Distance(navMeshAgent.transform.position, targetPosition) >= .1f)
            {
                // Debug.Log("Distance actuelle : " + Vector3.Distance(navMeshAgent.transform.position, targetPosition));
                yield return null;
            }

            navMeshAgent.SetDestination(targetPosition);

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
            currentAction.Execute();

            Debug.Log("Treating action : " + currentAction);

            if (currentAction is MoveToPosition moveToPosition)
            {
                StartCoroutine(moveToPosition.GoToPosition());
            }
            yield return new WaitUntil(() => currentAction.IsActionComplete);

            Debug.Log("Action terminée");

        }

        isActionQueueCoroutineRunning = false;
        yield break;
        
    }
}