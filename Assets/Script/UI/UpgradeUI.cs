using UnityEngine;
using UnityEngine.UI;
using TMPro; // 如果使用 TextMeshPro

public class UpgradeUI : MonoBehaviour
{
    public Button[] optionButtons; // 拖入 3 個 Button
    public TMP_Text[] optionTexts; // 拖入對應的 Text
    public Image[] optionIcons; // 拖入對應的 Image

    private PlayerInventory currentInventory;
    private UnitData currentUnitData;
    private AbilitySO[] currentChoices;

    public void ShowUpgradeOptions(Unit unit, AbilitySO[] choices)
    {
        // 舊方法，保留兼容
        currentInventory = null;
        currentUnitData = unit.unitData;
        currentChoices = choices;
        gameObject.SetActive(true);

        for (int i = 0; i < choices.Length && i < optionButtons.Length; i++)
        {
            optionTexts[i].text = choices[i].abilityName + "\n" + choices[i].description;
            optionIcons[i].sprite = choices[i].icon;
            int index = i; // 閉包
            optionButtons[i].onClick.AddListener(() => SelectUpgrade(index));
        }
    }

    public void ShowUpgradeOptionsForInventory(PlayerInventory inventory, UnitData unitData, AbilitySO[] choices)
    {
        currentInventory = inventory;
        currentUnitData = unitData;
        currentChoices = choices;
        gameObject.SetActive(true);

        for (int i = 0; i < choices.Length && i < optionButtons.Length; i++)
        {
            optionTexts[i].text = choices[i].abilityName + "\n" + choices[i].description;
            optionIcons[i].sprite = choices[i].icon;
            int index = i; // 閉包
            optionButtons[i].onClick.AddListener(() => SelectUpgradeForInventory(index));
        }
    }

    private void SelectUpgrade(int index)
    {
        if (currentInventory == null)
        {
            // 舊邏輯，假設有 Unit
            Unit unit = FindFirstObjectByType<Unit>(); // 簡化，實際應傳入
            if (unit != null) unit.UnlockAbility(currentChoices[index]);
        }
        gameObject.SetActive(false);
        foreach (var btn in optionButtons) btn.onClick.RemoveAllListeners();
    }

    private void SelectUpgradeForInventory(int index)
    {
        currentInventory.UnlockAbility(currentUnitData, currentChoices[index]);
        // 升級場上單位
        GameManager.Instance?.LevelUpUnit(currentUnitData);
        gameObject.SetActive(false);
        foreach (var btn in optionButtons) btn.onClick.RemoveAllListeners();
    }
}