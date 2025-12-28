using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] optionButtons;    // 拖入 3 個 Button
    public TMP_Text[] optionTexts;    // 對應的 Text
    public Image[] optionIcons;       // 對應的 Image

    private PlayerInventory currentInventory;
    private UnitData currentUnitData;
    private AbilitySO[] currentChoices;

    // ===== 供 Unit 呼叫（舊兼容） =====
    public void ShowUpgradeOptions(Unit unit, AbilitySO[] choices)
    {
        // 單純展示，不會改變 PlayerInventory
        currentInventory = null;
        currentUnitData = unit.unitData;
        currentChoices = choices;
        gameObject.SetActive(true);

        SetupButtons(SelectUpgrade);
    }

    // ===== 供 PlayerInventory 呼叫 =====
    public void ShowUpgradeOptionsForInventory(PlayerInventory inventory, UnitData unitData, AbilitySO[] choices)
    {
        currentInventory = inventory;
        currentUnitData = unitData;
        currentChoices = choices;
        gameObject.SetActive(true);

        SetupButtons(SelectUpgradeForInventory);
    }

    // ===== 生成按鈕事件 =====
    private void SetupButtons(System.Action<int> callback)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].gameObject.SetActive(i < currentChoices.Length);

            if (i < currentChoices.Length)
            {
                optionTexts[i].text = currentChoices[i].abilityName + "\n" + currentChoices[i].description;
                optionIcons[i].sprite = currentChoices[i].icon;

                int index = i; // 關閉引用
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => callback(index));
            }
        }
    }

    // ===== 按鈕回調（舊 Unit 模式） =====
    private void SelectUpgrade(int index)
    {
        Unit unit = FindFirstObjectByType<Unit>();
        if (unit != null && currentChoices != null && index < currentChoices.Length)
        {
            // 直接套用能力到單位（CreateInstance）
            unit.activeAbilities.Add(currentChoices[index].CreateInstance(unit));
            Debug.Log($"{unit.unitData.unitName} 套用能力 {currentChoices[index].abilityName}");
        }

        CloseUI();
    }

    // ===== 按鈕回調（Inventory 模式） =====
    private void SelectUpgradeForInventory(int index)
    {
        if (currentInventory != null && currentChoices != null && index < currentChoices.Length)
        {
            currentInventory.UnlockAbility(currentUnitData, currentChoices[index]);
        }

        CloseUI();
    }

    // ===== 通用關閉 UI =====
    private void CloseUI()
    {
        gameObject.SetActive(false);
        foreach (var btn in optionButtons)
            btn.onClick.RemoveAllListeners();
    }
}
