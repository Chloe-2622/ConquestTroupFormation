using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;

public class Options : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionReference returnAction;

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
    public void OnInputStarted(InputAction.CallbackContext context) { SceneManager.LoadScene("Title Screen"); }
}
