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
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private SelectionManager selectionManager;

    [Header("Timer")]
    [SerializeField] private GameObject timer;
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
    [SerializeField] private Sprite combattantSprite ;
    [SerializeField] private Sprite archerSprite;
    [SerializeField] private Sprite cavalierSprite;
    [SerializeField] private Sprite guerisseurSprite;
    [SerializeField] private Sprite catapulteSprite;
    [SerializeField] private Sprite porte_bouclierSprite;
    [SerializeField] private Sprite porte_etendardSprite;
    [SerializeField] private Sprite batisseurSprite;
    [SerializeField] private Sprite belierSprite;

    private List<Sprite> unitSpriteList;

    // Start is called before the first frame update
    void Start()
    {
        timer.SetActive(false);
        AIName_UI.text = AIName;
        playerName_UI.text = OptionsManager.Instance.getPlayerName();

        AIUnitCounter.text = GameManager.Instance.enemiesCount().ToString();
        playerUnitCounter.text = GameManager.Instance.alliesCount().ToString();

        GameManager.Instance.updateTroupCounter.AddListener(updateCounter);
        selectionManager.newSelection.AddListener(updateSelectedTroups);

        unitSpriteList = new List<Sprite> { combattantSprite, archerSprite, cavalierSprite,
                                            guerisseurSprite, catapulteSprite, porte_bouclierSprite,
                                            porte_etendardSprite, batisseurSprite, belierSprite};
    }




    public void updateSelectedTroups()
    {
        resetUnitIcon();

        List<GameObject> currentSelections = selectionManager.getCurrentSelection();
        List<int> selectionCount = new List<int>();
        for (int i = 0; i < unitSpriteList.Count; i++)
            selectionCount.Add(0);

        foreach (GameObject selectedGameObject in currentSelections)
        {
            selectionCount[(int)selectedGameObject.GetComponent<Troup>().unitType]++;
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

                unitIcon.transform.GetChild(0).GetComponent<Image>().sprite = unitSpriteList[i];
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

    public void startGame()
    {
        startGameButton.SetActive(false);
        timer.SetActive(true);
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
