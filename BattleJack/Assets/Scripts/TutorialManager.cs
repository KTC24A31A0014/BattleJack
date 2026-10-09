using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private Sprite[] pages;
    [SerializeField] private Image pageImage;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private int _index = 0;

    private void Start()
    {
        ShowPage(0);
    }

    // NextButtonのOnClickにアタッチ
    public void OnNext()
    {
        if (_index < pages.Length - 1) ShowPage(_index + 1);
    }

    // PrevButtonのOnClickにアタッチ
    public void OnPrev()
    {
        if (_index > 0) ShowPage(_index - 1);
    }

    // BackButton（タイトルへ）のOnClickにアタッチ
    public void OnBackToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    private void ShowPage(int index)
    {
        _index = index;
        pageImage.sprite = pages[_index];

        // 端のページでは押せなくする
        prevButton.interactable = _index > 0;
        nextButton.interactable = _index < pages.Length - 1;
    }
}