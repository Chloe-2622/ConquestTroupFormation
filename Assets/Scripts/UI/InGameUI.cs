using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Events;
using static Troup;
using System.ComponentModel;

public class InGameUI : MonoBehaviour
{
    private SelectionManager selectionManager;

    [Header("Timer")]
    [SerializeField] public GameObject timer;
    [SerializeField] private TextMeshProUGUI timer_text;

    [Header("Player Counter")]
    [SerializeField] private TextMeshProUGUI playerName_UI;
    [SerializeField] private TextMeshProUGUI playerUnitCounter;

    [Header("AI Counter")]
    [SerializeField] private string AIName;
    [SerializeField] private TextMeshProUGUI AIName_UI;
    [SerializeField] private TextMeshProUGUI AIUnitCounter;

    [Header("Unit Icon Sprite")]
    [SerializeField] private Vector3 iconPosition = new Vector3(0, 0, 0);
    [SerializeField] private float pading = 20;
    [SerializeField] private GameObject unitIconsSection;
    [SerializeField] private GameObject unitIconPrefab;

    [SerializeField] private Color combatantColor;
    [SerializeField] private Color archerColor;
    [SerializeField] private Color cavalierColor;
    [SerializeField] private Color guerisseurColor;
    [SerializeField] private Color catapulteColor;
    [SerializeField] private Color porte_bouclierColor;
    [SerializeField] private Color porte_etendardColor;
    [SerializeField] private Color batisseurColor;
    [SerializeField] private Color belierColor;

    [Header("Health and Ability Display")]
    [SerializeField] public GameObject bars;

    private List<Color> unitColorList;

    // Start is called before the first frame update
    void Start()
    {
        selectionManager = GameManager.Instance.selectionManager;

        AIName_UI.text = AIName;
        playerName_UI.text = OptionsManager.Instance.getPlayerName();

        AIUnitCounter.text = GameManager.Instance.enemiesCount().ToString();
        playerUnitCounter.text = GameManager.Instance.alliesCount().ToString();

        GameManager.Instance.updateTroupCounter.AddListener(updateCounter);
        selectionManager.newSelection.AddListener(updateSelectedTroups);

        unitColorList = new List<Color> { combatantColor, archerColor, cavalierColor,
                                            guerisseurColor, catapulteColor, porte_bouclierColor,
                                            porte_etendardColor, batisseurColor, belierColor};
    }

    public void updateSelectedTroups()
    {
        resetUnitIcon();

        List<GameObject> currentSelections = selectionManager.getCurrentSelection();
        List<int> selectionCount = new List<int>();
        for (int i = 0; i < unitColorList.Count; i++)
            selectionCount.Add(0);

        foreach (GameObject selectedGameObject in currentSelections)
        {
            selectionCount[(int)(selectedGameObject.GetComponent<Troup>().unitType - 1)]++;
        }

        string liste = "";
        for (int i = 0; i < selectionCount.Count; i++) { liste += selectionCount[i] + ", "; }            
        Debug.Log(liste);

        int j = 0;
        for (int i = 0; i < selectionCount.Count; i++)
        {
            if (selectionCount[i] != 0)
            {
                //Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                GameObject unitIcon = Instantiate(unitIconPrefab);

                unitIcon.transform.SetParent(unitIconsSection.transform);
                unitIcon.transform.localPosition = iconPosition + new Vector3(0, -pading*j, 0);
                j++;

                unitIcon.transform.GetChild(0).GetComponent<Image>().color = unitColorList[i];
                unitIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectionCount[i].ToString();
            }
        }
    }

    public void resetUnitIcon()
        {
            foreach (Transform child in unitIconsSection.transform)
            {
                Debug.Log(child);
                GameObject.Destroy(child.gameObject);
            }
    }

    private void updateCounter()
    {
        AIUnitCounter.text = GameManager.Instance.enemiesCount().ToString();
        playerUnitCounter.text = GameManager.Instance.alliesCount().ToString();
    }    

    public void startTimer()
    {
        StartCoroutine(Timer());
    }

    public IEnumerator Timer()
    {
        float beginning = Time.time;
        while (true)
        {
            int seconds = (int) (Time.time - beginning);
            int minutes = seconds / 60;
            seconds %= 60;

            string sec_str = seconds.ToString();
            string min_str = minutes.ToString();

            if (seconds < 10) { sec_str = "0" + sec_str; }
            if (minutes < 10) { min_str = "0" + min_str;  }

            timer_text.text = min_str + ":" + sec_str;
            yield return null;
        }
    }
}
