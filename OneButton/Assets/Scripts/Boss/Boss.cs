using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Boss : MonoBehaviour
{
    public static Boss instance;

    [Header("µØƒª")]
    public GameObject bossBullet;
    public float bulletSpeed =5f;
    public Vector3 bulletDirection;
    private float timer = 0f;
    public float time = 5f;

    [Header(" ‹…À")]
    public bool isDamageAni =false;
    private void Awake()
    {
        instance = this;
        bossBullet = Resources.Load<GameObject>("Models/BossBullet");
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer>=time&&GameManage.instance.gameState == GameState.Start)
        {
            timer = 0f;
            GetBulletDirection();
            Attect();
        }
    }
    public void GetBulletDirection()
    {
        float r = Random.Range(0, 361);
        float x = Mathf.Cos(r);
        float y = Mathf.Sin(r);
        bulletDirection = new Vector3(x, y, 0).normalized;
    }
    // ‹…À
    public void GetDamage()
    {
        StartCoroutine(Damage());
    }
    IEnumerator Damage()
    {
        if (isDamageAni)
        {
            yield break;
        }
        if(isDamageAni==false)
        {
            isDamageAni = true;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color color = sr.color;
            sr.color = new Color(255 / 255f, 125 / 255f, 125 / 255f, 1);
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(color.r, color.g, color.b, 1);
            isDamageAni=false;
        }
    }
    //π•ª˜
    public void Attect()
    {
        BossBullet bullet = Instantiate(bossBullet,transform.position,Quaternion.identity).GetComponent<BossBullet>();
        bullet.Init(bulletSpeed,bulletDirection);
    }
}
