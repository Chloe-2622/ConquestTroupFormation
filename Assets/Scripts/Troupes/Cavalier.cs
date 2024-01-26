using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cavalier : Troup
{

    [Header("------------------ Cavalier ------------------ ")]
    [Header("General stats")]
    [SerializeField] private float chargeTime;
    [SerializeField] private float chargeSpeed;

    [Header("Animation parameters")]
    [SerializeField] private float baseJambeRotationSpeed;
    [SerializeField] private float jambeRotationSpeed;
    [SerializeField] private float angleJambes;
    [SerializeField] private float swingTime;
    [SerializeField] private float swingAngle;

    // Private variables
    private bool isRunning;
    private GameObject[] jambes1 = new GameObject[2];
    private GameObject[] jambes2 = new GameObject[2];
    private GameObject head;
    IEnumerator animJambe1;
    IEnumerator animJambe2;


    // Main Functions ---------------------------------------------------------------------------------------------
    protected override void Awake()
    {
        base.Awake();

        jambes1[0] = (transform.Find("PateAvDP").gameObject);
        jambes1[1] = (transform.Find("PateArGP").gameObject);
        jambes2[0] = (transform.Find("PateArDP").gameObject);
        jambes2[1] = (transform.Find("PateAvGP").gameObject);

        head = transform.Find("Horse").Find("Head").gameObject;

        animJambe1 = MoveAnimation(true);
        animJambe2 = MoveAnimation(false);
    }

    protected override void Update()
    {
        base.Update();

        AttackBehaviour();

        Debug.Log("Vitesste = " + agent.velocity.magnitude);

        jambeRotationSpeed = baseJambeRotationSpeed * agent.velocity.magnitude;

        if (agent.velocity.magnitude > 0 && !isRunning)
        {
            isRunning = true;
            Debug.Log("Je tourne");
            StartCoroutine(animJambe1);
            StartCoroutine(animJambe2);
        }
        if (agent.velocity.magnitude == 0 && isRunning)
        {
            isRunning = false;
            StopCoroutine(animJambe1);
            StopCoroutine(animJambe2);
        }

        if (troupType == TroupType.Enemy) { IAEnemy(); }
    }

    // Attack and ability -----------------------------------------------------------------------------------------
    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null)
        {
            StartCoroutine(SwingHead());
            if (enemy.unitType == UnitType.Combattant)
            {
                enemy.TakeDamage(2 * attackDamage);
            } else
            {
                enemy.TakeDamage(attackDamage);
            }
            
            yield return new WaitForSeconds(attackRechargeTime);
        }
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Cavalier special ability activated");

        agent.speed = chargeSpeed;

        float elapsedTime = 0f;
        while (elapsedTime < chargeTime)
        {
            abilityBar.fillAmount = 1 - elapsedTime / chargeTime;
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        abilityBar.fillAmount = 0;

        agent.speed = movingSpeed;

        specialAbilityDelay = specialAbilityRechargeTime;
        StartCoroutine(SpecialAbilityCountdown());
    }

    // IA Enemy ---------------------------------------------------------------------------------------------------
    protected override void IAEnemy() 
    {
        if (gameManager.isCrownCollected && specialAbilityDelay == 0)
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
    private IEnumerator MoveAnimation(bool isRight)
    {
        GameObject[] jambes;

        jambes = isRight ? jambes1 : jambes2;

        if (isRight)
        {
            GameObject jambe00 = jambes[0];
            GameObject jambe01 = jambes[1];

            while (true)
            {
                float timer = 0f;

                while (jambe00.transform.localEulerAngles.x < angleJambes)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    // Debug.Log("swingR : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(angleJambes, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(angleJambes, 0f, 0f);
                // Debug.Log("swingRFinal ---------------- : " + jambe00.transform.localEulerAngles.x + " et " + (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= 31f));
                while (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= angleJambes + 1f)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    // Debug.Log("swingL : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                jambe00.transform.localEulerAngles = new Vector3(359f, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(359f, 0f, 0f);

                while (jambe00.transform.localEulerAngles.x > 360f - angleJambes)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    Debug.Log("swingR : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(360f - angleJambes, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(360f - angleJambes, 0f, 0f);
                Debug.Log("swingRFinal ---------------- : " + jambe00.transform.localEulerAngles.x + " et " + (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= 31f));
                while (jambe00.transform.localEulerAngles.x >= 359f - angleJambes && jambe00.transform.localEulerAngles.x < 360f)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    Debug.Log("swingL : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(0f, 0f, 0f);



                yield return null;
            }
        } else
        {
            GameObject jambe00 = jambes[0];
            GameObject jambe01 = jambes[1];

            while (true)
            {
                float timer = 0f;

                jambe00.transform.localEulerAngles = new Vector3(359f, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(359f, 0f, 0f);

                while (jambe00.transform.localEulerAngles.x > 360f - angleJambes)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    Debug.Log("swingR : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(360f - angleJambes, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(360f - angleJambes, 0f, 0f);
                Debug.Log("swingRFinal ---------------- : " + jambe00.transform.localEulerAngles.x + " et " + (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= 31f));
                while (jambe00.transform.localEulerAngles.x >= 359f - angleJambes && jambe00.transform.localEulerAngles.x < 360f)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    Debug.Log("swingL : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                while (jambe00.transform.localEulerAngles.x < angleJambes)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, angleJambes * Time.deltaTime * jambeRotationSpeed);
                    // Debug.Log("swingR : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(angleJambes, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(angleJambes, 0f, 0f);
                // Debug.Log("swingRFinal ---------------- : " + jambe00.transform.localEulerAngles.x + " et " + (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= 31f));
                while (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= angleJambes + 1f)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    // Debug.Log("swingL : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                yield return null;
            }
        }

    }

    private IEnumerator SwingHead()
    {
        float timer = 0f;
        Debug.Log("I am swinging sword");

        while (timer < swingTime / 2)
        {
            timer += Time.deltaTime;
            head.transform.RotateAround(head.transform.position, head.transform.right, -swingAngle * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingR : " + head.transform.localEulerAngles.x);

            yield return null;
        }
        head.transform.localEulerAngles = new Vector3(-swingAngle, 0f, 0f);
        Debug.Log("swingRL");
        while (timer < swingTime)
        {
            timer += Time.deltaTime;
            head.transform.RotateAround(head.transform.position, head.transform.right, +swingAngle * (Time.deltaTime / (swingTime / 2)));
            Debug.Log("swingL : " + head.transform.localEulerAngles.x);

            yield return null;
        }
        head.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

    }
}
