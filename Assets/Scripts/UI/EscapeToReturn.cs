using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapeToReturn : MonoBehaviour
{
    [SerializeField] private InputActionReference returnAction;

    private void OnEnable()
    {
        returnAction.action.Enable();
        returnAction.action.started += returnToTitleScreen;
    }

    private void OnDisable()
    {
        returnAction.action.Disable();
        returnAction.action.started -= returnToTitleScreen;
    }

    public void returnToTitleScreen(InputAction.CallbackContext context) { SceneManager.LoadScene("Title Screen"); }
}
