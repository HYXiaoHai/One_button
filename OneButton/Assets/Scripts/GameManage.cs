using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;
using DG.Tweening;

public enum GameState
{
    None,
    Meniu,//开始界面
    Start,//游戏开始
    End//游戏结束
}

public class GameManage : MonoBehaviour
{
    public static GameManage instance;
    public GameState gameState = GameState.None;
    public GameObject player;
    public Camera mainCamera;
    [Header("计时器")]
    public TMP_Text timeText;
    private float gameTime = 0f;//游戏进行的时间（秒）
    public int oneTimeScore = 1;
    [Header("玩家子弹")]
    public GameObject playerBullet;//子弹预制体
    public Transform playerBulletFather;//子弹预制体父级
    public int bulletCount = 3;//在场子弹最大数目
    public float bulletDuration = 5f;//生成子弹的冷却期
    private float bulletTimer = 0f;//冷却计时器

    [Header("得分")]
    public int attackScores;//攻击boss获得的分数
    public TMP_Text scoreText;
    private int finalTotalScore;//游戏结束时的总分（时间分 + 攻击分）

    [Header("排行榜")]
    [SerializeField] private List<int> leaderboardScores = new List<int>(); //存储时间（秒），降序排列
    private const string LeaderboardKey = "Leaderboard"; //PlayerPrefs键名

    private bool endPanelSkipped = false;

    private void Awake()
    {
        instance = this;
        LoadLeaderboard(); //加载排行榜数据
        playerBullet = Resources.Load<GameObject>("Models/PlayerBullet");
    }

