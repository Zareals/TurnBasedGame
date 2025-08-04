using Ilumisoft.HealthSystem;
using UnityEngine;

public class AttackCommand : ICommand
{
    public int ActionPointCost => 2;
    public Character Character { get; private set; }

    private Character target;
    private int damage;
    private static GameObject hitEffectPrefab;

    public AttackCommand(Character attacker, Character target, int damage = 10)
    {
        Character = attacker;
        this.target = target;
        this.damage = damage;
    }

    public static void SetHitEffect(GameObject prefab)
    {
        hitEffectPrefab = prefab;
    }

    public bool Execute()
    {
        if (target == null || target.IsDead())
        {
            Debug.LogWarning("[AttackCommand] Target is null or dead.");
            return false;
        }

        int distance = Mathf.Abs(Character.GridX - target.GridX) + Mathf.Abs(Character.GridZ - target.GridZ);
        if (distance > 1)
        {
            Debug.LogWarning("[AttackCommand] Target out of range.");
            return false;
        }

        Debug.Log($"[AttackCommand] {Character.name} attacks {target.name} for {damage} damage.");
        target.TakeDamage(damage);

        if (Character.hitEffectPrefab != null)
        {
            Debug.Log("[AttackCommand] Spawning hit effect.");
            GameObject effect = Object.Instantiate(Character.hitEffectPrefab, target.transform.position, Quaternion.identity);
            Object.Destroy(effect, 2f);
        }else
            Debug.LogWarning("[AttackCommand] No hit effect prefab set.");

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
