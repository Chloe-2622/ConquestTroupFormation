using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Archer : Troup
{

    [Header("------------------ Archer ------------------ ")]
    [Header("General stats")]
    [SerializeField] private float invisibleTime;

    [Header("Special ability parameters")]
    [SerializeField] private Material invisibleMaterial;

    [Header("Animation parameters")]
    [SerializeField] private float arrowSpeed;
    [SerializeField] private GameObject bow;

    // Private variables
    private GameObject arrowSpawnPoint;
    HashSet<GameObject> arrows = new HashSet<GameObject>();


    // Main Functions ---------------------------------------------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();

        bow = transform.Find("Bow").gameObject;
        arrowSpawnPoint = transform.Find("ArrowSpawnPoint").gameObject;
    }

    protected override void Update()
    {
        base.Update();

        AttackBehaviour();
        if (troupType == TroupType.Enemy) { IAEnemy(); }
    }

    private void OnDestroy()
    {
        foreach (GameObject arrow in arrows)
        {
            Destroy(arrow);
        }
    }

    // Attack and ability -----------------------------------------------------------------------------------------
    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null && currentAttackedTroup != null)
        {
            StartCoroutine(BowAnimation());
            MusicManager.Instance.PlaySound(MusicManager.SoundEffect.BowLoad, transform.position);
            StartCoroutine(ShootArrow(enemy));
            
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Archer special ability activated");

        isVisible = false;
        Material defaultMaterial = transform.Find("Model").gameObject.GetComponent<Renderer>().material;
        transform.Find("Model").gameObject.GetComponent<Renderer>().material = invisibleMaterial;

        float elapsedTime = 0f;
        while (elapsedTime < invisibleTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / invisibleTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        isVisible = true;
        transform.Find("Model").gameObject.GetComponent<Renderer>().material = defaultMaterial;

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }

    // IA Enemy ---------------------------------------------------------------------------------------------------
    protected override void IAEnemy()
    {
        if (health <= (maxHealth / 2) && specialAbilityDelay == 0)
        {
            StartCoroutine(SpecialAbility());
            specialAbilityDelay = -1f;
        }

        if (timeBeforeNextAction == 0f && currentFollowedTroup == null && currentAttackedTroup == null)
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

    // Animation --------------------------------------------------------------------------------------------------
    private IEnumerator BowAnimation()
    {
        float t = 0f;
        float t1 = 10f / 24f;
        float t2 = 2f / 24f;

        while (t < 1)
        {
            t += Time.deltaTime / t1;
            bow.transform.Find("Corde").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, Mathf.Lerp(0, 100, t));
            yield return null;
        }
        bow.transform.Find("Corde").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, 100);
        t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / t2;
            bow.transform.Find("Corde").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, Mathf.Lerp(100, 0, t));
            yield return null;
        }
        bow.transform.Find("Corde").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, 0);
    }

    private IEnumerator ShootArrow(Troup enemy)
    {

        yield return new WaitForSeconds(11f / 24f);
        
        if (enemy != null)
        {
            GameObject arrow = Instantiate(GameManager.Instance.ArrowPrefab, arrowSpawnPoint.transform.position, Quaternion.identity, null);
            MusicManager.Instance.PlaySound(MusicManager.SoundEffect.BowShoot, transform.position);
            arrows.Add(arrow);
            arrow.transform.LookAt(enemy.transform.position);
            arrow.transform.eulerAngles = new Vector3(0f, arrow.transform.eulerAngles.y + 180, arrow.transform.eulerAngles.z);

            float t = 0f;

            while (t < 1f && enemy != null)
            {
                t += Time.deltaTime / arrowSpeed;

                arrow.transform.position = Vector3.Lerp(arrowSpawnPoint.transform.position, enemy.transform.position + Vector3.up, t);
                Debug.Log("La ditance est de " + Vector3.Distance(arrow.transform.position, enemy.transform.position));
                if (Vector3.Distance(arrow.transform.position, enemy.transform.position) >= 4f)
                {

                    arrow.transform.LookAt(enemy.transform.position);
                    arrow.transform.eulerAngles = new Vector3(0f, arrow.transform.eulerAngles.y + 180, arrow.transform.eulerAngles.z);
                }



                yield return null;
            }

            if (enemy != null)
            {
                if (enemy.unitType == UnitType.Cavalier)
                {
                    enemy.TakeDamage(2 * attackDamage);
                }
                else
                {
                    enemy.TakeDamage(attackDamage);
                }
            }

            
            Destroy(arrow);
        }
    }

}
