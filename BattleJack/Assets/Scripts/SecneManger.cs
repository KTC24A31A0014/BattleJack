using System.Collections.Generic;
using UnityEngine;

public class SecneManger : MonoBehaviour
{
    [Min(100)]
    public int ShuffleCount = 100;

    List<Card.Data> cards;

    private void Awake()
    {
        InitCards();    // 確認用のコード
    }

    void InitCards()
    {
        cards = new List<Card.Data>(13 * 4);
        var marks = new List<Card.Mark>()
        {
            Card.Mark.Heart,
            Card.Mark.Diamond,
            Card.Mark.Spade,
            Card.Mark.Club,
        };

        foreach (var mark in marks)
        {
            for (var num = 1; num <= 13; ++num)
            {
                var card = new Card.Data()
                {
                    Mark = mark,
                    Number = num,
                };
                cards.Add(card);
            }
        }

        ShuffleCards();
    }

    void ShuffleCards()
    {
        // シャッフルする
        var random = new System.Random();
        for (var i = 0; i < ShuffleCount; ++i)
        {
            var index = random.Next(cards.Count);
            var index2 = random.Next(cards.Count);

            // カードの位置を入れ替える
            var tmp = cards[index];
            cards[index] = cards[index2];
            cards[index] = tmp;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
