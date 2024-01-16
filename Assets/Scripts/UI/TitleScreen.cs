using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void goToRules()
    {
        SceneManager.LoadScene("Rules Selection");
    }

    public void goToOptions()
    {
        OptionsManager.Instance.setPreviousScene(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("Options");
    }
    public void quit()
    {
        Application.Quit();
    }
}
