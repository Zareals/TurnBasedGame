using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private int health = 30;
    public bool isPlayerControlled = true;
    
    public int GridX { get; private set; }
    public int GridZ { get; private set; }
    
    private int currentActionPoints;
    private int currentHealth;
    private List<ICommand> commandQueue = new List<ICommand>();
    
    public System.Action<Character> OnCommandAdded;
    public System.Action<Character> OnCommandRemoved;
    
    void Start()
    {
        currentActionPoints = maxActionPoints;
        currentHealth = health;
    }
    
    public void InitializePosition(int x, int z)
    {
        MoveTo(x, z);
    }
    
    public void MoveTo(int x, int z)
    {
        GridX = x;
        GridZ = z;
        transform.position = new Vector3(x, transform.position.y, z);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(health, currentHealth + amount);
    }
    
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
    
    public bool CanAddCommand(ICommand command)
    {
        int totalCost = GetQueuedActionPointCost() + command.ActionPointCost;
        return totalCost <= maxActionPoints;
    }
    
    public void AddCommand(ICommand command)
    {
        if (CanAddCommand(command))
        {
            commandQueue.Add(command);
            OnCommandAdded?.Invoke(this);
        }
    }
    
    public void RemoveCommand(int index)
    {
        if (index >= 0 && index < commandQueue.Count)
        {
            commandQueue.RemoveAt(index);
            OnCommandRemoved?.Invoke(this);
        }
    }
    
    public void ClearCommands()
    {
        commandQueue.Clear();
        OnCommandRemoved?.Invoke(this);
    }
    
    public int GetQueuedActionPointCost()
    {
        int total = 0;
        foreach (var command in commandQueue)
        {
            total += command.ActionPointCost;
        }
        return total;
    }
    
    public int GetRemainingActionPoints()
    {
        return maxActionPoints - GetQueuedActionPointCost();
    }
    
    public int GetMaxActionPoints()
    {
        return maxActionPoints;
    }
    
    public List<ICommand> GetCommands()
    {
        return new List<ICommand>(commandQueue);
    }
    
    public IEnumerator ExecuteCommands()
    {
        foreach (var command in commandQueue)
        {
            bool success = command.Execute();
            if (!success)
            {
                Debug.Log($"Command failed to execute for {name}");
            }
            yield return new WaitForSeconds(0.5f);
        }
        
        commandQueue.Clear();
        currentActionPoints = maxActionPoints;
    }
}