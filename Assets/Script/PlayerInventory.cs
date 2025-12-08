using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // 玩家擁有的棋子 (紀錄每個 UnitData 與等級數量)
    public Dictionary<UnitData, int> units = new Dictionary<UnitData, int>();

    // 玩家初始金錢
    public int coins = 100;

    /// <summary>
    /// 嘗試購買並加入棋子
    /// </summary>
    public void AddUnit(UnitData unitData, int cost)
    {
        if (unitData == null) return;

        // 檢查金錢是否足夠
        if (coins < cost)
        {
            Debug.Log($"金錢不足！需要 {cost}，當前 {coins}");
            return;
        }

        // 扣除金錢
        coins -= cost;

        // 判斷是否已有該棋子 → 疊加等級
        if (units.ContainsKey(unitData))
        {
            units[unitData]++;
            Debug.Log($"{unitData.unitName} 升級到 Lv{units[unitData]}！(剩餘金錢 {coins})");
        }
        else
        {
            units[unitData] = 1;
            Debug.Log($"獲得新棋子：{unitData.unitName} (Lv1) (剩餘金錢 {coins})");
        }

        // 更新 UI
        FindFirstObjectByType<InventoryUI>()?.Refresh();
    }

    /// <summary>
    /// 查詢棋子等級
    /// </summary>
    public int GetLevel(UnitData unitData)
    {
        if (unitData == null) return 0;
        return units.ContainsKey(unitData) ? units[unitData] : 0;
    }

    /// <summary>
    /// 增加金錢（例如戰鬥勝利獎勵）
    /// </summary>
    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log($"獲得金錢：+{amount} (目前金錢 {coins})");

        // 之後你也可以在這裡觸發金錢 UI 更新
        FindFirstObjectByType<InventoryUI>()?.Refresh();
    }

    /// <summary>
    /// 回收棋子（不扣錢、不增加數量，只是讓棋子可再次上場）
    /// </summary>
    public void RecallUnit(UnitData unitData)
    {
        // 這裡不用做任何事，因為背包數量本來就沒減少
        // 你可以在這裡觸發 UI 更新
        FindFirstObjectByType<InventoryUI>()?.Refresh();
    }
}
