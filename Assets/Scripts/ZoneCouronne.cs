using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneCouronne : MonoBehaviour
{
    private GameObject crown;
    private bool crownCollected;

    void Awake()
    {
        crown = transform.Find("Crown").gameObject;
        StartCoroutine(FloatCrown());
        StartCoroutine(SpinCrown());
    }

    // Update is called once per frame
    void Update()
    {
        if (!crownCollected)
        {
            Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2);

            foreach (Collider collider in colliders)
            {
                Troup unit = collider.gameObject.GetComponent<Troup>();
                if (unit != null && unit.troupType == Troup.TroupType.Ally)
                {
                    unit.transform.Find("Crown").gameObject.SetActive(true);
                    crownCollected = true;
                    Destroy(crown);
                }
            }
        }

    }

    private IEnumerator FloatCrown()
    {
        float time = 0f;

        while (!crownCollected)
        {
            crown.transform.position = new Vector3(crown.transform.position.x, transform.position.y + .5f * Mathf.Sin(time), crown.transform.position.z);

            time += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator SpinCrown()
    {
        while (!crownCollected)
        {
            crown.transform.Rotate(new Vector3(0, 0, .5f));

            yield return null;
        }
    }
}
