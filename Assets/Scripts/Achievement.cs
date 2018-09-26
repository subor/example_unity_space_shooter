using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Achievement : Panel
{
    [SerializeField] private Transform m_AchievementContentTrans;
    [SerializeField] private Transform m_AwardedAchievementContentTrans;
    [SerializeField] private GameObject m_PnlAchievementPrefab;
    
    public override void Open()
    {
        base.Open();

        //ShowLoadingCircle();
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

        GameObject pnlAchievementGO = GameObject.Instantiate(m_PnlAchievementPrefab);

        pnlAchievementGO.transform.SetParent(m_AchievementContentTrans);
        pnlAchievementGO.GetComponentInChildren<Text>().text = "No Achievements";
    }

    public void GetAwardedAchievementList()
    {
        GameObject pnlAchievementGO = GameObject.Instantiate(m_PnlAchievementPrefab);

        pnlAchievementGO.transform.SetParent(m_AwardedAchievementContentTrans);
        pnlAchievementGO.GetComponentInChildren<Text>().text = "No Awarded Achievements";
    }

    private void OnGetAchievementListFinish() { }

    private void OnGetAwardedAchievementList() { }
}
