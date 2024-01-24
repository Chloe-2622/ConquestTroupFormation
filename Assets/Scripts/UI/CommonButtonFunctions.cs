using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonButtonFunctions : MonoBehaviour
{
    public void goToTitleScreen() { SceneManager.LoadScene("Title Screen"); }
    public void goToRules() { SceneManager.LoadScene("Rules Selection"); }
    public void goToOptions() { SceneManager.LoadScene("Options"); }
    public void resetLevel() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void quit() { Application.Quit(); }
}
