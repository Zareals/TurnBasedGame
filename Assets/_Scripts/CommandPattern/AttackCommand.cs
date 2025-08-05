using Ilumisoft.HealthSystem;
using UnityEngine;

public class AttackCommand : ICommand
{
    public int ActionPointCost => 1;
    public Character Character { get; private set; }
    public Sprite CardSprite { get; private set; }
    public GameObject EffectPrefab { get; private set; }

    private Character target;
    private int damage;

    public AttackCommand(Character attacker, Character target, int damage = 10, Sprite cardSprite = null, GameObject effectPrefab = null)
    {
        Character = attacker;
        this.target = target;
        this.damage = damage;
        CardSprite = cardSprite;
        EffectPrefab = effectPrefab;
    }

    public bool Execute()
    {
        if (target == null || target.IsDead())
            return false;

        int distance = Mathf.Abs(Character.GridX - target.GridX) + Mathf.Abs(Character.GridZ - target.GridZ);
        if (distance > 1)
            return false;

        target.TakeDamage(damage);

        // Spawn effect on target
        if (EffectPrefab != null)
        {
            GameObject effect = Object.Instantiate(EffectPrefab, target.transform.position + Vector3.up * 1f, Quaternion.identity);
            Object.Destroy(effect, 2f);
        }

        Debug.Log($"{Character.name} attacks {target.name} for {damage} damage!");
        return true;
    }

    public void Undo()
    {
        if (target != null)
            target.Heal(damage);
    }

    public string GetCommandName()
    {
        return "Attack";
    }
}
