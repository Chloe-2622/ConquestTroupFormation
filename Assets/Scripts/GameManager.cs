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
    public LayerMask troupMask;
    public float defaultHeight;
    public float outlineWidth;
    private bool pause;

    // Allies and Enemis dictionnary -----------------------------------------------------------------------------
    private static HashSet<Troup> Allies = new HashSet<Troup>();
    private static HashSet<Troup> Enemies = new HashSet<Troup>();
    public UnityEvent updateTroupCounter;

    private void Awake()
    {
        if (updateTroupCounter == null)
            updateTroupCounter = new UnityEvent();

        // Assurez-vous qu'il n'y a qu'une seule instance du GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Gardez le GameManager lors des changements de scène
        }
        else
        {
            Destroy(gameObject); // Détruisez les doublons
        }
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
    public void addAlly(Troup troup) { Allies.Add(troup); }
    public void removeAlly(Troup troup) { Allies.Remove(troup); updateTroupCounter.Invoke();  }
    public HashSet<Troup> getAllies() { return Allies; }
    public int alliesCount() { return Allies.Count; }

    // Enemies
    public void addEnemy(Troup troup) { Enemies.Add(troup); }
    public void removeEnemy(Troup troup) { Enemies.Remove(troup); updateTroupCounter.Invoke(); }
    public HashSet<Troup> getEnemies() { return Enemies; }
    public int enemiesCount() { return Enemies.Count; }
}
