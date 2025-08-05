using UnityEngine;

public enum CardType { Attack, Heal, Buff, Move }

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public CardType type;
    public Sprite cardSprite;
    public int actionPointCost = 1;
    public int value = 10;
    public GameObject effectPrefab;
}
