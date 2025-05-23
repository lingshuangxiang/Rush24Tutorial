using System.Collections.Generic;
using TwentyFour.Scripts.Audio;
using TwentyFour.Scripts.Utilities;
using UnityEngine;
using Logger = TwentyFour.Scripts.Utilities.Logger;
public enum AFXMusic
{
    BattleCorrect,
    BattleDraw,
    BattleLose,
    BattleMatchMakingSuccess,
    BattleRobbed,
    BattleStarUp,
    BattleWin,
    ButtonEffectClick,
    CardClick,
    StageWin,
    WrongAnswer
        
}
public class BGMManager : GenericSingleton<BGMManager>
{
    public enum BackgroundMusic
    {
        InitBGM = 0,
        HomepageBGM = 1
    }
    
    public AudioClip InitBGM;
    public AudioClip HomepageBGM;
    
    public AudioSource audioSource;

    public AudioSource AFXSource;
    // private bool checkBGM = false;
    [SerializeField]
    public List<AudioData> AFXClips = new List<AudioData>();
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 1;
        audioSource.clip = InitBGM;
        var afx = new GameObject(nameof(AFXSource));
        afx.transform.parent = transform;
        AFXSource = afx.AddComponent<AudioSource>();
        AFXSource.playOnAwake = false;
        AFXSource.volume = 1;
        DontDestroyOnLoad(gameObject);
        if (LocalStorageUtil.HasKey(LocalStorageKeys.BGM_ON))
        {
            EnableBGM(LocalStorageUtil.GetInt(LocalStorageKeys.BGM_ON) == 1);
        }
        else
        {
            EnableBGM(true);
        }

        if (LocalStorageUtil.HasKey(LocalStorageKeys.AFX_ON))
        {
            EnableAFX(LocalStorageUtil.GetInt(LocalStorageKeys.AFX_ON) == 1);
        }
        else
        {
            EnableAFX(true);
        }
    }

    public void Play()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void PlayAFX(AFXMusic music)
    {
        if (AFXSource == null)
        {
            Logger.LogError("AFXSource component not found");
            return;
        }

        if ((int)music < AFXClips.Count)
        {
            var data = AFXClips[(int)music];
            AFXSource.volume = data.Volume;
            AFXSource.clip = data.Clip;
            AFXSource.loop = false;
            AFXSource.Play();
        }
    }
    public void Play(BackgroundMusic bgm)
    {
        if (audioSource == null)
        {
            Logger.LogError("Audio Source component not found");
            return;
        }
        
        switch (bgm)
        {
            case BackgroundMusic.InitBGM:
                audioSource.clip = InitBGM;
                audioSource.volume = .7f;
                break;
            case BackgroundMusic.HomepageBGM:
                audioSource.clip = HomepageBGM;
                audioSource.volume = .15f;
                break;
        }
        
        Play();
    }

    public void EnableBGM(bool isOn)
    {
        audioSource.mute = !isOn;
        LocalStorageUtil.SetInt(LocalStorageKeys.BGM_ON, isOn ? 1 : 0);
    }

    public void EnableAFX(bool isOn)
    {
        AFXSource.mute = !isOn;
        LocalStorageUtil.SetInt(LocalStorageKeys.AFX_ON, isOn ? 1 : 0);
    }
    public void Stop()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (!checkBGM && Time.deltaTime > .5)
        // {
        //     AudioSource audioSource = GetComponent<AudioSource>();
        //     if (audioSource != null && !audioSource.isPlaying)
        //     {
        //         audioSource.Play();
        //     }
        //     checkBGM = true;
        // }
    }
}
