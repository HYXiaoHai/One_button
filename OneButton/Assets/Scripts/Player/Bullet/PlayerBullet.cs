using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class PlayerBullet : MonoBehaviour
{
    public float moveSpeed = 5;//移动速度
    public Transform boss;
    public int scores = 5;//得分
    public bool canMove;

    public AudioClip audio;
    public AudioSource audioSource;
   
    private void Start()
    {
        float animDuration = 0.3f;                     

       transform.DOScale(transform.localScale, animDuration)
            //.SetDelay(delay)
            .SetEase(Ease.OutBack);                    
    }
    private void Awake()
    {
        boss = GameObject.Find("Boss").transform;
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if(canMove)
        {
            MoveToBoss();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Boss")//攻击boss
        {
            Boss.instance.GetDamage();
            //GameManage.instance.attackScores += scores;
            Debug.Log("子弹碰撞" + scores);
            GameManage.instance.AddAttackScore(scores);
            audioSource.PlayOneShot(audio);
            StartCoroutine(DestroyBullet());
        }
        if (other.tag=="Player")
        {
            canMove = true;
        }
    }

    IEnumerator DestroyBullet()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f); // 设置不透明
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void MoveToBoss()
    {
        Vector3 dir = (boss.position - transform.position).normalized;
        transform.Translate(dir*moveSpeed*Time.deltaTime);
    }
}
