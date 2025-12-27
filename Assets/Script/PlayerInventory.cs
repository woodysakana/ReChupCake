using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // 玩家擁有的棋子 (紀錄每個 UnitData 與等級數量)
    public Dictionary<UnitData, int> units = new Dictionary<UnitData, int>();

    // 玩家解鎖的技能 (每個 UnitData 的解鎖技能列表)
    public Dictionary<UnitData, List<AbilitySO>> unlockedAbilities = new Dictionary<UnitData, List<AbilitySO>>();

    public UpgradeUI upgradeUI;

    // 玩家初始金錢
    public int coins = 100;
    public GameManager gameManager;

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
            int newLevel = units[unitData];
            Debug.Log($"{unitData.unitName} 升級到 Lv{newLevel}！(剩餘金錢 {coins})");

            // 檢查是否需要選擇技能
            if (newLevel == 3 || newLevel == 7 || newLevel == 9)
            {
                ShowUpgradeUIForUnit(unitData);
            }
            else
            {
                // 升級場上單位
                Debug.Log($"PlayerInventory: 升級 {unitData.unitName} 的場上單位。");
                GameManager.Instance?.LevelUpUnit(unitData);
            }
        }
        else
        {
            units[unitData] = 1;
            Debug.Log($"獲得新棋子：{unitData.unitName} (Lv1) (剩餘金錢 {coins})");
        }

        RecallUnit(unitData); // 回收棋子，讓它可再次上場

    }

    /// <summary>
    /// 解鎖指定 UnitData 的技能
    /// </summary>
    public void UnlockAbility(UnitData unitData, AbilitySO abilitySO)
    {
        if (unitData == null || abilitySO == null) return;

        if (!unlockedAbilities.ContainsKey(unitData))
        {
            unlockedAbilities[unitData] = new List<AbilitySO>();
        }

        if (!unlockedAbilities[unitData].Contains(abilitySO))
        {
            unlockedAbilities[unitData].Add(abilitySO);
            Debug.Log($"PlayerInventory: 解鎖 {unitData.unitName} 的技能 {abilitySO.abilityName}");
        }
    }

    /// <summary>
    /// 獲取指定 UnitData 的解鎖技能列表
    /// </summary>
    public List<AbilitySO> GetUnlockedAbilities(UnitData unitData)
    {
        if (unitData == null) return new List<AbilitySO>();
        return unlockedAbilities.ContainsKey(unitData) ? unlockedAbilities[unitData] : new List<AbilitySO>();
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
    /// 顯示指定 UnitData 的升級 UI
    /// </summary>
    public void RecallUnit(UnitData unitData)
    {
        // 這裡不用做任何事，因為背包數量本來就沒減少
        // 你可以在這裡觸發 UI 更新
        // 更新 UI
        FindFirstObjectByType<InventoryUI>()?.Refresh();

        // 更新所有拖曳圖標的狀態（新版、效能佳）
        var dragIcons = FindObjectsByType<DragUnitIcon>(FindObjectsSortMode.None);
        foreach (var icon in dragIcons)
        {
            icon.UpdateButtonState();
        }
    }
    private void ShowUpgradeUIForUnit(UnitData unitData)
    {
        List<AbilitySO> choices = new List<AbilitySO>();
        foreach (var option in unitData.upgradeOptions)
        {
            if (!GetUnlockedAbilities(unitData).Contains(option))
                choices.Add(option);
        }

        if (choices.Count > 0)
        {
            if (upgradeUI != null)
            {
                Debug.Log($"PlayerInventory: 顯示 {unitData.unitName} 的升級選項。");
                upgradeUI.ShowUpgradeOptionsForInventory(this, unitData, choices.ToArray());
            }
            else
            {
                Debug.LogError("PlayerInventory: 找不到 UpgradeUI！");
            }
        }
        else
        {
            Debug.Log($"PlayerInventory: {unitData.unitName} 沒有可選升級。");
            // 仍然升級場上單位
            GameManager.Instance?.LevelUpUnit(unitData);
        }
    }
    public int GetLevel(UnitData unitData){
        if (unitData == null) return 0;
        return units.ContainsKey(unitData) ? units[unitData] : 0;
    }
}
