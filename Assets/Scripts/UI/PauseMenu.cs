using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private InputActionReference pauseAction;
    [SerializeField] private Button pauseButton;

    [Header("Options")]
    [SerializeField] private GameObject optionsPanel;


    private GameManager gameManager;
    private bool isOptionsOpened;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        resumeGame();
    }

    // On s'abonne aux évènements du Event System
    private void OnEnable()
    {
        pauseAction.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        pauseAction.action.started += escapePress; // S'active à la pression initiale des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        pauseAction.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        pauseAction.action.started -= escapePress;

        pausePanel.SetActive(false);
        pauseButton.gameObject.SetActive(false);
    }

    public void escapePress(InputAction.CallbackContext context)
    {
        if (isOptionsOpened) { closeOptionsPanel(); }
        else
        {
            if (!gameManager.isInPause()) { pauseGame(); }
            else { resumeGame(); }
        }
    }


    // Options Panel
    public void openOptionsPanel() { isOptionsOpened = true; optionsPanel.SetActive(true); }
    public void closeOptionsPanel() { isOptionsOpened = false; optionsPanel.SetActive(false); }


    // Pause / Resume
    public void pauseGame()
    {
        pausePanel.SetActive(true);
        gameManager.PauseGame();
    }
    public void resumeGame()
    {
        pausePanel.SetActive(false);
        gameManager.ResumeGame();
    }
}
