using System.Collections;
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

    // デッキの位置
    [SerializeField] private RectTransform deckTransform;


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
    // Score hide
    private void ShowBetPhase()
    {
        gameUI.HideScores();
        gameUI.HideResult();
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
        _roundCount++;
        playerHand.ClearHand();
        dealerHand.ClearHand();
        gameUI.HideResult();

        StartCoroutine(DealCardsRoutine());
    }

    private IEnumerator DealCardsRoutine()
    {
        // 1毎ずつ間をあけて配る
        AddCardToPL();
        yield return new WaitForSeconds(0.35f);

        AddCardToDL(isReverse: false);
        yield return new WaitForSeconds(0.35f);

        AddCardToPL();
        yield return new WaitForSeconds(0.35f);

        _dealerHoleCard = AddCardToDL(isReverse: true);
        yield return new WaitForSeconds(0.35f);

        UpdatePLScoreUI();
        UpdateDLScoreUI();

        if (playerHand.IsBJ())
        {
            EndRound();
            yield break;
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
        StartCoroutine(DealerTurnRoutine()); // DealerTurn()から変更
    }

    public void OnDoubleDown()
    {
        AddCardToPL();
        UpdatePLScoreUI();
        SetBtnActive(false);

        if (playerHand.IsBust())
        {
            EndRound();
            return;
        }

        StartCoroutine(DealerTurnRoutine()); // DealerTurn()から変更
    }

    public void OnSplit()
    {
        // ToDo: Split実装
        Debug.Log("Split: 未実装");
    }

    // ディーラーのターン

    private IEnumerator DealerTurnRoutine()
    {
        // 伏せ札を公開して少し間を置く
        _dealerHoleCard.Flip(isReverse: false);
        gameUI.UpdateDLScore(dealerHand.GetTotalValue());
        yield return new WaitForSeconds(0.6f);

        // 17以上になるまで1枚ずつ引く
        while (dealerHand.GetTotalValue() < 17)
        {
            AddCardToDL(isReverse: false);
            gameUI.UpdateDLScore(dealerHand.GetTotalValue());
            yield return new WaitForSeconds(0.6f);
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
        int playerTotal = playerHand.GetTotalValue();
        int dealerTotal = dealerHand.GetTotalValue();

        switch (result)
        {
            case RoundResult.PlayerBJ:
                // BJ:DLにベット数の1.5倍ダメージ、PLはその1.5倍回復
                int bjDamage = Mathf.RoundToInt(betAmount * 1.5f);
                gameUI.ShowPopup(false, dealerStatus.TakeDamage(bjDamage), false);
                gameUI.ShowPopup(true, playerStatus.Heal(bjDamage), true);
                gameUI.ShowResult("BLACK JACK !!!");
                gameUI.ShakeScreen(15f, 0.3f);
                gameUI.ShakeDealer(30f, 0.45f);
                break;

            case RoundResult.PlayerWin:
                // 勝ち:DLにベット数ダメージ、PLその数回復
                gameUI.ShowPopup(false, dealerStatus.TakeDamage(betAmount), false);
                gameUI.ShowPopup(true, playerStatus.Heal(betAmount), true);
                gameUI.ShowResult("WIN!");
                gameUI.ShakeDealer();
                break;

            case RoundResult.Lose:
                // 負け:PLにベット数分のダメージ
                int extraDamage = playerHand.IsBust()
                    ? playerTotal - 21
                    : dealerTotal - playerTotal;
                gameUI.ShowPopup(true, playerStatus.TakeDamage(betAmount + extraDamage), false);
                gameUI.ShowResult("LOSE...");
                gameUI.ShakeScreen(30f, 0.45f);
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

    private Card AddCardToDL(bool isReverse)
    {
        Card card = deck.DrawCard(dealerHandTransform, isReverse, GetDeckPosition());
        dealerHand.AddCard(card);
        return card;
    }

    private Card AddCardToPL()
    {
        Card card = deck.DrawCard(playerHandTransform, false, GetDeckPosition());
        playerHand.AddCard(card);
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
        gameUI.UpdatePLScore(playerHand.GetTotalValue());
    }

    private void UpdateDLScoreUI()
    {
        gameUI.UpdateDLScore(dealerHand.GetVisibleValue());
    }

    // アニメーション 10/07
    private Vector3 GetDeckPosition()
    {
        return deckTransform.position;
    }
}
