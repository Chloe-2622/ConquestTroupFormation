using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class RulesSelection : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] TMP_Dropdown arenaSelection;
    [SerializeField] TMP_InputField playerNameInput;


    public void Start()
    {
        playButton.interactable = false;
    }

    public void setChosenArena()
    {
        OptionsManager.Instance.chosenArena = arenaSelection.value;
        enablePlayButton();
    }

    public void setPlayerName()
    {
        OptionsManager.Instance.setPlayerName(playerNameInput.text);
        enablePlayButton();
    }

    public void enablePlayButton()
    {
        playButton.GetComponent<Button>().interactable = OptionsManager.Instance.isReady();
    }

    public void startGame()
    {
        SceneManager.LoadScene("Arene_" + OptionsManager.Instance.chosenArena.ToString());
    }
}
