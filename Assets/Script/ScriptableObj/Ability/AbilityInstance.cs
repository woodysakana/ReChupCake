public abstract class AbilityInstance
{
    protected Unit owner;
    protected AbilitySO data;
    public AbilitySO Data => data;


    protected AbilityInstance(Unit owner, AbilitySO data)
    {
        this.owner = owner;
        this.data = data;
    }

    // ===== 戰鬥事件 =====
    public virtual void OnAttack(Unit target) { }

    public virtual void OnTakeDamage(ref int damage) { }

    public virtual void OnDeath() { }
}
