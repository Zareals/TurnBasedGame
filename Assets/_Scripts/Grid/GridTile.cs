using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int X { get; private set; }
    public int Z { get; private set; }
    public Character OccupyingCharacter { get; set; }
    
    private Renderer tileRenderer;
    private Color originalColor;
    
    void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
    }
    
    public void Initialize(int x, int z, Color defaultColor)
    {
        X = x;
        Z = z;
        gameObject.name = $"Tile_{x}_{z}";
        originalColor = defaultColor;
        tileRenderer.material.color = originalColor;
    }
    
    public void Highlight(Color color)
    {
        tileRenderer.material.color = color;
    }
    
    public void ResetHighlight()
    {
        tileRenderer.material.color = originalColor;
    }
    
    public bool IsOccupied()
    {
        return OccupyingCharacter != null;
    }
}