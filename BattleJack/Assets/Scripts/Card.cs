using UnityEngine;
using UnityEngine.UI;



public class Card : MonoBehaviour
{
    public enum Mark
    {
        Spade,
        Club,
        Heart,
        Diamond,
    }

    public class Data
    {
        public Mark Mark;
        public int Number;

        public Data(int number, Mark mark)
        {
            Number = number;
            Mark = mark;
        }

        // MarkとNumberから対応する画像をAssetフォルダから取得
        public Sprite GetSprite()
        {
            string folderName;
            string prefix;

            switch (Mark)
            {
                case Mark.Spade:   folderName = "1.Spade";     prefix = "s"; break;
                case Mark.Club :   folderName = "2.Club" ;     prefix = "c"; break;
                case Mark.Heart:   folderName = "3.Heart";     prefix = "h"; break;
                case Mark.Diamond: folderName = "4.Diamond";   prefix = "d"; break;

                default:                folderName = "1.Spade";     prefix = "s"; break;
            }

            string path = $"Card/{folderName}/playingCards_{prefix}{Number}";
            return Resources.Load<Sprite>(path);
        }

        public static Sprite GetBackSprite()
        {
            return Resources.Load<Sprite>("Card/back");
        }

        public int GetBlackJackValue()
        {
            if (Number >= 10) return 10;
            return Number;
        }

        public bool IsSameValueAs(Data other)
        {
            return this.GetBlackJackValue() == other.GetBlackJackValue();
        }
    }

    public bool IsReverse = false;
    public Data CardData { get; private set; }

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetCard(Data data, bool isReverse = false)
    {
        CardData = data;
        IsReverse = isReverse;

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
        _image.sprite = IsReverse ? Data.GetBackSprite() : CardData.GetSprite();
    }
}
