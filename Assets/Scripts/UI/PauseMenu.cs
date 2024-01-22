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

    // On s'abonne aux �v�nements du Event System
    private void OnEnable()
    {
        pauseAction.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        pauseAction.action.started += OnInputStarted; // S'active � la pression initiale des touches
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        pauseAction.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
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
