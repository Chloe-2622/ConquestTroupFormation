using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using static Troup;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class InGameUI : MonoBehaviour
{
    /* 
    TODO

    Dans l'ordre :
        - 
        - 
        - Catapulte



   Trouver de jolis fonds
   Ajouter les choix des options en jeu
   
    Cherche comment changer la résolution
        
    Bonus: Faire l'indication des dégats        

    
    Touches                                                         DONE
    Options                                                         DONE
    Refresh la sélection des troupes lors de la supression          DONE
    Bug des golds qui reapparaissent                                DONE
    Bug Camera quand on tiens troupe                                DONE      
    Mur prends pas dégat au centre                                  DONE
    Collision preview mur                                           DONE
    Attaque des murs par les unités si pas de chemin possible       DONE
    Changer la qualité                                              DONE
    Batisseur et murs                                               DONE
    Ecrans de victoire/défaite                                      DONE
    Debug le fait de pouvoir jouer en purchase phase                DONE
    Ctrl + A = sélectionne tout les unités                          DONE
    1,2,3,4 ... pour choisir les unités                             DONE
    R pour rotate les unités                                        DONE
    */





    private SelectionManager selectionManager;
    [Header("Global")]
    [SerializeField] private GameObject inGameUI;

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

    [Header("Is Unit's formation persistent")]
    [SerializeField] public GameObject checkBox;
    [SerializeField] private InputActionReference forcePersistentFormationAction;
    private bool isCheckBoxAvailable;
    private Toggle toggle;


    private List<Color> unitColorList;
    private GameManager gameManager;

    public void OnEnable()
    {
        inGameUI.SetActive(true);
        unitIconsSection.SetActive(true);
        forcePersistentFormationAction.action.Enable();
        forcePersistentFormationAction.action.started += checkBoxKeyBoardUpdate;
    }

    public void OnDisable()
    {
        forcePersistentFormationAction.action.Disable();
        forcePersistentFormationAction.action.started -= checkBoxKeyBoardUpdate;

        unitIconsSection.SetActive(false);
        checkBox.SetActive(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        selectionManager = gameManager.selectionManager;

        AIName_UI.text = AIName;
        playerName_UI.text = OptionsManager.Instance.getPlayerName();

        checkBox.SetActive(false);
        toggle = checkBox.GetComponent<Toggle>();

        AIUnitCounter.text = gameManager.enemiesCount().ToString();
        playerUnitCounter.text = gameManager.alliesCount().ToString();

        gameManager.updateTroupCounter.AddListener(updateCounter);
        selectionManager.newSelection.AddListener(updateSelectedTroups);
        gameManager.troupPurchase.resetSelection.AddListener(resetUnitIcon);

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
                GameObject unitIcon = Instantiate(unitIconPrefab);

                unitIcon.transform.SetParent(unitIconsSection.transform);
                unitIcon.transform.localPosition = iconPosition + new Vector3(0, -pading*j, 0);
                j++;

                unitIcon.transform.GetChild(0).GetComponent<Image>().color = unitColorList[i];
                unitIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = selectionCount[i].ToString();
            }
        }

        if (currentSelections.Count > 1) { showCheckBox(); }
        else { hideCheckBox(); }
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
        AIUnitCounter.text = gameManager.enemiesCount().ToString();
        playerUnitCounter.text = gameManager.alliesCount().ToString();
        if (gameManager.hasGameStarted())
        {
            if (gameManager.enemiesCount() == 0) { gameManager.allEnemiesDefeated(); }
            else if (gameManager.alliesCount() == 0) { gameManager.allUnitsAreDead(); }
        }
    }    


    // CheckBox
    public void hideCheckBox() { isCheckBoxAvailable = false;  checkBox.SetActive(false); }
    public void showCheckBox()
    { 
        if (gameManager.hasGameStarted())
        {
            isCheckBoxAvailable = true;
            toggle.SetIsOnWithoutNotify(gameManager.isFormationShapeForced);
            checkBox.SetActive(true);
        } 
    }
    public void checkBoxUpadte() { gameManager.isFormationShapeForced = toggle.isOn; showCheckBox(); }

    public void checkBoxKeyBoardUpdate(InputAction.CallbackContext context) 
    { 
        if (!isCheckBoxAvailable) { return; }
        gameManager.isFormationShapeForced = !gameManager.isFormationShapeForced;
        showCheckBox(); 
    }




    // Timer
    public void startTimer()
    {
        StartCoroutine(Timer());
    }

    public string getTimerText() { return timer_text.text; }

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
