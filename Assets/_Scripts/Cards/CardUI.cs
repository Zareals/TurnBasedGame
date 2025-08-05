using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [SerializeField] private Image outlineImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text valueText;

    public void Setup(CardData data)
    {
        if (iconImage) iconImage.sprite = data.cardSprite;
        if (nameText) nameText.text = data.cardName;
        if (costText) costText.text = $"AP: {data.actionPointCost}";
        if (valueText) valueText.text = data.type == CardType.Attack || data.type == CardType.Heal
            ? data.value.ToString()
            : string.Empty;

        switch (data.type)
        {
            case CardType.Attack:
                outlineImage.color = Color.red;
                break;
            case CardType.Heal:
                outlineImage.color = Color.green;
                break;
            case CardType.Buff:
                outlineImage.color = Color.yellow;
                break;
            case CardType.Move:
                outlineImage.color = Color.blue;
                break;
        }
    }

    public Sprite SetQueueSprite()
    {
        return iconImage.sprite;
    }
}
