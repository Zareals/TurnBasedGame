using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class CardHandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    public float curveStrength = 50f;
    public float spacing = 100f;
    public float hoverScale = 1.2f;
    public float animationDuration = 0.3f;

    [Header("References")]
    public Transform deckPosition;
    public RectTransform handArea;
    public GameObject cardPrefab;
    public TurnManager turnManager;

    [Header("Card Library")]
    [SerializeField] private List<CardData> availableCards;

    private List<RectTransform> handCards = new List<RectTransform>();
    private List<RectTransform> queuedCards = new List<RectTransform>();
    private List<ICommand> queuedCommands = new List<ICommand>();

    private Character selectedCharacter;

    /// <summary>
    /// Assign the selected character to this hand.
    /// </summary>
    public void SetSelectedCharacter(Character character)
    {
        selectedCharacter = character;
    }

    /// <summary>
    /// Draw multiple cards (default: 3).
    /// </summary>
    [ContextMenu("Draw 3 Cards")]
    public void DrawThreeCards()
    {
        for (int i = 0; i < 3; i++)
            DrawCard();
    }

    /// <summary>
    /// Draw a single card from available cards.
    /// </summary>
    public void DrawCard()
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("No selected character assigned!");
            return;
        }

        // Pick a random card (or weighted logic later)
        CardData cardData = availableCards[Random.Range(0, availableCards.Count)];

        // Create UI card
        GameObject newCard = Instantiate(cardPrefab, handArea);
        RectTransform cardRect = newCard.GetComponent<RectTransform>();

        // Set card visuals
        CardUI cardUI = newCard.GetComponent<CardUI>();
        cardUI.Setup(cardData);

        // Animate card from deck
        cardRect.position = deckPosition.position;
        cardRect.localScale = Vector3.zero;

        handCards.Add(cardRect);

        cardRect.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            CardHoverHandler hoverHandler = newCard.AddComponent<CardHoverHandler>();
            hoverHandler.Init(this, hoverScale, animationDuration);

            Button btn = newCard.GetComponent<Button>();
            if (btn == null) btn = newCard.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCardClicked(cardRect, cardData));
        });

        ArrangeCards();
    }

    /// <summary>
    /// When card is clicked, queue corresponding command.
    /// </summary>
    private void OnCardClicked(RectTransform card, CardData cardData)
{
    if (selectedCharacter == null)
    {
        Debug.LogWarning("No selected character assigned!");
        return;
    }

    if (selectedCharacter.GetRemainingActionPoints() < cardData.actionPointCost)
    {
        Debug.Log("Not enough action points to queue this card.");
        return;
    }

    handCards.Remove(card);

    Destroy(card.gameObject);

    ArrangeCards();

    ICommand command = null;
    switch (cardData.type)
    {
        case CardType.Attack:
            Character enemy = FindNearestEnemy();
            if (enemy != null)
                command = new AttackCommand(selectedCharacter, enemy, cardData.value, cardData.cardSprite, cardData.effectPrefab);
            break;

        case CardType.Heal:
            command = new HealCommand(selectedCharacter, cardData.value, cardData.effectPrefab);
            break;

        case CardType.Move:
            Vector2Int pos = new Vector2Int(selectedCharacter.GridX + 1, selectedCharacter.GridZ);
            command = new MoveCommand(selectedCharacter, pos.x, pos.y, FindObjectOfType<GridManager>());
            break;
    }

    // Queue the command
    if (command != null)
    {
        selectedCharacter.AddCommand(command);
        queuedCommands.Add(command);
        Debug.Log($"Queued {cardData.type} card: {cardData.cardName}");
    }
}


    /// <summary>
    /// Execute queued commands sequentially.
    /// </summary>
    public IEnumerator ExecuteQueuedCommands()
    {
        foreach (var command in queuedCommands)
        {
            yield return selectedCharacter.StartCoroutine(selectedCharacter.ExecuteCommandCoroutine(command));

            // If command has visual effect, spawn here
            if (command is AttackCommand attackCmd)
            {
                if (attackCmd.Character != null)
                {
                    var data = GetCardData(CardType.Attack);
                    if (data != null && data.effectPrefab != null)
                    {
                        Instantiate(data.effectPrefab,
                                    attackCmd.Character.transform.position + Vector3.up * 1f,
                                    Quaternion.identity);
                    }
                }
            }
            else if (command is HealCommand healCmd)
            {
                var data = GetCardData(CardType.Heal);
                if (data != null && data.effectPrefab != null)
                {
                    Instantiate(data.effectPrefab,
                                healCmd.Character.transform.position + Vector3.up * 1f,
                                Quaternion.identity);
                }
            }
        }

        queuedCommands.Clear();

        foreach (var card in queuedCards)
            Destroy(card.gameObject);
        queuedCards.Clear();
    }

    /// <summary>
    /// Find nearest enemy for targeting.
    /// </summary>
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

        return nearest;
    }

    /// <summary>
    /// Helper: Get card data by type.
    /// </summary>
    private CardData GetCardData(CardType type)
    {
        return availableCards.Find(c => c.type == type);
    }

    /// <summary>
    /// Arrange cards visually in hand.
    /// </summary>
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

    /// <summary>
    /// Arrange queued cards visually in queue.
    /// </summary>
    private void ArrangeQueuedCards()
    {
        int count = queuedCards.Count;
        float midIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = queuedCards[i];
            float xPos = (i - midIndex) * spacing * 0.6f;
            float yPos = 0;

            card.DOLocalMove(new Vector3(xPos, yPos, 0f), animationDuration).SetEase(Ease.OutQuad);
            card.DORotate(Vector3.zero, animationDuration).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// Clear all cards (hand + queue).
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
