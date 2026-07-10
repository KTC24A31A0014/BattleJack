using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefabs;

    private List<Card.Data> _deck = new List<Card.Data>();

    // 52枚の山札を生成 -> シャッフル
    public void SetUpDeck()
    {
        _deck.Clear();

        foreach (Card.Mark mark in System.Enum.GetValues(typeof(Card.Mark)))
        {
            for (int number = 1; number <= 13; number++)
            {
                _deck.Add(new Card.Data(number, mark));
            }
        }

        Shuffle();
    }

    // シャッフル
    private void Shuffle()
    {
        for (int i = _deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
        }
    }

    // 山札から1枚ドロー
    public Card DrawCard(Transform parent, bool isReverse = false)

    {
        if (_deck.Count == 0)
        {
            Debug.LogWarning("山札が空です。");
            return null;
        }

        Card.Data data = _deck[0];
        _deck.RemoveAt(0);

        GameObject obj = Instantiate(cardPrefabs, parent);
        Card card = obj.GetComponent<Card>();
        card.SetCard(data, isReverse);

        return card;
    }

    // 残り枚数の確認
    public int RemainingCount => _deck.Count;
}
