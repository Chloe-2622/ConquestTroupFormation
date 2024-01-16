using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tombe : MonoBehaviour
{
    [Header("General Stats")]
    [SerializeField] private TombeUnitType tombeUnitType;
    [SerializeField] public TombeTroupType tombeTroupType;
    [SerializeField] private float lifeTime = 5f;
    private float lifeTimeLeft;

    [Header("Scene objects")]
    [SerializeField] private Image lifeCircle;
    [SerializeField] private Camera camera1;


    public enum TombeTroupType { Ally, Ennemy }

    public enum TombeUnitType
    {
        Combattant, Archer, Cavalier, Guerisseur, Catapulte, Porte_bouclier, Porte_etendard, Batisseur, Belier
    }

    private void Awake()
    {
        camera1 = GameManager.Instance.mainCamera;
        lifeTimeLeft = lifeTime;

        Vector3 lifeCirclePosition = camera1.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        lifeCircle.transform.position = new Vector3(lifeCirclePosition.x, lifeCirclePosition.y, lifeCirclePosition.z);

        StartCoroutine(Timer());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 lifeCirclePosition = camera1.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        // Debug.Log("Position du lifecircle : " + lifeCirclePosition);
        // Debug.Log("position camera : " + camera1.transform.position);
        lifeCircle.transform.position = new Vector3(lifeCirclePosition.x, lifeCirclePosition.y, lifeCirclePosition.z);
    }

    public void SetUnitType(TombeUnitType type)
    {
        tombeUnitType = type;
    }

    public void SetTroupType(TombeTroupType type)
    {
        tombeTroupType = type;
        if (tombeTroupType == TombeTroupType.Ennemy)
        {
            lifeCircle.color = Color.red;
        }
    }

    public void Revive()
    {
        if (tombeUnitType == TombeUnitType.Combattant)
        {
            Instantiate(GameManager.Instance.Combattant, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Archer)
        {
            Instantiate(GameManager.Instance.Archer, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Cavalier)
        {
            Instantiate(GameManager.Instance.Cavalier, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Guerisseur)
        {
            Instantiate(GameManager.Instance.Guerisseur, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Catapulte)
        {
            Instantiate(GameManager.Instance.Catapulte, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Porte_bouclier)
        {
            Instantiate(GameManager.Instance.Porte_bouclier, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Porte_etendard)
        {
            Instantiate(GameManager.Instance.Porte_etendard, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Batisseur)
        {
            Instantiate(GameManager.Instance.Batisseur, transform.position, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Belier)
        {
            Instantiate(GameManager.Instance.Belier, transform.position, Quaternion.identity);
        }

        StopCoroutine(Timer());
        Destroy(gameObject);
    }

    public IEnumerator Timer()
    {
        while (lifeTimeLeft > 0f)
        {
            // Debug.Log(lifeCircle.transform.position);
            lifeCircle.fillAmount = lifeTimeLeft / lifeTime;
            lifeTimeLeft--;
            yield return new WaitForSeconds(1f);
        }

        Destroy(gameObject);
    }
}
