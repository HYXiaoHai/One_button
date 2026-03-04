using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq; // 用于排序

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

    [Header("计时器")]
    public TMP_Text timeText;
    private float gameTime = 0f;//游戏进行的时间（秒）

    [Header("排行榜")]
    [SerializeField] private List<float> leaderboardScores = new List<float>(); // 存储时间（秒），降序排列
    private const string LeaderboardKey = "Leaderboard"; // PlayerPrefs 键名

    private void Awake()
    {
        instance = this;
        LoadLeaderboard(); //加载排行榜数据
    }

    private void Start()
    {
        gameState = GameState.Meniu;
        Time.timeScale = 0f;

        UIManage.instance.OpenMeniu();
        UIManage.instance.CloseGamePanel();
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

    //开始游戏
    public void StartGame()
    {
        gameState = GameState.Start;
        player.GetComponent<PlayerAttake>().playerHP = 3;

        Time.timeScale = 1f;

        // 重置计时器
        gameTime = 0f;
        if (timeText != null) timeText.text = "0.0s";

        UIManage.instance.CloseMeniu();
        UIManage.instance.OpenGamePanel();
    }

    //游戏结束
    public void GameEnd()
    {
        gameState = GameState.End;
        Time.timeScale = 0f;
        Debug.Log("游戏结束，总时间：" + gameTime.ToString("F1") + "秒");

        // 将本次游戏时间加入排行榜
        AddScore(gameTime);

        BackMeniu();
    }

    //返回菜单
    public void BackMeniu()
    {
        gameState = GameState.Meniu;
        Time.timeScale = 0f;

        UIManage.instance.OpenMeniu();
        UIManage.instance.CloseGamePanel();

        //刷新排行榜显示
        RefreshLeaderboardUI();
    }

    // -------------------- 排行榜功能 --------------------
    //加载排行榜数据（从 PlayerPrefs）
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
                    if (float.TryParse(part, out float score))
                        leaderboardScores.Add(score);
                }
            }
        }
        //确保降序排列
        leaderboardScores = leaderboardScores.OrderByDescending(s => s).ToList();
    }

    //保存排行榜数据到 PlayerPrefs
    private void SaveLeaderboard()
    {
        // 只保留前10名
        var top10 = leaderboardScores.Take(10).ToList();
        string data = string.Join(",", top10.Select(s => s.ToString("F1")));
        PlayerPrefs.SetString(LeaderboardKey, data);
        PlayerPrefs.Save();
    }

    //添加新成绩，自动排序并保留前10
    public void AddScore(float newTime)
    {
        leaderboardScores.Add(newTime);
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