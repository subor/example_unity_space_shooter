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

        RuyiNet.PartyService.ListPartyInvitations(RuyiNet.ActivePlayerIndex,(RuyiNetPartyInvitation[] invitations) =>
        {
            HideLoadingCircle();

            if (invitations != null &&
                invitations.Length > 0)
            {
                var y = START_Y_POSITION;
                foreach (var i in invitations)
                {
                    var invitation = Instantiate(InvitePrefab, new Vector3(0, y, 0), Quaternion.identity);
                    invitation.transform.SetParent(ScrollViewContent.transform, false);

                    var textFields = invitation.GetComponentsInChildren<Text>();
                    textFields[0].text = i.FromPlayerId;

                    var buttons = invitation.GetComponentsInChildren<Button>();
                    buttons[0].onClick.AddListener(() =>
                    {
                        RuyiNet.PartyService.RejectPartyInvitation(RuyiNet.ActivePlayerIndex, i.PartyId, (RuyiNetParty data) =>
                        {
                            Open();
                        });
                    });

                    buttons[1].onClick.AddListener(() =>
                    {
                        RuyiNet.PartyService.AcceptPartyInvitation(RuyiNet.ActivePlayerIndex, i.PartyId, (RuyiNetParty data) =>
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
                    RuyiNet.PartyService.LeaveParty(RuyiNet.ActivePlayerIndex, mGroupId, (RuyiNetParty data) =>
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