    private void Start()
    {
        gameState = GameState.Meniu;
        Time.timeScale = 0f;
        attackScores = 0;
        finalTotalScore = 0;
        UpdateScore();

        UIManage.instance.OpenMeniu();
        UIManage.instance.CloseGamePanel();
        UIManage.instance.CloseEndPanel();
        RefreshLeaderboardUI(); //显示排行榜
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        // 菜单状态下按空格开始游戏
        if (gameState == GameState.Meniu)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                StartGame();
            }
        }
        // 游戏进行中，更新计时器
        if (gameState == GameState.Start)
        {
            UpdateTime();
            // ----- 新增：子弹生成计时器 -----
            bulletTimer += Time.deltaTime;
            if (bulletTimer >= bulletDuration)
            {
                bulletTimer = 0f;
                CreatPlayerBullet();
            }
        }
        // 游戏结束状态下处理空格
        if (gameState == GameState.End)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                if (!UIManage.instance.IsAnimationFinished())
                {
                    // 动画未结束：第一次按下，跳过动画
                    UIManage.instance.SkipAnimation();
                    endPanelSkipped = true;
                }
                else
                {
                    // 动画已结束或已跳过：第二次按下，保存并返回
                    AddScore(finalTotalScore);
                    BackMeniu();
                }
            }
        }

        // ESC 退出游戏
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            OnExit();
        }
    }

    //更新计时器文本
    public void UpdateTime()
    {
        if (timeText == null) return;
        gameTime += Time.deltaTime;
        timeText.text = gameTime.ToString("F1") + "s";
    }
    //更新攻击得分文本
    public void UpdateScore()
    {
        if (scoreText == null) return;
        scoreText.text = attackScores.ToString();
    }
    // -------------------- 玩家子弹生成 --------------------
    public void CreatPlayerBullet()
    {
        Debug.Log("生成子弹");
        float r = player.GetComponent<PlayerMove>().radius;//获得移动半径
        int count = Random.Range(1, bulletCount+1);//获得此次生成的数目
        for(int i =0;i<count;i++)
        {
            int Angle = Random.Range(0, 360);//获得随机角度
            float x = Mathf.Cos(Angle)*r;
            float y = Mathf.Sin(Angle)*r;
            Vector3 offset = new Vector3(x, y,0);
            GameObject go = Instantiate(playerBullet, playerBulletFather);
            go.transform.position = offset;//移到位置
        }
    }

    // -------------------- 游戏阶段更新 --------------------
    //开始游戏
    public void StartGame()
    {
        gameState = GameState.Start;

        Time.timeScale = 1f;
        //重置血量
        player.GetComponent<PlayerAttake>().playerHP = 5;
        UIManage.instance.InitHpUi();
        // 重置计时器
        gameTime = 0f;
        if (timeText != null) timeText.text = "0.0s";
        //重置分数
        attackScores = 0;
        finalTotalScore = 0;
        UpdateScore();

        UIManage.instance.CloseMeniu();
        UIManage.instance.OpenGamePanel();

        //生成
        bulletTimer = 0f;
    }

    //游戏结束
    public void GameEnd()
    {
        gameState = GameState.End;
        Time.timeScale = 0f;

        // 计算总分
        int timeScore = Mathf.FloorToInt(gameTime) * oneTimeScore; // 假设 oneTimeScore 为 1
        finalTotalScore = timeScore + attackScores;

        // 调用 UIManage 的动画方法（传递原始值）
        UIManage.instance.OpenEndPanelWithAnimation(gameTime, attackScores, finalTotalScore);

        // 重置跳过标志
        endPanelSkipped = false;

        UIManage.instance.CloseGamePanel();

        Debug.Log("游戏结束，总时间：" + gameTime.ToString("F1") + "秒");
    }


    //返回菜单
    public void BackMeniu()
    {
        gameState = GameState.Meniu;
        Time.timeScale = 0f;

        UIManage.instance.OpenMeniu();
        UIManage.instance.CloseGamePanel();
        UIManage.instance.CloseEndPanel();

        //刷新排行榜显示
        RefreshLeaderboardUI();
    }

    // -------------------- 排行榜功能 --------------------
    //加载排行榜数据
    private void LoadLeaderboard()
    {
        leaderboardScores.Clear();
        if (PlayerPrefs.HasKey(LeaderboardKey))
        {
            string data = PlayerPrefs.GetString(LeaderboardKey);
            if (!string.IsNullOrEmpty(data))
            {
                string[] parts = data.Split(',');
                foreach (string part in parts)
                {
                    if (int.TryParse(part, out int score))
                        leaderboardScores.Add(score);
                }
            }
        }
        // 确保降序排列
        leaderboardScores = leaderboardScores.OrderByDescending(s => s).ToList();
    }

    //保存排行榜数据到 PlayerPrefs
    private void SaveLeaderboard()
    {
        // 只保留前10名
        var top10 = leaderboardScores.Take(10).ToList();
        string data = string.Join(",", top10);
        PlayerPrefs.SetString(LeaderboardKey, data);
        PlayerPrefs.Save();
    }

    //添加新成绩，自动排序并保留前10
    public void AddScore(int newTotalScore)
    {
        leaderboardScores.Add(newTotalScore);
        // 降序排序
        leaderboardScores = leaderboardScores.OrderByDescending(s => s).ToList();
        // 只保留前10
        if (leaderboardScores.Count > 10)
            leaderboardScores = leaderboardScores.Take(10).ToList();
        SaveLeaderboard();
    }

    //重置排行榜（调试用）
    public void ResetLeaderboard()
    {
        leaderboardScores.Clear();
        SaveLeaderboard();
        RefreshLeaderboardUI();
        Debug.Log("排行榜已重置");
    }

    //刷新排行榜 UI
    private void RefreshLeaderboardUI()
    {
        if (UIManage.instance != null)
        {
            UIManage.instance.UpdateLeaderboardDisplay(leaderboardScores);
        }
    }

    //esc退出游戏
    private void OnExit()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        // 在编辑器中停止运行前恢复时间缩放（避免编辑器卡住）
        Time.timeScale = 1f;
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在构建的游戏中退出应用
        Application.Quit();
#endif
    }
}