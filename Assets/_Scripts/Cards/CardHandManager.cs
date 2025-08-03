using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardHandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    public float curveStrength = 50f;
    public float spacing = 100f;
    public float hoverScale = 1.2f;
    public float animationDuration = 0.3f;

    [Header("References")]
    public Transform deckPosition;           // Position from which cards appear
    public RectTransform handArea;           // UI container for hand cards
    public RectTransform commandQueueArea;   // UI container for queued cards
    public GameObject cardPrefab;             // Card prefab with Image/Button
    public TurnManager turnManager;           // Reference to TurnManager

    private List<RectTransform> handCards = new List<RectTransform>();
    private List<RectTransform> queuedCards = new List<RectTransform>();
    private List<AttackCommand> queuedCommands = new List<AttackCommand>();
    [SerializeField] private GameObject hitEffectPrefab;

    private Character selectedCharacter;

    /// <summary>
    /// Assigns the currently selected character who will queue commands.
    /// </summary>
    public void SetSelectedCharacter(Character character)
    {
        selectedCharacter = character;
    }

    /// <summary>
    /// Draws 3 cards at once.
    /// </summary>
    [ContextMenu("Draw 3 Cards")]
    public void DrawThreeCards()
    {
        for (int i = 0; i < 3; i++)
            DrawCard();
    }

    /// <summary>
    /// Draws a single card into the hand.
    /// </summary>
    public void DrawCard()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("No selected character assigned!");
            return;
        }

        GameObject newCard = Instantiate(cardPrefab, handArea);
        RectTransform cardRect = newCard.GetComponent<RectTransform>();

        cardRect.position = deckPosition.position;
        cardRect.localScale = Vector3.zero;

        handCards.Add(cardRect);

        cardRect.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            CardHoverHandler hoverHandler = newCard.AddComponent<CardHoverHandler>();
            hoverHandler.Init(this, hoverScale, animationDuration);

            Button btn = newCard.GetComponent<Button>();
            if (btn == null) btn = newCard.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCardClicked(cardRect));
        });

        ArrangeCards();
    }

    private void ArrangeCards()
    {
        int count = handCards.Count;
        float midIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = handCards[i];
            float xPos = (i - midIndex) * spacing;
            float yPos = -Mathf.Pow(i - midIndex, 2) * curveStrength;

            card.DOLocalMove(new Vector3(xPos, yPos, 0f), animationDuration).SetEase(Ease.OutQuad);
            float angle = (i - midIndex) * -5f;
            card.DORotate(new Vector3(0f, 0f, angle), animationDuration).SetEase(Ease.OutQuad);
        }
    }

    private void ArrangeQueuedCards()
    {
        int count = queuedCards.Count;
        float midIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = queuedCards[i];
            float xPos = (i - midIndex) * spacing * 0.6f; // tighter spacing for queue
            float yPos = 0;

            card.DOLocalMove(new Vector3(xPos, yPos, 0f), animationDuration).SetEase(Ease.OutQuad);
            card.DORotate(Vector3.zero, animationDuration).SetEase(Ease.OutQuad);
        }
    }

    private void OnCardClicked(RectTransform card)
    {
        int cardCost = 2; // Match with AttackCommand cost

        if (selectedCharacter == null)
        {
            Debug.LogWarning("No selected character assigned!");
            return;
        }

        if (selectedCharacter.GetRemainingActionPoints() < cardCost)
        {
            Debug.Log("Not enough action points to queue this attack.");
            return;
        }

        // Move card UI to queue area
        handCards.Remove(card);
        queuedCards.Add(card);

        card.SetParent(commandQueueArea);
        card.localScale = Vector3.one;

        ArrangeCards();
        ArrangeQueuedCards();

        // Create and add AttackCommand to the character’s queue
        Character nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            Debug.LogWarning("No enemy found to target!");
            return;
        }

        AttackCommand command = new AttackCommand(selectedCharacter, nearestEnemy, 10);

        // Add command to character queue
        selectedCharacter.AddCommand(command);
        Debug.Log($"Queued AttackCommand for {selectedCharacter.name} targeting {nearestEnemy.name}");
    }


    private Character FindNearestEnemy()
    {
        List<Character> enemies = turnManager.GetEnemyTeam();
        Character nearest = null;
        float closestDist = float.MaxValue;
        Vector2Int playerPos = new Vector2Int(selectedCharacter.GridX, selectedCharacter.GridZ);

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead()) continue;
            Vector2Int enemyPos = new Vector2Int(enemy.GridX, enemy.GridZ);
            float dist = Vector2Int.Distance(playerPos, enemyPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                nearest = enemy;
            }
        }
        Debug.Log("Nearest enemy: " + nearest.name);
        return nearest;
    }

    /// <summary>
    /// Execute all queued attack commands sequentially.
    /// </summary>
    public IEnumerator ExecuteQueuedCommands()
    {
        foreach (var command in queuedCommands)
        {
            yield return selectedCharacter.StartCoroutine(selectedCharacter.ExecuteCommandCoroutine(command));
        }
        queuedCommands.Clear();

        // Clean up queued cards UI
        foreach (var card in queuedCards)
            Destroy(card.gameObject);
        queuedCards.Clear();
    }

    /// <summary>
    /// Clear all cards from hand and queue.
    /// </summary>
    public void ClearAllCards()
    {
        foreach (var card in handCards)
            Destroy(card.gameObject);
        handCards.Clear();

        foreach (var card in queuedCards)
            Destroy(card.gameObject);
        queuedCards.Clear();

        queuedCommands.Clear();
    }
}
