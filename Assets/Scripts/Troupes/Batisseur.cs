using Autodesk.Fbx;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Batisseur : Troup
{
    [Header("Batisseur properties")]
    [SerializeField] private float swingTime;
    [SerializeField] private float constructionRange;

    private LayerMask floorMaskLayer;

    [SerializeField] private bool isBuilding;

    private GameObject hammer;
    private GameObject wallPrefab;
    private GameObject enemieWallPrefab;

    private GameObject preview;
    private Wall previewWallComponent;

    [SerializeField] private Vector3 firstPos = new Vector3(0,0,0);
    [SerializeField] private Vector3 secondPos = new Vector3(0, 0, 0);


    protected override void Awake()
    {
        base.Awake();

        hammer = transform.Find("Hammer").gameObject;

        wallPrefab = gameManager.WallPrefab;
        enemieWallPrefab = gameManager.EnemieWallPrefab;
        floorMaskLayer = gameManager.floorMask;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        swingTime = attackRechargeTime / 2;

        if (!GameManager.Instance.hasGameStarted()) { return; }
        AttackBehaviour();
        Debug.Log(troupType);
        if (troupType == TroupType.Enemy) { IAEnemy(); }
    }

    protected override IEnumerator SpecialAbility()
    {
        bool hasSelectedFirstPos = false;
        bool hasSelectedSecondPos = false;

        Vector3 lastFirstPosition = new Vector3();
        Vector3 lastSecondPosition = new Vector3();

        yield return new WaitWhile(() => Input.GetKeyDown(KeyCode.F));

        // On montre la pose du premier mur, que on annule
        while (!hasSelectedFirstPos && !Input.GetKeyDown(KeyCode.Alpha1) && !Input.GetKeyDown(KeyCode.Alpha2) && !Input.GetKeyDown(KeyCode.Alpha3) && !Input.GetKeyDown(KeyCode.Alpha4) && !Input.GetKeyDown(KeyCode.F) && !Input.GetKeyDown(KeyCode.LeftControl))
        {
            Ray ray_1 = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit_1;

            if (Physics.Raycast(ray_1, out hit_1, Mathf.Infinity, floorMaskLayer))
            {
                NavMeshHit closestHit_1;
                if (NavMesh.SamplePosition(hit_1.point, out closestHit_1, 10, 1))
                {
                    lastFirstPosition = closestHit_1.position;
                    showPreview(lastFirstPosition, lastFirstPosition);
                }
                else
                {
                    Debug.Log("Couldn't find near NavMesh");
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                hasSelectedFirstPos = true;
                firstPos = lastFirstPosition;
                Debug.Log("First pos " + firstPos);
            }
            yield return null;
        }

        if (hasSelectedFirstPos) { isPlacingWall = true;  StartCoroutine(DecreaseAbilityBar()); }

        while (!hasSelectedSecondPos && isPlacingWall)
        {
            Ray ray_2 = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit_2;


            if (Physics.Raycast(ray_2, out hit_2, Mathf.Infinity, floorMaskLayer))
            {
                NavMeshHit closestHit_2;
                if (NavMesh.SamplePosition(hit_2.point, out closestHit_2, 10, 1))
                {
                    if (Vector3.Distance(firstPos, closestHit_2.position) < previewWallComponent.maxLenght)
                    {
                        lastSecondPosition = closestHit_2.position;
                        showPreview(firstPos, lastSecondPosition);
                    }
                }
                else
                {
                    Debug.Log("Couldn't find near NavMesh");
                }
            }
            if (Input.GetMouseButtonDown(0) && lastSecondPosition != lastFirstPosition)
            {
                hasSelectedSecondPos = true;
                secondPos = lastSecondPosition;
                Debug.Log("Second pos " + secondPos);
            }
            yield return null;
        }

        GameObject.Destroy(preview);
        if (hasSelectedSecondPos) { StartCoroutine(BuildWall()); }
        else { specialAbilityDelay = 0f; }
    }

    public void showPreview(Vector3 tower_1_Position, Vector3 tower_2_Position)
    {
        if (preview == null)
        {
            preview = Instantiate(wallPrefab, tower_1_Position, new Quaternion(0, 0, 0, 0));
            previewWallComponent = preview.GetComponent<Wall>();
            // On désactive toutes les collisions possibles avec la preview
            //preview.transform.GetChild(0).GetComponent<Collider>().enabled = false;
            preview.transform.GetChild(0).GetComponent<NavMeshObstacle>().enabled = false;
            //preview.transform.GetChild(1).GetComponent<Collider>().enabled = false;
            preview.transform.GetChild(1).GetComponent<NavMeshObstacle>().enabled = false;
            //preview.transform.GetChild(2).GetComponent<Collider>().enabled = false;
            preview.transform.GetChild(2).GetComponent<NavMeshObstacle>().enabled = false;
        }
        previewWallComponent.setTower_1_Position(tower_1_Position);
        previewWallComponent.setTower_2_Position(tower_2_Position);
    }

    public IEnumerator DecreaseAbilityBar()
    {
        abilityBar.fillAmount = 0;

        specialAbilityDelay = specialAbilityRechargeTime;
        yield return null;
    }

    public void findNearestWall()
    {
        HashSet<Wall> allyWalls = (troupType == TroupType.Ally) ? GameManager.Instance.getAllyWalls() : GameManager.Instance.getEnemyWalls();

        float minDistanceTower_1 = wallPrefab.GetComponent<Wall>().wallFusionMaxDistance;
        Vector3 nearestPosition_1 = new Vector3();

        float minDistanceTower_2 = wallPrefab.GetComponent<Wall>().wallFusionMaxDistance;
        Vector3 nearestPosition_2 = new Vector3();


        foreach (Wall allyWall in allyWalls)
        {
            List<Vector3> towersPositions = allyWall.getTowersPosition();
            for (int i = 0; i < towersPositions.Count; i++)
            {
                if (Vector3.Distance(firstPos, towersPositions[i]) < minDistanceTower_1)
                {
                    minDistanceTower_1 = Vector3.Distance(firstPos, towersPositions[i]);
                    nearestPosition_1 = towersPositions[i];

                    Debug.Log("Previous " + allyWall + " " + nearestPosition_1);
                }

                if (Vector3.Distance(secondPos, towersPositions[i]) < minDistanceTower_2)
                {
                    minDistanceTower_2 = Vector3.Distance(firstPos, towersPositions[i]);
                    nearestPosition_2 = towersPositions[i];

                    Debug.Log("Next " + allyWall + " " + nearestPosition_2);
                }
            }
        }
        if (minDistanceTower_1 != wallPrefab.GetComponent<Wall>().wallFusionMaxDistance) { firstPos = nearestPosition_1; }
        if (minDistanceTower_2 != wallPrefab.GetComponent<Wall>().wallFusionMaxDistance) { secondPos = nearestPosition_2; }
    }

    protected IEnumerator BuildWall()
    {
        isBuilding = true;
        findNearestWall();

        Debug.Log("Go to first pos" + firstPos);
        AddAction(new MoveToPosition(agent, firstPos, positionThreshold));
        yield return new WaitWhile(() => Vector3.Distance(transform.position, firstPos) > constructionRange);
        AddAction(new Standby());

        GameObject newWall;
        StartCoroutine(SwingHammer());
        MusicManager.Instance.PlaySound(MusicManager.SoundEffect.Hammer1, transform.position);
        MusicManager.Instance.PlaySound(MusicManager.SoundEffect.Hammer2, transform.position);
        if (troupType == Troup.TroupType.Ally)
        {
            newWall = Instantiate(wallPrefab, firstPos, new Quaternion(0, 0, 0, 0));
        }
        else
        {
            newWall = Instantiate(enemieWallPrefab, firstPos, new Quaternion(0, 0, 0, 0));
        }
        Wall newWallComponent = newWall.GetComponent<Wall>();
        newWallComponent.setTower_1_Position(firstPos);
        newWallComponent.setTower_2_Position(firstPos);
        newWallComponent.addToGroup();

        Debug.Log("Go to second pos" + secondPos);

        if (NavMesh.SamplePosition(hit_1.point, out closestHit_1, 10, 1))
        {
            lastFirstPosition = closestHit_1.position;
            showPreview(lastFirstPosition, lastFirstPosition);
        }

        AddAction(new MoveToPosition(agent, secondPos, positionThreshold));
        yield return new WaitWhile(() => Vector3.Distance(transform.position, secondPos) > constructionRange);
        AddAction(new Standby());
        StartCoroutine(SwingHammer());
        MusicManager.Instance.PlaySound(MusicManager.SoundEffect.Hammer1, transform.position);
        MusicManager.Instance.PlaySound(MusicManager.SoundEffect.Hammer2, transform.position);
        newWallComponent.setTower_2_Position(secondPos);
        isPlacingWall = false;
        isBuilding = false;

        StartCoroutine(SpecialAbilityCountdown());
    }

    // Attack
    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null)
        {
            StartCoroutine(SwingHammer());
            enemy.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    private IEnumerator SwingHammer()
    {
        float timer = 0f;
        Debug.Log("I am swinging hammer");

        while (timer < swingTime / 2)
        {
            timer += Time.deltaTime;
            hammer.transform.RotateAround(hammer.transform.position, hammer.transform.right, 90 * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingR : " + hammer.transform.localEulerAngles.x);

            yield return null;
        }
        hammer.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        Debug.Log("swingRL");
        while (timer < swingTime)
        {
            timer += Time.deltaTime;
            hammer.transform.RotateAround(hammer.transform.position, hammer.transform.right, -90 * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingL : " + hammer.transform.localEulerAngles.x);

            yield return null;
        }
        hammer.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
    }

    private void IABuildWall(Vector3 firstLocation, Vector3 secondLocation)
    {
        firstPos = firstLocation;
        secondPos = secondLocation;

        StartCoroutine(BuildWall());
    }

    protected override void IAEnemy()
    {
        Debug.Log(specialAbilityDelay);

        if (specialAbilityDelay == 0f)
        {
            specialAbilityDelay = specialAbilityRechargeTime;
            

            int nextActionIndex = gameManager.getEnemyWalls().Count > 0 ? Random.Range(0, 2) : 0;

            if (nextActionIndex == 0)
            {
                Vector3 firstBuildLocation = RandomVectorInFlatCircle(transform.position, 5f);
                Vector3 secondBuildLocation = RandomVectorInFlatCircle(firstBuildLocation, wallPrefab.GetComponent<Wall>().getMaxLength());
                IABuildWall(firstBuildLocation, secondBuildLocation);
            }
            else
            {
                HashSet<Wall> enemyWalls = gameManager.getEnemyWalls();
                int index = Random.Range(0, enemyWalls.Count);
                int currentIndex = 0;

                foreach (Wall enemyWall in enemyWalls)
                {
                    if (currentIndex == index)
                    {
                        List<Vector3> towers = enemyWall.getTowersPosition();
                        int i = Random.Range(0, 2);

                        Vector3 firstBuildLocation = (i == 0) ? towers[0] : towers[1];
                        Vector3 secondBuildLocation = RandomVectorInFlatCircle(firstBuildLocation, wallPrefab.GetComponent<Wall>().getMaxLength());
                        IABuildWall(firstBuildLocation, secondBuildLocation);
                    }

                    currentIndex++;
                }
            }
        }

        if (timeBeforeNextAction == 0f && currentFollowedTroup == null && currentAttackedTroup == null && !isBuilding)
        {
            int nextActionIndex = Random.Range(0, 2);

            if (nextActionIndex == 0)
            {
                actionQueue.Enqueue(new MoveToPosition(agent, RandomVectorInFlatCircle(defaultPosition, 5f), positionThreshold));
            }
            else
            {
                actionQueue.Enqueue(new Patrol(agent, RandomVectorInFlatCircle(defaultPosition, 5f), RandomVectorInFlatCircle(defaultPosition, 5f)));
            }

            timeBeforeNextAction = Random.Range(5f, 10f);
            StartCoroutine(IAactionCountdown());
        }
    }
}
