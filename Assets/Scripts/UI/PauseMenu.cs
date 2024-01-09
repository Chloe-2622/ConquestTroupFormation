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

    // On s'abonne aux �v�nements du Event System
    private void OnEnable()
    {
        pause.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        pause.action.started += OnInputStarted; // S'active � la pression initiale des touches
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        pause.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
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
