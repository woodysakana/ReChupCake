using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragUnitIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject unitPrefab;
    public Vector2 defaultPosition;
    public UnitData unitData;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Button button; // 新增：按鈕元件

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        button = GetComponent<Button>(); // 取得按鈕元件（如果有）

        rectTransform.anchoredPosition = defaultPosition;
    }

    void OnEnable()
    {
        UpdateButtonState();
    }

    public void UpdateButtonState()
    {
        // 檢查背包是否有該單位
        var inventory = FindFirstObjectByType<PlayerInventory>();
        bool hasUnit = inventory != null && inventory.GetLevel(unitData) > 0;

        if (button != null)
            button.interactable = hasUnit; // 禁用或啟用按鈕

        // 如果用 CanvasGroup 也可以這樣做
        if (canvasGroup != null)
            canvasGroup.interactable = hasUnit;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return; // 沒有該單位不能拖曳
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GridCellDrop cellDrop = hit.collider.GetComponent<GridCellDrop>();
            if (cellDrop != null)
            {
                GameManager.Instance.SpawnUnit(
                    unitData,
                    cellDrop.gridX,
                    cellDrop.gridZ,
                    "Player"
                );
            }
        }

        rectTransform.anchoredPosition = defaultPosition;
    }
}