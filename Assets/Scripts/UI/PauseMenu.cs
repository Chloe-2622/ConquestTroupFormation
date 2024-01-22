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

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        resumeGame();

        if (OptionsManager.Instance.getPreviousScene() == SceneManager.GetActiveScene().name)
        {
            pauseGame();
        }
    }

    // On s'abonne aux évènements du Event System
    private void OnEnable()
    {
        pauseAction.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        pauseAction.action.started += OnInputStarted; // S'active à la pression initiale des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        pauseAction.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        pauseAction.action.started -= OnInputStarted;

        pausePanel.SetActive(false);
        pauseButton.gameObject.SetActive(false);
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        Debug.Log(gameManager.isInPause());
        if (!gameManager.isInPause())
        {
            pauseGame();
        }
        else
        {
            resumeGame();
        }
    }

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

    public void goToOptions()
    {
        OptionsManager.Instance.setPreviousScene(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("Options");
    }

    public void goToTitleScreen()
    {
        OptionsManager.Instance.setPreviousScene("Title Screen");
        SceneManager.LoadScene("Title Screen");
    }

    public void resetGame() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void quit() { Application.Quit(); }
}
