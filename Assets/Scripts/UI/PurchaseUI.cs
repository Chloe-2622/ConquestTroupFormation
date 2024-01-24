using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.PackageManager;

public class PurchaseUI : MonoBehaviour
{
    [Header("UI Customization")]
    [SerializeField] private GameObject purchaseUI;
    [SerializeField] private GameObject purchaseButtonPrefab;
    [SerializeField] private float buttonSize;
    [SerializeField] private float buttonBackgroundSize;
    [SerializeField] private float buttonSpacing;

    [Header("Gold")]
    [SerializeField] private TextMeshProUGUI goldCount;
    [SerializeField] private TextMeshProUGUI popUp;
    [SerializeField] private float loopTime;
    [SerializeField] private int numberOfLoop;

    [Header("Unit Info")]
    [SerializeField] private UnitInfo unitInfoPrefab;


    private InGameUI inGameUI;
    private GameManager gameManager;
    private TroupPurchase troupPurchase;


    // Start is called before the first frame update
    void Start()
    {
        popUp.gameObject.SetActive(false);
        gameManager  = GameManager.Instance;
        troupPurchase = gameManager.troupPurchase;

        troupPurchase.goldUpdate.AddListener(updateGoldCount);
        troupPurchase.notEnoughtGold.AddListener(notEnoughtGoldShow);

        inGameUI = this.GetComponent<InGameUI>();
        //inGameUI.timer.SetActive(false);

        //Create a new navigation
        Navigation newNav = new Navigation();
        newNav.mode = Navigation.Mode.Horizontal;

        int numberOfUnitType = gameManager.getUnitPrebasLenght();

        for (int i = 1; i <= numberOfUnitType; i++)
        {
            createButton(i - (numberOfUnitType + 1) / 2f, i, newNav);
        }
    }

    // Create a button for selecting an element
    private void createButton(float place, int index, Navigation newNav)
    {
        GameObject purchaseButton = GameObject.Instantiate(purchaseButtonPrefab, Vector3.zero, Quaternion.identity);
        RectTransform rectTrans = purchaseButton.GetComponent<RectTransform>();

        purchaseButton.transform.SetParent(purchaseUI.transform); // setting parent
        rectTrans.anchoredPosition = new Vector2(place * (buttonSize + buttonSpacing), 0f); // set position

        GameObject unitImage = purchaseButton.transform.GetChild(0).gameObject;

        // To add a custom size (prefab as normally the good size)
        rectTrans.sizeDelta = new Vector2(buttonBackgroundSize, buttonBackgroundSize);
        rectTrans.localScale = new Vector2(1, 1);
        RectTransform rectTransButton = unitImage.gameObject.GetComponent<RectTransform>();
        rectTransButton.sizeDelta = new Vector2(buttonSize, buttonSize);

        // Add image to the button
        Texture2D tex = Resources.Load<Texture2D>(((Troup.UnitType)index).ToString());

        Image buttonImage = unitImage.gameObject.GetComponent<Image>();
        buttonImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        // Add function to the button
        Button button = purchaseButton.GetComponent<Button>();
        button.onClick.AddListener(delegate { chooseTroup(index); });

        // Get unit informations
        GameObject unitInfo = purchaseButton.transform.GetChild(1).gameObject;
        if (gameManager.getUnitPrefabs()[index - 1] != null)
        {
            Troup unit = gameManager.getUnitPrefabs()[index - 1].GetComponent<Troup>();
            unitInfo.GetComponent<UnitInfo>().completeValues(unit.getCost(), unit.getHealth(), 
                                                             unit.getArmor(), unit.getSpeed(),
                                                             unit.getAttack(), unit.getAttackSpeed(),
                                                             unit.getAttackRange(), unit.getAbilityRecharge());
        }
        unitInfo.SetActive(false);

        // Add event to the button
        EventTrigger eventTrigger = purchaseButton.GetComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => { unitInfo.SetActive(true); });
        eventTrigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => { unitInfo.SetActive(false); });
        eventTrigger.triggers.Add(exitEntry);

        //Assign the new navigation to the button
        button.navigation = newNav;
    }

    public void chooseTroup(int unitIndex)
    {
        Debug.Log(((Troup.UnitType)unitIndex).ToString());
        if ((Troup.UnitType)unitIndex == troupPurchase.getCurrentSelectedTroupType())
        {
            troupPurchase.setCurrentSelectedTroupType(Troup.UnitType.Null);
        }
        else
        {
            troupPurchase.setCurrentSelectedTroupType((Troup.UnitType)unitIndex);
        }
    }

    public void startGame()
    {
        troupPurchase.gameObject.SetActive(false);
        purchaseUI.SetActive(false);
        inGameUI.timer.SetActive(true);
        inGameUI.startTimer();
        gameManager.startGame();
    }

    // Gold
    public void updateGoldCount() { goldCount.text = troupPurchase.getUsableGold().ToString(); }
    public void notEnoughtGoldShow() { StartCoroutine(NotEnoughtGoldPopup()); }

    protected IEnumerator NotEnoughtGoldPopup()
    {
        popUp.gameObject.SetActive(true);
        popUp.alpha = 0;

        float t = 0f;
        int demiLoop = 0;

        while (demiLoop < 2*numberOfLoop)
        {
            t += Time.deltaTime;
            if (demiLoop % 2 == 0)
            {
                popUp.alpha = Mathf.Lerp(0, 1, 2*t/loopTime);
            }
            else
            {
                popUp.alpha = 1 - Mathf.Lerp(0, 1, 2 * t / loopTime);
            }

            if (t >= loopTime/2)
            {
                demiLoop++;
                t = 0f;
            }
            yield return null;
        }
        popUp.alpha = 0;
        popUp.gameObject.SetActive(true);
    }



    public void enterUI() { troupPurchase.set_isOnUI(true); }
    public void exitUI() { troupPurchase.set_isOnUI(false); }
}
