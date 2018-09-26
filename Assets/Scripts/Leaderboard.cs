using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Ruyi.SDK.Online;

public class Leaderboard : Panel
{
    public string LeaderboardID = "1";
    [SerializeField] private GameObject m_PnlLeaderboardEntryPrefab;
    [SerializeField] private Done_GameController m_GameController;
    [SerializeField] private Text m_InfoText;

    public override void Open()
    {
        base.Open();

        ShowLoadingCircle();
        CleanProfileData();

        ClearLeaderboardPageContent();
        GetLeadboardPage();

        m_InfoText.text = "";
    }

    public override void Close()
    {
        base.Close();
    }

    private void ClearLeaderboardPageContent()
    {
        for (int i = 0; i < ScrollViewContent.transform.childCount; ++i)
        {
            ScrollViewContent.transform.GetChild(i).gameObject.SetActive(false);
            GameObject.Destroy(ScrollViewContent.transform.GetChild(i).gameObject);
        }
    }

    public void PostScoreToLeaderboard()
    {
        RuyiNet.LeaderboardService.PostScoreToLeaderboard(0, 154, LeaderboardID, OnPostScoreToLeaderboardFinish);

        
    }

    private void OnPostScoreToLeaderboardFinish(bool isSuccess)
    {
        if (isSuccess)
        {
            m_InfoText.text = "Post score Success";
        } else
        {
            m_InfoText.text = "Post score Fail";
        }
    }

    public void GetLeadboardPage()
    {
        RuyiNet.LeaderboardService.GetGlobalLeaderboardPage(0, LeaderboardID, Ruyi.SDK.BrainCloudApi.SortOrder.HIGH_TO_LOW, 0, 10, OnGetLeadboardPageFinish);
    }

    private void OnGetLeadboardPageFinish(RuyiNetLeaderboardPage page)
    {
        for (int i = 0; i < page.Entries.Count; ++i)
        {
            GameObject pnlLeaderboardEntryGO = GameObject.Instantiate(m_PnlLeaderboardEntryPrefab);
            pnlLeaderboardEntryGO.transform.SetParent(ScrollViewContent.transform);

            string leaderboardEntryStr = page.Entries[i].PlayerId + " " + page.Entries[i].Name + " " + page.Entries[i].Score;
            pnlLeaderboardEntryGO.GetComponentInChildren<Text>().text = leaderboardEntryStr;
        }

        HideLoadingCircle();
    }
}
