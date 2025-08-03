using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI actionPointsText;
    [SerializeField] private TMPro.TextMeshProUGUI turnText;
    [SerializeField] private UnityEngine.UI.Button attackButton;
    [SerializeField] private UnityEngine.UI.Button endTurnButton;
    [SerializeField] private Transform commandQueueParent;
    [SerializeField] private CardHandManager cardHandManager;
    
    [Header("Command Prefabs")]
    [SerializeField] private GameObject moveCommandPrefab;
    [SerializeField] private GameObject attackCommandPrefab;
    
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    
    private Character selectedCharacter;
    private List<GameObject> commandUIElements = new List<GameObject>();
    
    void Start()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged += UpdateTurnDisplay;
        }
        
        if (attackButton != null)
            attackButton.onClick.AddListener(ToggleAttackMode);
        
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(() => turnManager.EndTurn());
        
        UpdateTurnDisplay(true);
    }
    
    public void SetSelectedCharacter(Character character)
    {
        if (selectedCharacter != null)
        {
            selectedCharacter.OnCommandAdded -= UpdateCommandQueue;
            selectedCharacter.OnCommandRemoved -= UpdateCommandQueue;
        }
        
        selectedCharacter = character;
        cardHandManager.SetSelectedCharacter(character);
        
        if (selectedCharacter != null)
        {
            selectedCharacter.OnCommandAdded += UpdateCommandQueue;
            selectedCharacter.OnCommandRemoved += UpdateCommandQueue;
        }
        
        UpdateActionPointsDisplay();
        UpdateCommandQueue(selectedCharacter);
    }
    
    private void UpdateTurnDisplay(bool isPlayerTurn)
    {
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "Player Turn" : "Enemy Turn";
            turnText.color = isPlayerTurn ? Color.blue : Color.red;
        }
        
        bool showUI = isPlayerTurn && !turnManager.IsExecutingTurn();
        
        if (attackButton != null)
            attackButton.gameObject.SetActive(showUI);
        if (endTurnButton != null)
            endTurnButton.gameObject.SetActive(showUI);
    }
    
    private void UpdateActionPointsDisplay()
    {
        if (actionPointsText != null && selectedCharacter != null)
        {
            actionPointsText.text = $"AP: {selectedCharacter.GetRemainingActionPoints()}/{selectedCharacter.GetMaxActionPoints()}";
        }
        else if (actionPointsText != null)
        {
            actionPointsText.text = "No Character Selected";
        }
    }
    
    private void UpdateCommandQueue(Character character)
    {
        // Clear existing UI elements
        foreach (var element in commandUIElements)
        {
            if (element != null)
                Destroy(element);
        }
        commandUIElements.Clear();
        
        if (character == null || commandQueueParent == null) return;
        
        var commands = character.GetCommands();
        for (int i = 0; i < commands.Count; i++)
        {
            var command = commands[i];
            GameObject prefab = command.GetCommandName() == "Move" ? moveCommandPrefab : attackCommandPrefab;
            
            if (prefab != null)
            {
                GameObject commandUI = Instantiate(prefab, commandQueueParent);
                commandUIElements.Add(commandUI);
                
                // Set up click to remove command
                int commandIndex = i;
                var button = commandUI.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                    button.onClick.AddListener(() => RemoveCommand(commandIndex));
            }
        }
        
        UpdateActionPointsDisplay();
    }
    
    private void RemoveCommand(int index)
    {
        if (selectedCharacter != null)
        {
            selectedCharacter.RemoveCommand(index);
        }
    }
    
    private bool attackMode = false;
    
    private void ToggleAttackMode()
    {
        attackMode = !attackMode;
        if (attackButton != null)
        {
            var buttonText = attackButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            cardHandManager.DrawThreeCards();
            if (buttonText != null)
            {
                buttonText.text = attackMode ? "Cancel Attack" : "Attack Mode";
                buttonText.color = attackMode ? Color.red : Color.white;
            }
        }
    }
    
    public bool IsAttackMode() => attackMode;
    
    public void SetAttackMode(bool mode)
    {
        attackMode = mode;
        if (attackButton != null)
        {
            var buttonText = attackButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = attackMode ? "Cancel Attack" : "Attack Mode";
                buttonText.color = attackMode ? Color.red : Color.white;
            }
        }
    }
}
