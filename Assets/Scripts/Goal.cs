using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] Vector3 goalScale;
    private GameManager gameManager;
    private LayerMask troupLayerMask;

    private void Start()
    {
        gameManager = GameManager.Instance;
        troupLayerMask = gameManager.troupMask;
    }

    // Start is called before the first frame update
    void Update()
    {
        if (gameManager.isInPause()) { return; }

        if (GameManager.Instance.isCrownCollected)
        {
            Collider[] colliders = Physics.OverlapBox(transform.position, goalScale/2, new Quaternion(0, 0, 0, 0), troupLayerMask);
            foreach (Collider collider in colliders)
            {
                Troup unit = collider.gameObject.GetComponent<Troup>();

                if (unit != null && unit.troupType == Troup.TroupType.Ally && unit.isKing())
                {
                    Debug.Log("!! " + unit + unit.gameObject.transform.position);
                    gameManager.crownCaptured();
                }
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, goalScale);
    }
}
