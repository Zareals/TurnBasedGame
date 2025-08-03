using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int gridSize = 5;
    [SerializeField] private float cubeSpacing = 1f;
    
    [Header("Grid Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color validMoveColor = Color.green;
    [SerializeField] private Color invalidMoveColor = Color.red;
    [SerializeField] private Color selectedColor = Color.yellow;
    
    private GridTile[,] grid;
    
    void Start()
    {
        CreateGrid();
    }
    
    void CreateGrid()
    {
        grid = new GridTile[gridSize, gridSize];
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 position = new Vector3(x * cubeSpacing, 0, z * cubeSpacing);
                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                
                GridTile tile = cube.GetComponent<GridTile>();
                if (tile == null)
                    tile = cube.AddComponent<GridTile>();
                
                tile.Initialize(x, z, defaultColor);
                grid[x, z] = tile;
            }
        }
    }
    
    public GridTile GetTile(int x, int z)
    {
        if (x >= 0 && x < gridSize && z >= 0 && z < gridSize)
            return grid[x, z];
        return null;
    }
    
    public bool IsValidPosition(int x, int z)
    {
        return x >= 0 && x < gridSize && z >= 0 && z < gridSize;
    }
    
    public void HighlightValidMoves(Character character)
    {
        ResetAllTileColors();
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                GridTile tile = grid[x, z];
                if (!tile.IsOccupied())
                {
                    tile.Highlight(validMoveColor);
                }
                else if (tile.OccupyingCharacter != character && !tile.OccupyingCharacter.isPlayerControlled)
                {
                    // Enemy tile - can attack
                    tile.Highlight(invalidMoveColor);
                }
            }
        }
    }
    
    public void ResetAllTileColors()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                grid[x, z].ResetHighlight();
            }
        }
    }
    
    public Vector2Int GetRandomPositionOnSide(bool leftSide)
    {
        int x = leftSide ? Random.Range(0, 2) : Random.Range(3, 5); // Left side: 0-1, Right side: 3-4
        int z = Random.Range(0, gridSize);
        
        // Make sure position is not occupied
        int attempts = 0;
        while (grid[x, z].IsOccupied() && attempts < 10)
        {
            z = Random.Range(0, gridSize);
            attempts++;
        }
        
        return new Vector2Int(x, z);
    }
    
    public int GridSize => gridSize;
}