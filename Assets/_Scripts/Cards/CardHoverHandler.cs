using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float hoverScale;
    private float duration;
    private Vector3 originalScale;

    public void Init(CardHandManager manager, float hoverScale, float duration)
    {
        this.hoverScale = hoverScale;
        this.duration = duration;

        // Original scale is Vector3.one (normal scale after animation)
        originalScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(originalScale * hoverScale, duration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }
}
