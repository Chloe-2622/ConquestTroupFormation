using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static Troup;

public class Wall : Troup
{
    [Header("Wall Parameters")]
    public float maxLenght;
    public float wallFusionMaxDistance;
    [SerializeField] private bool hideCircle;
    [SerializeField] private Vector3 healthOffset;

    [Header("GameObjects")]
    [SerializeField] private GameObject tower_1;
    [SerializeField] private GameObject tower_2;
    [SerializeField] private GameObject junctionWall;
    [SerializeField] private GameObject maxLenghtCircle;

    protected override void Awake()
    {
        maxLenghtCircle.transform.localScale = new Vector3(maxLenght*2, maxLenght*2, maxLenghtCircle.transform.localScale.z);
        if (hideCircle) { maxLenghtCircle.SetActive(false); }

        base.Awake();
    }

    public override void addToGroup()
    {
        maxLenghtCircle.SetActive(false);
        if (troupType == TroupType.Ally)
        {
            GameManager.Instance.addAllyWall(this);
        }
        if (troupType == TroupType.Enemy)
        {
            GameManager.Instance.addEnemyWall(this);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        HealthBarControl();
    }

    public override void TakeDamage(float damage)
    {
        float beforeHealth = health;

        health -= Mathf.Max(damage - armor, 0);

        ParticleSystem damageParticle = Instantiate(gameManager.DamageParticlePrefab.GetComponent<ParticleSystem>(), transform.position, Quaternion.identity);
        damageParticle.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        damageParticle.Play();
        Destroy(damageParticle.gameObject, damageParticle.main.duration);

        float newHealth = health;
        Debug.Log("--- Mur a pris dégat : " + damage);
        Debug.Log("--- Mur a " + health + " vie");

        if (beforeHealth * newHealth <= 0)
        {
            if (troupType == TroupType.Ally) { gameManager.removeAllyWall(this); }
            if (troupType == TroupType.Enemy) { gameManager.removeEnemyWall(this); }

            Destroy(SelectionParticleCircle);
            Destroy(BoostParticle);
            Destroy(ArmorBoostParticle);
            Destroy(FirstPatrolPoint);
            Destroy(SecondPatrolPoint);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(UpdateHealthBar());
        }
    }

    public List<Vector3> getTowersPosition() { return new List<Vector3>() { tower_1.transform.position, tower_2.transform.position }; }
    public Vector3 getCentralPosition() {  return junctionWall.transform.position; }
    public Vector3 getHealthPosition() { return junctionWall.transform.position + healthOffset; }

    public void setTower_1_Position(Vector3 position) { tower_1.transform.position = position; updateJunctionWall(); }
    public void setTower_2_Position(Vector3 position) { tower_2.transform.position = position; updateJunctionWall(); }

    public void updateJunctionWall()
    {
        Vector3 tower_1Pos = tower_1.transform.position;
        Vector3 tower_2Pos = tower_2.transform.position;
        junctionWall.transform.position = (tower_1Pos + tower_2Pos) /2f;
        junctionWall.transform.localScale = new Vector3(1, 1, Vector3.Distance(tower_1Pos, tower_2Pos) / 10f);
        junctionWall.transform.LookAt(tower_1.transform);
    }

    // Les murs n'attaquent pas
    protected override void IAEnemy() { }
    protected override IEnumerator Attack(Troup enemy) { yield return null; }
    protected override IEnumerator SpecialAbility() { yield return null; }
}
