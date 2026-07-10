using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    private List<Card> _cards = new List<Card>();

    // 手札にカードを追加
    public void AddCard(Card card)
    {
        _cards.Add(card);
    }

    // ラウンド終了時ー手札をすべて捨てる
    public void ClearHand()
    {
        foreach (Card card in _cards)
        {
            Destroy(card.gameObject);
        }
        _cards.Clear();
    }

    // 手札の合計点を返す (Ace = 11)
    public int GetTotalValue()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in _cards)
        {
            int value = card.CardData.GetBlackJackValue();
            total += value;
            if (card.CardData.Number == 1) aceCount++;
        }

        // Ace を11 として扱えるなら1枚だけ11 にする
        while (aceCount > 0 && total + 10 <= 21)
        {
            total += 10;
            aceCount--;
        }

        return total;
    }

    // バースト判定
    public bool IsBust() => GetTotalValue() > 21;

    // BJ判定　(最初の2枚
    public bool IsBJ() => _cards.Count == 2 && GetTotalValue() == 21;

    // スプリットの可否
    public bool CanSplit() => _cards.Count == 2 && _cards[0].CardData.IsSameValueAs(_cards[1].CardData);

    // 手札の枚数
    public int CardCount => _cards.Count;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
