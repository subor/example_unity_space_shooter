using Ruyi.SDK.Online;
using UnityEngine;
using UnityEngine.UI;

public class Party : Panel
{
    public override void Open()
    {
        base.Open();

        ShowLoadingCircle();
        CleanProfileData();

        RuyiNet.PartyService.GetPartyInfo(RuyiNet.ActivePlayerIndex, (RuyiNetGetPartyInfoResponse response) =>
        {
            HideLoadingCircle();

            var groups = response.data.response.groups;
            var invited = response.data.response.invited;
            //var requested = response.data.response.requested;

            if (groups.Length > 0)
            {
                mGroupId = groups[0].groupId;
                RuyiNet.PartyService.GetPartyMembersInfo(RuyiNet.ActivePlayerIndex, OnGetPartyMembersInfo);
            }
            else if (invited.Length > 0)
            {
                var y = START_Y_POSITION;
                foreach (var i in invited)
                {
                    var invitation = Instantiate(InvitePrefab, new Vector3(0, y, 0), Quaternion.identity);
                    invitation.transform.SetParent(ScrollViewContent.transform, false);

                    var textFields = invitation.GetComponentsInChildren<Text>();
                    textFields[0].text = i.name;
                    textFields[3].text = i.memberCount.ToString() + "/4";

                    var buttons = invitation.GetComponentsInChildren<Button>();
                    buttons[0].onClick.AddListener(() =>
                    {
                        RuyiNet.PartyService.RejectPartyInvitation(RuyiNet.ActivePlayerIndex, i.groupId, (RuyiNetResponse data) =>
                        {
                            Open();
                        });
                    });

                    buttons[1].onClick.AddListener(() =>
                    {
                        RuyiNet.PartyService.AcceptPartyInvitation(RuyiNet.ActivePlayerIndex, i.groupId, (RuyiNetResponse data) =>
                        {
                            Open();
                        });
                    });

                    y += Y_POSITION_OFFSET;
                }
            }
        });
    }

    private void OnGetPartyMembersInfo(RuyiNetGetProfilesResponse response)
    {
        var members = response.data.response;
        var y = START_Y_POSITION;
        foreach (var i in members)
        {
            var playerProfile = AddProfileEntry(y, i.profileName, i.profileId, i.pictureUrl, "");

            var buttons = playerProfile.GetComponentsInChildren<Button>();
            if (i.profileId == RuyiNet.ActivePlayer.profileId)
            {
                var buttonText = buttons[0].GetComponentInChildren<Text>();
                buttonText.text = "LEAVE PARTY";
                buttons[0].onClick.AddListener(() =>
                {
                    RuyiNet.PartyService.LeaveParty(RuyiNet.ActivePlayerIndex, mGroupId, (RuyiNetResponse data) =>
                    {
                        Open();
                    });
                });
            }
            else
            {
                buttons[0].gameObject.SetActive(false);
            }

            buttons[1].gameObject.SetActive(false);

            y += Y_POSITION_OFFSET;
        }
    }

    public GameObject InvitePrefab;

    private string mGroupId;
}
