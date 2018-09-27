using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Ruyi.SDK.Online;

public class Achievement : Panel
{
    [SerializeField] private Transform m_AchievementContentTrans;
    [SerializeField] private Transform m_AwardedAchievementContentTrans;
    [SerializeField] private GameObject m_PnlAchievementPrefab;
    
    public override void Open()
    {
        base.Open();

        ClearAllAchievements();

        ShowLoadingCircle();
        CleanProfileData();

        GetAchievementList();
        GetAwardedAchievementList();
    }

    public override void Close()
    {
        base.Close();
    }

    public void GetAchievementList()
    {
        RuyiNet.GamificationService.ReadAchievements(0, false, OnGetAchievementListFinish);
    }

    public void GetAwardedAchievementList()
    {
        RuyiNet.GamificationService.ReadAchievedAchievements(0, false, OnGetAchievementListFinish);
    }

    private void OnGetAchievementListFinish(List<RuyiNetAchievement> achievements)
    {

        if (null == achievements || 0 == achievements.Count)
        {
            GameObject pnlAchievementGO = GameObject.Instantiate(m_PnlAchievementPrefab);

            pnlAchievementGO.transform.SetParent(m_AchievementContentTrans);
            pnlAchievementGO.GetComponentInChildren<Text>().text = "No Achievements";
        } else
        {
            for (int i = 0; i < achievements.Count; ++i)
            {
                GameObject pnlAchievementGO = GameObject.Instantiate(m_PnlAchievementPrefab);

                pnlAchievementGO.transform.SetParent(m_AchievementContentTrans);
                pnlAchievementGO.GetComponentInChildren<Text>().text = achievements[i].AchievementId + " " + achievements[i].Title;
            }
        }
    }

    private void OnGetAwardedAchievementList(List<RuyiNetAchievement> achievements)
    {
        if (null == achievements || 0 == achievements.Count)
        {
            GameObject pnlAchievementGO = GameObject.Instantiate(m_PnlAchievementPrefab);

            pnlAchievementGO.transform.SetParent(m_AwardedAchievementContentTrans);
            pnlAchievementGO.GetComponentInChildren<Text>().text = "No Awarded Achievements";
        } else
        {
            for (int i = 0; i < achievements.Count; ++i)
            {
                GameObject pnlAchievementGO = GameObject.Instantiate(m_PnlAchievementPrefab);

                pnlAchievementGO.transform.SetParent(m_AchievementContentTrans);
                pnlAchievementGO.GetComponentInChildren<Text>().text = achievements[i].AchievementId + " " + achievements[i].Title;
            }
        }
    }

    private void ClearAllAchievements()
    {
        for (int i = 0; i < m_AchievementContentTrans.childCount; ++i)
        {
            m_AchievementContentTrans.GetChild(i).gameObject.SetActive(false);
            GameObject.Destroy(m_AchievementContentTrans.GetChild(i).gameObject);
        }
    }
}
