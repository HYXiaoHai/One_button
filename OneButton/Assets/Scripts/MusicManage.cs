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
            bgmSource.loop = true;      // 默认循环播放
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
        //设置片段并重新播放
        bgmSource.clip = bgmAudioClip;
        bgmSource.Stop();//停止当前播放
        bgmSource.Play();//从头开始播放
    }
}
