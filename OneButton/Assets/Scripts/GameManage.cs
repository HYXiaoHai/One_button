using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    None,
    Meniu,//开始界面
    Start,//游戏开始
    Stop,//游戏暂停
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

    [Header("得分弹出")]
    public TMP_Text addScoreText;
    public float addScoreCold = 0.5f;//加分冷却
    private int pendingTotalScore = 0;//冷却期内累计的待加分数
    private Coroutine pendingScoreCoroutine;//冷却等待协程
    private Coroutine scoreAnimCoroutine;//分数动画协程
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
        // 处理游戏状态相关的按键
        switch (gameState)
        {
            case GameState.Meniu:
                if (keyboard.spaceKey.wasPressedThisFrame)
                    StartGame();
                if (keyboard.escapeKey.wasPressedThisFrame)
                    OnExit(); // 菜单界面直接退出
                break;

            case GameState.Start:
                //游戏进行中的逻辑（计时、子弹生成等）
                UpdateTime();
                bulletTimer += Time.deltaTime;
                if (bulletTimer >= bulletDuration)
                {
                    bulletTimer = 0f;
                    CreatPlayerBullet();
                }
                //按下 ESC 暂停游戏
                if(keyboard.escapeKey.wasPressedThisFrame)
                    GameStop();
                break;

            case GameState.Stop:
                //暂停状态：按 ESC 恢复游戏，其他键忽略
                if(keyboard.escapeKey.wasPressedThisFrame)
                    OnExit();//esc退出游戏
                if(keyboard.spaceKey.wasPressedThisFrame)
                    ResumeGame();//调用恢复方法
                break;

            case GameState.End:
                //结束状态：空格跳过动画或返回菜单，ESC 保存并退出
                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    if (!UIManage.instance.IsAnimationFinished())
                    {
                        UIManage.instance.SkipAnimation();
                        endPanelSkipped = true;
                    }
                    else
                    {
                        AddScore(finalTotalScore);
                        BackMeniu();
                    }
                }
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    AddScore(finalTotalScore); // 保存分数
                    OnExit();
                }
                break;
        }
        //if (gameState == GameState.Meniu)
        //{
        //    if (keyboard.spaceKey.wasPressedThisFrame)
        //    {
        //        StartGame();
        //    }
        //}
        //// 游戏进行中，更新计时器
        //if (gameState == GameState.Start)
        //{
        //    UpdateTime();
        //    // ----- 新增：子弹生成计时器 -----
        //    bulletTimer += Time.deltaTime;
        //    if (bulletTimer >= bulletDuration)
        //    {
        //        bulletTimer = 0f;
        //        CreatPlayerBullet();
        //    }
        //}
        //// 游戏结束状态下处理空格
        //if (gameState == GameState.End)
        //{
        //    if (keyboard.spaceKey.wasPressedThisFrame)
        //    {
        //        if (!UIManage.instance.IsAnimationFinished())
        //        {
        //            // 动画未结束：第一次按下，跳过动画
        //            UIManage.instance.SkipAnimation();
        //            endPanelSkipped = true;
        //        }
        //        else
        //        {
        //            // 动画已结束或已跳过：第二次按下，保存并返回
        //            AddScore(finalTotalScore);
        //            BackMeniu();
        //        }
        //    }
        //}

        ////退出游戏功能
        //if ( gameState == GameState.Stop)
        //{
        //    if(keyboard.escapeKey.wasPressedThisFrame)
        //    {
        //        //暂停模式下再次摁esc
        //        //关闭面板 继续游戏
        //        gameState = GameState.Start;
        //        Time.timeScale = 1f;//开始
        //        UIManage.instance.CloseStopPanel();//弹出暂停面板
        //    }
        //    else if(keyboard.spaceKey.wasPressedThisFrame)
        //    {
        //        //按下空格键执行退出游戏机制
        //        //删除本局数据

        //        //退出
        //        OnExit();
        //    }
        //}
        //// ESC 退出游戏
        //if (keyboard.escapeKey.wasPressedThisFrame&&gameState ==GameState.Start)
        //{
        //    //游戏模式下
        //    GameStop();
        //}
        //else if (keyboard.escapeKey.wasPressedThisFrame &&( gameState == GameState.End|| gameState == GameState.Meniu))
        //{
        //    //结束模式退出，保存当前数据，然后直接退出
        //    if (gameState == GameState.End)
        //    {
        //        AddScore(finalTotalScore);
        //        OnExit();
        //    }
        //    //meniu模式退出，直接退出
        //    if(gameState == GameState.Meniu)
        //    {
        //        OnExit();
        //    }
        //}
    }

    //更新计时器文本
    public void UpdateTime()
    {
        if (timeText == null) return;
        gameTime += Time.deltaTime;
        timeText.text = gameTime.ToString("F1") + "s";
    }

    //更新攻击得分文本
    public void AddAttackScore(int addScore)
    {
        //如果已有动画正在执行，立即停止（因为我们要重新累计）
        if (scoreAnimCoroutine != null)
        {
            StopCoroutine(scoreAnimCoroutine);
            scoreAnimCoroutine = null;
        }
        //累加分数
        
        pendingTotalScore += addScore;
        //动画
        //1.缩放脉冲：先放大到 1.5 倍，然后回弹
        addScoreText.transform.DOPunchScale(Vector3.one * 0.5f, 0.2f, 5, 1f);
        //2.向上脉冲：向上移动 30 像素并回弹
        addScoreText.transform.DOPunchPosition(Vector3.up * 15f, 0.2f, 5, 1f);
        //3.透明度脉冲：快速闪一下（前提是文本本身支持透明度）
        addScoreText.DOFade(0.8f, 0.1f).From(1f).SetLoops(2, LoopType.Yoyo);
        // ---------------------------------

        //更新显示文本（显示当前累计值）
        addScoreText.text = "+" + pendingTotalScore.ToString();
        addScoreText.enabled = true;

        //重置冷却等待协程（如果已有，先取消）
        if (pendingScoreCoroutine != null)
        {
            StopCoroutine(pendingScoreCoroutine);
        }
        //开始新的冷却等待
        pendingScoreCoroutine = StartCoroutine(WaitForCoolDownAndAnimate());
    }

    IEnumerator WaitForCoolDownAndAnimate()
    {
        //等待冷却时间
        yield return new WaitForSeconds(addScoreCold);

        //冷却结束，如果还有待加分数，则开始动画
        if (pendingTotalScore > 0)
        {
            scoreAnimCoroutine = StartCoroutine(AnimateScoreAddition());
        }
        pendingScoreCoroutine = null; //冷却协程结束
    }

    IEnumerator AnimateScoreAddition()
    {
        while (pendingTotalScore > 0)
        {
            pendingTotalScore--;
            attackScores++;
            addScoreText.text = "+" + pendingTotalScore.ToString();
            UpdateScore();
            yield return new WaitForSeconds(0.1f);
        }

        addScoreText.enabled = false;
        scoreAnimCoroutine = null;
    }

    
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
        int count = UnityEngine.Random.Range(1, bulletCount+1);//获得此次生成的数目
        for(int i =0;i<count;i++)
        {
            int Angle = UnityEngine.Random.Range(0, 360);//获得随机角度
            float x = Mathf.Cos(Angle)*r;
            float y = Mathf.Sin(Angle)*r;
            Vector3 offset = new Vector3(x, y,0);
            GameObject go = Instantiate(playerBullet, playerBulletFather);
            go.transform.position = offset;//移到位置
            //go.transform.SetParent(playerBulletFather);
        }
    }
    public void ClearPlayerBullet()
    {
        // 遍历父物体下的所有子物体并销毁
        foreach (Transform child in playerBulletFather)
        {
            Destroy(child.gameObject);
        }
    }

    // -------------------- 游戏阶段更新 --------------------
    //开始游戏
    public void StartGame()
    {
        gameState = GameState.Start;

        Time.timeScale = 1f;
        //重置玩家移动状态（方向、速度等）
        player.GetComponent<PlayerMove>().ResetPlayer();
        //重置玩家血量状态（血量、受击冷却、显示）
        player.GetComponent<PlayerAttake>().ResetPlayerHP();
        //开启拖尾
        player.GetComponent<PlayerMove>().playerTril.enabled = true;
        //重置 UI 血量显示
        UIManage.instance.InitHpUi();
        //重置计时器
        gameTime = 0f;
        if (timeText != null) timeText.text = "0.0s";
        //重置分数
        attackScores = 0;
        finalTotalScore = 0;
        UpdateScore();
        // 重置得分累计动画状态
        pendingTotalScore = 0;
        if (pendingScoreCoroutine != null) { StopCoroutine(pendingScoreCoroutine); pendingScoreCoroutine = null; }
        if (scoreAnimCoroutine != null) { StopCoroutine(scoreAnimCoroutine); scoreAnimCoroutine = null; }
        addScoreText.enabled = false;
        //重置音乐
        //MusicManage.instance.Replay();
        //重置玩家子弹
        ClearPlayerBullet();
        //更改ui
        UIManage.instance.CloseMeniu();
        UIManage.instance.OpenGamePanel();

        //生成
        bulletTimer = 0f;
    }

    //游戏结束
    public void GameEnd()
    {
        gameState = GameState.End;
        //Time.timeScale = 0f;
        //关闭拖尾
        player.GetComponent<PlayerMove>().playerTril.enabled = false;
        //隐藏玩家
        player.SetActive(false);

        //计算总分
        int timeScore = Mathf.FloorToInt(gameTime) * oneTimeScore; //oneTimeScore为1
        finalTotalScore = timeScore + attackScores;

        //调用 UIManage 的动画方法（传递原始值）
        UIManage.instance.OpenEndPanelWithAnimation(gameTime, attackScores, finalTotalScore);

        //重置跳过标志
        endPanelSkipped = false;

        UIManage.instance.CloseGamePanel();

        Debug.Log("游戏结束，总时间：" + gameTime.ToString("F1") + "秒");
    }
    //游戏暂停
    public void GameStop()
    {
        gameState = GameState.Stop;
        Time.timeScale = 0f;//暂停
        UIManage.instance.OpenStopPanel();//弹出暂停面板

    }
    //返回游戏
    public void ResumeGame()
    {
        gameState = GameState.Start;
        Time.timeScale = 1f;
        UIManage.instance.CloseStopPanel(); // 需要你在 UIManage 中实现
                                            // 可选：重新启用玩家输入（如果 PlayerMove 的 Action Map 是动态启用的，无需额外操作）
    }
    //返回菜单
    public void BackMeniu()
    {
        //获取圆心和半径
        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        float radius = playerMove.radius;
        Vector3 center = playerMove.centerPoint.position; // 圆心世界坐标

        //1.重置玩家位置到圆心正上方
        player.transform.position = center + new Vector3(0, radius, 0);
        player.SetActive(true);

        //2.重置玩家移动状态（方向、速度等）
        player.GetComponent<PlayerMove>().ResetPlayer();

        //3.重置玩家血量状态（血量、受击冷却、显示）
        player.GetComponent<PlayerAttake>().ResetPlayerHP();
        //4.切换游戏状态和 UI
        gameState = GameState.Meniu;
        Time.timeScale = 0f;
        //重置玩家子弹
        ClearPlayerBullet();
        //重置得分累计动画状态（与 StartGame 相同）
        pendingTotalScore = 0;
        if (pendingScoreCoroutine != null) { StopCoroutine(pendingScoreCoroutine); pendingScoreCoroutine = null; }
        if (scoreAnimCoroutine != null) { StopCoroutine(scoreAnimCoroutine); scoreAnimCoroutine = null; }
        addScoreText.enabled = false;

        //重置音乐
        MusicManage.instance.Replay();
        //ui
        UIManage.instance.OpenMeniu();
        UIManage.instance.CloseGamePanel();
        UIManage.instance.CloseEndPanel();
        UIManage.instance.CloseHighlightBoard();
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
            UIManage.instance.UpdateLeaderboardDisplay(leaderboardScores,finalTotalScore);
        }
    }

    //esc退出游戏
    public void OnExit()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        Time.timeScale = 1f;
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在构建的游戏中退出应用
        Application.Quit();
#endif
    }
}