using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    // Instance statique du GameManager
    public static GameManager Instance { get; private set; }

    [Header("Units Prefabs")]
    public GameObject Combattant;
    public GameObject Archer;
    public GameObject Cavalier;
    public GameObject Guerisseur;
    public GameObject Catapulte;
    public GameObject Porte_bouclier;
    public GameObject Porte_etendard;
    public GameObject Batisseur;
    public GameObject Belier;

    [Header("Common to all units")]
    public GameObject unitBarsPrefab;
    public GameObject FirstPatrolPointPrefab;
    public GameObject SecondPatrolPointPrefab;
    public GameObject SelectionParticleCirclePrefab;
    public GameObject BoostParticleEffectPrefab;
    public GameObject ArmorBoostParticleEffectPrefab;
    public GameObject DamageParticlePrefab;

    [Header("Catapulte")]
    public GameObject CatapulteCroixPrefab;
    public GameObject CatapulteCroix;
    public GameObject BoulderPrefab;
    public GameObject BigBoulderPrefab;

    [Header("Archer")]
    public GameObject ArrowPrefab;

    [Header("Batisseur")]
    public GameObject WallPrefab;

    [Header("Scene Dependant Objects")]
    public Camera mainCamera;
    public SelectionManager selectionManager;
    public TroupPurchase troupPurchase;
    public EventSystem eventSystem;
    public GameObject PatrolingCircles;
    public GameObject SelectionParticleCircles;
    public GameObject CrownPosition;

    [Header("Layers")]
    public LayerMask floorMask;
    public LayerMask troupMask;
    public LayerMask allyZoneMask;

    [Header("Other")]    
    public Transform selectionArrow;
    public GameObject tombe;
    public GameObject BoostParticles;
    public float defaultHeight;
    public float outlineWidth;

    [Header("Arena Gold")]
    [SerializeField] private List<int> goldPerArena = new List<int>(5);
    private Dictionary<string, int> goldenBook = new Dictionary<string, int>();

    [Header("Text PopUps")]
    public TextMeshProUGUI TroupSelectionPopUp;
    public TextMeshProUGUI PlaceSelectionPopUp;
    public TextMeshProUGUI PatrolSelectionPopUp1;
    public TextMeshProUGUI PatrolSelectionPopUp2;
    public TextMeshProUGUI FollowSelectionPopUp;

    [Header("Victory Sentences")]
    [SerializeField] private string crownArrived;
    [SerializeField] private string allEnemiesDead;

    [Header("Defeat Sentences")]
    [SerializeField] private string deathOfKing;
    [SerializeField] private string noUnitsRemaining;

    private bool pause;
    private bool gameHasStarted;
    public bool isCrownCollected;
    public GameObject king;

    // Allies and Enemis dictionnary -----------------------------------------------------------------------------
    private static HashSet<Troup> Allies = new HashSet<Troup>();
    private static HashSet<Wall> AllyWalls = new HashSet<Wall>();
    private static HashSet<Troup> Enemies = new HashSet<Troup>();
    private static HashSet<Wall> EnemiyWalls = new HashSet<Wall>();

    private List<GameObject> UnitPrefabs = new List<GameObject>();


    // Events
    [HideInInspector] public UnityEvent updateTroupCounter;

    private void Awake()
    {
        if (updateTroupCounter == null)
            updateTroupCounter = new UnityEvent();

        // Assurez-vous qu'il n'y a qu'une seule instance du GameManager
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Gardez le GameManager lors des changements de scène
        }
        else
        {
            Destroy(gameObject); // Détruisez les doublons
        }

        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        troupPurchase = GameObject.Find("TroupPurchase").GetComponent<TroupPurchase>();
        eventSystem = GameObject.Find("UI").transform.Find("EventSystem").GetComponent<EventSystem>();
        PatrolingCircles = transform.Find("PatrolingCircles").gameObject;
        SelectionParticleCircles = transform.Find("SelectionParticleCircles").gameObject;
        CrownPosition = GameObject.Find("ZoneCouronne").gameObject;

        chargeUnitPrefab();
        completeGoldenBook();
    }

    private void OnLevelWasLoaded(int level)
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        selectionManager = GameObject.Find("SelectionManager").GetComponent<SelectionManager>();
        troupPurchase = GameObject.Find("TroupPurchase").GetComponent<TroupPurchase>();
        eventSystem = GameObject.Find("UI").transform.Find("EventSystem").GetComponent<EventSystem>();
        PatrolingCircles = transform.Find("PatrolingCircles").gameObject;
        SelectionParticleCircles = transform.Find("SelectionParticleCircles").gameObject;
        CrownPosition = GameObject.Find("ZoneCouronne").gameObject;
    }

    public void crownCaptured() { victoryOrDefeat(true, crownArrived);  }

    public void allEnemiesDefeated() { victoryOrDefeat(true, allEnemiesDead); }

    public void kingIsDead() { victoryOrDefeat(false, deathOfKing); }
    public void allUnitsAreDead() { victoryOrDefeat(false, noUnitsRemaining); }
    public void victoryOrDefeat(bool victory, string sentence)
    {
        PauseGame();
        Debug.Log("!! " + sentence);
        eventSystem.GetComponent<GameEnd>().showEndPanel(victory, sentence);
    }



    // Game has started
    public bool hasGameStarted() { return gameHasStarted; }
    public void startGame() { gameHasStarted = true;  }

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
    public void addAlly(Troup troup) 
    { 
        Allies.Add(troup);
        selectionManager.completeDictionnary(troup.gameObject);
        updateTroupCounter.Invoke(); 
    }
    public void removeAlly(Troup troup) 
    {
        Allies.Remove(troup);
        selectionManager.removeObject(troup.gameObject);
        updateTroupCounter.Invoke();
    }

    public HashSet<Troup> getAllies() { return Allies; }
    public int alliesCount() { return Allies.Count; }

    // Enemies
    public void addEnemy(Troup troup) { Enemies.Add(troup); updateTroupCounter.Invoke(); }
    public void removeEnemy(Troup troup) { Enemies.Remove(troup); updateTroupCounter.Invoke(); }
    public HashSet<Troup> getEnemies() { return Enemies; }
    public int enemiesCount() { return Enemies.Count; }

    // Ally walls
    public void addAllyWall(Wall wall) { AllyWalls.Add(wall);  }
    public void removeAllyWall(Wall wall) { AllyWalls.Remove(wall);  }
    public HashSet<Wall> getAllyWalls() { return AllyWalls; }

    // Enemy walls
    public void addEnemyWall(Wall wall) { EnemiyWalls.Add(wall); }
    public void removeEnemyWall(Wall wall) { EnemiyWalls.Remove(wall); }
    public HashSet<Wall> getEnemyWalls() { return EnemiyWalls; }


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

    // List of gold allowed for each arena
    public int getGoldInArena(string arenaName) { return goldenBook[arenaName]; }

    public void completeGoldenBook()
    {
        for (int i = 0; i < goldPerArena.Count; i++)
        {
            goldenBook.Add("Arene_" + (i+1).ToString(), goldPerArena[i]);
        }
        Debug.Log("Golden Book Completed");
    }

    private void OnDestroy()
    {
        Enemies = new HashSet<Troup>();
        Allies = new HashSet<Troup>();
    }
}
