using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SoundManager : NetworkBehaviour
{
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

    private float bgmVolume = .3f;
    private float sfxVolume = .3f;

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

    public bool isHostMapBattleOn = false;
    public bool isClientMapBattleOn = false;
    public bool isHostMapWaveOn = false;
    public bool isClientMapWaveOn = false;

    float fadeSeconds = 1.0f;

    //Coroutine bgmFadeIn;
    //Coroutine bgmFadeOut;

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

        //임시로 소리끄기
        audioMixer.SetFloat("Master", -80);
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
                break;
            case 1:
                bgmPlayer.clip = audioClipRefsSO.inGameBgm[Random.Range(0, audioClipRefsSO.inGameBgm.Length)];
                bgmPlayer.Play();
                break;
        }
        // 씬 로드 시 실행할 코드
    }

    private void OnSceneUnloaded(Scene scene)
    {
        mainCamera = null;
        // 씬 언로드 시 실행할 코드
    }

    void Update()
    {
        if (!bgmPlayer.isPlaying)
        {
            PlayBgmMapCheck();
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
    }

    #endregion

    void SFXPlayerSet()
    {
        BgmPlayerSet();
        StructureSFXPlayerSet();
        UnitSFXPlayerSet();
        UIPlayerSet();
    }

    public void PlayBgmMapCheck()
    {
        if (GameManager.instance != null)
        {
            if (GameManager.instance.isPlayerInHostMap)
            {
                PlayBGM(isHostMapBattleOn, isHostMapWaveOn);
            }
            else
            {
                PlayBGM(isClientMapBattleOn, isClientMapWaveOn);
            }
        }
    }

    void BgmPlayerSet()
    {
        bgmPlayer = PlayerBaseSet("BGM");
        bgmPlayer.clip = audioClipRefsSO.inGameBgm[Random.Range(0, audioClipRefsSO.inGameBgm.Length)];
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

    void PlayBGM(bool isBattle, bool isWave)
    {
        if (isWave)
        {
            bgmPlayer.clip = audioClipRefsSO.waveBgm[Random.Range(0, audioClipRefsSO.waveBgm.Length)];
        }
        else if (isBattle)
        {
            bgmPlayer.clip = audioClipRefsSO.battleBgm[Random.Range(0, audioClipRefsSO.battleBgm.Length)];
        }
        else
        {
            bgmPlayer.clip = audioClipRefsSO.inGameBgm[Random.Range(0, audioClipRefsSO.inGameBgm.Length)];
        }

        bgmPlayer.Play();
        StartCoroutine(nameof(SoundFadeIn));
    }

    [ServerRpc]
    public void BattleStateSetServerRpc(bool isBattle, bool isWave, bool isHostMap)
    {
        BattleStateSetClientRpc(isBattle, isWave, isHostMap);
    }

    [ClientRpc]
    public void BattleStateSetClientRpc(bool isBattle, bool isWave, bool isHostMap)
    {
        if (GameManager.instance.isPlayerInHostMap && isHostMap)
        {
            isHostMapBattleOn = isBattle;
            isHostMapWaveOn = isWave;
            StartCoroutine(nameof(SoundFadeOut));

        }
        else if(!GameManager.instance.isPlayerInHostMap && !isHostMap)
        {
            isClientMapBattleOn = isBattle;
            isClientMapWaveOn = isWave;
            StartCoroutine(nameof(SoundFadeOut));
        }
    }

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
        float time = 0.0f;

        while (time < fadeSeconds)
        {
            time += Time.deltaTime;

            bgmPlayer.volume = bgmVolume * ((fadeSeconds - time) / fadeSeconds);

            yield return null;
        }

        bgmPlayer.volume = 0;
        PlayBgmMapCheck();
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
                    SFXAudioSet(structureSfxPlayer, audioClipRefsSO.structureSfx, sfxName);
                    structureSfxPlay = true;
                }
                break;
            case "unitSFX":
                {
                    if (unitSfxPlay)
                        return;
                    SFXAudioSet(unitSfxPlayer, audioClipRefsSO.unitSfx, sfxName);
                    unitSfxPlay = true;
                }
                break;
            default :
                return;
        }
    }

    void SFXAudioSet(List<AudioSource> audioList, AudioClip[] audioClips, string sfxName)
    {
        int playAudioCount = 0;
        float soundValue = 1;

        for (int j = 0; j < audioList.Count; j++)
        {
            if (audioList[j].isPlaying)
            {
                playAudioCount++;
            }
        }

        if (playAudioCount < 3)
            soundValue = 1.2f;
        else if (playAudioCount < 6)
            soundValue = 2.5f;
        else if (playAudioCount < 12)
            soundValue = 3.5f;

        for (int i = 0; i < audioClips.Length; i++)
        {
            if (sfxName == audioClips[i].name)
            {
                for (int j = 0; j < audioList.Count; j++)
                {
                    if (!audioList[j].isPlaying)
                    {
                        audioList[j].clip = audioClips[i];
                        audioList[j].volume = sfxVolume / soundValue;
                        audioList[j].Play();
                        return;
                    }
                }
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
            musicMasterSlider.value = PlayerPrefs.GetFloat("TempMasterValume", 0);
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
            musicBGMSlider.value = PlayerPrefs.GetFloat("TempBGMValume", 0);
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
            musicSFXSlider.value = PlayerPrefs.GetFloat("TempSFXValume", 0);
        }
        else
        {
            musicSFXToggle.isOn = false;
            musicSFXSlider.value = sfxVolume;
        }
    }
}