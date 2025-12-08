/*
基本用途
用途：當玩家從 UI 拖動角色圖示到棋盤上的格子時，這個格子能接收角色並在該位置生成角色。
放置位置：通常掛在每個棋盤格子物件上。
*/


// 建議放在棋盤格子物件上
using UnityEngine;
using UnityEngine.EventSystems;

public class GridCellDrop : MonoBehaviour, IDropHandler//實作 IDropHandler 介面 → 表示這個物件可以接收拖放事件。
{
    public int gridX, gridZ; // 每個格子會知道自己在棋盤上的座標。方便生成角色時定位格子。

    public void OnDrop(PointerEventData eventData)
    {
        DragUnitIcon dragIcon = eventData.pointerDrag?.GetComponent<DragUnitIcon>();
        if (dragIcon != null)
        {
            // 取得 GridManager 實例（新版 API）
            GridManager gridManager = FindFirstObjectByType<GridManager>();
            if (gridManager != null)
            {
                Vector3 worldPos = gridManager.GetWorldPosition(gridX, gridZ);
                GameManager.Instance.SpawnUnit(dragIcon.unitData, gridX, gridZ, "Player");

                Debug.Log($"Dropped {dragIcon.unitPrefab.name} to cell ({gridX},{gridZ})");
            }
            else
            {
                Debug.LogWarning("找不到 GridManager 實例！");
            }
        }
    }

}