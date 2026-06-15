using UnityEngine;
using UnityEngine.UI;

public class Data
{
    public Mark Mark;
    public int Number;

    // MarkとNumberから対応する画像をAssetフォルダから取得
    public Sprite GetSprite()
    {
        string markName = Mark.ToString().ToLower();
        string path = $"Card"
    }
}

public class Card : MonoBehaviour
{
    public enum Mark
    {
        Heart,
        Diamond,
        Spade,
        Club,
    }

    [SerializeField] private Sprite cardFaceSprite;
    [SerializeField] private Sprite cardBackSprite;

    public bool IsReverse = false;

    [Range(1, 13)]
    public int Number = 1;

    public Mark CurrentMark = Mark.Heart;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetCard(int number, Mark mark, Sprite faceSprite, Sprite backSprite, bool isReverse = false)
    {
        Number = Mathf.Clamp(number,1,13);
        CurrentMark = mark;
        IsReverse = isReverse;
        cardFaceSprite = faceSprite;
        cardBackSprite = backSprite;

        UpdateVisual();
    }

    public void Flip(bool isReverse)
    {
        //カードの裏表を切り替える

        IsReverse = isReverse;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_image == null) _image = GetComponent<Image>();
        _image.sprite = IsReverse ? cardBackSprite : cardFaceSprite;
    }

    public int GetBlackJackValue()
    {
        //絵札をすべて10として扱う

        if (Number >= 10) return 10;
        return Number;
    }

    public bool IsSameValue(Card other)
    {
        return this.GetBlackJackValue() == other.GetBlackJackValue();
    }
}
