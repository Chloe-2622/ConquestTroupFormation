using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Awake()
    {
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
}
