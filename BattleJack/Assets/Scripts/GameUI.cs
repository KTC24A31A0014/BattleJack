using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    }

    public void UpdateDealerHp(int current, int max)
    {
        dealerHpText.text = $"HP: {current} / {max}";
        float ratio = (float)current / max;
        DLHpSlider.value = ratio;
        DLHpFill.color = GetHpColor(ratio);
    }

    public void UpdatePlayerScore(int score)
    {
        playerScoreText.text = $"{score}";
    }

    public void UpdateDLScore(int score)
    {
        dealerScoreText.text = $"{score}";
    }

    public void ShowResult(string result)
    {
        resultText.text = result;
        resultText.gameObject.SetActive(true);
    }

    public void HideResult()
    {
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
}
