using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // ===== 單位進度 =====
    // UnitData -> 等級
    private Dictionary<UnitData, int> unitLevels = new();

    // UnitData -> 已解鎖能力
    private Dictionary<UnitData, List<AbilitySO>> unlockedAbilities = new();

    [Header("Refs")]
    public UpgradeUI upgradeUI;
    public GameManager gameManager;

    [Header("Economy")]
    public int coins = 100;

    // ===== 公開查詢 =====

    public int GetLevel(UnitData unitData)
    {
        if (unitData == null) return 0;
        return unitLevels.TryGetValue(unitData, out int lv) ? lv : 0;
    }

    public List<AbilitySO> GetUnlockedAbilities(UnitData unitData)
    {
        if (unitData == null) return new List<AbilitySO>();
        return unlockedAbilities.TryGetValue(unitData, out var list)
            ? list
            : new List<AbilitySO>();
    }

    public List<UnitData> GetAllUnits()
    {
        return new List<UnitData>(unitLevels.Keys);
    }


    // ===== 購買 / 合成入口 =====

    public void AddUnit(UnitData unitData, int cost)
    {
        if (unitData == null) return;

        if (coins < cost)
        {
            Debug.Log($"金錢不足，需要 {cost}，目前 {coins}");
            return;
        }

        coins -= cost;

        int newLevel = IncreaseLevel(unitData);

        Debug.Log($"{unitData.unitName} 升級至 Lv{newLevel}（剩餘金錢 {coins}）");

        if (IsAbilityChoiceLevel(newLevel))
        {
            ShowUpgradeUI(unitData);
        }
        else
        {
            ApplyLevelUpToFieldUnit(unitData);
        }

        RefreshInventoryUI();
    }

    // ===== 等級處理 =====

    private int IncreaseLevel(UnitData unitData)
    {
        if (!unitLevels.ContainsKey(unitData))
            unitLevels[unitData] = 1;
        else
            unitLevels[unitData]++;

        return unitLevels[unitData];
    }

    private bool IsAbilityChoiceLevel(int level)
    {
        return level == 3 || level == 7 || level == 9;
    }

    // ===== 升級技能 =====

    private void ShowUpgradeUI(UnitData unitData)
    {
        if (upgradeUI == null)
        {
            Debug.LogError("PlayerInventory: 找不到 UpgradeUI");
            ApplyLevelUpToFieldUnit(unitData);
            return;
        }

        List<AbilitySO> choices = new();

        if (!unlockedAbilities.ContainsKey(unitData))
            unlockedAbilities[unitData] = new List<AbilitySO>();

        foreach (var ability in unitData.upgradeOptions)
        {
            if (!unlockedAbilities[unitData].Contains(ability))
                choices.Add(ability);
        }

        if (choices.Count == 0)
        {
            ApplyLevelUpToFieldUnit(unitData);
            return;
        }

        upgradeUI.ShowUpgradeOptionsForInventory(this, unitData, choices.ToArray());
    }

    // UpgradeUI 會呼叫這個
    public void UnlockAbility(UnitData unitData, AbilitySO abilitySO)
    {
        if (unitData == null || abilitySO == null) return;

        if (!unlockedAbilities.ContainsKey(unitData))
            unlockedAbilities[unitData] = new List<AbilitySO>();

        if (unlockedAbilities[unitData].Contains(abilitySO))
            return;

        unlockedAbilities[unitData].Add(abilitySO);

        Debug.Log($"解鎖技能：{unitData.unitName} - {abilitySO.abilityName}");

        ApplyLevelUpToFieldUnit(unitData);
    }

    // ===== 場上單位同步 =====

    private void ApplyLevelUpToFieldUnit(UnitData unitData)
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        gameManager?.LevelUpUnit(unitData);
    }

    // ===== UI =====

    private void RefreshInventoryUI()
    {
        FindFirstObjectByType<InventoryUI>()?.Refresh();

        var icons = FindObjectsByType<DragUnitIcon>(FindObjectsSortMode.None);
        foreach (var icon in icons)
        {
            icon.UpdateButtonState();
        }
    }

    // ===== 金錢 =====

    public void AddCoins(int amount)
    {
        coins += amount;
        RefreshInventoryUI();
    }
}
