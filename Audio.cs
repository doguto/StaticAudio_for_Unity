using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Audio;

public class Audio: Singleton<Audio>
{
    //WARNING => This is a "Static" class.
    [System.Serializable]
    public class BGMInfo
    {
        public AudioClip clip;
        public string name;
        public int loopEndTime;
        public int loopLength;
    }

    [System.Serializable]
    public class SEInfo
    {
        public AudioClip clip;
        public string name;
    }

    //Inspector操作用
    [SerializeField] AudioMixer _audioMixer;
    [SerializeField] AudioSource _bgmSource;
    [SerializeField] AudioSource _seSource;
    [SerializeField] BGMInfo[] _bgmInfos;
    [SerializeField] SEInfo[] _seInfos;

    //static関数内使用
    public static AudioMixer audioMixer;
    public static AudioSource bgmSource;
    public static AudioSource seSource;
    public static BGMInfo[] bgmInfos;
    public static SEInfo[] seInfos;
    public static Dictionary<string, int> bgmIndexDictionary;
    public static Dictionary<string, AudioClip> seClipDictionary;

    //BGMLoop check用変数
    private static string _name;

    //Fade In/Out用変数
    private static float _fadeSeconds = 0.5f; //Fadeにかかる時間
    private static float _fadeing = 0;
    private static float _nowVolume = 0;

    public override void Awake()
    {
        Instantiate();
        RemoveDuplicates(); //Singleton
    }

    public void Update()
    {
        CheckBGMLoop();
    }


    private void Instantiate()
    {
        bgmInfos = new BGMInfo[_bgmInfos.Length];
        seInfos = new SEInfo[_seInfos.Length];

        bgmIndexDictionary = new Dictionary<string, int>();
        seClipDictionary = new Dictionary<string, AudioClip>();

        audioMixer = _audioMixer;
        bgmSource = _bgmSource;
        seSource = _seSource;

        for (int i = 0; i < _bgmInfos.Length; i++)
        {
            bgmInfos[i] = _bgmInfos[i];
            bgmIndexDictionary[_bgmInfos[i].name] = i;
        }
        for (int i = 0; i < _seInfos.Length; i++)
        {
            seInfos[i] = _seInfos[i];
            seClipDictionary[_seInfos[i].name] = _seInfos[i].clip;
        }
    }

    //SEPlay用関数
    public static void SEPlayOneShot(string name)
    {
        seSource.PlayOneShot(seClipDictionary[name]);
    }

    public static void BGMPlayLoop(string name)
    {
        bgmSource.loop = true;
        bgmSource.clip = bgmInfos[bgmIndexDictionary[name]].clip;
        bgmSource.Play();

        _name = name;
    }

    void CheckBGMLoop()
    {
        int index = bgmIndexDictionary[_name];
        //流れてる音楽のSample数がEndの時刻を越えたらloopの長さだけ戻す
        if (_bgmSource.timeSamples >= bgmInfos[index].loopEndTime)
        {
            _bgmSource.timeSamples -= bgmInfos[index].loopLength;
        }
    }
    
    //Pause関数群
    public static void BGMPause()
    {
        bgmSource.Pause();
    }
    public static void SEPause()
    {
        seSource.Pause();
    }

    //UnPose関数群
    public static void BGMUnPause()
    {
        bgmSource.UnPause();
    }
    public static void SEUnPose()
    {
        seSource.UnPause();
    }

    //BGM音量調整関数
    public static void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGM", volume);
    }

    //SE音量調整関数
    public static void SetSEVolume(float volume)
    {
        audioMixer.SetFloat("SE", volume);
    }

    //BGM設定関数
    public static void BGMSetter(int index)
    {
        audioMixer.GetFloat("BGM", out float nowvol);
        BGMFadeOut(nowvol);
        bgmSource.clip = bgmInfos[index].clip;
    }

    //FadeOut関数
    private static void BGMFadeOut(float nowVolume)
    {
        _fadeing = 0;
        Audio._nowVolume = nowVolume;
        while (_fadeing < _fadeSeconds)
        {
            _fadeing += Time.deltaTime;
            SetBGMVolume(nowVolume * (1 - _fadeing / _fadeSeconds));
        }
        SetBGMVolume(0);
    }

    //FadeIn関数
    private static void BGMFadeIn(float now)
    {
        _fadeing = 0;
        _nowVolume = now;
        while (_fadeing < _fadeSeconds)
        {
            _fadeing += Time.deltaTime;
            SetBGMVolume(now * (_fadeing / _fadeSeconds));
        }
        SetBGMVolume(_nowVolume);
    }
}
