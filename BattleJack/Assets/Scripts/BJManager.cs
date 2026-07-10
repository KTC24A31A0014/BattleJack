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
        AddCardToPlayer();
        AddCardToDealer(isReverse: false); // ディーラーの1枚目：表
        AddCardToPlayer();
        _dealerHoleCard = AddCardToDealer(isReverse: true); // ディーラーの2枚目：裏

        // BJ判定
        if (playerHand.IsBJ())
        {
            EndRound();
            return;
        }

        SetButtonsActive(true);
        UpdateSplitButton();
    }

    // Player's ACT

    public void OnHit()
    {
        AddCardToPlayer();

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
        SetButtonsActive(false);
        DealerTurn();
    }

    public void OnDoubleDown()
    {
        // カードを1枚だけ追加してそのままスタンド
        AddCardToPlayer();
        SetButtonsActive(false);

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
            AddCardToDealer(isReverse: false);
        }

        EndRound();
    }

    // 勝敗判定
    private void EndRound()
    {
        SetButtonActive(false);

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
        if (playerHand.IsBJ()) return RoundResult.PlayerBJ;
    }
}
