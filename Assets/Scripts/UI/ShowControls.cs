using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControls : MonoBehaviour
{
    [SerializeField] private GameObject controlsSection;
    [SerializeField] private Vector3 firstPosition = new Vector3(0, -50f, 0);
    [SerializeField] private Vector3 secondPosition = new Vector3(0, -300f, 0);
    [SerializeField] private Vector3 thirdPosition = new Vector3(0, -530f, 0);

    [Header("Controls Prefab")]
    [SerializeField] private GameObject defaultControlsPrefab;
    [SerializeField] private GameObject purchaseControlsPrefab;
    [SerializeField] private GameObject unitControlsPrefab;
    [SerializeField] private GameObject catapulteControlPrefab;

    private GameObject defaultControls;
    private GameObject purchaseControls;
    private GameObject unitControls;
    private GameObject catapulteControl;


    // Start is called before the first frame update
    void Start()
    {
        defaultControls = Instantiate(defaultControlsPrefab, controlsSection.transform);
        defaultControls.transform.localPosition = firstPosition;

        // Purchsase controls
        purchaseControls = Instantiate(purchaseControlsPrefab, controlsSection.transform);
        purchaseControls.transform.localPosition = secondPosition;

        // Unit Controls
        unitControls = Instantiate(unitControlsPrefab, controlsSection.transform);
        unitControls.transform.localPosition = secondPosition;

        // Catapulte Control
        catapulteControl = Instantiate(catapulteControlPrefab, controlsSection.transform);
        catapulteControl.transform.localPosition = thirdPosition;
        hideUnitControls();
    }

    // Purchase controls
    public void showPurchaseControls() { purchaseControls.SetActive(true); }
    public void hidePurchaseControls() { purchaseControls.SetActive(false); }

    // Unit controls
    public void showUnitControls(bool thereIsCatapulte)
    {
        if (!GameManager.Instance.hasGameStarted()) { return; }

        unitControls.SetActive(true);

        if (thereIsCatapulte) { catapulteControl.SetActive(true); }
        else { catapulteControl.SetActive(false); }
    }
    public void hideUnitControls() { unitControls.SetActive(false); catapulteControl.SetActive(false); }
}
