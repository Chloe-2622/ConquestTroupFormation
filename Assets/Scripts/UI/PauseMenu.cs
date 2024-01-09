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
    [SerializeField] private InputActionReference pause;

    [SerializeField] private TextMeshProUGUI areneText;

    private bool isInPause = false;

    // Start is called before the first frame update
    void Start()
    {
        areneText.text = SceneManager.GetActiveScene().name;

        pausePanel.SetActive(false);
        isInPause = false;
        if (OptionsManager.Instance.getPreviousScene() == SceneManager.GetActiveScene().name)
        {
            pausePanel.SetActive(true);
            isInPause = true;
        }
    }

    // On s'abonne aux évènements du Event System
    private void OnEnable()
    {
        pause.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        pause.action.started += OnInputStarted; // S'active à la pression initiale des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        pause.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        pause.action.started -= OnInputStarted;
    }

    public void OnInputStarted(InputAction.CallbackContext context)
    {
        isInPause = !isInPause;
        pausePanel.SetActive(isInPause);
    }

    public void restart()
    {
        isInPause = !isInPause;
        pausePanel.SetActive(isInPause);
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
