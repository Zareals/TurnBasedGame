using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EnhancedTestUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UIManager uiManager;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private InputActionReference pointerPositionAction;
    [SerializeField] private InputActionReference cancelAction;
    
    private Character selectedCharacter;
    private Camera mainCamera;
    private bool isShowingMoveOptions = false;
    
    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
    }
    
    void Start()
    {
        // Enable input actions
        if (clickAction != null)
        {
            clickAction.action.Enable();
            clickAction.action.performed += OnClick;
        }
        
        if (pointerPositionAction != null)
            pointerPositionAction.action.Enable();
        
        if (cancelAction != null)
        {
            cancelAction.action.Enable();
            cancelAction.action.performed += OnCancel;
        }
    }
    
    void OnDestroy()
    {
        // Disable input actions and unsubscribe from events
        if (clickAction != null)
        {
            clickAction.action.performed -= OnClick;
            clickAction.action.Disable();
        }
        
        if (pointerPositionAction != null)
            pointerPositionAction.action.Disable();
        
        if (cancelAction != null)
        {
            cancelAction.action.performed -= OnCancel;
            cancelAction.action.Disable();
        }
    }
    
    private void OnClick(InputAction.CallbackContext context)
    {
        if (!turnManager.IsPlayerTurn()) return;
        
        Vector2 pointerPosition = pointerPositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if we clicked on a character first
            Character clickedCharacter = hit.collider.GetComponent<Character>();
            if (clickedCharacter != null && clickedCharacter.isPlayerControlled)
            {
                SelectCharacter(clickedCharacter);
                return;
            }
            
            // Check if we clicked on a grid tile
            GridTile tile = hit.collider.GetComponent<GridTile>();
            if (tile != null && selectedCharacter != null)
            {
                HandleTileClick(tile);
            }
        }
    }
    
    private void OnCancel(InputAction.CallbackContext context)
    {
        if (selectedCharacter != null)
        {
            selectedCharacter.ClearCommands();
            Debug.Log($"Cleared all commands for {selectedCharacter.name}");
        }
        
        // Reset UI state
        if (isShowingMoveOptions)
        {
            gridManager.ResetAllTileColors();
            isShowingMoveOptions = false;
        }
        
        if (uiManager != null)
            uiManager.SetAttackMode(false);
    }
    
    private void SelectCharacter(Character character)
    {
        // Deselect previous character
        if (selectedCharacter != null)
        {
            ResetCharacterHighlight(selectedCharacter);
        }
        
        selectedCharacter = character;
        
        // Update UI Manager
        if (uiManager != null)
            uiManager.SetSelectedCharacter(selectedCharacter);
        
        // Highlight selected character
        HighlightCharacter(character);
        
        // Show movement options
        ShowMovementOptions();
        
        Debug.Log($"Selected character: {character.name} | Remaining AP: {character.GetRemainingActionPoints()}");
    }
    
    private void ShowMovementOptions()
    {
        if (selectedCharacter != null && gridManager != null)
        {
            gridManager.HighlightValidMoves(selectedCharacter);
            isShowingMoveOptions = true;
        }
    }
    
    private void HandleTileClick(GridTile tile)
    {
        if (selectedCharacter == null) return;
        
        bool isAttackMode = uiManager != null && uiManager.IsAttackMode();
        
        if (isAttackMode)
        {
            HandleAttackCommand(tile);
        }
        else
        {
            HandleMoveOrAttackCommand(tile);
        }
    }
    
    private void HandleMoveOrAttackCommand(GridTile tile)
    {
        // Check if there's an enemy on the tile for attack
        if (tile.IsOccupied() && tile.OccupyingCharacter != selectedCharacter)
        {
            Character target = tile.OccupyingCharacter;
            if (!target.isPlayerControlled) // It's an enemy
            {
                var attackCmd = new AttackCommand(selectedCharacter, target);
                if (selectedCharacter.CanAddCommand(attackCmd))
                {
                    selectedCharacter.AddCommand(attackCmd);
                    Debug.Log($"Added attack command against {target.name} | Remaining AP: {selectedCharacter.GetRemainingActionPoints()}");
                }
                else
                {
                    Debug.Log("Not enough action points for attack!");
                }
            }
            else
            {
                Debug.Log("Cannot attack friendly units!");
            }
        }
        else if (!tile.IsOccupied()) // Empty tile - move command
        {
            var moveCmd = new MoveCommand(selectedCharacter, tile.X, tile.Z, gridManager);
            if (selectedCharacter.CanAddCommand(moveCmd))
            {
                selectedCharacter.AddCommand(moveCmd);
                Debug.Log($"Added move command to ({tile.X}, {tile.Z}) | Remaining AP: {selectedCharacter.GetRemainingActionPoints()}");
            }
            else
            {
                Debug.Log("Not enough action points for move!");
            }
        }
        else
        {
            Debug.Log("Cannot move to occupied tile!");
        }
    }
    
    private void HandleAttackCommand(GridTile tile)
    {
        if (tile.IsOccupied() && tile.OccupyingCharacter != selectedCharacter)
        {
            Character target = tile.OccupyingCharacter;
            if (!target.isPlayerControlled) // It's an enemy
            {
                // Check if target is in range (adjacent for now)
                int distance = Mathf.Abs(selectedCharacter.GridX - target.GridX) + Mathf.Abs(selectedCharacter.GridZ - target.GridZ);
                if (distance <= 1)
                {
                    var attackCmd = new AttackCommand(selectedCharacter, target);
                    if (selectedCharacter.CanAddCommand(attackCmd))
                    {
                        selectedCharacter.AddCommand(attackCmd);
                        Debug.Log($"Added attack command against {target.name} | Remaining AP: {selectedCharacter.GetRemainingActionPoints()}");
                        
                        // Exit attack mode after selecting target
                        if (uiManager != null)
                            uiManager.SetAttackMode(false);
                    }
                    else
                    {
                        Debug.Log("Not enough action points for attack!");
                    }
                }
                else
                {
                    Debug.Log("Target is too far away! Move closer first.");
                }
            }
            else
            {
                Debug.Log("Cannot attack friendly units!");
            }
        }
        else
        {
            Debug.Log("No valid target at this location!");
        }
    }
    
    private void HighlightCharacter(Character character)
    {
        var renderer = character.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }
    
    private void ResetCharacterHighlight(Character character)
    {
        var renderer = character.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = character.isPlayerControlled ? Color.blue : Color.red;
        }
    }
    
    void Update()
    {
        // Handle UI updates if needed
        if (selectedCharacter != null && !turnManager.IsPlayerTurn())
        {
            // Deselect character when it's not player turn
            ResetCharacterHighlight(selectedCharacter);
            selectedCharacter = null;
            
            if (uiManager != null)
                uiManager.SetSelectedCharacter(null);
            
            if (isShowingMoveOptions)
            {
                gridManager.ResetAllTileColors();
                isShowingMoveOptions = false;
            }
        }
    }
}