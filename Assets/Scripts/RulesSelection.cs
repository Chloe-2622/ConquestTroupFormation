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

    [Header("Input System")]
    [SerializeField] private InputActionReference returnAction;

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















    private void OnEnable()
    {
        returnAction.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        returnAction.action.started += OnInputStarted; // S'active � la pression initiale des touches
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        returnAction.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
        returnAction.action.started -= OnInputStarted;
    }
    public void OnInputStarted(InputAction.CallbackContext context)
    {
        goToTitleScreen();
    }

    public void goToTitleScreen()
    {
        SceneManager.LoadScene("Title Screen");
    }

}
