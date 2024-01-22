using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class EndPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalSentence;
    [SerializeField] TextMeshProUGUI timer;

    private GameManager gameManager;

    public void OnEnable()
    {
        gameManager = GameManager.Instance;

        gameManager.PauseGame();
        gameManager.eventSystem.GetComponent<InGameUI>().enabled = false;
        gameManager.eventSystem.GetComponent<PauseMenu>().enabled = false;
    }

    public void OnDisable()
    {
        gameManager.eventSystem.GetComponent<InGameUI>().enabled = true;
        gameManager.eventSystem.GetComponent<PauseMenu>().enabled = true;
        GameManager.Instance.ResumeGame();
    }

    public void completeValues(string finalSentence, string timer)
    {
        this.finalSentence.text = finalSentence;
        this.timer.text = timer;
    }


}
