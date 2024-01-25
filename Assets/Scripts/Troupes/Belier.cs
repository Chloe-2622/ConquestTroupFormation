using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belier : Troup
{
    [Header("Belier properties")]
    [SerializeField] private float wheelRotationSpeed;

    [Header("Ram properties")]
    [SerializeField] private float maxRamDistance;
    [SerializeField] private float ramTravelTime;

    private GameObject ramPrefab;
    private bool isRamLaunched;

    private GameObject ram;

    private Animator mAnimator;
    private bool isRolling;
    private HashSet<GameObject> roues = new HashSet<GameObject>();


    IEnumerator moveAnimation;
    private IEnumerator attackCoroutine;

    protected override void Awake()
    {
        base.Awake();

        mAnimator = transform.Find("Model").Find("Belier").GetComponent<Animator>();

        roues.Add(transform.Find("Model").Find("Belier").Find("Roue1").gameObject);
        roues.Add(transform.Find("Model").Find("Belier").Find("Roue2").gameObject);
        roues.Add(transform.Find("Model").Find("Belier").Find("Roue3").gameObject);
        roues.Add(transform.Find("Model").Find("Belier").Find("Roue4").gameObject);
        roues.Add(transform.Find("Model").Find("Belier").Find("Roue5").gameObject);
        roues.Add(transform.Find("Model").Find("Belier").Find("Roue6").gameObject);

        ramPrefab = gameManager.RamPrefab;
        ram = transform.Find("Model").Find("Belier").Find("Ram").gameObject;

        moveAnimation = MoveAnimation();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (agent.velocity.magnitude > 0 && !isRolling)
        {
            isRolling = true;
            // Debug.Log("Je tourne");
            StartCoroutine(moveAnimation);
        }
        if (agent.velocity.magnitude == 0 && isRolling)
        {
            isRolling = false;
            StopCoroutine(moveAnimation);
        }
        
        if (currentAttackedTroup != null)
        {
            mAnimator.SetBool("Attack", true);
        } else
        {
            mAnimator.SetBool("Attack", false);
        }
        
        if (isRamLaunched) { return; }
        Wall nearestWall = detectNearestWall();
        if (nearestWall != null) { AttackWallBehaviour(nearestWall); }
        else { AttackBehaviour(); }
    }

    public Wall detectNearestWall()
    {
        if (!gameManager.hasGameStarted()) { return null; }

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, wallMask);
        Wall nearestWall = null;
        float wallDistance = detectionRange;

        foreach (Collider collider in colliders)
        {
            Debug.Log("collider : " + collider.gameObject);
            Wall wall = collider.transform.parent.GetComponent<Wall>();

            if (wall != null && collider.name == "JunctionWall" && wall.troupType != troupType)
            {
                if (Vector3.Distance(transform.position, wall.transform.position) < wallDistance)
                {
                    wallDistance = Vector3.Distance(transform.position, wall.transform.position);
                    nearestWall = wall;
                }
            }
        }
        return nearestWall;
    }

    public void AttackWallBehaviour(Troup wall)
    {
        // If King is present, attack it instead
        if (hasCrown) { return; }

        if (troupType == TroupType.Enemy && gameManager.isCrownCollected && gameManager.king != null)
        {
            AttackKingBehaviour();
            return;
        }

        // Follow closestEnemy until it's in range
        if (!isFollowingEnemy && !isFollowingOrders)
        {
            isFollowingEnemy = true;
            currentFollowedTroup = wall.gameObject;
            actionQueue.Clear();
            agent.isStopped = true;
            agent.ResetPath();

            // Debug.Log(gameObject + " -3 - Getting closer to enemy : " + closestEnemy);
            StopCoroutine(currentActionCoroutine);
            actionQueue.Enqueue(new MoveToUnit(agent, wall.gameObject, getAttackRange()));
            StartCoroutine(currentActionCoroutine);
        }

        // Clear currentFollowedTroup if it dies
        if (currentFollowedTroup == null)
        {
            isFollowingEnemy = false;
        }

        // Si le mur est dans l'Attack Range
        if (Vector3.Distance(transform.position, wall.transform.position) < getAttackRange())
        {
            // Starts attacking closest enemy in range
            if (currentAttackedTroup == null && !isFollowingOrders)
            {
                isAttackingEnemy = false;
                if (attackCoroutine != null)
                {
                    StopCoroutine(attackCoroutine);
                }

                if (wall.gameObject != null)
                {
                    currentAttackedTroup = wall.gameObject;

                    actionQueue.Clear();
                    agent.isStopped = true;
                    agent.ResetPath();

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
            if (wall.gameObject == null && currentAttackedTroup != null)
            {
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

        
        
    }


    protected override void IAEnemy() { }

    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null && !isFollowingOrders)
        {
            if (enemy.unitType == UnitType.Mur)
            {
                enemy.TakeDamage(50 * attackDamage);
            }
            else
            {
                enemy.TakeDamage(attackDamage);
            }
            yield return new WaitForSeconds(attackRechargeTime);
        }
        Debug.Log("Belier attack");
        yield return null;
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Belier special ability activated");

        Vector3 exPos = ram.transform.position;
        Quaternion exQua = ram.transform.rotation;
        GameObject.Destroy(ram.gameObject);

        Ram newRam = Instantiate(ramPrefab, exPos, exQua).GetComponent<Ram>();
        newRam.transform.localScale = new Vector3(21f, 21f, 21f);
        newRam.ramType = troupType;
        
        
        Vector3 endPoint = transform.position + transform.forward * maxRamDistance;
        Debug.Log("!!! endpoint " +  endPoint);

        newRam.isLaunch = true;
        float t = 0f;
        while (t < ramTravelTime)
        {
            t += Time.deltaTime;
            Debug.Log("!!! " + newRam.transform.position);
            Debug.Log("!!! Lerp " + Vector3.Lerp(transform.position, endPoint, t / ramTravelTime));
            Vector3 newPosition = Vector3.Lerp(transform.position, endPoint, t / ramTravelTime);
            newRam.transform.position = newPosition;
            yield return null;
        }
        GameObject.Destroy(newRam.gameObject);
        isRamLaunched = true;
    }

    private IEnumerator MoveAnimation()
    {
        while (true)
        {
            
            foreach (GameObject roue in roues)
            {
                StartCoroutine(TurnWheel(roue, wheelRotationSpeed));
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator TurnWheel(GameObject roue, float rotationSpeed)
    {
        float timer = 0f;

        while (timer < 1f)
        {
            roue.transform.Rotate(new Vector3(- (rotationSpeed * (agent.velocity.magnitude / agent.speed) ) / 12, 0f, 0f));

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
