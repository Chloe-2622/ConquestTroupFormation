using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private GameObject timer;
    [SerializeField] private TextMeshProUGUI timer_text;

    [SerializeField] private GameObject startGameButton;

    // Start is called before the first frame update
    void Start()
    {
        timer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }














    public void startGame()
    {
        startGameButton.SetActive(false);
        timer.SetActive(true);
        StartCoroutine(Timer());
    }


    public IEnumerator Timer()
    {
        float beginning = Time.time;
        while (true)
        {
            int seconds = (int) (Time.time - beginning);
            int minutes = seconds / 60;
            seconds %= 60;

            string sec_str = seconds.ToString();
            string min_str = minutes.ToString();

            if (seconds < 10) { sec_str = "0" + sec_str; }
            if (minutes < 10) { min_str = "0" + min_str;  }

            timer_text.text = min_str + ":" + sec_str;
            yield return null;
        }
    }
}
