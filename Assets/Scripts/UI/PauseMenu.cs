using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private InputActionReference pauseAction;

    // Start is called before the first frame update
    void Start()
    {
        resumeGame();

        if (OptionsManager.Instance.getPreviousScene() == SceneManager.GetActiveScene().name)
        {
            pausePanel.SetActive(true);
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
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        Debug.Log(GameManager.Instance.isInPause());
        if (!GameManager.Instance.isInPause())
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
        GameManager.Instance.PauseGame();
    }
    public void resumeGame()
    {
        pausePanel.SetActive(false);
        GameManager.Instance.ResumeGame();
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

    public void quit()
    {
        Application.Quit();
    }
}
