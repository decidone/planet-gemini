using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Pool;

public class SoundManager : MonoBehaviour
{
    enum SceneIndex
    {
        MainScene = 0,
        GameScene = 1
    }

    SceneIndex sceneIndex;

    [SerializeField]
    private AudioClipRefsSO audioClipRefsSO;

    private AudioSource bgmPlayer;

    public AudioSource uiSfxPlayer;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    [SerializeField]
    Camera mainCamera;

    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private Slider musicMasterSlider;
    [SerializeField]
    private Slider musicBGMSlider;
    [SerializeField]
    private Slider musicSFXSlider;
    [SerializeField]
    private Toggle musicMasterToggle;
    [SerializeField]
    private Toggle musicBGMToggle;
    [SerializeField]
    private Toggle musicSFXToggle;

    public bool isHostMapBattleOn = false;
    public bool isClientMapBattleOn = false;
    public bool isHostMapWaveOn = false;
    public bool isClientMapWaveOn = false;

    float fadeSeconds = 0.3f;

    private Coroutine currentFade;

    int defaultPoolSize = 16;
    int maxPoolSize = 32;
    float defaultMinDistance = 4f;
    float defaultMaxDistance = 20f;
    Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
    AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    ObjectPool<AudioSource> pool;

    public static SoundManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;

        pool = new ObjectPool<AudioSource>(
            createFunc: CreatePooledSource,
            actionOnGet: OnGetSource,
            actionOnRelease: OnReleaseSource,
            actionOnDestroy: OnDestroySource,
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
    }

    private void OnEnable()
    {
        SFXPlayerSet();
        UIVolumeSet();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
        switch (scene.buildIndex)
        {
            case 0:
                bgmPlayer.clip = audioClipRefsSO.mainSceneBgm[Random.Range(0, audioClipRefsSO.mainSceneBgm.Length)];
                bgmPlayer.Play();
                sceneIndex = SceneIndex.MainScene;
                break;
            case 1:
                sceneIndex = SceneIndex.GameScene;
                break;
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        mainCamera = null;
    }

    void Update()
    {
        if (Time.timeScale == 0)
            return;

        if (currentFade == null && !bgmPlayer.isPlaying)
        {
            PlayBgmMapCheck();
        }
        else if (bgmPlayer.isPlaying && currentFade == null)
        {
            if (bgmPlayer.clip.length - bgmPlayer.time <= fadeSeconds)
            {
                AudioClip nextClip = GetCurrentContextClip();
                if (nextClip != null)
                    ChangeBGM(nextClip);
            }
        }
    }

    #region CrossFade

    public void ChangeBGM(AudioClip newClip)
    {
        if (newClip == null) return;

        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(CrossFade(newClip));
    }

    private IEnumerator CrossFade(AudioClip newClip)
    {
        yield return StartCoroutine(FadeTo(0f));

        bgmPlayer.clip = newClip;
        bgmPlayer.Play();

        yield return StartCoroutine(FadeTo(bgmVolume));

        currentFade = null;
    }

    private IEnumerator FadeTo(float targetVolume)
    {
        float startVolume = bgmPlayer.volume;
        float time = 0f;

        while (time < fadeSeconds)
        {
            time += Time.deltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVolume, targetVolume, time / fadeSeconds);
            yield return null;
        }

        bgmPlayer.volume = targetVolume;
    }

    #endregion

    #region BGM Context

    AudioClip GetCurrentContextClip()
    {
        if (sceneIndex != SceneIndex.GameScene)
            return audioClipRefsSO.mainSceneBgm[Random.Range(0, audioClipRefsSO.mainSceneBgm.Length)];

        if (GameManager.instance.isPlayerInMarket)
        {
            int index = GameManager.instance.dayIndex > 2 ? 1 : 0;
            return audioClipRefsSO.marketBgm[index];
        }

        bool playerMap = GameManager.instance.isPlayerInHostMap;
        bool isWave = playerMap ? isHostMapWaveOn : isClientMapWaveOn;
        bool isBattle = playerMap ? isHostMapBattleOn : isClientMapBattleOn;

        return GetBGMClip(isWave, isBattle);
    }

