using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManage : MonoBehaviour
{
    public static UIManage instance;

    public CanvasGroup meniuPanel;
    public CanvasGroup gamePanel;
    [Header("排行榜")]
    public GameObject leaderboardPanel;//排行榜面板（可选）
    public TMP_Text[] leaderboardEntries;//长度为10的文本数组


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        
    }

    //更新排行榜显示
    public void UpdateLeaderboardDisplay(List<float> scores)
    {
        for (int i = 0; i < leaderboardEntries.Length; i++)
        {
            if (i < scores.Count)
            {
                // 格式：排名.  时间s  （例如 "1.  56.1s"）
                leaderboardEntries[i].text = $"{i + 1}.  {scores[i]:F1}s";
            }
            else
            {
                // 空位显示 "--"
                leaderboardEntries[i].text = $"{i + 1}.  --";
            }
        }
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
}
