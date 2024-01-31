using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    public static SoundManager Instance { get; private set; }

    [SerializeField] 
    private AudioClipRefsSO audioClipRefsSO;

    private AudioSource audioSource;
    private float bgmVolume = .3f;
    private float sfxVolume = .3f;

    //int bgmIndex;
    //int bgmCount;

    [SerializeField]
    List<AudioSource> sfxPlayer;
    [SerializeField]
    int sfxPlayerMaxCount;

    [SerializeField]
    Camera mainCamera;

    private void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();

        audioSource.volume = bgmVolume;
        SfxPlayerSet();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        //bgmCount = audioClipRefsSO.bgm.Length;
        //bgmIndex = 0;
        BgmPlaySound();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            //bgmIndex++;
            //if (bgmIndex >= bgmCount)
            //{
            //    bgmIndex = 0;
            //}

            //audioSource.clip = audioClipRefsSO.bgm[bgmIndex];
            BgmPlaySound();
        }
    }

    void BgmPlaySound()
    {
        audioSource.clip = audioClipRefsSO.bgm[Random.Range(0, audioClipRefsSO.bgm.Length)];
        audioSource.volume = bgmVolume;
        audioSource.Play();
    }

    void SfxPlayerSet()
    {
        for (int i = 0; i < sfxPlayerMaxCount; i++) 
        {
            AudioSource audio = gameObject.AddComponent<AudioSource>();
            audio.volume = sfxVolume;
            sfxPlayer.Add(audio);
        }
    }

    public void PlaySFX(GameObject obj, string p_sfxName)
    {
        if (!CheckObjectIsInCamera(obj))
            return;

        for (int i = 0; i < audioClipRefsSO.sfx.Length; i++)
        {
            if (p_sfxName == audioClipRefsSO.sfx[i].name)
            {
                for (int j = 0; j < sfxPlayer.Count; j++)
                {
                    // SFXPlayer에서 재생 중이지 않은 Audio Source를 발견했다면 
                    if (!sfxPlayer[j].isPlaying)
                    {
                        sfxPlayer[j].clip = audioClipRefsSO.sfx[i];
                        sfxPlayer[j].volume = sfxVolume;
                        sfxPlayer[j].Play();
                        return;
                    }
                }
                Debug.Log("모든 오디오 플레이어가 재생중입니다.");
                return;
            }
        }
        Debug.Log(p_sfxName + " 이름의 효과음이 없습니다.");
        return;
    }

    public bool CheckObjectIsInCamera(GameObject obj)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(obj.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        return onScreen;
    }
}