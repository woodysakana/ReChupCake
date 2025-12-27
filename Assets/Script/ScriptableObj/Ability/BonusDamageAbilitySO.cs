using UnityEngine;

[CreateAssetMenu(fileName = "BonusDamageAbility", menuName = "AutoChess/Ability/BonusDamage")]
public class BonusDamageAbilitySO : AbilitySO
{
    public int bonusDamage = 5;

    public override AbilityInstance CreateInstance(Unit owner)
    {
        return new BonusDamageInstance(owner, this);
    }
}