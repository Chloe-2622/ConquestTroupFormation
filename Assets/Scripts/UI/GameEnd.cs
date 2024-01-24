using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnd : MonoBehaviour
{
    [SerializeField] EndPanel victoryPanel;
    [SerializeField] EndPanel defeatPanel;

    public void Start()
    {
        victoryPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);
    }
    
    public void showEndPanel(bool victory, string finalSentence)
    {
        Debug.Log("!! " + victory);
        GetComponent<InGameUI>().StopAllCoroutines();
        if (victory)
        {
            victoryPanel.completeValues(finalSentence, GetComponent<InGameUI>().getTimerText());
            victoryPanel.gameObject.SetActive(true);
        }
        else
        {
            defeatPanel.completeValues(finalSentence, GetComponent<InGameUI>().getTimerText());
            defeatPanel.gameObject.SetActive(true);
        }
    }
}
