// using UnityEngine;
// using UnityEngine.UI;

// public class TestButton : MonoBehaviour
// {
//     public PlayerInventory playerInventory; // 在 Inspector 指定

//     // 按鈕呼叫時直接傳入 UnitData
//     public void OnGetUnitButton(UnitData unitData)
//     {
//         playerInventory.AddUnit(unitData);
//         // 獲得單位後刷新所有 DragUnitIcon 狀態
//         foreach (var icon in FindObjectsOfType<DragUnitIcon>())
//             icon.UpdateButtonState();
//     }
// }
