using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    enum SceneIndex
    {
        MainScene = 0,
        GameScene = 1
    }

    SceneIndex sceneIndex;

    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    public static SoundManager instance { get; private set; }

    [SerializeField] 
    private AudioClipRefsSO audioClipRefsSO;

    private AudioSource bgmPlayer;

    List<AudioSource> structureSfxPlayer = new List<AudioSource>();
    [SerializeField]
    int structureSfxPlayerMaxCount;

    List<AudioSource> unitSfxPlayer = new List<AudioSource>();
    [SerializeField]
    int unitSfxPlayerMaxCount;

    AudioSource uiSfxPlayer;

    private float bgmVolume = .6f;
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

    bool structureSfxPlay = false;
    bool unitSfxPlay = false;

    float structureDelayTimer = 0.0f;
    float structureDelayInterval = 0.05f;
    float unitDelayTimer = 0.0f;
    float unitDelayInterval = 0.05f;

    //public bool isHostMapBattleOn = false;
    //public bool isClientMapBattleOn = false;
    public bool isHostMapWaveOn = false;
    public bool isClientMapWaveOn = false;

    float fadeSeconds = 1.0f;
    bool isFadingOut;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
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

    void GameBgmByTime()
    {
        if (GameManager.instance.dayIndex > 2)
        {
            bgmPlayer.clip = audioClipRefsSO.nightBgm[Random.Range(0, audioClipRefsSO.nightBgm.Length)];
        }
        else
        {
            bgmPlayer.clip = audioClipRefsSO.dayBgm[Random.Range(0, audioClipRefsSO.dayBgm.Length)];
        }
    }

    public void GameSceneLoad()
    {
        GameBgmByTime();
        bgmPlayer.Play();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        mainCamera = null;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (!bgmPlayer.isPlaying)
        {
            PlayBgmMapCheck();
        }
        else if (bgmPlayer.isPlaying && !isFadingOut)
        {
            if (bgmPlayer.clip.length - bgmPlayer.time <= 1.0f)
            {
                StartCoroutine(nameof(SoundFadeOut));
            }
        }

        if (structureSfxPlay)
        {
            structureDelayTimer += Time.deltaTime;
            if(structureDelayTimer > structureDelayInterval)
            {
                structureSfxPlay = false;
            }
        }

        if (unitSfxPlay)
        {
            unitDelayTimer += Time.deltaTime;
            if (unitDelayTimer > unitDelayInterval)
            {
                unitSfxPlay = false;
            }
        }
    }

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
            if (sound == -40f)
                audioMixer.SetFloat("Master", -80);
            else
                audioMixer.SetFloat("Master", sound);
        }
    }

    public void SetMasterMute()
    {
        if (!musicMasterToggle.isOn)
        {
            float sound = musicMasterSlider.value;
            audioMixer.SetFloat("Master", sound);
        }
        else
        {
            audioMixer.SetFloat("Master", -80);
        }
        PlayUISFX("ButtonClick");
    }

    public void SetBGMVolume()
    {
        float sound = musicBGMSlider.value;
        if (!musicBGMToggle.isOn)
        {
            if (sound == -40f)
                audioMixer.SetFloat("BGM", -80);
            else
                audioMixer.SetFloat("BGM", sound);
        }
    }

    public void SetBGMMute()
    {
        if (!musicBGMToggle.isOn)
        {
            float sound = musicBGMSlider.value;
            audioMixer.SetFloat("BGM", sound);
        }
        else
        {
            audioMixer.SetFloat("BGM", -80);
        }
        PlayUISFX("ButtonClick");
    }

    public void SetSFXVolume()
    {
        float sound = musicSFXSlider.value;
        if (!musicSFXToggle.isOn)
        {
            if (sound == -40f)
                audioMixer.SetFloat("SFX", -80);
            else
                audioMixer.SetFloat("SFX", sound);
        }
    }

    public void SetSFXMute()
    {
        if (!musicSFXToggle.isOn)
        {
            float sound = musicSFXSlider.value;
            audioMixer.SetFloat("SFX", sound);
        }
        else
        {
            audioMixer.SetFloat("SFX", -80);
        }
        PlayUISFX("ButtonClick");
    }

    #endregion

    void SFXPlayerSet()
    {
        BgmPlayerSet();
        StructureSFXPlayerSet();
        UnitSFXPlayerSet();
        UIPlayerSet();
    }

    public void PlayerBgmMapCheck()
    {
        if (sceneIndex == SceneIndex.GameScene)
        {
            if (isHostMapWaveOn != isClientMapWaveOn)
            {
                if (GameManager.instance.isPlayerInHostMap)
                {
                    PlayBGM(isHostMapWaveOn);
                }
                else
                {
                    PlayBGM(isClientMapWaveOn);
                }
            }
        }
    }

    public void PlayerMarketBgm()
    {
        StartCoroutine(nameof(SoundFadeOut));
        StartCoroutine(MarketBgmChange());
    }

    public IEnumerator MarketBgmChange()
    {
        yield return new WaitForSecondsRealtime(fadeSeconds);

        StartCoroutine(nameof(SoundFadeIn));

        if (GameManager.instance.dayIndex > 2)
        {
            bgmPlayer.clip = audioClipRefsSO.marketBgm[1];
        }
        else
        {
            bgmPlayer.clip = audioClipRefsSO.marketBgm[0];
        }
        bgmPlayer.Play();
    }

    public void PlayBgmMapCheck()
    {
        //웨이브 상태에서 BGM이 바뀌지 않게 예외처리
        if (sceneIndex == SceneIndex.GameScene)
        {
            bool playerMap = GameManager.instance.isPlayerInHostMap;
            bool isWaveActive = (playerMap && isHostMapWaveOn) || (!playerMap && isClientMapWaveOn);

            if (!GameManager.instance.isPlayerInMarket && isWaveActive)
            {
                return;
            }
        }
 
        StartCoroutine(nameof(SoundFadeOut));
        StartCoroutine(BgmChange());
    }

    public IEnumerator BgmChange()
    {
        yield return new WaitForSecondsRealtime(fadeSeconds);

        if (sceneIndex == SceneIndex.GameScene)
        {
            if (GameManager.instance.isPlayerInMarket)
            {
                PlayerMarketBgm();
            }
            else if (GameManager.instance.isPlayerInHostMap)
            {
                PlayBGM(isHostMapWaveOn);
            }
            else
            {
                PlayBGM(isClientMapWaveOn);
            }
        }
        else
        {
            bgmPlayer.clip = audioClipRefsSO.mainSceneBgm[Random.Range(0, audioClipRefsSO.mainSceneBgm.Length)];
            bgmPlayer.Play();
            StartCoroutine(nameof(SoundFadeIn));
        }
    }

    void BgmPlayerSet()
    {
        bgmPlayer = PlayerBaseSet("BGM");
    }

    void StructureSFXPlayerSet()
    {
        for (int i = 0; i < structureSfxPlayerMaxCount; i++)
        {
            AudioSource audio = PlayerBaseSet("SFX");
            structureSfxPlayer.Add(audio);
        }
    }

    void UnitSFXPlayerSet()
    {
        for (int i = 0; i < unitSfxPlayerMaxCount; i++)
        {
            AudioSource audio = PlayerBaseSet("SFX");
            unitSfxPlayer.Add(audio);
        }
    }

    void UIPlayerSet()
    {
        uiSfxPlayer = PlayerBaseSet("SFX");
    }

    AudioSource PlayerBaseSet(string group)
    {
        AudioSource newAuido = gameObject.AddComponent<AudioSource>();
        newAuido.dopplerLevel = 0;
        newAuido.reverbZoneMix = 0;
        newAuido.outputAudioMixerGroup = audioMixer.FindMatchingGroups(group)[0];

        switch(group)
        {
            case "BGM" :
                newAuido.volume = bgmVolume;
                break;
            case "SFX":
                newAuido.volume = sfxVolume;
                break;
        }

        return newAuido;
    }

    void PlayBGM(bool isWave)
    {
        if (isWave)
        {
            bgmPlayer.clip = audioClipRefsSO.waveBgm[Random.Range(0, audioClipRefsSO.waveBgm.Length)];
        }
        else
        {
            GameBgmByTime();
        }

        bgmPlayer.Play();
        StartCoroutine(nameof(SoundFadeIn));
    }

    public void BattleStateSet(bool isHostMap, bool waveState)
    {
        if (isHostMap)
            isHostMapWaveOn = waveState;
        else
            isClientMapWaveOn = waveState;

        if (!GameManager.instance.isPlayerInMarket && GameManager.instance.isPlayerInHostMap == isHostMap)
        {
            PlayBgmMapCheck();
        }
    }

    //[ServerRpc]
    //public void BattleStateSetServerRpc(bool isBattle, bool isWave, bool isHostMap)
    //{
    //    BattleStateSetClientRpc(isBattle, isWave, isHostMap);
    //}

    //[ClientRpc]
    //public void BattleStateSetClientRpc(bool isBattle, bool isWave, bool isHostMap)
    //{
    //    if (isHostMap)
    //    {
    //        if (isHostMapBattleOn == isBattle && isHostMapWaveOn == isWave)
    //        {
    //            return;
    //        }
    //        isHostMapBattleOn = isBattle;
    //        isHostMapWaveOn = isWave;
    //        if (GameManager.instance.isPlayerInMarket)
    //        {
    //            return;
    //        }
    //        PlayBgmMapCheck();
    //        //StartCoroutine(nameof(SoundFadeOut));
    //    }
    //    else if(!isHostMap)
    //    {
    //        if (isClientMapBattleOn == isBattle && isClientMapWaveOn == isWave)
    //        {
    //            return;
    //        }
    //        isClientMapBattleOn = isBattle;
    //        isClientMapWaveOn = isWave;
    //        if (GameManager.instance.isPlayerInMarket)
    //        {
    //            return;
    //        }
    //        PlayBgmMapCheck();
    //        //StartCoroutine(nameof(SoundFadeOut));
    //    }
    //}

    IEnumerator SoundFadeIn() // 점점 커지는
    {
        float time = 0.0f;

        while (time < fadeSeconds)
        {
            time += Time.deltaTime;

            bgmPlayer.volume = bgmVolume * (time / fadeSeconds) ;

            yield return null;
        }

        bgmPlayer.volume = bgmVolume;
    }

    IEnumerator SoundFadeOut() // 점점 작아지는
    {
        isFadingOut = true;
        float time = 0.0f;

        while (time < fadeSeconds)
        {
            time += Time.deltaTime;

            bgmPlayer.volume = bgmVolume * ((fadeSeconds - time) / fadeSeconds);

            yield return null;
        }

        bgmPlayer.volume = 0;
        //PlayBgmMapCheck();
        isFadingOut = false;
    }

    public void PlaySFX(GameObject obj, string sfxGroupName, string sfxName)
    {
        if (!CheckObjectIsInCamera(obj) || mainCamera == null)
            return;

        switch (sfxGroupName)
        {
            case "structureSFX":
                {
                    if (structureSfxPlay)
                        return;
                    SFXAudioSet(structureSfxPlayer, true, sfxName);
                    structureSfxPlay = true;
                }
                break;
            case "unitSFX":
                {
                    if (unitSfxPlay)
                        return;
                    SFXAudioSet(unitSfxPlayer, false, sfxName);
                    unitSfxPlay = true;
                }
                break;
            default :
                return;
        }
    }

    void SFXAudioSet(List<AudioSource> audioList, bool strSound, string sfxName)
    {
        AudioClip playClip = null;
        AudioClip[] audioClips;

        if (strSound)
        {
            audioClips = audioClipRefsSO.structureSfx;
        }
        else
        {
            audioClips = audioClipRefsSO.unitSfx;
        }

        for (int i = 0; i < audioClips.Length; i++)
        {
            if (sfxName == audioClips[i].name)
            {
                playClip = audioClips[i];
            }
        }

        if (strSound)
        {
            for (int j = 0; j < audioList.Count; j++)
            {
                if (audioList[j].clip == playClip && audioList[j].isPlaying)
                {
                    return;
                }
            }
        }


        for (int j = 0; j < audioList.Count; j++)
        {
            if (!audioList[j].isPlaying)
            {
                audioList[j].clip = playClip;
                audioList[j].volume = sfxVolume;
                audioList[j].Play();
                return;
            }
        }
    }

    public void PlayUISFX(string p_sfxName)
    {
        for (int i = 0; i < audioClipRefsSO.uiSfx.Length; i++)
        {
            if (p_sfxName == audioClipRefsSO.uiSfx[i].name)
            {
                uiSfxPlayer.clip = audioClipRefsSO.uiSfx[i];
                uiSfxPlayer.volume = sfxVolume;
                uiSfxPlayer.Play();
                return;
            }
        }
        return;
    }

    public bool CheckObjectIsInCamera(GameObject obj)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(obj.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }

    public void SaveData()
    {
        audioMixer.GetFloat("Master", out float masterVolume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("TempMasterValume", musicMasterSlider.value);
        PlayerPrefs.SetInt("MasterMute", musicMasterToggle.isOn ? 0 : 1);

        audioMixer.GetFloat("BGM", out float bgmVolume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("TempBGMValume", musicBGMSlider.value);
        PlayerPrefs.SetInt("BGMMute", musicBGMToggle.isOn ? 0 : 1);

        audioMixer.GetFloat("SFX", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("tempSFXValume", musicSFXSlider.value);
        PlayerPrefs.SetInt("SFXMute", musicSFXToggle.isOn ? 0 : 1);

        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume");
        audioMixer.SetFloat("Master", masterVolume);
        if(PlayerPrefs.GetInt("MasterMute", 0) == 0)
        {
            musicMasterToggle.isOn = true;
            musicMasterSlider.value = PlayerPrefs.GetFloat("TempMasterValume", -10);
        }
        else
        {
            musicMasterToggle.isOn = false;
            musicMasterSlider.value = masterVolume;
        }

        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume");
        audioMixer.SetFloat("BGM", bgmVolume);
        if (PlayerPrefs.GetInt("BGMMute", 0) == 0)
        {
            musicBGMToggle.isOn = true;
            musicBGMSlider.value = PlayerPrefs.GetFloat("TempBGMValume", -10);
        }
        else
        {
            musicBGMToggle.isOn = false;
            musicBGMSlider.value = bgmVolume;
        }

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
        audioMixer.SetFloat("SFX", sfxVolume);
        if (PlayerPrefs.GetInt("SFXMute", 0) == 0)
        {
            musicSFXToggle.isOn = true;
            musicSFXSlider.value = PlayerPrefs.GetFloat("TempSFXValume", -10);
        }
        else
        {
            musicSFXToggle.isOn = false;
            musicSFXSlider.value = sfxVolume;
        }
    }
}