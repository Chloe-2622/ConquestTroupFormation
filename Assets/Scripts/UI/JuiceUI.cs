using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuiceUI : MonoBehaviour
{
    [SerializeField] float animationDuration = 0.1f;
    [SerializeField] float endScale = 1.1f;

    private bool inBox;

    public void resizeUp()
    {
        inBox = true;
        //Debug.Log("Start Up");
        StartCoroutine(resizeUpAnimation());
    }

    public void resizeDown()
    {
        inBox = false;
        //Debug.Log("Start Down");
        //StartCoroutine(resizeDownAnimation());
    }

    public IEnumerator resizeUpAnimation()
    {
        //Do a small smooth resize animation to scale up the button a bit
        float time = 0;
        float startScale = 1f;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration  ;
            float scale = Mathf.Lerp(startScale, endScale, Mathf.SmoothStep(0.0f, 1.0f, t));
            transform.localScale = new Vector3(scale, scale, scale);

            if (!inBox)
            {
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator resizeDownAnimation()
    {
        //Do a small smooth resize animation to scale up the button a bit
        float time = 0;
        float startScale = 1f;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;
            float scale = Mathf.Lerp(startScale, endScale, Mathf.SmoothStep(0.0f, 1.0f, t));
            transform.localScale = new Vector3(scale, scale, scale);

            if (inBox)
            {
                yield break;
            }

            yield return null;
        }
    }
}
