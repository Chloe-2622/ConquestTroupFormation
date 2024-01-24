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
        returnAction.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        returnAction.action.started += OnInputStarted; // S'active � la pression initiale des touches
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        returnAction.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
        returnAction.action.started -= OnInputStarted;
    }
    public void OnInputStarted(InputAction.CallbackContext context) { SceneManager.LoadScene("Title Screen"); }
}
