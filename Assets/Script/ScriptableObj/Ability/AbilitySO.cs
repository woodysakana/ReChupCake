using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    [Header("Base Info")]
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    /// <summary>
    /// 由 Unit 呼叫，建立此能力的「單位專屬實例」
    /// </summary>
    public abstract AbilityInstance CreateInstance(Unit owner);
}
