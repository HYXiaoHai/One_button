using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerAttake : MonoBehaviour
{
    public SpriteRenderer sr;
    public Color oldColor;
    [Header("血量")]
    public int maxHP = 5;
    public int playerHP = 5;
    public float hitCooldown =1f;//受击冷却
    private float hitTime =0f;
    private bool canGetDamage = true;
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

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        oldColor = sr.color;
    }

    private void Update()
    {
        if(!canGetDamage)
        {
            hitTime += Time.deltaTime;
            if(hitTime>=hitCooldown)
            {
                canGetDamage = true;
            }
        }
    }

    public void PlayerGetDamage()
    {
        if(playerHP-1<0|| !canGetDamage)
        {
            return;
        }
        if (playerHP - 1 == 0)
        {
            audioSource.PlayOneShot(getDamageClip);
            UIManage.instance.RemoveHpUi();
            GameManage.instance.GameEnd();
        }
        else
        {
            canGetDamage = false;
            hitTime = 0f;
            StartCoroutine(GetDamage());
            playerHP -= 1;
            UIManage.instance.RemoveHpUi();
            UIManage.instance.Injured();
            //GameManage.instance.mainCamera.DOShakePosition(duration, strength, vibrato, 80,true,ShakeRandomnessMode.Full);
            audioSource.PlayOneShot(getDamageClip);
        }
    }

    public void ResetPlayerHP()
    {
        //重置血量（使用与 GameManage 一致的初始值，建议用变量管理）
        playerHP = maxHP;          // 与 GameManage.StartGame 中设置的值保持一致
        canGetDamage = true;
        hitTime = 0f;

        //恢复颜色（如果之前被隐藏或闪烁）
        if (sr != null)
        {
            sr.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
        }
    }

    IEnumerator GetDamage()
    {
        //sr.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0f);
        sr.color = new Color(1, 98/255f, 0, 1f);
        yield return new WaitForSeconds(0.1f);
        sr.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
        yield return new WaitForSeconds(0.1f);
        //sr.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0f);
        sr.color = new Color(1, 98 / 255f, 0, 1f);
        yield return new WaitForSeconds(0.1f);
        sr.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
        yield return new WaitForSeconds(0.1f);
        //sr.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0f);
        sr.color = new Color(1, 98 / 255f, 0, 1f);
        yield return new WaitForSeconds(0.1f);
        sr.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
    }
}
