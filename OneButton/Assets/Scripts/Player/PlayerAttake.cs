using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttake : MonoBehaviour
{
    public int playerHP = 3;

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip getDamageClip;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("受伤");
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
            audioSource.PlayOneShot(getDamageClip);
        }
    }
}
