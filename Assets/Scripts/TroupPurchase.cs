using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private InputActionReference rotateUnitAction;
    [SerializeField] private InputActionReference removeUnitAction;
    [SerializeField] private InputActionReference removeSelectionAction;
    [SerializeField] private InputActionReference chooseUnitAction;

    [Header("Placement Options")]
    [SerializeField] private float minDistanceBetweenUnits;
    [SerializeField] private float maxRemoveDistance;
    [SerializeField] private float unitRotation;

    [Header("Debug")]
    [SerializeField] private bool debugMode;

    private LayerMask allyZoneLayerMask;
    private LayerMask troupMask;   

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
    private int maxUsableGold;

    [HideInInspector] public UnityEvent refreshPreview;
    [HideInInspector] public UnityEvent goldUpdate;
    [HideInInspector] public UnityEvent notEnoughtGold;


    private void OnEnable()
    {
        refreshPreview.AddListener(callShowPlacement);

        placeUnitAction.action.Enable(); // Activer l'action d'entrée lorsque le script est désactivé
        showPlacementAction.action.Enable();
        rotateUnitAction.action.Enable();
        removeUnitAction.action.Enable();
        removeSelectionAction.action.Enable();
        chooseUnitAction.action.Enable();
        placeUnitAction.action.performed += placeUnit; // S'active lorque la valeur de ReadValue change
        showPlacementAction.action.performed += showPlacement;
        rotateUnitAction.action.started += rotateUnit;
        removeUnitAction.action.performed += removeUnit;
        removeSelectionAction.action.started += removeSelection;
        chooseUnitAction.action.started += chooseUnit;
    }

    // On se désabonne aux évènements du Event System
    private void OnDisable()
    {
        Debug.Log("Troup Purchase Disabled");
        GameObject.Destroy(preview);

        refreshPreview.RemoveAllListeners();

        placeUnitAction.action.Disable(); // Désactiver l'action d'entrée lorsque le script est désactivé
        showPlacementAction.action.Disable();
        rotateUnitAction.action.Disable();
        removeUnitAction.action.Disable();
        removeSelectionAction.action.Disable();
        chooseUnitAction.action.Disable();
        placeUnitAction.action.performed -= placeUnit;
        showPlacementAction.action.performed -= showPlacement;
        rotateUnitAction.action.started -= rotateUnit;
        removeUnitAction.action.performed -= removeUnit;
        removeSelectionAction.action.started -= removeSelection;
        chooseUnitAction.action.started -= chooseUnit;
    }

    public void Start()
    {
        gameManager = GameManager.Instance;

        // Layers
        allyZoneLayerMask = gameManager.allyZoneMask;
        troupMask = gameManager.troupMask;
        
        if (debugMode) { usableGold = gameManager.getGoldInArena("Arene_1"); }
        else { usableGold = gameManager.getGoldInArena(SceneManager.GetActiveScene().name); }
        goldUpdate.Invoke();
        unitPrefabs = gameManager.getUnitPrefabs();

        Debug.Log("-- "  + usableGold);
        maxUsableGold = usableGold;
    }


    public void callShowPlacement()
    {
        showPlacement(lastContext);
    }

    private void showPlacement(InputAction.CallbackContext context)
    {
        // Si la souris est sur l'UI, on ne fait rien
        if (isOnUI) { return; }

        // Si on a pas sélectionné de troupe, on ne fait rien
        if (currentSelectedUnitType == Troup.UnitType.Null) { return; }

        // On regarde le point du niveau touché par notre souris
        Ray ray = gameManager.mainCamera.ScreenPointToRay(context.action.ReadValue<Vector2>());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, allyZoneLayerMask))
        {
            // On cherche le point le plus proche sur le NavMesh
            NavMeshHit closestHit;

            if (NavMesh.SamplePosition(hit.point, out closestHit, 10, 1))
            {
                // Si la preview existe et est du bon type, on la déplace vers la position pointée
                if (preview != null && previewTroupComponent.unitType == currentSelectedUnitType)
                {
                    preview.transform.position = closestHit.position;
                }
                // Sinon on (re)crée la preview
                else
                {
                    GameObject.Destroy(preview);
                    preview = Instantiate(unitPrefabs[(int)currentSelectedUnitType - 1], closestHit.position, new Quaternion(0, 0, 0, 0));
                    preview.transform.eulerAngles = new Vector3(0, 90, 0);
                    previewTroupComponent = preview.GetComponent<Troup>();
                    previewTroupComponent.enabled = false;
                    unitGoldCost = previewTroupComponent.getCost();
                    preview.GetComponent<NavMeshAgent>().enabled = false;
                }

                // On sauvegarde la dernière position valide
                lastPosition = closestHit.position;
                lastContext = context;
            }
            else
            {
                Debug.Log("Couldn't find near NavMesh");
            }
        }
    }

    public void rotateUnit(InputAction.CallbackContext context)
    {
        // Si on a pas sélectionné de troupe, on ne fait rien
        if (currentSelectedUnitType == Troup.UnitType.Null) { return; }

        if (preview != null)
        {
            preview.transform.Rotate(new Vector3(0, unitRotation, 0));
        }
    }

    private void placeUnit(InputAction.CallbackContext context)
    {
        // Si la souris est sur l'UI, le click peut être celui d'un bouton
        if (isOnUI) { return; }

        // Si on a pas sélectionné de troupe, on ne fait rien
        if (currentSelectedUnitType == Troup.UnitType.Null) { return; }

        if (usableGold - unitGoldCost < 0)
        {
            notEnoughtGold.Invoke();
            return;
        }

        // Si l'unité est trop près d'une unité déjà placée, il ne se passe rien
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
        newUnit.transform.eulerAngles = new Vector3(0, 90, 0);
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
        else
        {
            setCurrentSelectedTroupType(Troup.UnitType.Null);
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

    public void chooseUnit(InputAction.CallbackContext context)
    {
        // Pour obtenir la touche pressée
        InputControl control = context.control;
        int unitIndex = int.Parse(control.name);

        Debug.Log(((Troup.UnitType)unitIndex).ToString());
        if ((Troup.UnitType)unitIndex == currentSelectedUnitType)
        {
            setCurrentSelectedTroupType(Troup.UnitType.Null);
        }
        else
        {
            setCurrentSelectedTroupType((Troup.UnitType)unitIndex);
        }
        refreshPreview.Invoke();
    }

    // is Cursor on UI
    public void set_isOnUI(bool newValue) { isOnUI = newValue; }

    // Current Troup Selection
    public Troup.UnitType getCurrentSelectedTroupType() { return currentSelectedUnitType; }
    public void setCurrentSelectedTroupType(Troup.UnitType selectedTroupType) { currentSelectedUnitType = selectedTroupType; GameObject.Destroy(preview); }

    // Usable gold gold
    public int getUsableGold() { return usableGold; }
}
