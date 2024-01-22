using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Belier : Troup
{
    [Header("Belier properties")]
    [SerializeField] private float wheelRotationSpeed;

    private Animator mAnimator;
    private bool isRolling;
    private HashSet<GameObject> roues = new HashSet<GameObject>();

    IEnumerator moveAnimation;

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

        moveAnimation = MoveAnimation();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        AttackBehaviour();

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
    }

    protected override void IAEnemy() { }

    protected override IEnumerator Attack(Troup enemy)
    {
        // mAnimator.SetTrigger("Attack");
        Debug.Log("Belier attack");
        yield return null;
    }

    protected override IEnumerator SpecialAbility()
    {
        Debug.Log("Bélier special ability activated");
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
            roue.transform.Rotate(new Vector3(- (rotationSpeed * (agent.velocity.magnitude / agent.speed) ) / 12, 0f, 0f));

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
