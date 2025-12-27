using UnityEngine;

public class BonusDamageInstance : AbilityInstance
{
    private int bonusDamage;

    public BonusDamageInstance(Unit owner, AbilitySO data) : base(owner, data)
    {
        bonusDamage = ((BonusDamageAbilitySO)data).bonusDamage;
    }

    public override void OnAttack(Unit target)
    {
        if (target != null) target.TakeDamage(bonusDamage);
        Debug.Log($"{owner.name} dealt extra {bonusDamage} damage!");
    }
}