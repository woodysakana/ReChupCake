using UnityEngine;

[CreateAssetMenu(
    fileName = "HealOnAttack",
    menuName = "AutoChess/Ability/Heal On Attack"
)]
public class HealOnAttackAbilitySO : AbilitySO
{
    public int healAmount = 2;

    public override AbilityInstance CreateInstance(Unit owner)
    {
        return new HealOnAttackInstance(owner, this);
    }
}
