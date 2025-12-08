using UnityEngine;
using UnityEngine.UI;
using TMPro; // 如果用 TextMeshPro

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public Transform contentRoot; // 指向 Vertical Layout Group
    public GameObject unitInfoTextPrefab; // 指向 Text 或 TMP_Text 預製物件

    public void Refresh()
    {
        // 清空舊內容
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        // 顯示所有棋子和等級
        foreach (var kv in playerInventory.units)
        {
            var go = Instantiate(unitInfoTextPrefab, contentRoot);
            var text = go.GetComponent<Text>();
            if (text != null)
                text.text = $"{kv.Key.unitName}  Lv.{kv.Value}";
            var tmp = go.GetComponent<TMP_Text>();
            if (tmp != null)
                tmp.text = $"{kv.Key.unitName}  Lv.{kv.Value}";
        }

        foreach (var icon in FindObjectsByType<DragUnitIcon>(FindObjectsSortMode.None))
        {
            icon.UpdateButtonState();
        }

    }
}