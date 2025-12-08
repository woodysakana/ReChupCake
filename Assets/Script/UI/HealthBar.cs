using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    public Slider slider;
    private Unit targetUnit;

    public void Setup(Unit unit)
    {
        targetUnit = unit;
        slider.maxValue = unit.maxHealth;  // 用 maxHealth 做基準
        slider.value = unit.health;
    }

    void Update()
    {
        if (targetUnit != null)
        {
            slider.value = targetUnit.health;

            // 跟隨角色頭頂
            Vector3 screenPos = Camera.main.WorldToScreenPoint(
                targetUnit.transform.position + Vector3.up * 2f
            );
            transform.position = screenPos;
        }
    }
}
