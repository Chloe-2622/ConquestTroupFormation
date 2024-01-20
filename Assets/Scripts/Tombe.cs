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
    [SerializeField] private float reviveTime;
    private float lifeTimeLeft;

    [Header("Scene objects")]
    [SerializeField] private Image lifeCircle;
    [SerializeField] private Camera mainCamera;


    public enum TombeTroupType { Ally, Enemy }

    public enum TombeUnitType
    {
        Combattant, Archer, Cavalier, Guerisseur, Catapulte, Porte_bouclier, Porte_etendard, Batisseur, Belier
    }

    private void Awake()
    {
        lifeCircle = transform.Find("Canvas").Find("LifeCircle").GetComponent<Image>();

        mainCamera = GameManager.Instance.mainCamera;
        lifeTimeLeft = lifeTime;

        Vector3 lifeCirclePosition = mainCamera.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
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
        Vector3 lifeCirclePosition = mainCamera.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
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
        if (tombeTroupType == TombeTroupType.Enemy)
        {
            lifeCircle.color = Color.red;
        }
    }

    public void Revive()
    {
        GameObject model;

        Vector3 positionModel = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        if (tombeUnitType == TombeUnitType.Combattant)
        {
            model = Instantiate(GameManager.Instance.Combattant.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Archer)
        {
            model = Instantiate(GameManager.Instance.Archer.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Cavalier)
        {
            model = Instantiate(GameManager.Instance.Cavalier.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Guerisseur)
        {
            model = Instantiate(GameManager.Instance.Guerisseur.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Catapulte)
        {
            model = Instantiate(GameManager.Instance.Catapulte.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Porte_bouclier)
        {
            model = Instantiate(GameManager.Instance.Porte_bouclier.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Porte_etendard)
        {
            model = Instantiate(GameManager.Instance.Porte_etendard.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Batisseur)
        {
            model = Instantiate(GameManager.Instance.Batisseur.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Belier)
        {
            model = Instantiate(GameManager.Instance.Belier.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }

        
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

    private IEnumerator ReviveAnimation(GameObject model)
    {
        Vector3 firstPosition = model.transform.position - .8f * transform.right;
        Vector3 finalPosition = transform.position + new Vector3(0, GameManager.Instance.defaultHeight / 2, 0) - .8f * transform.right;

        model.transform.position = firstPosition;

        float elapsedTime = 0f;
        while (elapsedTime < reviveTime)
        {
            model.transform.position = firstPosition + (elapsedTime / reviveTime) * (finalPosition - firstPosition);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        model.transform.position = finalPosition;

        Destroy(model);

        if (tombeUnitType == TombeUnitType.Combattant)
        {
            Instantiate(GameManager.Instance.Combattant, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Archer)
        {
            Instantiate(GameManager.Instance.Archer, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Cavalier)
        {
            Instantiate(GameManager.Instance.Cavalier, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Guerisseur)
        {
            Instantiate(GameManager.Instance.Guerisseur, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Catapulte)
        {
            Instantiate(GameManager.Instance.Catapulte, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Porte_bouclier)
        {
            Instantiate(GameManager.Instance.Porte_bouclier, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Porte_etendard)
        {
            Instantiate(GameManager.Instance.Porte_etendard, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Batisseur)
        {
            Instantiate(GameManager.Instance.Batisseur, transform.position - .8f * transform.right, Quaternion.identity);
        }
        if (tombeUnitType == TombeUnitType.Belier)
        {
            Instantiate(GameManager.Instance.Belier, transform.position - .8f * transform.right, Quaternion.identity);
        }

        StopCoroutine(Timer());
        Destroy(gameObject);
    }
}
