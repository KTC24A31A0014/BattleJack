using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI dealerHpText;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI dealerScoreText;
    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField] private Slider PLHpSlider;
    [SerializeField] private Slider DLHpSlider;

    // SliderのFillのImageコンポーネントをアタッチ
    [SerializeField] private Image PLHpFill;
    [SerializeField] private Image DLHpFill;

    // ベットUI
    [SerializeField] private GameObject BetPanel;
    [SerializeField] private Slider BetSlider;
    [SerializeField] private TextMeshProUGUI BetAmountText;

    // GameOverUI
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverResultText;
    [SerializeField] private TextMeshProUGUI gameOverRoundText;

    // 10/08 - 振動
    [SerializeField] private RectTransform shakeRoot;

    // ダメージ表示
    [SerializeField] private TextMeshProUGUI popupPrefab;
    [SerializeField] private RectTransform playerPopupAnchor;
    [SerializeField] private RectTransform dealerPopupAnchor;

    // ディーラーの立ち絵振動
    [SerializeField] private RectTransform dealerSprite;
    [SerializeField] private Image dealerSpriteImage;

    // HPBarの残像
    [SerializeField] private Slider PLHpTrail;
    [SerializeField] private Slider DLHpTrail;

    private Tween _plTrailTween;
    private Tween _dlTrailTween;

    private Sequence _bannerSeq;


    // HP割合に応じた色
    private static readonly Color ColorHigh = new Color(0.18f, 0.80f, 0.18f); // 緑
    private static readonly Color ColorMid = new Color(1.00f, 0.75f, 0.00f); // 黄
    private static readonly Color ColorLow = new Color(0.90f, 0.18f, 0.18f); // 赤

    public void UpdatePlayerHp(int current, int max)
    {
        playerHpText.text = $"HP: {current} / {max}";
        float ratio = (float)current / max;
        PLHpSlider.value = ratio;
        PLHpFill.color = GetHpColor(ratio);
        UpdateTrail(PLHpTrail, ratio, ref _plTrailTween);
    }

    public void UpdateDealerHp(int current, int max)
    {
        dealerHpText.text = $"HP: {current} / {max}";
        float ratio = (float)current / max;
        DLHpSlider.value = ratio;
        DLHpFill.color = GetHpColor(ratio);
        UpdateTrail(DLHpTrail, ratio, ref _dlTrailTween);
    }

    public void UpdatePLScore(int score)
    {
        playerScoreText.gameObject.SetActive(true);
        playerScoreText.text = $"{score}";
    }

    public void UpdateDLScore(int score)
    {
        dealerScoreText.gameObject.SetActive(true);
        dealerScoreText.text = $"{score}";
    }

    public void ShowResult(string result)
    {
        _bannerSeq?.Kill();

        resultText.text = result;
        resultText.gameObject.SetActive(true);
        resultText.alpha = 0f;
        resultText.transform.localScale = Vector3.one * 2f;

        _bannerSeq = DOTween.Sequence();
        _bannerSeq.Append(resultText.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        _bannerSeq.Join(DOTween.To(() => resultText.alpha, x => resultText.alpha = x, 1f, 0.2f));
    }

    public void HideResult()
    {
        _bannerSeq?.Kill();
        resultText.gameObject.SetActive(false);
    }

    public void ShowGameOver(bool playerWin, int roundCount)
    {
        gameOverPanel.SetActive(true);
        gameOverResultText.text = playerWin ? "YOU WIN!" : "GAME OVER";
        gameOverRoundText.text = $"{roundCount}";
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }

    // ベットパネルを表示してSliderの範囲を設定
    public void ShowBetPanel(int maxBet)
    {
        BetPanel.SetActive(true);
        BetSlider.minValue = 1;
        BetSlider.maxValue = maxBet;
        BetSlider.value = 1;
        BetSlider.wholeNumbers = true;
        UpdateBetText();
    }

    public void HideBetPanel()
    {
        BetPanel.SetActive(false);
    }

    public void OnBetSliderChanged(float value)
    {
        UpdateBetText();
    }

    private void UpdateBetText()
    {
        BetAmountText.text = $"BET : {(int)BetSlider.value}";
    }

    // 現在のベット数を返す
    public int GetBetAmount()
    {
        return (int)BetSlider.value;
    }

    // HP割合に応じて色を返す
    // 半分以上で緑、４分の１で黄、４分の１未満で赤
    private Color GetHpColor(float ratio)
    {
        if (ratio > 0.5f) return ColorHigh;
        if (ratio > 0.25f) return ColorMid;

        return ColorLow;
    }

    // 10/08 手持ちの合計点の非表示処理
    public void HideScores()
    {
        playerScoreText.gameObject.SetActive(false);
        dealerScoreText.gameObject.SetActive(false);
    }    

    // 画面の振動
    public void ShakeScreen(float strength = 25f, float duration = 0.4f)
    {
        shakeRoot.DOKill(true);
        shakeRoot.DOShakeAnchorPos(duration, strength, 20, 90f, false, true);
    }

    // ディーラーの振動
    public void ShakeDealer(float strength = 20f, float duration = 0.35f)
    {
        // shake
        dealerSprite.DOKill(true);
        dealerSprite.DOShakeAnchorPos(duration, strength, 25, 90f, false, true);

        // 赤くする
        dealerSpriteImage.DOKill();
        dealerSpriteImage.color = new Color(1f, 0.4f, 0.4f);
        dealerSpriteImage.DOColor(Color.white, duration);
    }

    // ダメージ表示
    public void ShowPopup(bool onPlayer, int amount, bool isHeal)
    {
        if (amount <= 0) return;

        RectTransform anchor = onPlayer ? playerPopupAnchor : dealerPopupAnchor;
        TextMeshProUGUI t = Instantiate(popupPrefab, anchor);
        t.rectTransform.anchoredPosition = Vector2.zero;
        t.text = isHeal ? $"+{amount}" : $"-{amount}";
        t.color = isHeal ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
        t.alpha = 1f;

        // PlayerとDealerの切り替え
        float moveY = onPlayer ? 80f : -80f;

        Sequence seq = DOTween.Sequence();
        seq.Append(t.rectTransform.DOAnchorPosY(moveY, 0.9f).SetRelative().SetEase(Ease.OutCubic));
        seq.Join(DOTween.To(() => t.alpha, x => t.alpha = x, 0f, 0.4f).SetDelay(0.5f));
        seq.OnComplete(() => Destroy(t.gameObject));
    }
    
    // HPBarの残像
    private void UpdateTrail(Slider trail, float ratio, ref Tween tween)
    {
        tween?.Kill();

        if (ratio >= trail.value)
        {
            //回復時は即座に追従
            trail.value = ratio;
            return;
        }

        // ダメージを受けた時は少し待って、ゆっくり追いつく
        tween = DOTween.To(() => trail.value, x => trail.value = x, ratio, 0.4f).SetDelay(0.5f).SetEase(Ease.OutQuad);
    }
}
