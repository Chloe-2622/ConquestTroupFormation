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

    // On s'abonne aux �v�nements du Event System
    private void OnEnable()
    {
        pauseAction.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        pauseAction.action.started += escapePress; // S'active � la pression initiale des touches
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        pauseAction.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
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