    AudioClip GetBGMClip(bool isWave, bool isBattle)
    {
        if (isWave)
            return audioClipRefsSO.waveBgm[Random.Range(0, audioClipRefsSO.waveBgm.Length)];
        else if (isBattle)
            return audioClipRefsSO.battleBgm[Random.Range(0, audioClipRefsSO.battleBgm.Length)];
        else
            return GetTimeBgmClip();
    }

    AudioClip GetTimeBgmClip()
    {
        if (GameManager.instance.dayIndex > 2)
            return audioClipRefsSO.nightBgm[Random.Range(0, audioClipRefsSO.nightBgm.Length)];
        else
            return audioClipRefsSO.dayBgm[Random.Range(0, audioClipRefsSO.dayBgm.Length)];
    }

    #endregion

    #region BGM Control

    public void GameSceneLoad()
    {
        ChangeBGM(GetTimeBgmClip());
    }

    public void PlayerMarketBgm()
    {
        int index = GameManager.instance.dayIndex > 2 ? 1 : 0;
        ChangeBGM(audioClipRefsSO.marketBgm[index]);
    }

    public void PlayBgmMapCheck()
    {
        if (sceneIndex == SceneIndex.GameScene)
        {
            bool playerMap = GameManager.instance.isPlayerInHostMap;
            bool isWaveActive = (playerMap && isHostMapWaveOn) || (!playerMap && isClientMapWaveOn);
            bool isBattleActive = (playerMap && isHostMapBattleOn) || (!playerMap && isClientMapBattleOn);
            if (!GameManager.instance.isPlayerInMarket)
            {
                // 웨이브 중이면 유지
                if (isWaveActive) return;

                // 웨이브 끝났지만 전투 중이면 전투 BGM으로
                if (isBattleActive)
                {
                    ChangeBGM(GetBGMClip(false, true));
                    return;
                }
            }
        }

        ChangeBGM(GetCurrentContextClip());
    }

    public void PlayerBgmMapCheck()
    {
        if (sceneIndex != SceneIndex.GameScene) return;

        if (isHostMapWaveOn != isClientMapWaveOn)
        {
            bool playerMap = GameManager.instance.isPlayerInHostMap;
            bool isWave = playerMap ? isHostMapWaveOn : isClientMapWaveOn;
            bool isBattle = playerMap ? isHostMapBattleOn : isClientMapBattleOn;
            ChangeBGM(GetBGMClip(isWave, isBattle));
        }
    }

    public void WaveStateSet(bool isHostMap, bool waveState)
    {
        if (isHostMap)
            isHostMapWaveOn = waveState;
        else
            isClientMapWaveOn = waveState;

        if (!GameManager.instance.isPlayerInMarket && GameManager.instance.isPlayerInHostMap == isHostMap)
            ChangeBGM(GetBGMClip(waveState, false));
    }

    public void BattleStateSet(bool isHostMap, bool battleState)
    {
        if (isHostMap)
            isHostMapBattleOn = battleState;
        else
            isClientMapBattleOn = battleState;

        if (!GameManager.instance.isPlayerInMarket && GameManager.instance.isPlayerInHostMap == isHostMap)
            PlayBgmMapCheck();
    }

    #endregion

    #region VolumeSet

