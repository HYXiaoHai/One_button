using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerAttake : MonoBehaviour
{
    public int playerHP = 10;

    [Header("相机震动")]
    public float duration = 0.2f;//震动时长
    public float strength = 1f;//强度
    public int vibrato = 1;//力度


    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip getDamageClip;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "button" )
        {
            //玩家受击
            PlayerGetDamage();
        }
    }

    public void PlayerGetDamage()
    {
        if(playerHP-1<0)
        {
            return;
        }
        if (playerHP - 1 == 0)
        {
            GameManage.instance.GameEnd();
        }
        else
        {
            playerHP -= 1;
            UIManage.instance.RemoveHpUi();
            GameManage.instance.mainCamera.DOShakePosition(duration, strength, vibrato, 80,true,ShakeRandomnessMode.Full);
            audioSource.PlayOneShot(getDamageClip);
        }
    }
}
