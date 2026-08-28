using UnityEngine;
using UnityEngine.UI;

public class BJManager : MonoBehaviour
{
    [SerializeField] private Deck deck;
    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand dealerHand;
    [SerializeField] private Transform playerHandTransform;
    [SerializeField] private Transform dealerHandTransform;

    // UIボタン
    [SerializeField] private Button hit;
    [SerializeField] private Button stand;
    [SerializeField] private Button doubleDown;
    [SerializeField] private Button split;

    private Card _dealerHoleCard;   // ディーラーの伏せ札

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deck.SetUpDeck();
        StartRound();
    }

    // ------------------------------------------

    private void StartRound()
    {
        // ラウンド開始：カードを2枚ずつ配る
        playerHand.ClearHand();
        dealerHand.ClearHand();

        // 2枚ずつ配る
        AddCardToPL();
        AddCardToDL(isReverse: false); // ディーラーの1枚目：表
        AddCardToPL();
        _dealerHoleCard = AddCardToDL(isReverse: true); // ディーラーの2枚目：裏

        // BJ判定
        if (playerHand.IsBJ())
        {
            EndRound();
            return;
        }

        SetBtnActive(true);
        UpdateSplitBtn();
    }

    // Player's ACT

    public void OnHit()
    {
        AddCardToPL();

        if (playerHand.IsBust())
        {
            EndRound();
            return;
        }

        // hit後はスプリット・ダブルダウン不可
        split.interactable = false;
        doubleDown.interactable = false;
    }

    public void OnStand()
    {
        SetBtnActive(false);
        DealerTurn();
    }

    public void OnDoubleDown()
    {
        // カードを1枚だけ追加してそのままスタンド
        AddCardToPL();
        SetBtnActive(false);

        if (playerHand.IsBust())
        {
            EndRound();
            return;
        }

        DealerTurn();
    }

    public void OnSplit()
    {
        // ToDo: Split実装
        Debug.Log("Split: 未実装");
    }

    // ディーラーのターン

    private void DealerTurn()
    {
        // 伏せカードを公開
        _dealerHoleCard.Flip(isReverse: false);

        // 17以上になるまで引く
        while (dealerHand.GetTotalValue() < 17)
        {
            AddCardToDL(isReverse: false);
        }

        EndRound();
    }

    // 勝敗判定
    private void EndRound()
    {
        SetBtnActive(false);

        // 伏せカードが残っていたらオープン
        if (_dealerHoleCard != null && _dealerHoleCard.IsReverse)
        {
            _dealerHoleCard.Flip(isReverse: false);
        }

        int playerTotal = playerHand.GetTotalValue();
        int dealerTotal = dealerHand.GetTotalValue();

        RoundResult result = JudgeResult(playerTotal, dealerTotal);
        Debug.Log($"player: {playerTotal} / Dealer: {dealerTotal} -> {result}");

        // ToDo: resultに応じてHPの増減処理を呼ぶ
    }

    private enum RoundResult { PlayerBJ, PlayerWin, Lose, Draw }

    private RoundResult JudgeResult(int playerTotal, int dealerTotal)
    {
        if (playerHand.IsBJ())          return RoundResult.PlayerBJ;
        if (playerHand.IsBust())        return RoundResult.Lose;
        if (dealerHand.IsBust())        return RoundResult.PlayerWin;
        if (playerTotal > dealerTotal)  return RoundResult.PlayerWin;
        if (playerTotal < dealerTotal)  return RoundResult.Lose;

        return RoundResult.Draw;
    }

    private Card AddCardToPL()
    {
        Card card = deck.DrawCard(playerHandTransform);
        playerHand.AddCard(card);
        return card;
    }

    private Card AddCardToDL(bool isReverse)
    {
        Card card = deck.DrawCard(dealerHandTransform);
        dealerHand.AddCard(card);
        return card;
    }

    private void SetBtnActive(bool active)
    {
        hit.interactable = active;
        stand.interactable = active;
        doubleDown.interactable = active;
        split.interactable = active;
    }

    private void UpdateSplitBtn()
    {
        split.interactable = playerHand.CanSplit();
    }
}
