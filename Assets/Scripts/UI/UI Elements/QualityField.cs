using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QualityField : MonoBehaviour
{
    private TMP_Dropdown qualitySelection;

    // Start is called before the first frame update
    void Start()
    {
        qualitySelection = transform.GetChild(0).GetComponent<TMP_Dropdown>();
        qualitySelection.SetValueWithoutNotify(OptionsManager.Instance.currentQualitySelection);
    }

    public void setQuality() { OptionsManager.Instance.ChangeQuality(qualitySelection.value); }
}
