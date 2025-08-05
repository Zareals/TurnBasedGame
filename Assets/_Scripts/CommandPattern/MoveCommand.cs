using UnityEngine;
using DG.Tweening;

public class MoveCommand : ICommand
{
    public int ActionPointCost => 1;
    public Character Character { get; private set; }

    private int targetX, targetZ;
    private int previousX, previousZ;
    private GridManager gridManager;

    // Animation settings
    private float moveDuration = 0.5f;
    private Ease moveEase = Ease.OutQuad;

    public MoveCommand(Character character, int targetX, int targetZ, GridManager gridManager)
    {
        Character = character;
        this.targetX = targetX;
        this.targetZ = targetZ;
        this.gridManager = gridManager;

        previousX = character.GridX;
        previousZ = character.GridZ;
    }

    public bool Execute()
    {
        GridTile targetTile = gridManager.GetTile(targetX, targetZ);
        GridTile currentTile = gridManager.GetTile(Character.GridX, Character.GridZ);

        if (targetTile == null || targetTile.IsOccupied())
            return false;

        if (currentTile != null)
            currentTile.OccupyingCharacter = null;

        targetTile.OccupyingCharacter = Character;

        Vector3 targetPos = new Vector3(targetX, Character.transform.position.y, targetZ);

        Character.transform.DOMove(targetPos, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                Character.MoveTo(targetX, targetZ);
            });

        return true;
    }

    public void Undo()
    {
        GridTile targetTile = gridManager.GetTile(targetX, targetZ);
        GridTile previousTile = gridManager.GetTile(previousX, previousZ);

        if (targetTile != null)
            targetTile.OccupyingCharacter = null;

        if (previousTile != null)
            previousTile.OccupyingCharacter = Character;

        Vector3 previousPos = new Vector3(previousX, Character.transform.position.y, previousZ);

        Character.transform.DOMove(previousPos, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                Character.MoveTo(previousX, previousZ);
            });
    }

    public string GetCommandName()
    {
        return "Move";
    }
}
