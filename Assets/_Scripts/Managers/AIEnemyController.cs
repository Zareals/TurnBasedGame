using UnityEngine;
using System.Collections.Generic;

public class AIEnemyController : MonoBehaviour
{
    private Character character;
    private GridManager gridManager;
    private TurnManager turnManager;

    private void Awake()
    {
        character = GetComponent<Character>();
        gridManager = FindObjectOfType<GridManager>();
        turnManager = FindObjectOfType<TurnManager>();
    }

    [ContextMenu("Plan Turn")]
    public void PlanTurn()
    {
        if (character.IsDead()) return;
        character.ClearCommands();

        Debug.Log($"Planning turn for {character.name}");

        Character target = FindClosestPlayer();
        if (target == null) return;

        if (IsAdjacent(target))
        {
            character.AddCommand(new AttackCommand(character, target, 10));
        }
        else
        {
            Vector2Int nextStep = GetNextStepTowards(target);

            if (gridManager.IsValidPosition(nextStep.x, nextStep.y) &&
                !gridManager.GetTile(nextStep.x, nextStep.y).IsOccupied())
            {
                character.AddCommand(new MoveCommand(character, nextStep.x, nextStep.y, gridManager));
            }
        }
    }

    private Character FindClosestPlayer()
    {
        List<Character> players = turnManager.GetPlayerTeam();
        Character closest = null;
        float closestDist = float.MaxValue;

        foreach (var player in players)
        {
            if (player.IsDead()) continue;

            float dist = Vector2Int.Distance(
                new Vector2Int(character.GridX, character.GridZ),
                new Vector2Int(player.GridX, player.GridZ)
            );

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = player;
            }
        }

        return closest;
    }

    private bool IsAdjacent(Character target)
    {
        int dx = Mathf.Abs(character.GridX - target.GridX);
        int dz = Mathf.Abs(character.GridZ - target.GridZ);
        return (dx + dz) == 1;
    }

    private Vector2Int GetNextStepTowards(Character target)
    {
        int x = character.GridX;
        int z = character.GridZ;

        if (target.GridX > x) x++;
        else if (target.GridX < x) x--;

        else if (target.GridZ > z) z++;
        else if (target.GridZ < z) z--;

        return new Vector2Int(x, z);
    }
}
