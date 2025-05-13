using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    public string PlayerName;
    public int ConectionID;
    public ulong PlayerSteamID;
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI PlayerReadyText;
    public RawImage PlayerIcon;
    public bool Ready;
    //public Color ReadyColor;
    //public Color NotReadyColor;

    private bool avatarReceived;

    protected Callback<AvatarImageLoaded_t> imageLoaded;

    public void ChangeReadyStatus()
    {
        if(Ready)
        {
            PlayerReadyText.text = "ready";
            PlayerReadyText.color = Color.green;
        }
        else
        {
            PlayerReadyText.text = "not ready";
            PlayerReadyText.color = Color.red;
        }
    }

    private void Start()
    {
        imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    public void SetPlayerValues()
    {
        PlayerNameText.text = PlayerName;
        ChangeReadyStatus();

        if (!avatarReceived)
        {
            GetPlayerIcon();
        }
    }

    void GetPlayerIcon()
    {
        int imageID = SteamFriends.GetLargeFriendAvatar(new CSteamID(PlayerSteamID));
        if(imageID == -1)
        {
            Debug.LogError("Failed to get avatar image ID");
            return;
        }
        PlayerIcon.texture = GetSteamImageAsTexture(imageID);
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == PlayerSteamID)
        {
            if(callback.m_steamID.m_SteamID == PlayerSteamID)
            {
                PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
            }
        }
        else
        {
            return;
        }
    }

    private Texture2D GetSteamImageAsTexture(int imageId)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(imageId, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];
            isValid = SteamUtils.GetImageRGBA(imageId, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        avatarReceived = true;
        return texture;
    }
}
