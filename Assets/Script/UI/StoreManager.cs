using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StoreManager : MonoBehaviour
{
    [System.Serializable]
    public class StoreSlot
    {
        public Button button;               // 商店按鈕
        public Image iconImage;             // 顯示角色圖片
        public TextMeshProUGUI nameText;    // 顯示角色名稱
        [HideInInspector] public UnitData currentUnit;
    }


    [System.Serializable]
    public class RarityProbability
    {
        public int playerLevel;        // 玩家等級
        public float commonChance;     // 普通機率
        public float rareChance;       // 稀有機率
        public float epicChance;       // 史詩機率
        public float legendaryChance;  // 傳說機率
    }

    public PlayerInventory playerInventory;   // 玩家背包
    public List<UnitData> allUnits;           // 所有棋子 (從 Inspector 指派)
    public StoreSlot[] storeSlots;            // 4 個商店格子
    public Button rerollButton;

    [Header("抽卡機率表")]
    public List<RarityProbability> rarityChances;
    public int playerLevel = 1; // TODO: 之後可以從玩家資料取得

    void Start()
    {
        GenerateStoreOptions();
        rerollButton.onClick.AddListener(GenerateStoreOptions);
    }

    // 生成商店
    public void GenerateStoreOptions()
    {
        for (int i = 0; i < storeSlots.Length; i++)
        {
            if (storeSlots[i] == null) continue;

            UnitData unit = GetRandomUnit();
            if (unit == null) continue;

            storeSlots[i].currentUnit = unit;

            // 設置圖片
            if (storeSlots[i].iconImage != null)
                storeSlots[i].iconImage.sprite = unit.unitIcon;

            // 設置文字
            if (storeSlots[i].nameText != null)
                storeSlots[i].nameText.text = $"{unit.unitName} (${unit.Cost})";

            // 設置按鈕事件
            int index = i;
            if (storeSlots[i].button != null)
            {
                storeSlots[i].button.onClick.RemoveAllListeners();
                storeSlots[i].button.onClick.AddListener(() =>
                {
                    playerInventory.AddUnit(storeSlots[index].currentUnit, storeSlots[index].currentUnit.Cost);
                });
            }
        }
    }

    // 根據稀有度抽角色
    private UnitData GetRandomUnit()
    {
        if (allUnits.Count == 0) return null;

        // 找到對應等級的機率表
        RarityProbability rp = rarityChances.Find(r => r.playerLevel == playerLevel);
        if (rp == null)
        {
            Debug.LogWarning($"找不到等級 {playerLevel} 的機率表，將隨機返回角色");
            return allUnits[Random.Range(0, allUnits.Count)];
        }

        // 抽稀有度
        float rand = Random.Range(0f, 100f);
        Rarity selectedRarity = Rarity.Common;

        if (rand < rp.commonChance)
            selectedRarity = Rarity.Common;
        else if (rand < rp.commonChance + rp.rareChance)
            selectedRarity = Rarity.Rare;
        else if (rand < rp.commonChance + rp.rareChance + rp.epicChance)
            selectedRarity = Rarity.Epic;
        else
            selectedRarity = Rarity.Legendary;

        // 過濾角色池
        List<UnitData> candidates = allUnits.FindAll(u => u.rarity == selectedRarity);

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"該稀有度 {selectedRarity} 沒有角色，退回全角色隨機");
            return allUnits[Random.Range(0, allUnits.Count)];
        }

        return candidates[Random.Range(0, candidates.Count)];
    }
}
