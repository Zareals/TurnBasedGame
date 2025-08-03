using UnityEngine;

public class AttackCommand : ICommand
{
    public int ActionPointCost => 2;
    public Character Character { get; private set; }
    
    private Character target;
    private int damage;
    
    public AttackCommand(Character attacker, Character target, int damage = 10)
    {
        Character = attacker;
        this.target = target;
        this.damage = damage;
    }
    
    public bool Execute()
    {
        if (target == null || target.IsDead())
            return false;
        
        int distance = Mathf.Abs(Character.GridX - target.GridX) + Mathf.Abs(Character.GridZ - target.GridZ);
        if (distance > 1)
            return false;
        
        target.TakeDamage(damage);
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
