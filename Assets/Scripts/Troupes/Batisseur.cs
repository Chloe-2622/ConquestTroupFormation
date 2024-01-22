using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Batisseur : Troup
{
    [Header("Batisseur properties")]
    [SerializeField] private float swingTime;
    [SerializeField] private float wallRechargeTime;
    [SerializeField] private float wallPlacementRange;
    [SerializeField] private bool isPlacingWall;


    private LayerMask floorMaskLayer;
    private LayerMask maxCircleMask;

    private GameObject hammer;
    private GameObject wallPrefab;

    private GameObject preview;
    private Wall previewWallComponent;

    private Vector3 firstPos;
    private Vector3 secondPos;

    private bool hasSelectedFistPos;
    private bool hasSelectedSecondPos;




    protected override void Awake()
    {
        base.Awake();

        hammer = transform.Find("Hammer").gameObject;

        wallPrefab = GameManager.Instance.WallPrefab;
        floorMaskLayer = GameManager.Instance.floorMask;
        maxCircleMask = GameManager.Instance.wallMaxCircleMask;

        hasSelectedFistPos = false;
        hasSelectedSecondPos = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        swingTime = attackRechargeTime / 2;

        AttackBehaviour();
        WallPlacementBehaviour();
    }

    public void WallPlacementBehaviour()
    {
        if (isSelected)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!isPlacingWall)
                {
                    StartCoroutine(WallPlacement());
                }
                else
                {
                    StopCoroutine(WallPlacement());
                }

                isPlacingWall = !isPlacingWall;
            }
        }
    }      

    protected IEnumerator WallPlacement()
    {
        Vector3 lastFirstPosition = new Vector3();
        Vector3 lastSecondPosition = new Vector3();

        while (!hasSelectedFistPos && isPlacingWall)
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
                hasSelectedFistPos = true;
                firstPos = lastFirstPosition;
                Debug.Log("--- First pos " + firstPos);
            }
            yield return null;
        }

        while (!hasSelectedSecondPos && isPlacingWall)
        {
            Ray ray_2 = camera1.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit_2;

            if (Physics.Raycast(ray_2, out hit_2, Mathf.Infinity))
            {
                Debug.Log("--- Touché");
                Debug.Log(hit_2.transform.gameObject.layer);

                

                NavMeshHit closestHit_2;
                if (NavMesh.SamplePosition(hit_2.point, out closestHit_2, 10, 1))
                {
                    if (Vector3.Distance(lastFirstPosition, closestHit_2.position) < previewWallComponent.maxLenght)
                    {
                        lastSecondPosition = closestHit_2.position;
                        showPreview(firstPos, lastSecondPosition);
                    }
                    else { Debug.Log("--- Trop loin"); }
                }
                else
                {
                    Debug.Log("Couldn't find near NavMesh");
                }
                
            }

            if (Input.GetMouseButtonDown(0) && lastSecondPosition != null && lastSecondPosition != lastFirstPosition)
            {
                hasSelectedSecondPos = true;
                secondPos = lastSecondPosition;
                Debug.Log("--- Second pos " + lastSecondPosition);
            }
            yield return null;
        }

        if (isPlacingWall)
        {
            findNearestWall();
            //GameObject.Destroy(preview);
            StartCoroutine(BuildBehaviour());
        }
    }
    



    public void showPreview(Vector3 tower_1_Position, Vector3 tower_2_Position)
    {
        if (preview == null)
        {
            preview = Instantiate(wallPrefab, tower_1_Position, new Quaternion(0, 0, 0, 0));
            previewWallComponent = preview.GetComponent<Wall>();
        }
        previewWallComponent.setTower_1_Position(tower_1_Position);
        previewWallComponent.setTower_2_Position(tower_2_Position);
    }

    public void findNearestWall()
    {
        HashSet<Wall> allyWalls = GameManager.Instance.getAllyWalls();

        Vector3 nearestTower_1 = new Vector3();
        float distanceTower_1 = previewWallComponent.wallFusionMaxDistance;
        Vector3 nearestTower_2 = new Vector3();
        float distanceTower_2 = previewWallComponent.wallFusionMaxDistance;

        foreach (Wall allyWall in allyWalls)
        {
            HashSet<Vector3> towersPos = allyWall.getTowersPosition();
            foreach (Vector3 pos in towersPos)
            {
                if (Vector3.Distance(firstPos, pos) < distanceTower_1)
                {
                    nearestTower_1 = pos;
                    distanceTower_1 = Vector3.Distance(firstPos, pos);
                }
                if (Vector3.Distance(secondPos, pos) < distanceTower_2)
                {
                    nearestTower_2 = pos;
                    distanceTower_2 = Vector3.Distance(secondPos, pos);
                }
            }
        }
        if (nearestTower_1 != null) { firstPos = nearestTower_1; }
        if (nearestTower_2 != null) { secondPos = nearestTower_2; }
    }


    protected IEnumerator BuildBehaviour()
    {
        //AddAction(new MoveToPosition(agent, firstPos, positionThreshold));
        yield return null;
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Batisseur special ability activated");

        float elapsedTime = 0f;
        while (elapsedTime < wallRechargeTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / wallRechargeTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }

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
}
