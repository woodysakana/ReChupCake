using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 這個腳本實現了單位圖標的拖曳功能，允許玩家從UI拖曳單位到遊戲場景中的網格上放置單位
// 只有當玩家背包中有該單位時才能進行拖曳
public class DragUnitIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 公開變量：在Unity Inspector中設置
    public GameObject unitPrefab; // 單位的預製體，用於生成單位
    public Vector2 defaultPosition; // 圖標的默認位置（錨點位置）
    public UnitData unitData; // 單位的數據，包含單位信息

    // 私有變量：組件引用
    private Canvas canvas; // 父Canvas，用於縮放因子計算
    private RectTransform rectTransform; // 自身的RectTransform，用於移動
    private CanvasGroup canvasGroup; // CanvasGroup，用於控制透明度和射線投射
    private Button button; // 按鈕元件，用於控制互動性

    // Awake在物件被創建時調用，用於初始化組件引用
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); // 獲取自身的RectTransform
        canvasGroup = GetComponent<CanvasGroup>(); // 獲取CanvasGroup組件
        canvas = GetComponentInParent<Canvas>(); // 獲取父Canvas
        button = GetComponent<Button>(); // 獲取Button組件（如果存在）

        rectTransform.anchoredPosition = defaultPosition; // 設置初始位置
    }

    // OnEnable在物件啟用時調用，用於更新按鈕狀態
    void OnEnable()
    {
        UpdateButtonState();
    }

    // 更新按鈕的互動狀態，根據背包中是否有該單位來決定是否可拖曳
    public void UpdateButtonState()
    {
        // 查找場景中的PlayerInventory腳本，檢查是否有該單位
        var inventory = FindFirstObjectByType<PlayerInventory>();
        bool hasUnit = inventory != null && inventory.GetLevel(unitData) > 0;

        if (button != null)
            button.interactable = hasUnit; // 設置按鈕是否可互動

        // 如果使用CanvasGroup，也設置其互動性
        if (canvasGroup != null)
            canvasGroup.interactable = hasUnit;
    }

    // 實現IBeginDragHandler：當開始拖曳時調用
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return; // 如果按鈕不可互動，阻止拖曳
        canvasGroup.alpha = 0.6f; // 設置半透明
        canvasGroup.blocksRaycasts = false; // 阻止射線投射，避免干擾
    }

    // 實現IDragHandler：拖曳過程中持續調用
    public void OnDrag(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return; // 再次檢查是否可拖曳
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // 根據滑鼠移動更新位置，除以Canvas縮放因子
    }

    // 實現IEndDragHandler：拖曳結束時調用
    public void OnEndDrag(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return; // 檢查是否可拖曳
        canvasGroup.alpha = 1f; // 恢復不透明
        canvasGroup.blocksRaycasts = true; // 恢復射線投射

        // 從螢幕位置發射射線，檢查是否擊中3D物件
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // 檢查擊中的物件是否有GridCellDrop組件
            GridCellDrop cellDrop = hit.collider.GetComponent<GridCellDrop>();
            if (cellDrop != null)
            {
                // 如果是有效的放置位置，通過GameManager生成單位
                GameManager.Instance.SpawnUnit(
                    unitData, // 單位數據
                    cellDrop.gridX, // 網格X座標
                    cellDrop.gridZ, // 網格Z座標
                    "Player" // 玩家標籤
                );
            }
        }

        rectTransform.anchoredPosition = defaultPosition; // 重置位置到默認位置
    }
}