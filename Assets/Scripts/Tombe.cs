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
    private bool hasRevived;

    [Header("Scene objects")]
    [SerializeField] private Image lifeCircle;
    [SerializeField] private Camera mainCamera;

    private GameManager gameManager;

    public enum TombeTroupType { Ally, Enemy }

    public enum TombeUnitType
    {
        Null, Combattant, Archer, Cavalier, Guerisseur, Catapulte, Porte_bouclier, Porte_etendard, Batisseur, Belier
    }

    private void Awake()
    {
        gameManager = GameManager.Instance;

        lifeCircle = transform.Find("Canvas").Find("LifeCircle").GetComponent<Image>();

        mainCamera = gameManager.mainCamera;
        lifeTimeLeft = lifeTime;

        Vector3 lifeCirclePosition = mainCamera.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y, transform.position.z));
        lifeCircle.transform.position = new Vector3(lifeCirclePosition.x, lifeCirclePosition.y, lifeCirclePosition.z);

        StartCoroutine(Timer());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool HasRevived() { return hasRevived; }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.isInPause()) { lifeCircle.enabled = false; }
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

        hasRevived = true;

        Vector3 positionModel = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);

        if (tombeUnitType == TombeUnitType.Combattant)
        {
            model = Instantiate(gameManager.Combattant.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Archer)
        {
            model = Instantiate(gameManager.Archer.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Cavalier)
        {
            model = Instantiate(gameManager.Cavalier.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Guerisseur)
        {
            model = Instantiate(gameManager.Guerisseur.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Catapulte)
        {
            model = Instantiate(gameManager.Catapulte.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Porte_bouclier)
        {
            model = Instantiate(gameManager.Porte_bouclier.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Porte_etendard)
        {
            model = Instantiate(gameManager.Porte_etendard.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Batisseur)
        {
            model = Instantiate(gameManager.Batisseur.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
            StartCoroutine(ReviveAnimation(model));
        }
        if (tombeUnitType == TombeUnitType.Belier)
        {
            model = Instantiate(gameManager.Belier.transform.Find("Model").gameObject, positionModel, Quaternion.identity);
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
        Vector3 finalPosition = transform.position + new Vector3(0, gameManager.defaultHeight / 2, 0) - .8f * transform.right;

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
            GameObject spawnedUnit = Instantiate(gameManager.Combattant, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Archer)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Archer, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Cavalier)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Cavalier, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Guerisseur)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Guerisseur, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Catapulte)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Catapulte, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Porte_bouclier)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Porte_bouclier, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Porte_etendard)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Porte_etendard, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Batisseur)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Batisseur, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }
        if (tombeUnitType == TombeUnitType.Belier)
        {
            GameObject spawnedUnit = Instantiate(gameManager.Belier, transform.position - .8f * transform.right, Quaternion.identity);
            spawnedUnit.GetComponent<Troup>().troupType = (Troup.TroupType)(tombeTroupType);
            spawnedUnit.GetComponent<Troup>().addToGroup();
            Color color;
            if (tombeTroupType == TombeTroupType.Enemy && ColorUtility.TryParseHtmlString("#FF5733", out color))
            {
                Debug.Log("Couleur de la troup rez : " + color);
                spawnedUnit.GetComponent<Outline>().OutlineColor = color;
            }
        }

        

        StopCoroutine(Timer());
        Destroy(gameObject);
    }
}
