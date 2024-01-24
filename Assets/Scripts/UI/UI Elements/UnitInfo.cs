using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitInfo : MonoBehaviour
{
    [Header("Values")]
    [SerializeField] private TextMeshProUGUI cost; 
    [SerializeField] private TextMeshProUGUI health;
    [SerializeField] private TextMeshProUGUI armor;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TextMeshProUGUI attack;
    [SerializeField] private TextMeshProUGUI attackSpeed;
    [SerializeField] private TextMeshProUGUI attackRange;
    [SerializeField] private TextMeshProUGUI abilityRecharge;



    public void completeValues(int cost, float health, float armor, float speed, float attack, float attackSpeed, float attackRange, float abilityRecharge)
    {
        this.cost.text = cost.ToString();
        this.health.text = health.ToString();
        this.armor.text = armor.ToString();
        this.speed.text = speed.ToString();
        this.attack.text = attack.ToString();
        this.attackSpeed.text = attackSpeed.ToString();
        this.attackRange.text = attackRange.ToString();
        this.abilityRecharge.text = abilityRecharge.ToString();

    }
}
