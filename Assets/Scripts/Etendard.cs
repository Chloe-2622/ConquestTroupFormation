using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Etendard : MonoBehaviour
{
    [Header("Combattant properties")]
    [SerializeField] private float damageBoost;
    [SerializeField] private float attackSpeedBoost;
    [SerializeField] private float zoneRadius;

    public bool isPlaced;

    HashSet<Troup> troupToBoost = new HashSet<Troup>();

    private EtendardType etendardType;

    public enum EtendardType { Ally, Enemy }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaced)
        {
            BoostBehaviour();
        }
    }

    private void BoostBehaviour()
    {


        HashSet<Troup> troupToCheck = etendardType == EtendardType.Ally ? GameManager.Instance.getAllies() : GameManager.Instance.getEnemies();
        foreach (Troup troup in troupToCheck)
        {
            if (Vector3.Distance(transform.position, troup.transform.position) <= zoneRadius)
            {
                if (!troupToBoost.Contains(troup))
                {
                    troupToBoost.Add(troup);
                    troup.AddDamage(damageBoost);
                    troup.ChangeAttackSpeed(attackSpeedBoost);
                    // troup.ActivateBoostParticle(true);
                }
            }
            else
            {
                if (troupToBoost.Contains(troup))
                {
                    troupToBoost.Remove(troup);
                    troup.AddDamage(-damageBoost);
                    troup.ChangeAttackSpeed(1 / attackSpeedBoost);
                    // troup.ActivateBoostParticle(false);
                }
            }

            if (!troup.IsBoosted())
            {
                troup.ActivateBoostParticle(troupToBoost.Contains(troup));
            }

        }

    }
}
