using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Catapulte : Troup
{
    [Header("Catapulte properties")]
    [SerializeField] private float wheelRotationSpeed;
    [SerializeField] private GameObject croix;
    [SerializeField] private Vector3 shootPoint;
    [SerializeField] private float maxShootingRange;
    [SerializeField] private float oneShotTime;


    [Header("Debug variables")]
    [SerializeField] private bool isSelectingPlacement;
    [SerializeField] private bool isShooting;
    [SerializeField] private float heightAttenuation;
    [SerializeField] private float timeAttenuation;
    [SerializeField] private bool isLauchingBoulder;

    [SerializeField] private GameObject newBoulder;
    private Animator mAnimator;
    private bool isRolling;
    private HashSet<GameObject> roues = new HashSet<GameObject>();
    private GameObject boulderPrefab;
    private GameObject boulderSpawnPoint;
    private GameObject boulder;
    [SerializeField] private bool hasSelected;

    IEnumerator moveAnimation;
    IEnumerator attack;

    protected override void Awake()
    {
        base.Awake();

        attack = Attack(null);

        boulderPrefab = GameManager.Instance.BoulderPrefab;

        boulder = Instantiate(boulderPrefab, transform.Find("Model").Find("Lance").Find("BoulderSpawnPoint").position, Quaternion.identity, transform);
        if (troupType == TroupType.Ally)
        {
            boulder.GetComponent<Boulder>().boulderTroupType = Boulder.BoulderTroupType.Ally;
        } else
        {
            boulder.GetComponent<Boulder>().boulderTroupType = Boulder.BoulderTroupType.Enemy;
        }

        croix = Instantiate(GameManager.Instance.CatapulteCroixPrefab, GameManager.Instance.CatapulteCroix.transform);
        boulderSpawnPoint = transform.Find("Model").Find("Lance").Find("BoulderSpawnPoint").gameObject;

        mAnimator = transform.Find("Model").GetComponent<Animator>();

        roues.Add(transform.Find("Model").Find("Roue1").gameObject);
        roues.Add(transform.Find("Model").Find("Roue2").gameObject);
        roues.Add(transform.Find("Model").Find("Roue3").gameObject);
        roues.Add(transform.Find("Model").Find("Roue4").gameObject);

        moveAnimation = MoveAnimation();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (gameManager.hasGameStarted()) { AttackZoneBehaviour(); }

        

        if (agent.velocity.magnitude > 0 && !isRolling)
        {
            isRolling = true;
            isShooting = false;
            hasSelected = false;
            // Debug.Log("Je tourne");
            StartCoroutine(moveAnimation);
        }
        if (agent.velocity.magnitude == 0 && isRolling)
        {
            isRolling = false;
            StopCoroutine(moveAnimation);
        }

        if (newBoulder != null)
        {
            Debug.Log("teeeeeeeest + " + newBoulder.transform.position + " et " + boulderSpawnPoint.transform.position);
            newBoulder.transform.position = boulderSpawnPoint.transform.position;
        }

        if (troupType == TroupType.Enemy && currentFollowedTroup == null && currentAttackedTroup == null) { IAEnemy(); }

    }

    protected override void IAEnemy() { }

    protected override IEnumerator Attack(Troup enemy)
    {
        Debug.Log("BBBBBBB" + (isShooting && agent.velocity.magnitude == 0));

        while (isShooting && agent.velocity.magnitude == 0)
        {
            if (!isLauchingBoulder)
            {
                isLauchingBoulder = true;
                Debug.Log("Catapulte attack !!");
                newBoulder = null;
                StartCoroutine(LaunchBoulder());
                mAnimator.SetBool("Attack", true);
                yield return null;
                mAnimator.SetBool("Attack", false);
                yield return new WaitForSeconds(2f - Time.deltaTime);
                mAnimator.SetBool("Attack", false);
                yield return new WaitForSeconds(attackRechargeTime - 2f);
                isLauchingBoulder = false;
            }
            
            
        }
    }

    private void AttackZoneBehaviour()
    {
        Debug.Log("isSelected : " + isSelected);

        if (isSelected)
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                if (!isSelectingPlacement)
                {
                    StartCoroutine(ShootPlaceSeletion());
                } else
                {
                    StopCoroutine(ShootPlaceSeletion());
                }

                isSelectingPlacement = !isSelectingPlacement;
            }
        }

        croix.SetActive((isSelectingPlacement || isShooting) && isSelected);
    }

    private IEnumerator LaunchBoulder()
    {
        Vector3 spawnPoint = boulderSpawnPoint.transform.position;
        Vector3 endPoint = shootPoint;

        bool spawnedNewBoulder = false;

        float hauteur = Vector3.Distance(spawnPoint, endPoint) / heightAttenuation;
        float travelTime = Vector3.Distance(spawnPoint, endPoint) / timeAttenuation;

        yield return new WaitForSeconds(10/24);

        float t = 10/24;
        while (t < travelTime)
        {
            t += Time.deltaTime;

            float height = Mathf.Sin(Mathf.PI * (t / travelTime)) * hauteur;

            Vector3 newPosition = Vector3.Lerp(spawnPoint, endPoint, t / travelTime) + Vector3.up * height;
            if (boulder != null)
            {
                boulder.transform.Rotate(new Vector3(0, 1, 1));
                boulder.transform.position = newPosition;
            }
            

            if (t > .9f * travelTime && !spawnedNewBoulder)
            {
                spawnedNewBoulder = true;
                newBoulder = Instantiate(boulderPrefab, boulderSpawnPoint.transform.position, Quaternion.identity, transform);
                if (troupType == TroupType.Ally)
                {
                    newBoulder.GetComponent<Boulder>().boulderTroupType = Boulder.BoulderTroupType.Ally;
                }
                else
                {
                    newBoulder.GetComponent<Boulder>().boulderTroupType = Boulder.BoulderTroupType.Enemy;
                }
            }

            

            yield return null;
        }

        
        Destroy(boulder);
        

        boulder = newBoulder;
        
    }

    private IEnumerator ShootPlaceSeletion()
    {

        Debug.Log("AAAAAAAAAAAAAAAAA");

        while (!hasSelected)
        {
            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log("Debug hit : " + hit.point + " et " + hit.transform.gameObject);
                Debug.DrawLine(transform.position, hit.point);

                croix.transform.position = hit.point;

                if (Input.GetMouseButton(0))
                {
                    if (Vector3.Distance(transform.position, hit.point) > maxShootingRange)
                    {
                        AddAction(new MoveToPosition(agent, hit.point, positionThreshold));
                        yield return new WaitWhile(() => Vector3.Distance(transform.position, hit.point) > maxShootingRange);
                        AddAction(new Standby());
                    }

                    Debug.Log("Target position clicked : " + hit.point);
                    // hasSelected = true;
                    SelectionArrow.GetComponent<MeshRenderer>().enabled = false;
                    PlaceSelectionPopUp.enabled = false;
                    isSelectingPlacement = false;
                    isShooting = true;
                    hasSelected = true;
                    shootPoint = hit.point;
                    StopCoroutine(attack);
                    StopCoroutine(LaunchBoulder());
                    Destroy(boulder);
                    boulder = Instantiate(boulderPrefab, transform.Find("Model").Find("Lance").Find("BoulderSpawnPoint").position, Quaternion.identity, transform);
                    if (troupType == TroupType.Ally)
                    {
                        boulder.GetComponent<Boulder>().boulderTroupType = Boulder.BoulderTroupType.Ally;
                    }
                    else
                    {
                        boulder.GetComponent<Boulder>().boulderTroupType = Boulder.BoulderTroupType.Enemy;
                    }
                    Debug.Log("Starting Attack Coroutine " + attack);
                    attack = Attack(null);
                    StartCoroutine(attack);
                }
            }

            yield return null;
        }

        transform.LookAt(shootPoint);
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Catapulte special ability activated");

        boulderPrefab = GameManager.Instance.BigBoulderPrefab;
        boulderPrefab.GetComponent<Boulder>().OneShotMode(true);

        float elapsedTime = 0f;
        while (elapsedTime < oneShotTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / oneShotTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        boulderPrefab = GameManager.Instance.BoulderPrefab;
        boulderPrefab.GetComponent<Boulder>().OneShotMode(false);

        Debug.Log("Catapulte special ability ended");

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
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
            roue.transform.Rotate(new Vector3((rotationSpeed * (agent.velocity.magnitude / agent.speed)) / 12, 0f, 0f));

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
