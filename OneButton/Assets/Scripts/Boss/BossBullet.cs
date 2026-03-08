using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed;
    public Vector3 dir;

    private void Update()
    {
        Move();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("boundary"))
        {
            Destroy(gameObject);
        }
    }
    public void Move()
    {
        transform.Translate(dir*speed*Time.deltaTime);
    }
    public void Init(float sp,Vector3 d)
    {
        speed = sp;
        dir = d;
    }
}
