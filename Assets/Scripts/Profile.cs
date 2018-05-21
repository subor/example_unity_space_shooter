using Ruyi.SDK.Online;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    public void OpenProfile()
    {
        gameObject.SetActive(true);

        var playerProfile = GameObject.FindGameObjectWithTag("Profile");

        var textFields = playerProfile.GetComponentsInChildren<Text>();
        textFields[0].text = RuyiNet.ActivePlayer.profileName;

        if (!string.IsNullOrEmpty(RuyiNet.ActivePlayer.pictureUrl))
        {
            mProfileImageDownload = new WWW(RuyiNet.ActivePlayer.pictureUrl);
        }
        
        var button = playerProfile.GetComponentsInChildren<Button>();
        button[0].onClick.RemoveAllListeners();
        button[0].onClick.AddListener(UpdateAvatar);

        var buttonText = button[0].GetComponentInChildren<Text>();
        buttonText.text = "CHANGE AVATAR";

        button[1].gameObject.SetActive(false);

        var summary = GameObject.FindGameObjectWithTag("ProfileSummary");
        var summaryText = summary.GetComponentsInChildren<Text>();
        summaryText[1].text = RuyiNet.ActivePlayer.summaryFriendData.gender;
        summaryText[3].text = RuyiNet.ActivePlayer.summaryFriendData.dob;
        summaryText[5].text = RuyiNet.ActivePlayer.summaryFriendData.location;
    }

    public void CloseProfile()
    {
        gameObject.SetActive(false);
    }

    public void UpdateAvatar()
    {
        String[] extensions = { "jpg", "png", "bmp" };
        Browser.OpenFile(@".\", extensions, OnFileSelected);
    }

    private void Awake()
    {
        CloseProfile();
    }

    private void Update()
    {
        if (mProfileImageDownload != null
            && mProfileImageDownload.isDone)
        {
            var profileImage = GetComponentInChildren<RawImage>();
            profileImage.texture = mProfileImageDownload.texture;
            mProfileImageDownload = null;
        }
    }

    private void OnFileSelected(string filename)
    {
        RuyiNet.ProfileService.UpdateUserPicture(RuyiNet.ActivePlayerIndex, filename, (RuyiNetGetCDNResponse response) =>
        {
            if (response.status == 200)
            {
                RuyiNet.ActivePlayer.pictureUrl = response.data.cdnUrl;
            }
        });

        var profileImage = GetComponentInChildren<RawImage>();
        profileImage.texture = LoadTexture(filename);
    }

    private Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

    public RuyiNet RuyiNet;
    public Browser Browser;

    private WWW mProfileImageDownload;
}
