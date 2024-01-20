using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // Instance statique du GameManager
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject Combattant;
    public GameObject Archer;
    public GameObject Cavalier;
    public GameObject Guerisseur;
    public GameObject Catapulte;
    public GameObject Porte_bouclier;
    public GameObject Porte_etendard;
    public GameObject Batisseur;
    public GameObject Belier;
    public GameObject FirstPatrolPointPrefab;
    public GameObject SecondPatrolPointPrefab;
    public GameObject SelectionParticleCirclePrefab;
    public GameObject CatapulteCroixPrefab;
    public GameObject BoulderPrefab;

    [Header("Text PopUps")]
    public TextMeshProUGUI TroupSelectionPopUp;
    public TextMeshProUGUI PlaceSelectionPopUp;
    public TextMeshProUGUI PatrolSelectionPopUp1;
    public TextMeshProUGUI PatrolSelectionPopUp2;
    public TextMeshProUGUI FollowSelectionPopUp;

    [Header("Misc")]
    public SelectionManager selectionManager;
    public Transform selectionArrow;
    public GameObject tombe;
    public Camera mainCamera;
    public LayerMask floorMask;
    public GameObject PatrolingCircles;
    public GameObject SelectionParticleCircles;
    public GameObject CatapulteCroix;
    public LayerMask troupMask;
    public float defaultHeight;
    public float outlineWidth;

    [Header("Purchase")]
    public TroupPurchase troupPurchase;

    private bool pause;
    public bool isCrownCollected;
    public GameObject king;

    // Allies and Enemis dictionnary -----------------------------------------------------------------------------
    private static HashSet<Troup> Allies = new HashSet<Troup>();
    private static HashSet<Troup> Enemies = new HashSet<Troup>();

    private List<GameObject> UnitPrefabs = new List<GameObject>();
    public UnityEvent updateTroupCounter;

    private void Awake()
    {
        if (updateTroupCounter == null)
            updateTroupCounter = new UnityEvent();

        // Assurez-vous qu'il n'y a qu'une seule instance du GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Gardez le GameManager lors des changements de sc�ne
        }
        else
        {
            Destroy(gameObject); // D�truisez les doublons
        }

        chargeUnitPrefab();
    }

        // Pause
    public bool isInPause() { return pause; }

    public void PauseGame()
    {
        pause = true;
        Time.timeScale = 0;
    }
    public void ResumeGame()
    {
        pause = false;
        Time.timeScale = 1; 
    }

    // Allies
    public void addAlly(Troup troup) { Allies.Add(troup); updateTroupCounter.Invoke(); }
    public void removeAlly(Troup troup) { Allies.Remove(troup); updateTroupCounter.Invoke();  }
    public HashSet<Troup> getAllies() { return Allies; }
    public int alliesCount() { return Allies.Count; }

    // Enemies
    public void addEnemy(Troup troup) { Enemies.Add(troup); updateTroupCounter.Invoke(); }
    public void removeEnemy(Troup troup) { Enemies.Remove(troup); updateTroupCounter.Invoke(); }
    public HashSet<Troup> getEnemies() { return Enemies; }
    public int enemiesCount() { return Enemies.Count; }

    // List of unit prefabs

    public List<GameObject> getUnitPrefabs() { return UnitPrefabs; }
    public int getUnitPrebasLenght() { return UnitPrefabs.Count; }


    // Complete list of Unit Prefabs
    public void chargeUnitPrefab()
    {
        UnitPrefabs.Add(Combattant);
        UnitPrefabs.Add(Archer);
        UnitPrefabs.Add(Cavalier);
        UnitPrefabs.Add(Guerisseur);
        UnitPrefabs.Add(Catapulte);
        UnitPrefabs.Add(Porte_bouclier);
        UnitPrefabs.Add(Porte_etendard);
        UnitPrefabs.Add(Batisseur);
        UnitPrefabs.Add(Belier);

        Debug.Log(UnitPrefabs.Count);
    }
}
