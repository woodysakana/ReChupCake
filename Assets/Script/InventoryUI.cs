using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public Transform contentRoot;           // Vertical Layout Group
    public GameObject unitInfoTextPrefab;   // Text 或 TMP_Text

    public void Refresh()
    {
        if (playerInventory == null || contentRoot == null || unitInfoTextPrefab == null)
            return;

        // 清空舊內容
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

        // 顯示背包中的所有單位
        foreach (UnitData unitData in playerInventory.GetAllUnits())
        {
            int level = playerInventory.GetLevel(unitData);
            int unlockedCount = playerInventory.GetUnlockedAbilities(unitData).Count;

            string displayText = $"{unitData.unitName}  Lv.{level}";
            if (unlockedCount > 0)
                displayText += $"（{unlockedCount} 能力）";

            GameObject go = Instantiate(unitInfoTextPrefab, contentRoot);

            // 同時支援 Text / TMP_Text
            if (go.TryGetComponent(out TMP_Text tmp))
                tmp.text = displayText;
            else if (go.TryGetComponent(out Text text))
                text.text = displayText;
        }

        // 更新所有拖曳圖示狀態
        foreach (var icon in FindObjectsByType<DragUnitIcon>(FindObjectsSortMode.None))
        {
            icon.UpdateButtonState();
        }
    }
}
