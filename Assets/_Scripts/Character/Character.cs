using System.Collections;
using System.Collections.Generic;
using Ilumisoft.HealthSystem;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private int maxActionPoints = 3;
    public bool isPlayerControlled = true;

    public int GridX { get; private set; }
    public int GridZ { get; private set; }

    private int currentActionPoints;
    private List<ICommand> commandQueue = new List<ICommand>();

    // Reference to Ilumisoft Health component
    private Health healthComponent;

    public System.Action<Character> OnCommandAdded;
    public System.Action<Character> OnCommandRemoved;

    void Awake()
    {
        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
            Debug.LogError($"{name} is missing Health component!");
    }

    void Start()
    {
        currentActionPoints = maxActionPoints;
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

    // Forward damage to Health system
    public void TakeDamage(float damage)
    {
        var health = GetComponent<Health>();
        if (health != null)
        {
            Debug.Log($"{name} before damage: {health.CurrentHealth}");
            health.ApplyDamage(damage);
            Debug.Log($"{name} took {damage} damage. Current health now: {health.CurrentHealth}");
            if (!health.IsAlive)
            {
                Debug.Log($"{name} died.");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"{name} has no Health component!");
        }
    }


    public void Heal(float amount)
    {
        if (healthComponent != null)
        {
            healthComponent.AddHealth(amount);
        }
    }

    public bool IsDead()
    {
        return healthComponent == null || !healthComponent.IsAlive;
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

    public IEnumerator ExecuteCommandCoroutine(ICommand command)
    {
        bool success = command.Execute();
        if (!success)
            Debug.Log($"Command {command.GetCommandName()} failed.");
        yield return new WaitForSeconds(0.5f);
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
