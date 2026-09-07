using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BJManager : MonoBehaviour
{
    // 手札関連
    [SerializeField] private Deck deck;
    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand dealerHand;
    [SerializeField] private Transform playerHandTransform;
    [SerializeField] private Transform dealerHandTransform;

    // Status
    [SerializeField] private CharacterStatus playerStatus;
    [SerializeField] private CharacterStatus dealerStatus;
    [SerializeField] private GameUI gameUI;

    // UIボタン
    [SerializeField] private Button hit;
    [SerializeField] private Button stand;
    [SerializeField] private Button doubleDown;
    [SerializeField] private Button split;


    // 賭けることができるHP量
    private int betAmount = 1;

    // ラウンドカウント
    private int _roundCount = 0;

    private Card _dealerHoleCard;   // ディーラーの伏せ札

    /// <summary>
    /// Split用
    /// 
    /// private bool _IsSplitting = false;
    ///private Hand _splitHand1;
    ///private Hand _splitHand2;
    ///private int _splitHand1Result = 0;
    ///private int _splitHand2Result = 0;
    ///private bool IsPLHand2 = false;     // 現在２つ目をプレイ中か
    ///
    ///[SerializeField] private Transform splitHandTransform; // 2つめの手札を置く場所
    /// </summary>


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deck.SetUpDeck();
        UpdateHpUI();
        ShowBetPhase();
    }

    // ベット入力フェーズ
    private void ShowBetPhase()
    {
        gameUI.ShowBetPanel(playerStatus.CurrentHp);
    }

    public void OnBetConfirm()
    {
        betAmount = gameUI.GetBetAmount();
        gameUI.HideBetPanel();
        StartRound();
    }

    // ------------------------------------------

    private void StartRound()
    {
        // ラウンド数のカウント
        _roundCount++;

        // ラウンド開始：カードを2枚ずつ配る
        playerHand.ClearHand();
        dealerHand.ClearHand();

        gameUI.HideResult();
        gameUI.UpdateDLScore(0);

        // 2枚ずつ配る
        AddCardToPL();
        AddCardToDL(isReverse: false); // ディーラーの1枚目：表
        AddCardToPL();
        _dealerHoleCard = AddCardToDL(isReverse: true); // ディーラーの2枚目：裏

        // PLの合計表示
        UpdatePLScoreUI();
        UpdateDLScoreUI();

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
        UpdatePLScoreUI();

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
        UpdatePLScoreUI();
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

        gameUI.UpdateDLScore(dealerHand.GetTotalValue());
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
            gameUI.UpdateDLScore(dealerHand.GetTotalValue());
        }

        int playerTotal = playerHand.GetTotalValue();
        int dealerTotal = dealerHand.GetTotalValue();

        RoundResult result = JudgeResult(playerTotal, dealerTotal);
        Debug.Log($"player: {playerTotal} / Dealer: {dealerTotal} -> {result}");

        // ToDo: resultに応じてHPの増減処理を呼ぶ
        ApplyResult(result);
        UpdateHpUI();

        // GameOver処理
        if (playerStatus.IsDead() || dealerStatus.IsDead())
        {
            bool playerWin = dealerStatus.IsDead();
            ShowGameOver(playerWin);
            return;
        }

        // Next Round
        Invoke(nameof(ShowBetPhase), 2f);
    }

    private void ApplyResult(RoundResult result)
    {
        switch (result)
        {
            case RoundResult.PlayerBJ:
                // BJ:DLにベット数の1.5倍ダメージ、PLはその1.5倍回復
                int bjDamage = Mathf.RoundToInt(betAmount * 1.5f);
                dealerStatus.TakeDamage(bjDamage);
                gameUI.ShowResult("BLACK JACK !!!");
                break;

            case RoundResult.PlayerWin:
                // 勝ち:DLにベット数ダメージ、PLその数回復
                dealerStatus.TakeDamage(betAmount);
                playerStatus.Heal(betAmount);
                gameUI.ShowResult("WIN!");
                break;

            case RoundResult.Lose:
                // 負け:PLにベット数分のダメージ
                playerStatus.TakeDamage(betAmount);
                gameUI.ShowResult("LOSE...");
                break;

            case RoundResult.Draw:
                // 引き分け:HP変動なし
                gameUI.ShowResult("DRAW");
                break;
        }
    }

    private enum RoundResult { PlayerBJ, PlayerWin, Lose, Draw }

    private RoundResult JudgeResult(int playerTotal, int dealerTotal)
    {
        if (playerHand.IsBJ()) return RoundResult.PlayerBJ;
        if (playerHand.IsBust()) return RoundResult.Lose;
        if (dealerHand.IsBust()) return RoundResult.PlayerWin;
        if (playerTotal > dealerTotal) return RoundResult.PlayerWin;
        if (playerTotal < dealerTotal) return RoundResult.Lose;

        return RoundResult.Draw;
    }

    private void ShowGameOver(bool playerWin)
    {
        gameUI.ShowResult(playerWin ? "YOU WIN!" : "GAME OVER");
        gameUI.ShowGameOver(playerWin, _roundCount);
    }

    // GameOverUIのボタン
    public void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    private Card AddCardToPL()
    {
        Card card = deck.DrawCard(playerHandTransform);
        playerHand.AddCard(card);
        return card;
    }

    private Card AddCardToDL(bool isReverse)
    {
        Card card = deck.DrawCard(dealerHandTransform, isReverse);
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

    private void UpdateHpUI()
    {
        gameUI.UpdatePlayerHp(playerStatus.CurrentHp, playerStatus.MaxHp);
        gameUI.UpdateDealerHp(dealerStatus.CurrentHp, dealerStatus.MaxHp);
    }

    private void UpdatePLScoreUI()
    {
        gameUI.UpdatePlayerScore(playerHand.GetTotalValue());
    }

    private void UpdateDLScoreUI()
    {
        gameUI.UpdateDLScore(dealerHand.GetVisibleValue());
    }
}
