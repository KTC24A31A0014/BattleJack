using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    private List<Card> _cards = new List<Card>();

    [SerializeField] private float cardSpacing = 240f;

    // 手札にカードを追加
    public void AddCard(Card card)
    {
        if (card == null)
        {
            Debug.LogError("nullのカードが渡された。");
            return;
        }

        _cards.Add(card);
        ArrangeCards();
    }

    // カードを横に並べる
    private void ArrangeCards()
    {
        int count = _cards.Count;
        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform rt = _cards[i].GetComponent<RectTransform>();
            // 追加
            if (rt == null)
            {
                Debug.LogError($"RectTransformがありません: {_cards[i].gameObject.name}");
                continue;
            }
            rt.anchoredPosition = new Vector2(startX + i * cardSpacing, 0);
        }
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

    // ディーラーの表向きのカードのみ合計を返す
    public int GetVisibleValue()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in _cards)
        {
            if (card.IsReverse) continue; // 伏せカードはSkip

            int value = card.CardData.GetBlackJackValue();
            total += value;
            if (card.CardData.Number == 1) aceCount++;
        }

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
}
