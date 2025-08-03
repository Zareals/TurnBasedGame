using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private List<Character> playerTeam = new List<Character>();
    [SerializeField] private List<Character> enemyTeam = new List<Character>();
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CardHandManager cardHandManager;

    private bool isPlayerTurn = true;
    private bool isExecutingTurn = false;

    public System.Action<bool> OnTurnChanged;

    void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        StartCoroutine(InitializeCharacterPositions());
    }

    private IEnumerator InitializeCharacterPositions()
    {
        yield return new WaitForSeconds(0.1f); // Wait for grid to be created

        // Place player characters on left side
        foreach (var character in playerTeam)
        {
            if (character != null)
            {
                Vector2Int pos = gridManager.GetRandomPositionOnSide(true);
                character.InitializePosition(pos.x, pos.y);
                gridManager.GetTile(pos.x, pos.y).OccupyingCharacter = character;
            }
        }

        // Place enemy characters on right side
        foreach (var character in enemyTeam)
        {
            if (character != null)
            {
                Vector2Int pos = gridManager.GetRandomPositionOnSide(false);
                character.InitializePosition(pos.x, pos.y);
                gridManager.GetTile(pos.x, pos.y).OccupyingCharacter = character;
            }
        }
    }

    public void EndTurn()
    {
        if (isExecutingTurn) return;

        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        isExecutingTurn = true;
        gridManager.ResetAllTileColors();

        List<Character> currentTeam = isPlayerTurn ? playerTeam : enemyTeam;

        // PLAN enemy actions if it's their turn
        if (!isPlayerTurn)
        {
            foreach (var enemy in enemyTeam)
            {
                if (enemy == null || enemy.IsDead()) continue;
                enemy.GetComponent<AIEnemyController>()?.PlanTurn();
            }
        }

        // EXECUTE queued commands
        foreach (var character in currentTeam)
        {
            if (!character.IsDead())
            {
                yield return StartCoroutine(character.ExecuteCommands());
            }
        }

        // Flip turn
        isPlayerTurn = !isPlayerTurn;
        isExecutingTurn = false;

        // Notify UI
        OnTurnChanged?.Invoke(isPlayerTurn);
        cardHandManager.ClearAllCards();
        Debug.Log($"Turn ended. Now it's {(isPlayerTurn ? "Player" : "Enemy")} turn");

        // *** If it's enemy's turn next, auto-execute immediately ***
        if (!isPlayerTurn)
        {
            // Wait briefly so UI can update
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(ExecuteTurn());
        }
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn && !isExecutingTurn;
    }

    public bool IsExecutingTurn()
    {
        return isExecutingTurn;
    }

    public List<Character> GetPlayerTeam() => playerTeam;
    public List<Character> GetEnemyTeam() => enemyTeam;
}
