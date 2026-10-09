using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SceneBgm
    {
        public string sceneName;
        public AudioClip clip;
    }

    [SerializeField] private SceneBgm[] sceneBgms;
    [SerializeField] private float fadeTime = 1f;

    private const string VolumeKey = "BgmVolume";
    private float _bgmVolume = 0.6f;

    // クロスフェード用に2本使う
    private AudioSource[] _sources = new AudioSource[2];
    private Tween[] _tweens = new Tween[2];
    private int _active = 0;

    private void Awake()
    {
        // 既に存在していたら自分は消える（各シーンに置いても重複しない）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < _sources.Length; i++)
        {
            _sources[i] = gameObject.AddComponent<AudioSource>();
            _sources[i].loop = true;
            _sources[i].playOnAwake = false;
            _sources[i].volume = 0f;
        }

        _bgmVolume = PlayerPrefs.GetFloat(VolumeKey, _bgmVolume);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this) return;
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    private void PlayForScene(string sceneName)
    {
        foreach (var b in sceneBgms)
        {
            if (b.sceneName == sceneName)
            {
                PlayBgm(b.clip);
                return;
            }
        }
        // 登録のないシーンではBGMを止める
        StopBgm();
    }

    /// <summary>
    /// BGMを切り替える。同じ曲なら何もしない（途切れない）
    /// </summary>
    public void PlayBgm(AudioClip clip)
    {
        if (clip == null)
        {
            StopBgm();
            return;
        }

        AudioSource current = _sources[_active];
        if (current.clip == clip && current.isPlaying) return;

        // 今の曲をフェードアウト
        FadeOut(_active);

        // 次の曲をフェードイン
        _active = 1 - _active;
        AudioSource next = _sources[_active];
        _tweens[_active]?.Kill();
        next.clip = clip;
        next.volume = 0f;
        next.Play();
        _tweens[_active] = DOTween.To(() => next.volume, v => next.volume = v, _bgmVolume, fadeTime);
    }

    public void StopBgm()
    {
        FadeOut(_active);
    }

    private void FadeOut(int index)
    {
        AudioSource src = _sources[index];
        _tweens[index]?.Kill();
        if (!src.isPlaying) return;

        _tweens[index] = DOTween.To(() => src.volume, v => src.volume = v, 0f, fadeTime)
            .OnComplete(() =>
            {
                src.Stop();
                src.clip = null;
            });
    }

    /// <summary>
    /// 音量を変更して保存する（ポーズ画面のスライダー用）。0〜1
    /// </summary>
    public void SetBgmVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, _bgmVolume);

        // 再生中の曲に即反映（フェード中なら止めて上書き）
        _tweens[_active]?.Kill();
        if (_sources[_active].isPlaying) _sources[_active].volume = _bgmVolume;
    }

    public float GetBgmVolume() => _bgmVolume;
}