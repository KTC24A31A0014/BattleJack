using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void OnGameStart()
    {
        SceneManager.LoadScene("GameScene");
    }
}
