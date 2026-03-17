using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicManage : MonoBehaviour
{
    public static MusicManage instance;
    public AudioClip bgmAudioClip;
    public AudioSource bgmSource;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (bgmAudioClip != null)
        {
            bgmSource.clip = bgmAudioClip;
            bgmSource.loop = false;      // 默认循环播放
            bgmSource.Play();
        }
    }
    //重新播放
    public void Replay()
    {
        if (bgmAudioClip == null)
        {
            return;
        }
        if(GameManage.instance.gameState==GameState.Meniu)
        {
            bgmSource.loop = true;
        }
        else
        {
            bgmSource.loop = false;
        }
        bgmSource.Stop();
        bgmSource.clip = bgmAudioClip;
        bgmSource.Play();
    }
    // 暂停 BGM
    public void Pause()
    {
        if (bgmSource.isPlaying)
            bgmSource.Pause();
    }

    // 恢复 BGM
    public void Resume()
    {
        if (!bgmSource.isPlaying && bgmSource.clip != null)
            bgmSource.UnPause();
    }
}
