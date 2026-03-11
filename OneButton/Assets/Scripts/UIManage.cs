using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManage : MonoBehaviour
{
    public static UIManage instance;

    public CanvasGroup meniuPanel;
    [Header("游戏面板")]
    public CanvasGroup gamePanel;
    public GameObject healthFather;
    public Queue<GameObject> hpS = new Queue<GameObject>();
    public GameObject hpPrefabs;
    public Image injuredImage;
    [Header("结束面板")]
    public CanvasGroup endPanel;
    //实际用于动画的数值
    public float targetTimeScore;
    public int targetAttackScore;
    public int targetFinalScore;

    public TMP_Text endTimeText;//时间
    public TMP_Text endScoreText;//攻击分数
    public TMP_Text finalScoreText;//最终得分

    // 动画控制
    private Coroutine animationCoroutine;
    private bool isAnimating = false;
    private bool animationSkipped = false;

    [Header("排行榜")]
    public GameObject leaderboardPanel;//排行榜面板（可选）
    public TMP_Text[] leaderboardEntries;//长度为10的文本数组
    public Image hightImage;


    private void Awake()
    {
        instance = this;
        hpPrefabs = Resources.Load<GameObject>("Models/HPUI");
    }
    private void Start()
    {
        
    }
    public void InitHpUi()
    {
        int hp = GameManage.instance.player.GetComponent<PlayerAttake>().playerHP;

        for(int i =0;i<hp; i++)
        {
            GameObject go = Instantiate(hpPrefabs, healthFather.transform);
            hpS.Enqueue(go);
        }
    }
    public void RemoveHpUi()
    {
        GameObject go = hpS.Dequeue();
        Destroy(go);
       
    }

    //受伤
    public void Injured()
    {
        StartCoroutine(InjuredAni());
    }

    IEnumerator InjuredAni()
    {
        //动态变透明
        injuredImage.DOFade(1f, 0.6f);
        yield return new WaitForSeconds(0.2f);
        injuredImage.DOFade(0f, 0.6f);

    }

    //更新排行榜显示
    public void UpdateLeaderboardDisplay(List<int> scores,int lastScore)
    {
        for (int i = 0; i < leaderboardEntries.Length; i++)
        {
            if (i < scores.Count)
            {
                // 格式：排名.  总分  （例如 "1.--156"）
                leaderboardEntries[i].text = $"{i + 1}.  --  {scores[i]}";
                //高光提示
                if (scores[i] == lastScore&&hightImage==null)
                {
                    hightImage = leaderboardEntries[i].GetComponentInChildren<Image>();
                    hightImage.enabled = true;
                }
            }
            else
            {
                // 空位显示 "--"
                leaderboardEntries[i].text = $"{i + 1}.  --";
            }
        }
    }
    //排行榜面板，用来高光上一把的成绩
    public void CloseHighlightBoard()
    {
        if(hightImage == null)
        {
            return;
        }
        else
        {
            hightImage.enabled = false;
            hightImage = null;
        }
    }
    // 打开结束面板并启动动画
    public void OpenEndPanelWithAnimation(float timeScore, int attackScore, int finalScore)
    {
        // 记录目标值
        targetTimeScore = timeScore;
        targetAttackScore = attackScore;
        targetFinalScore = finalScore;

        // 初始化文本（动画开始时显示原始值）
        endTimeText.text = targetTimeScore.ToString("F1") + "s";
        endScoreText.text = targetAttackScore.ToString();
        finalScoreText.text = "0"; // 最终得分从0开始增加

        // 停止之前的动画（如果有）
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        // 重置跳过标志
        animationSkipped = false;

        // 开始新动画
        animationCoroutine = StartCoroutine(AnimateScores());

        // 显示面板
        OpenEndPanel();
    }
    // 分数动画协程（使用 unscaledDeltaTime，忽略时间缩放）
    private IEnumerator AnimateScores()
    {
        isAnimating = true;
        float duration = 1.5f; // 动画时长（秒）
        float elapsed = 0f;

        //起始值
        float startTime = targetTimeScore;
        int startAttack = targetAttackScore;
        int startFinal = 0;

        while (elapsed < duration && !animationSkipped)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 时间和攻击得分线性减少
            float currentTime = Mathf.Lerp(startTime, 0, t);
            int currentAttack = Mathf.RoundToInt(Mathf.Lerp(startAttack, 0, t));
            // 最终得分线性增加
            int currentFinal = Mathf.RoundToInt(Mathf.Lerp(startFinal, targetFinalScore, t));

            // 更新UI
            endTimeText.text = currentTime.ToString("F1") + "s";
            endScoreText.text = currentAttack.ToString();
            finalScoreText.text = currentFinal.ToString();

            yield return null;
        }

        // 动画结束或跳过时，确保显示最终值
        if (!animationSkipped)
        {
            // 正常结束
            endTimeText.text = "0.0s";
            endScoreText.text = "0";
            finalScoreText.text = targetFinalScore.ToString();
        }

        isAnimating = false;
        animationCoroutine = null;
    }
    // 跳过动画，直接显示最终值
    public void SkipAnimation()
    {
        if (!isAnimating) return;

        animationSkipped = true;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // 直接设置为最终值
        endTimeText.text = "0.0s";
        endScoreText.text = "0";
        finalScoreText.text = targetFinalScore.ToString();

        isAnimating = false;
    }

    //检查动画是否已结束（或已跳过）
    public bool IsAnimationFinished()
    {
        return !isAnimating;
    }

    public void OpenMeniu()
    {
        meniuPanel.alpha = 1;
        meniuPanel.interactable = true;
        meniuPanel.blocksRaycasts = true;
    }
    public void CloseMeniu()
    {
        meniuPanel.alpha = 0;
        meniuPanel.interactable = false;
        meniuPanel.blocksRaycasts = false;
    }
    public void OpenGamePanel()
    {
        gamePanel.alpha = 1;
        gamePanel.interactable = true;
        gamePanel.blocksRaycasts = true;
    }
    public void CloseGamePanel()
    {
        gamePanel.alpha = 0;
        gamePanel.interactable = false;
        gamePanel.blocksRaycasts = false;
    }
    public void OpenEndPanel()
    {
        endPanel.alpha = 1;
        endPanel.interactable = true;
        endPanel.blocksRaycasts = true;
    }
    public void CloseEndPanel()
    {
        endPanel.alpha = 0;
        endPanel.interactable = false;
        endPanel.blocksRaycasts = false;
    }
}
