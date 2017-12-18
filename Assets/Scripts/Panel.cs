using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    protected delegate void ButtonAction(Button button, string profileId);

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        mProfileImageDownloads = null;
        gameObject.SetActive(false);
    }

    protected void ShowLoadingCircle()
    {
        LoadingCircle.SetActive(true);
    }

    protected void HideLoadingCircle()
    {
        LoadingCircle.SetActive(false);
    }

    protected virtual void Awake()
    {
        UserInfoPrefab = Resources.Load("Pnl-Profile", typeof(GameObject)) as GameObject;

        var exitButton = GameObject.FindGameObjectWithTag("ExitButton").GetComponent<Button>();
        exitButton.onClick.AddListener(Close);

        Close();
    }

    protected virtual void Update()
    {
        if (mProfileImageDownloads != null)
        {
            if (mProfileImageDownloads.Count > 0)
            {
                foreach (var i in mProfileImageDownloads)
                {
                    if (i.First.isDone)
                    {
                        i.Second.texture = i.First.texture;
                    }
                }

                mProfileImageDownloads.RemoveAll(x => x.First.isDone);
            }
            else
            {
                mProfileImageDownloads = null;
            }
        }
    }

    protected GameObject AddProfileEntry(int y, string name, string profileId, string pictureUrl, string score)
    {
        var playerProfile = Instantiate(UserInfoPrefab, new Vector3(0, y, 0), Quaternion.identity);
        playerProfile.transform.SetParent(ScrollViewContent.transform, false);

        var textFields = playerProfile.GetComponentsInChildren<Text>();
        textFields[0].text = name;
        textFields[1].text = score;

        var button = playerProfile.GetComponentsInChildren<Button>();
        if (profileId == RuyiNet.ActivePlayer.profileId)
        {
            button[1].gameObject.SetActive(false);
        }
        else
        {
            button[1].onClick.AddListener(() =>
            {
                RuyiNet.PartyService.SendPartyInvitation(0, profileId, null);
            });
        }


        if (!string.IsNullOrEmpty(pictureUrl))
        {
            if (mProfileImageDownloads == null)
            {
                mProfileImageDownloads = new List<Tuple<WWW, RawImage>>();
            }

            var profileImage = playerProfile.GetComponentInChildren<RawImage>();
            var download = new WWW(pictureUrl);
            mProfileImageDownloads.Add(new Tuple<WWW, RawImage>(download, profileImage));
        }

        return playerProfile;
    }

    protected void CleanProfileData()
    {
        mProfileImageDownloads = null;

        var profiles = GameObject.FindGameObjectsWithTag("Profile");
        foreach (var i in profiles)
        {
            Destroy(i);
        }
    }

    public GameObject ScrollViewContent;
    public GameObject LoadingCircle;
    public RuyiNet RuyiNet;

    protected const int START_Y_POSITION = -10;
    protected const int Y_POSITION_OFFSET = -90;

    private GameObject UserInfoPrefab;
    private List<Tuple<WWW, RawImage>> mProfileImageDownloads;
}
