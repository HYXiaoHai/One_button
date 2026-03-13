using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Boss : MonoBehaviour
{
    public static Boss instance;
    public AudioSource bossAudio;

    [Header("歌曲参数")]
    public float bpm = 154f;
    public float offset = 0f;

    [Header("弹幕")]
    public Transform bossBulletFather;
    public GameObject bossBullet;
    public float bulletSpeed =5f;
    public Vector3 bulletDirection;
    private float timer = 0f;
    public float time = 5f;
    public AudioClip bulletAudioClip;


    [Header("扇形")]
    public GameObject sectorBullet;
    public float sectorTime = 2f;//扇形攻击预警时间
    public AudioClip sectorAudioClip;


    [Header("受伤")]
    public bool isDamageAni =false;

    // 内部控制攻击状态的参数
    private float attackParameter = 0f;
    private bool isAttacking = false;

    private GameObject playerObj;
    private PlayerMove playerMove;

    private void Awake()
    {
        instance = this;
        bossBullet = Resources.Load<GameObject>("Models/BossBullet");

    }
    private void Start()
    {
        playerObj = GameManage.instance.player;
        playerMove = playerObj.GetComponent<PlayerMove>();
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer>=time&&GameManage.instance.gameState == GameState.Start && !isAttacking)
        {
            timer = 0f;
            int currentAttackState = Mathf.FloorToInt(attackParameter);
            Debug.Log($"{attackParameter}");
            //StartCoroutine(ExecuteAttackState(4));
            StartCoroutine(ExecuteAttackState(currentAttackState));
            attackParameter += Random.Range(1f, 2f);
            if (attackParameter > 5f) attackParameter %= 5f;
        }
    }

    private IEnumerator ExecuteAttackState(int state)
    {
        isAttacking = true;
        // 停止当前可能正在播放的音频
        if (bossAudio.isPlaying) bossAudio.Stop();
        // 根据状态设置并播放音频
        if (state >= 0 && state <= 3)
        {
            bossAudio.clip = bulletAudioClip;
            bossAudio.loop = true;
            bossAudio.pitch = 1f;
            bossAudio.Play();
        }
        else if (state == 4)
        {
            float warningDuration = 60f / bpm;          // 预警一拍
            float attackDuration = 60f / bpm;           // 攻击一拍
            float totalSectorDuration = warningDuration + attackDuration;
            float originalSectorLength = 2.482f;        // 给定音频长度
            bossAudio.clip = sectorAudioClip;
            bossAudio.loop = false;
            bossAudio.pitch = originalSectorLength / totalSectorDuration; // 加速
            bossAudio.Play();
        }
        switch (state)
        {
            case 0:
                Debug.Log("攻击状态0");
                int offsetDir = playerMove.direction;

                // 计算时间间隔
                float bulletInterval = 15f / bpm;
                float waitAfterFire = 90f / bpm;

                // 计算第一颗子弹朝向玩家的初始角度
                Vector3 dirToPlayer = (playerObj.transform.position - transform.position).normalized;
                float currentAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg + 45f * offsetDir;

                // 发射7颗子弹
                for (int i = 0; i < 7; i++)
                {
                    float rad = currentAngle * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0).normalized;

                    Attack();

                    if (i < 6)
                    {
                        currentAngle += 6f * offsetDir;
                        yield return new WaitForSeconds(bulletInterval);
                    }
                }

                yield return new WaitForSeconds(waitAfterFire);
                break;
            case 1:
                Debug.Log("攻击状态1");
                int offsetDir1 = playerMove.direction;

                float groupInterval = 30f / bpm;
                float waitAfterFire1 = 90f / bpm;

                Vector3 dirToPlayer1 = (playerObj.transform.position - transform.position).normalized;
                float baseAngle = Mathf.Atan2(dirToPlayer1.y, dirToPlayer1.x) * Mathf.Rad2Deg + 45f * offsetDir1;

                // 发射4组子弹
                for (int groupIdx = 0; groupIdx < 4; groupIdx++)
                {
                    // 每组发射3颗子弹
                    float rad1 = baseAngle * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad1), Mathf.Sin(rad1), 0).normalized;
                    Attack();

                    float rad2 = (baseAngle + 60f) * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad2), Mathf.Sin(rad2), 0).normalized;
                    Attack();

                    float rad3 = (baseAngle - 60f) * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad3), Mathf.Sin(rad3), 0).normalized;
                    Attack();

                    if (groupIdx < 3)
                        yield return new WaitForSeconds(groupInterval);
                }

                yield return new WaitForSeconds(waitAfterFire1);
                break;
            case 2:
                Debug.Log("攻击状态2");

                int offsetDir2 = playerMove.direction;
                float groupInterval2 = 30f / bpm;
                float waitAfterFire2 = 90f / bpm;

                Vector3 dirToPlayer2 = (playerObj.transform.position - transform.position).normalized;
                // 初始基础角度
                float baseAngle2 = Mathf.Atan2(dirToPlayer2.y, dirToPlayer2.x) * Mathf.Rad2Deg;

                // 发射4组子弹
                for (int groupIdx2 = 0; groupIdx2 < 4; groupIdx2++)
                {

                    // 每组发射4颗十字形子弹
                    float rad2_1 = baseAngle2 * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad2_1), Mathf.Sin(rad2_1), 0).normalized;
                    Attack();

                    float rad2_2 = (baseAngle2 + 90f) * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad2_2), Mathf.Sin(rad2_2), 0).normalized;
                    Attack();

                    float rad2_3 = (baseAngle2 + 180f) * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad2_3), Mathf.Sin(rad2_3), 0).normalized;
                    Attack();

                    float rad2_4 = (baseAngle2 + 270f) * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad2_4), Mathf.Sin(rad2_4), 0).normalized;
                    Attack();

                    baseAngle2 += 5f * offsetDir2;

                    if (groupIdx2 < 3)
                    {
                        yield return new WaitForSeconds(groupInterval2);
                    }
                }

                yield return new WaitForSeconds(waitAfterFire2);
                break;
            case 3:
                Debug.Log("攻击状态3");
                int offsetDir3 = playerMove.direction;
                float bulletInterval3 = 15f / bpm;
                float waitAfterFire3 = 30f / bpm;
                float[] angleOffsets = new float[] { 30f, 25f, 20f, 15f, 10f, 5f };

                Vector3 dirToPlayer3 = (playerObj.transform.position - transform.position).normalized;
                float baseAngle3 = Mathf.Atan2(dirToPlayer3.y, dirToPlayer3.x) * Mathf.Rad2Deg;

                float angleA = baseAngle3 - 90f * offsetDir3;
                float angleB = baseAngle3 + 180f * offsetDir3;

                // 发射7组子弹
                for (int groupIdx3 = 0; groupIdx3 < 7; groupIdx3++)
                {
                    float rad3_A = angleA * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad3_A), Mathf.Sin(rad3_A), 0).normalized;
                    Attack();

                    float rad3_B = angleB * Mathf.Deg2Rad;
                    bulletDirection = new Vector3(Mathf.Cos(rad3_B), Mathf.Sin(rad3_B), 0).normalized;
                    Attack();

                    yield return new WaitForSeconds(bulletInterval3);

                    if (groupIdx3 < 6)
                    {
                        angleA += angleOffsets[groupIdx3] * offsetDir3;
                        angleB -= angleOffsets[groupIdx3] * offsetDir3;
                    }
                }

                yield return new WaitForSeconds(waitAfterFire3);
                break;
            case 4:
                float warningDuration = 60f / bpm;   // 预警时长（一拍）
                float attackDuration = 60f / bpm;    // 攻击碰撞时长（一拍）

                // 随机旋转角度
                float angle = Random.Range(0, 360);
                sectorBullet.transform.rotation = Quaternion.Euler(0, 0, angle);

                // 获取组件
                SpriteRenderer srSector = sectorBullet.GetComponent<SpriteRenderer>();
                PolygonCollider2D collSector = sectorBullet.GetComponent<PolygonCollider2D>();

                // 重置扇形状态（假设之前已隐藏）
                sectorBullet.transform.localScale = Vector3.zero;
                collSector.enabled = false;

                // 快速放大出现（0.2秒内完成）
                sectorBullet.transform.DOScale(1.2f, 0.2f);

                srSector.DOFade(0.2f, 0.2f);

                //srSector.color = new Color(204 / 255f, 61 / 255f, 61 / 255f, 0.2f);

                // 预警阶段：透明度在0.3和0.6之间来回脉动，总时长 = warningDuration
                // 每半个预警时长变化一次，形成两次闪烁
                Tween fadeTween = srSector.DOFade(0.4f, warningDuration / 2f)
                                           .SetLoops(2, LoopType.Yoyo)
                                           .SetEase(Ease.Linear);
                yield return fadeTween.WaitForCompletion(); // 等待预警结束

                // 攻击阶段：开启碰撞，变为不透明
                collSector.enabled = true;
                srSector.DOFade(1f, 0.1f); // 快速变亮
                yield return new WaitForSeconds(attackDuration); // 持续一拍

                // 攻击结束：关闭碰撞，隐藏扇形
                collSector.enabled = false;
                srSector.DOFade(0f, 0.2f);
                yield return sectorBullet.transform.DOScale(0f, 0.2f).WaitForCompletion();

                break;
            default:
                break;
        }
        // 攻击结束，停止音频
        if (bossAudio.isPlaying) 
            bossAudio.Stop();
        isAttacking = false;
    }

    public void GetBulletDirection()
    {
        float r = Random.Range(0, 360);
        float x = Mathf.Cos(r);
        float y = Mathf.Sin(r);
        bulletDirection = new Vector3(x, y, 0).normalized;
    }

    //受伤
    public void GetDamage()
    {
        StartCoroutine(Damage());
        StartCoroutine(HitStop());

    }

    IEnumerator HitStop()
    {
        Camera.main.DOShakePosition(0.1f, 0.2f, 10, 90);
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
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

    //攻击
    public void Attack()
    {
        BossBullet bullet = Instantiate(bossBullet,transform.position,Quaternion.identity).GetComponent<BossBullet>();
        bullet.transform.SetParent(bossBulletFather);
        bullet.Init(bulletSpeed,bulletDirection);
    }
    //清理bullet
    public void ClearPlayerBullet()
    {
        sectorBullet.GetComponent<SpriteRenderer>().DOFade(0f, 0.1f);
        sectorBullet.GetComponent<PolygonCollider2D>().enabled = false;
        // 遍历父物体下的所有子物体并销毁
        foreach (Transform child in bossBulletFather)
        {
            Destroy(child.gameObject);
        }
    }
}
