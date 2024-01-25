using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Catapulte : Troup
{
    [Header("Catapulte properties")]
    [SerializeField] private float wheelRotationSpeed;
    [SerializeField] private GameObject croix;
    [SerializeField] private Vector3 shootPoint;
    [SerializeField] private float oneShotTime;

    [Header("Debug variables")]
    [SerializeField] private bool isSelectingPlacement;
    [SerializeField] private float heightAttenuation;
    [SerializeField] private float timeAttenuation;

    [Header("Attack time repartion")]
    [SerializeField] private float reloadFraction = 5/10f;
    [SerializeField] private float stayFraction = 4/10f;
    [SerializeField] private float swingFraction = 1/10f;

    // Boulders
    private GameObject boulderPrefab;
    private GameObject boulderSpawnPoint;
    private GameObject newBoulder;

    // Lance
    private float baseRotationLance;
    private float maxRotationRance;
    private GameObject lance;

    // Roues
    private bool isRolling;
    private HashSet<GameObject> roues = new HashSet<GameObject>();

    // Targets
    private bool firstTargetChoosen;
    private bool isTargetSelected;
    private Vector3 lastCrossPosition;

    private IEnumerator moveAnimation;

    protected override void Awake()
    {
        base.Awake();

        firstTargetChoosen = false;
        isTargetSelected = false;

        boulderPrefab = gameManager.BoulderPrefab;

        newBoulder = Instantiate(boulderPrefab, transform.Find("Model").Find("Lance").Find("BoulderSpawnPoint").position, Quaternion.identity, transform);
        newBoulder.GetComponent<Boulder>().boulderType = troupType;

        croix = Instantiate(GameManager.Instance.CatapulteCroixPrefab, GameManager.Instance.CatapulteCroix.transform);
        boulderSpawnPoint = transform.Find("Model").Find("Lance").Find("BoulderSpawnPoint").gameObject;

        roues.Add(transform.Find("Model").Find("Roue1").gameObject);
        roues.Add(transform.Find("Model").Find("Roue2").gameObject);
        roues.Add(transform.Find("Model").Find("Roue3").gameObject);
        roues.Add(transform.Find("Model").Find("Roue4").gameObject);

        lance = transform.Find("Model").Find("Lance").gameObject;

        moveAnimation = MoveAnimation();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!gameManager.hasGameStarted()) { return; }

        if (Input.GetKeyDown(KeyCode.E) && (isTargetSelected || !firstTargetChoosen)) 
        {
            Debug.Log("*** Start Placement");
            StartCoroutine(ShootPlaceSeletion());
        }

        if (isSelected) { croix.SetActive(true); }
        else {  croix.SetActive(false); }

        if (agent.velocity.magnitude > 0 && !isRolling)
        {
            isRolling = true;
            StartCoroutine(moveAnimation);
        }
        if (agent.velocity.magnitude == 0 && isRolling)
        {
            isRolling = false;
            StopCoroutine(moveAnimation);
        }
        
        if (troupType == TroupType.Enemy && currentFollowedTroup == null && currentAttackedTroup == null) { IAEnemy(); }

    }

    private IEnumerator ShootPlaceSeletion()
    {
        bool hasSelected = false;
        isTargetSelected = false;

        AddAction(new Standby());

        yield return new WaitWhile(() => Input.GetKeyDown(KeyCode.E));

        while (!hasSelected && !Input.GetKeyDown(KeyCode.Alpha1) && !Input.GetKeyDown(KeyCode.Alpha2) && !Input.GetKeyDown(KeyCode.Alpha3) && !Input.GetKeyDown(KeyCode.Alpha4) && !Input.GetKeyDown(KeyCode.F) && !Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.LeftControl))
        {
            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorMask))
            {
                NavMeshHit closestHit;
                if (NavMesh.SamplePosition(hit.point, out closestHit, 10, 1))
                {
                    lastCrossPosition = closestHit.position;
                    croix.transform.position = lastCrossPosition;
                }
                else
                {
                    Debug.Log("Couldn't find near NavMesh");
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                hasSelected = true;
                isTargetSelected = true;
                firstTargetChoosen = true;
                shootPoint = lastCrossPosition;
                croix.transform.position = lastCrossPosition;
                Debug.Log("--- Pos choisie " + shootPoint);
            }
            yield return null;
        }

        if (hasSelected) { Debug.Log("*** has selected" + shootPoint);  StartCoroutine(MoveToRange()); }
        else { Debug.Log("*** has not selected");  isTargetSelected = true;  croix.transform.position = shootPoint;  }
    }

    private IEnumerator MoveToRange()
    {
        AddAction(new MoveToPosition(agent, croix.transform.position, positionThreshold));
        yield return new WaitWhile(() => Vector3.Distance(transform.position, croix.transform.position) > getAttackRange());
        Debug.Log("*** Arrived");
        AddAction(new Standby());

        StartCoroutine(SwingBoulder());
    }

    private IEnumerator SwingBoulder()
    {
        float timer = 0f;

        while (timer < swingFraction * attackRechargeTime)
        {
            timer += Time.deltaTime;
            lance.transform.RotateAround(lance.transform.position, lance.transform.right, -32 * (Time.deltaTime / (swingFraction * attackRechargeTime)));
            Debug.Log("swingR : " + lance.transform.localEulerAngles.x);

            yield return null;
        }
        lance.transform.localEulerAngles = new Vector3(32f, 0f, 0f);

        StartCoroutine(LaunchBoulder());
        StartCoroutine(LoadBoulder());
    }

    private IEnumerator LoadBoulder()
    {
        Debug.Log("*** Reloading ");
        float timer = 0f;

        while (timer < reloadFraction*attackRechargeTime)
        {
            timer += Time.deltaTime;
            lance.transform.RotateAround(lance.transform.position, lance.transform.right, 32 * (Time.deltaTime / (reloadFraction * attackRechargeTime)));
            Debug.Log("swingR : " + lance.transform.localEulerAngles.x);

            yield return null;
        }
        lance.transform.localEulerAngles = Vector3.zero;

        Debug.Log("*** Catapulte Reloaded");

        newBoulder = Instantiate(boulderPrefab, boulderSpawnPoint.transform.position, Quaternion.identity, transform);
        newBoulder.GetComponent<Boulder>().boulderType = troupType;
        yield return new WaitForSeconds(stayFraction*attackRechargeTime);
        Debug.Log("*** Ready to Shoot");

        if (isTargetSelected) { StartCoroutine(SwingBoulder()); }
    }

    

    private IEnumerator LaunchBoulder()
    {
        Debug.Log("*** Boulder launched");
        Vector3 spawnPoint = boulderSpawnPoint.transform.position;
        Vector3 endPoint = shootPoint;
        Boulder boulder = newBoulder.GetComponent<Boulder>();

        float hauteur = Vector3.Distance(spawnPoint, endPoint) / heightAttenuation;
        float travelTime = Vector3.Distance(spawnPoint, endPoint) / timeAttenuation;

        float t = 0f;
        while (boulder != null)
        {
            t += Time.deltaTime;

            float height = Mathf.Sin(Mathf.PI * (t / travelTime)) * hauteur;

            Vector3 newPosition = Vector3.Lerp(spawnPoint, endPoint, t / travelTime) + Vector3.up * height;
            boulder.transform.Rotate(new Vector3(0, 1, 1));
            boulder.transform.position = newPosition;
            yield return null;
        }
        Debug.Log("*** boulder arrived");
    }
    protected override void IAEnemy() { }

    protected override IEnumerator Attack(Troup enemy) { yield return null; }
    
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
