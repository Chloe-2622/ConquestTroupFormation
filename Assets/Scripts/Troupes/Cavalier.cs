using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cavalier : Troup
{
    [Header("Cavalier properties")]
    [SerializeField] private float chargeTime;
    [SerializeField] private float chargeSpeed;
    [SerializeField] private float baseJambeRotationSpeed;
    [SerializeField] private float jambeRotationSpeed;
    [SerializeField] private float angleJambes;

    private bool isRunning;
    private GameObject[] jambes1 = new GameObject[2];
    private GameObject[] jambes2 = new GameObject[2];

    IEnumerator animJambe1;
    IEnumerator animJambe2;

    protected override void Awake()
    {
        base.Awake();

        jambes1[0] = (transform.Find("PateAvDP").gameObject);
        jambes1[1] = (transform.Find("PateArGP").gameObject);
        jambes2[0] = (transform.Find("PateArDP").gameObject);
        jambes2[1] = (transform.Find("PateAvGP").gameObject);



        animJambe1 = MoveAnimation(true);
        animJambe2 = MoveAnimation(false);
    }

    // Update is called once per frame
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
    }

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
                    Debug.Log("swingR : " + jambe00.transform.localEulerAngles.x);

                    yield return null;
                }
                jambe00.transform.localEulerAngles = new Vector3(30f, 0f, 0f);
                jambe01.transform.localEulerAngles = new Vector3(30f, 0f, 0f);
                Debug.Log("swingRFinal ---------------- : " + jambe00.transform.localEulerAngles.x + " et " + (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= 31f));
                while (jambe00.transform.localEulerAngles.x > 0 && jambe00.transform.localEulerAngles.x <= 31f)
                {
                    timer += Time.deltaTime;
                    jambe00.transform.RotateAround(jambe00.transform.position, jambe00.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
                    jambe01.transform.RotateAround(jambe01.transform.position, jambe01.transform.right, -angleJambes * Time.deltaTime * jambeRotationSpeed);
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
                while (jambe00.transform.localEulerAngles.x >= 369f - angleJambes && jambe00.transform.localEulerAngles.x < 0f)
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
        }

    }

    protected override IEnumerator Attack(Troup enemy)
    {
        while (enemy != null)
        {
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
}
