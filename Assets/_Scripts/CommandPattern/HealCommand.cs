using UnityEngine;

public class HealCommand : ICommand
{
    public int ActionPointCost => 1;
    public Character Character { get; private set; }
    public int HealAmount { get; private set; }
    public GameObject EffectPrefab { get; private set; }

    public HealCommand(Character character, int healAmount, GameObject effectPrefab = null)
    {
        Character = character;
        HealAmount = healAmount;
        EffectPrefab = effectPrefab;
    }

    public bool Execute()
    {
        Character.Heal(HealAmount);

        if (EffectPrefab != null)
        {
            GameObject effect = Object.Instantiate(EffectPrefab, Character.transform.position + Vector3.up * 1f, Quaternion.identity);
            Object.Destroy(effect, 2f);
        }

        Debug.Log($"{Character.name} healed for {HealAmount}");
        return true;
    }

    public void Undo()
    {
        Character.TakeDamage(HealAmount);
    }

    public string GetCommandName()
    {
        return "Heal";
    }
}
