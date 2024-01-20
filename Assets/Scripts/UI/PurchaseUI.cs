using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseUI : MonoBehaviour
{
    [Header("UI Customization")]
    [SerializeField] GameObject purchaseUI;
    [SerializeField] GameObject purchaseButtonPrefab;
    [SerializeField] float buttonSize;
    [SerializeField] float buttonBackgroundSize;
    [SerializeField] float buttonSpacing;

    private InGameUI inGameUI;
    private GameManager gameManager;
    private TroupPurchase troupPurchase;


    // Start is called before the first frame update
    void Start()
    {
        gameManager  = GameManager.Instance;
        troupPurchase = gameManager.troupPurchase;

        inGameUI = this.GetComponent<InGameUI>();
        inGameUI.timer.SetActive(false);

        //Create a new navigation
        Navigation newNav = new Navigation();
        newNav.mode = Navigation.Mode.Horizontal;

        int numberOfUnitType = gameManager.getUnitPrebasLenght();

        for (int i = 1; i <= numberOfUnitType; i++)
        {
            createButton(i - numberOfUnitType / 2f, i, newNav);
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

        //Assign the new navigation to the button
        button.navigation = newNav;
    }

    public void chooseTroup(int i)
    {
        Debug.Log(((Troup.UnitType)i).ToString());
        if ((Troup.UnitType)i == troupPurchase.getCurrentSelectedTroupType())
        {
            troupPurchase.setCurrentSelectedTroupType(Troup.UnitType.Null);
        }
        else
        {
            troupPurchase.setCurrentSelectedTroupType((Troup.UnitType)i);
        }
    }

    public void startGame()
    {
        troupPurchase.gameObject.SetActive(false);
        purchaseUI.SetActive(false);
        inGameUI.timer.SetActive(true);
        inGameUI.startTimer();
    }

    public void enterUI() { Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! UI Enter");  troupPurchase.set_isOnUI(true); }
    public void exitUI() { Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! UI Exit");  troupPurchase.set_isOnUI(false); }
}
