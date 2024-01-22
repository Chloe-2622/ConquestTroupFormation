using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Troup;

public class Wall : Troup
{
    [Header("Wall Parameters")]
    public float maxLenght;
    public float wallFusionMaxDistance;

    [Header("GameObjects")]
    [SerializeField] private GameObject tower_1;
    [SerializeField] private GameObject tower_2;
    [SerializeField] private GameObject junctionWall;
    [SerializeField] private GameObject maxLenghtCircle;


    protected override void Awake()
    {
        maxLenghtCircle.transform.localScale = new Vector3(maxLenght*2, maxLenght*2, maxLenghtCircle.transform.localScale.z);
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

    public HashSet<Vector3> getTowersPosition() { return new HashSet<Vector3>() { tower_1.transform.position, tower_2.transform.position }; }
    public Vector3 getCentralPosition() {  return junctionWall.transform.position; }
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
