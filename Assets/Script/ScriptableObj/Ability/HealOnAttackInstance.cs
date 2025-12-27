public class HealOnAttackInstance : AbilityInstance
{
    private int healAmount;

    public HealOnAttackInstance(Unit owner, AbilitySO data) : base(owner, data)
    {
        healAmount = ((HealOnAttackAbilitySO)data).healAmount;
    }

    public override void OnAttack(Unit target)
    {
        owner.health = UnityEngine.Mathf.Min(
            owner.maxHealth,
            owner.health + healAmount
        );
    }
}
