using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

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

    private List<RectTransform> cards = new List<RectTransform>();

    [ContextMenu("Draw Card")]
    public void DrawCard()
{
    GameObject newCard = Instantiate(cardPrefab, handArea);
    RectTransform cardRect = newCard.GetComponent<RectTransform>();

    cardRect.position = deckPosition.position;
    cardRect.localScale = Vector3.zero;

    cards.Add(cardRect);

    cardRect.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
    {
        CardHoverHandler hoverHandler = newCard.AddComponent<CardHoverHandler>();
        hoverHandler.Init(this, hoverScale, animationDuration);
    });

    ArrangeCards();
}


    private void ArrangeCards()
    {
        int count = cards.Count;
        float midIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform card = cards[i];

            float xPos = (i - midIndex) * spacing;

            float yPos = -Mathf.Pow(i - midIndex, 2) * curveStrength;

            card.DOLocalMove(new Vector3(xPos, yPos, 0f), animationDuration).SetEase(Ease.OutQuad);

            float angle = (i - midIndex) * -5f;
            card.DORotate(new Vector3(0f, 0f, angle), animationDuration).SetEase(Ease.OutQuad);
        }
    }
}
