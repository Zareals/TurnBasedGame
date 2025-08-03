using UnityEngine;

public class MoveCommand : ICommand
{
    public int ActionPointCost => 1;
    public Character Character { get; private set; }
    
    private int targetX, targetZ;
    private int previousX, previousZ;
    private GridManager gridManager;
    
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
        Character.MoveTo(targetX, targetZ);
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
        
        Character.MoveTo(previousX, previousZ);
    }
    
    public string GetCommandName()
    {
        return "Move";
    }
}