    void UIVolumeSet()
    {
        musicMasterSlider.onValueChanged.AddListener(delegate { SetMasterVolume(); });
        musicMasterToggle.onValueChanged.AddListener(delegate { SetMasterMute(); });
        musicBGMSlider.onValueChanged.AddListener(delegate { SetBGMVolume(); });
        musicBGMToggle.onValueChanged.AddListener(delegate { SetBGMMute(); });
        musicSFXSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });
        musicSFXToggle.onValueChanged.AddListener(delegate { SetSFXMute(); });
    }

    public void SetMasterVolume()
    {
        float sound = musicMasterSlider.value;
        if (!musicMasterToggle.isOn)
        {
            audioMixer.SetFloat("Master", sound == -40f ? -80 : sound);
        }
    }

    public void SetMasterMute()
    {
        if (!musicMasterToggle.isOn)
            audioMixer.SetFloat("Master", musicMasterSlider.value);
        else
            audioMixer.SetFloat("Master", -80);

        PlayUISFX("ButtonClick");
    }

    public void SetBGMVolume()
    {
        float sound = musicBGMSlider.value;
        if (!musicBGMToggle.isOn)
        {
            audioMixer.SetFloat("BGM", sound == -40f ? -80 : sound);
        }
    }

    public void SetBGMMute()
    {
        if (!musicBGMToggle.isOn)
            audioMixer.SetFloat("BGM", musicBGMSlider.value);
        else
            audioMixer.SetFloat("BGM", -80);

        PlayUISFX("ButtonClick");
    }

    public void SetSFXVolume()
    {
        float sound = musicSFXSlider.value;
        if (!musicSFXToggle.isOn)
        {
            audioMixer.SetFloat("SFX", sound == -40f ? -80 : sound);
        }
    }

    public void SetSFXMute()
    {
        if (!musicSFXToggle.isOn)
            audioMixer.SetFloat("SFX", musicSFXSlider.value);
        else
            audioMixer.SetFloat("SFX", -80);

        PlayUISFX("ButtonClick");
    }

    #endregion

    #region PlayerSetup

    void SFXPlayerSet()
    {
        BgmPlayerSet();
        UIPlayerSet();
    }

    void BgmPlayerSet()
    {
        bgmPlayer = PlayerBaseSet("BGM");
    }

    void UIPlayerSet()
    {
        uiSfxPlayer = PlayerBaseSet("SFX");
    }

    AudioSource PlayerBaseSet(string group)
    {
        AudioSource newAudio = gameObject.AddComponent<AudioSource>();
        newAudio.dopplerLevel = 0;
        newAudio.reverbZoneMix = 0;
        newAudio.outputAudioMixerGroup = audioMixer.FindMatchingGroups(group)[0];
        newAudio.volume = group == "BGM" ? bgmVolume : sfxVolume;
        return newAudio;
    }

    #endregion

    #region SFX

    public void PlaySFX(GameObject obj, string sfxGroupName, string sfxName)
    {
        if (!CheckObjectIsInCamera(obj) || mainCamera == null)
            return;

        switch (sfxGroupName)
        {
            case "structureSFX":
                PlaySFX(obj, true, sfxName, 0.2f);
                break;
            case "unitSFX":
                PlaySFX(obj, false, sfxName, 0.03f);
                break;
            default:
                return;
        }
    }

    public void PlaySFX(GameObject target, bool isStrSound, string sfxName, float cooldown)
    {
        if (target == null) return;

        float distance = Vector3.Distance(target.transform.position, mainCamera.transform.position);
        if (distance > defaultMaxDistance)
            return;

        if (lastPlayTime.TryGetValue(sfxName, out float lastTime))
        {
            if (Time.time - lastTime < cooldown)
                return;
        }

        AudioClip[] audioClips = isStrSound ? audioClipRefsSO.structureSfx : audioClipRefsSO.unitSfx;
        AudioClip clip = null;

        for (int i = 0; i < audioClips.Length; i++)
        {
            if (sfxName == audioClips[i].name)
            {
                clip = audioClips[i];
                break;
            }
        }
        if (clip == null) return;

        AudioSource source = pool.Get();
        source.transform.position = target.transform.position;
        source.clip = clip;
        source.volume = sfxVolume;
        source.maxDistance = defaultMaxDistance;
        source.Play();

        lastPlayTime[sfxName] = Time.time;
        StartCoroutine(ReleaseAfterPlay(source, clip.length));
    }

    private IEnumerator ReleaseAfterPlay(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (source != null && source.gameObject.activeSelf)
            pool.Release(source);
    }

    public void PlayUISFX(string sfxName)
    {
        for (int i = 0; i < audioClipRefsSO.uiSfx.Length; i++)
        {
            if (sfxName == audioClipRefsSO.uiSfx[i].name)
            {
                uiSfxPlayer.clip = audioClipRefsSO.uiSfx[i];
                uiSfxPlayer.volume = sfxVolume;
                uiSfxPlayer.Play();
                return;
            }
        }
    }

    #endregion

    #region Camera Check

    public bool CheckObjectIsInCamera(GameObject obj)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(obj.transform.position);
        return screenPoint.z > 0 &&
               screenPoint.x > 0 && screenPoint.x < 1 &&
               screenPoint.y > 0 && screenPoint.y < 1;
    }

    #endregion

    #region Pool

    private AudioSource CreatePooledSource()
    {
        GameObject go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();

        source.spatialBlend = 1f;
        source.minDistance = defaultMinDistance;
        source.maxDistance = defaultMaxDistance;
        source.rolloffMode = rolloffMode;
        source.playOnAwake = false;
        source.spread = 60f;
        source.dopplerLevel = 0f;
        source.reverbZoneMix = 0f;
        source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];

        go.SetActive(false);
        return source;
    }

    private void OnGetSource(AudioSource source) => source.gameObject.SetActive(true);

    private void OnReleaseSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
    }

    private void OnDestroySource(AudioSource source)
    {
        if (source != null)
            Destroy(source.gameObject);
    }

    #endregion

    #region Save / Load

    public void SaveData()
    {
        audioMixer.GetFloat("Master", out float masterVol);
        PlayerPrefs.SetFloat("MasterVolume", masterVol);
        PlayerPrefs.SetFloat("TempMasterVolume", musicMasterSlider.value);
        PlayerPrefs.SetInt("MasterMute", musicMasterToggle.isOn ? 1 : 0);

        audioMixer.GetFloat("BGM", out float bgmVol);
        PlayerPrefs.SetFloat("BGMVolume", bgmVol);
        PlayerPrefs.SetFloat("TempBGMVolume", musicBGMSlider.value);
        PlayerPrefs.SetInt("BGMMute", musicBGMToggle.isOn ? 1 : 0);

        audioMixer.GetFloat("SFX", out float sfxVol);
        PlayerPrefs.SetFloat("SFXVolume", sfxVol);
        PlayerPrefs.SetFloat("TempSFXVolume", musicSFXSlider.value);
        PlayerPrefs.SetInt("SFXMute", musicSFXToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        float loadedMasterVol = PlayerPrefs.GetFloat("MasterVolume");
        audioMixer.SetFloat("Master", loadedMasterVol);
        musicMasterSlider.value = PlayerPrefs.GetFloat("TempMasterVolume", -10);
        musicMasterToggle.isOn = PlayerPrefs.GetInt("MasterMute", 0) != 0;

        float loadedBgmVol = PlayerPrefs.GetFloat("BGMVolume");
        audioMixer.SetFloat("BGM", loadedBgmVol);
        musicBGMSlider.value = PlayerPrefs.GetFloat("TempBGMVolume", -10);
        musicBGMToggle.isOn = PlayerPrefs.GetInt("BGMMute", 0) != 0;

        float loadedSfxVol = PlayerPrefs.GetFloat("SFXVolume");
        audioMixer.SetFloat("SFX", loadedSfxVol);
        musicSFXSlider.value = PlayerPrefs.GetFloat("TempSFXVolume", -10);
        musicSFXToggle.isOn = PlayerPrefs.GetInt("SFXMute", 0) != 0;
    }

    #endregion
}