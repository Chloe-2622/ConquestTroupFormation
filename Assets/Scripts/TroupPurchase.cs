using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TroupPurchase : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] private InputActionReference placeUnitAction;
    [SerializeField] private InputActionReference showPlacementAction;
    [SerializeField] private InputActionReference removeUnitAction;
    [SerializeField] private InputActionReference removeSelectionAction;

    [Header("Placement Options")]
    [SerializeField] private float minDistanceBetweenUnits;
    [SerializeField] private float maxRemoveDistance;

    [Header("Layers")]
    [SerializeField] private LayerMask allyZoneLayerMask;
    [SerializeField] private LayerMask troupMask;

    [Header("Debug")]
    [SerializeField] private bool debugMode;

    private List<GameObject> unitPrefabs;
    private Troup.UnitType currentSelectedUnitType;

    private GameManager gameManager;
    private bool isOnUI;
    private Vector3 lastPosition;
    private InputAction.CallbackContext lastContext;

    private GameObject preview;
    private Troup previewTroupComponent;
    private int unitGoldCost;
    private int usableGold;

    [HideInInspector] public UnityEvent cameraMovement;
    [HideInInspector] public UnityEvent goldUpdate;
    [HideInInspector] public UnityEvent notEnoughtGold;


    private void OnEnable()
    {
        cameraMovement.AddListener(callShowPlacement);

        placeUnitAction.action.Enable(); // Activer l'action d'entr�e lorsque le script est d�sactiv�
        showPlacementAction.action.Enable();
        removeUnitAction.action.Enable();
        removeSelectionAction.action.Enable();
        placeUnitAction.action.performed += placeUnit; // S'active lorque la valeur de ReadValue change
        showPlacementAction.action.performed += showPlacement;
        removeUnitAction.action.performed += removeUnit;
        removeSelectionAction.action.started += removeSelection;
    }

    // On se d�sabonne aux �v�nements du Event System
    private void OnDisable()
    {
        Debug.Log("Troup Purchase Disabled");
        GameObject.Destroy(preview);

        cameraMovement.RemoveAllListeners();

        placeUnitAction.action.Disable(); // D�sactiver l'action d'entr�e lorsque le script est d�sactiv�
        showPlacementAction.action.Disable();
        removeUnitAction.action.Disable();
        removeSelectionAction.action.Disable();
        placeUnitAction.action.performed -= placeUnit;
        showPlacementAction.action.performed -= showPlacement;
        removeUnitAction.action.performed -= removeUnit;
        removeSelectionAction.action.started -= removeSelection;
    }

    public void Start()
    {
        gameManager = GameManager.Instance;
        if (debugMode) { usableGold = gameManager.getGoldInArena("Arene_1"); }
        else { usableGold = gameManager.getGoldInArena(SceneManager.GetActiveScene().name); }
        goldUpdate.Invoke();
        unitPrefabs = gameManager.getUnitPrefabs();
    }


    public void callShowPlacement()
    {
        showPlacement(lastContext);
    }

    private void showPlacement(InputAction.CallbackContext context)
    {
        // Si la souris est sur l'UI, on ne fait rien
        if (isOnUI) { return; }

        // Si on a pas s�lectionn� de troupe, on ne fait rien
        if (currentSelectedUnitType == Troup.UnitType.Null) { return; }

        // On regarde le point du niveau touch� par notre souris
        Ray ray = gameManager.mainCamera.ScreenPointToRay(context.action.ReadValue<Vector2>());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, allyZoneLayerMask))
        {
            // On cherche le point le plus proche sur le NavMesh
            NavMeshHit closestHit;

            if (NavMesh.SamplePosition(hit.point, out closestHit, 10, 1))
            {
                // Si la preview existe et est du bon type, on la d�place vers la position point�e
                if (preview != null && previewTroupComponent.unitType == currentSelectedUnitType)
                {
                    preview.transform.position = closestHit.position;
                }
                // Sinon on (re)cr�e la preview
                else
                {
                    GameObject.Destroy(preview);
                    preview = Instantiate(unitPrefabs[(int)currentSelectedUnitType - 1], closestHit.position, new Quaternion(0, 0, 0, 0));
                    previewTroupComponent = preview.GetComponent<Troup>();
                    previewTroupComponent.enabled = false;
                    unitGoldCost = previewTroupComponent.getCost();
                    preview.GetComponent<NavMeshAgent>().enabled = false;
                }

                // On sauvegarde la derni�re position valide
                lastPosition = closestHit.position;
                lastContext = context;
            }
            else
            {
                Debug.Log("Couldn't find near NavMesh");
            }
        }
    }   

    private void placeUnit(InputAction.CallbackContext context)
    {
        // Si la souris est sur l'UI, le click peut �tre celui d'un bouton
        if (isOnUI) { return; }

        // Si on a pas s�lectionn� de troupe, on ne fait rien
        if (currentSelectedUnitType == Troup.UnitType.Null) { return; }

        if (usableGold - unitGoldCost < 0)
        {
            notEnoughtGold.Invoke();
            return;
        }

        // Si l'unit� est trop pr�s d'une unit� d�j� plac�e, il ne se passe rien
        HashSet<Troup> allies = gameManager.getAllies();
        Debug.Log(allies.Count);
        foreach (Troup ally in allies)
        {
            if (Vector3.Distance(ally.gameObject.transform.position, lastPosition) < minDistanceBetweenUnits)
            {
                Debug.Log(Vector3.Distance(ally.gameObject.transform.position, lastPosition));
                return;
            }
        }

        usableGold -= unitGoldCost;
        goldUpdate.Invoke();
        GameObject newUnit = Instantiate(unitPrefabs[(int)currentSelectedUnitType - 1], lastPosition, new Quaternion(0, 0, 0, 0));
        //newUnit.GetComponent<Troup>().enabled = false;
        gameManager.addAlly(newUnit.GetComponent<Troup>());
        Debug.Log("Unit succesfully added");
    }

    private void removeUnit(InputAction.CallbackContext context)
    {


        Ray ray = gameManager.mainCamera.ScreenPointToRay(context.action.ReadValue<Vector2>());
        RaycastHit hit_troup;
        RaycastHit hit_allyZone;

        Troup troupToRemove = null;

        if (Physics.Raycast(ray, out hit_troup, Mathf.Infinity, troupMask))
        {
            troupToRemove = hit_troup.transform.gameObject.GetComponent<Troup>();
        }
        else if (Physics.Raycast(ray, out hit_allyZone, Mathf.Infinity, allyZoneLayerMask))
        {
            float minDistance = maxRemoveDistance;
            HashSet<Troup> allies = gameManager.getAllies();
            foreach (Troup ally in allies)
            {
                float distance = Vector3.Distance(hit_allyZone.point, ally.gameObject.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    troupToRemove = ally;
                }
            }
        }
        if (troupToRemove != null && troupToRemove != previewTroupComponent)
        {
            usableGold += troupToRemove.getCost();
            goldUpdate.Invoke();

            GameObject.Destroy(troupToRemove.gameObject);
            gameManager.removeAlly(troupToRemove);
        }
    }

    private void removeSelection(InputAction.CallbackContext context)
    {
        List<GameObject> listToRemove = gameManager.selectionManager.getCurrentSelection();
        foreach (GameObject selected in listToRemove)
        {
            Troup troupToRemove = selected.GetComponent<Troup>();
            usableGold += troupToRemove.getCost();
            goldUpdate.Invoke();

            GameObject.Destroy(troupToRemove.gameObject);
            gameManager.removeAlly(troupToRemove);
        }
    }



    // is Cursor on UI
    public void set_isOnUI(bool newValue) { isOnUI = newValue; }

    // Current Troup Selection
    public Troup.UnitType getCurrentSelectedTroupType() { return currentSelectedUnitType; }
    public void setCurrentSelectedTroupType(Troup.UnitType selectedTroupType) { currentSelectedUnitType = selectedTroupType; GameObject.Destroy(preview); }

    // Usable gold gold
    public int getUsableGold() { return usableGold; }
}