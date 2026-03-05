using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttake : MonoBehaviour
{
    public int playerHP = 5;

    [Header("“Ù–ß")]
    public AudioSource audioSource;
    public AudioClip getDamageClip;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "button" )
        {
            //ÕÊº“ ‹ª˜
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
            audioSource.PlayOneShot(getDamageClip);
        }
    }
}
