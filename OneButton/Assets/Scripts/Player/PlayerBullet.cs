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

            Destroy(gameObject);
        }
        if (other.tag=="Player")
        {
            canMove = true;
        }
    }

    void MoveToBoss()
    {
        Vector3 dir = (boss.position - transform.position).normalized;
        transform.Translate(dir*moveSpeed*Time.deltaTime);
    }
}
