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
    private void Start()
    {
        // 添加动画：移动到目标位置并缩放至正常大小
        float animDuration = 0.3f;                       // 动画时长
        //float delay = i * 0.05f;                          // 延迟，使子弹依次出现

       transform.DOScale(transform.localScale, animDuration)
            //.SetDelay(delay)
            .SetEase(Ease.OutBack);                      // 弹性缩放效果
    }
    private void Awake()
    {
        boss = GameObject.Find("Boss").transform;
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
            GameManage.instance.attackScores += scores;
            GameManage.instance.UpdateScore();
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
