using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;

public class Options : MonoBehaviour
{
    [SerializeField] TMP_Dropdown QualitySelection;

    [Header("Input System")]
    [SerializeField] private InputActionReference returnAction;

    private void Start()
    {
        QualitySelection.value = OptionsManager.Instance.currentQualitySelection;
    }

    private void OnEnable()
    {
        returnAction.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        returnAction.action.started += OnInputStarted; // S'active à la pression initiale des touches
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        returnAction.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        returnAction.action.started -= OnInputStarted;
    }
    public void OnInputStarted(InputAction.CallbackContext context)
    {
        returnToPreviousScene();
    }

    public void returnToPreviousScene() 
    {
        if (OptionsManager.Instance.getPreviousScene() != null)
        {
            SceneManager.LoadScene(OptionsManager.Instance.getPreviousScene());
        }
        else
        {
            Debug.Log("There is no precedent scene");
        }
        
    }
    public void SetQuality()
    {
        OptionsManager.Instance.ChangeQuality(QualitySelection.value);

    }
}
