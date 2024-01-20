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
    

    [Header("Debug variables")]
    [SerializeField] private bool isSelectingPlacement;
    [SerializeField] private bool isShooting;

    private Animator mAnimator;
    private bool isRolling;
    private HashSet<GameObject> roues = new HashSet<GameObject>();
    private Vector3 boulderSpawnPoint;
    private GameObject boulder;

    IEnumerator moveAnimation;

    protected override void Awake()
    {
        base.Awake();

        boulder = Instantiate(GameManager.Instance.BoulderPrefab, transform.Find("BoulderSpawnPoint").position, Quaternion.identity, transform);

        croix = Instantiate(GameManager.Instance.CatapulteCroixPrefab, GameManager.Instance.CatapulteCroix.transform);
        boulderSpawnPoint = transform.Find("BoulderSpawnPoint").position;

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

        AttackZoneBehaviour();

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

        mAnimator.SetBool("Attack", isShooting);

    }

    protected override IEnumerator Attack(Troup enemy)
    {
        while (isShooting && agent.velocity.magnitude == 0)
        {
            Debug.Log("Catapulte attack !!");
            StartCoroutine(LaunchBoulder());
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    private void AttackZoneBehaviour()
    {
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
        Vector3 spawnPoint = boulderSpawnPoint;
        Vector3 endPoint = shootPoint;

        float hauteur = 3f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1f;

            // Utiliser une interpolation quadratique (parabole) pour ajuster la hauteur
            float height = Mathf.Sin(Mathf.PI * t) * hauteur;

            // Interpolation linéaire entre spawnPoint et endPoint avec la hauteur ajustée
            Vector3 newPosition = Vector3.Lerp(spawnPoint, endPoint, t) + Vector3.up * height;

            // Appliquer la nouvelle position au GameObject du boulder
            boulder.transform.position = newPosition;

            yield return null;
        }
        Destroy(boulder);
        boulder = Instantiate(GameManager.Instance.BoulderPrefab, transform);
    }

    private IEnumerator ShootPlaceSeletion()
    {
        bool hasSelected = false;

        while (!hasSelected)
        {
            Ray ray = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                croix.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);

                if (Input.GetMouseButton(0))
                {
                    Debug.Log("Target position clicked : " + hit.point);
                    // hasSelected = true;
                    SelectionArrow.GetComponent<MeshRenderer>().enabled = false;
                    PlaceSelectionPopUp.enabled = false;
                    isSelectingPlacement = false;
                    isShooting = true;
                    hasSelected = true;
                    shootPoint = hit.point;
                    StartCoroutine(Attack(null));
                }
            }

            yield return null;
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Catapulte special ability activated");
        yield return null;
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
