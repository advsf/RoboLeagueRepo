using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class HandleChatbox : NetworkBehaviour
{
    public static HandleChatbox instance;

    [Header("Prefab References")]
    [SerializeField] private GameObject chatTextPrefab; // to be put in the scrollview with the chat being opened

    [Header("References")]
    [SerializeField] private GameObject quickChatObj;
    [SerializeField] private GameObject emoteChatObj;
    [SerializeField] private GameObject chatBoxObj;
    [SerializeField] private GameObject closedChat; // when the chat isnt opened
    [SerializeField] private GameObject openedChat; // when the chat is opened
    [SerializeField] private Transform openChatParent;
    [SerializeField] private Transform closedChatParent;

    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_InputField mobileInputField;
    [SerializeField] private TextMeshProUGUI currentChatOption;

    [Header("Chat Settings")]
    [SerializeField] private float maxTimeBeforeClosedChatClosesAfterNoMessages = 15;
    private float timeSinceLastMessage = 0;
    private bool isChattingGlobally;

    [Header("Spam Detection")]
    [SerializeField] private int maxAmountOfText = 5; // max amount of time 
    [SerializeField] private float spamDuration; // time before user can type again
    public bool canText = true;
    private int amountOfTextSent = 0;
    private bool isMuted = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        base.OnNetworkSpawn();

        instance = this;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner)
            return;

        instance = null;
    }

    private void Start()
    {
        if (!IsOwner)
        {
            chatBoxObj.SetActive(false);
            return;
        }

        chatBoxObj.SetActive(true);
        closedChat.SetActive(true);
        openedChat.SetActive(false);

        isChattingGlobally = true;
        currentChatOption.text = "(ALL)";

        inputField.onSubmit.AddListener(OnInputSubmit);
    }

    private void OnEnable()
    {
        if (!IsOwner)
            return;

        EnableChat(FBPP.GetInt("EnableChat") == 1);

        PlayerInputReference.instance.controls.Gameplay.Chat.Enable();
        PlayerInputReference.instance.controls.Gameplay.ChatOption.Enable();
    }

    private void OnDisable()
    {
        if (!IsOwner)
            return;

        PlayerInputReference.instance.controls.Gameplay.Chat.Disable();
        PlayerInputReference.instance.controls.Gameplay.ChatOption.Disable();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleChatToggleInput();

        // switch from ALL -> Team or Team -> All
        if (openedChat.activeInHierarchy)
            HandleChatChannelSwitch();

        if (closedChat.activeInHierarchy)
        {
            timeSinceLastMessage += Time.deltaTime;

            // if there was no text messages we can close the "closed" chat object
            if (timeSinceLastMessage > maxTimeBeforeClosedChatClosesAfterNoMessages)
                closedChat.SetActive(false);
        }

        // handle spamming detection and prevent them from doing it
        HandleSpamming();

        // destroy messages for optimization
        if (openChatParent.childCount > 100)
            Destroy(openChatParent.GetChild(0).gameObject);

        if (closedChatParent.childCount > 100)
            Destroy(closedChatParent.GetChild(0).gameObject);
    }

    public void EnableChat(bool condition)
    {
        openedChat.SetActive(condition);
        closedChat.SetActive(condition);
    }

    private void HandleChatToggleInput()
    {
        if (FBPP.GetInt("EnableChat") == 0)
            return;

        if (PlayerInputReference.instance.controls.Gameplay.Chat.WasPressedThisFrame() && !quickChatObj.activeInHierarchy && !emoteChatObj.activeInHierarchy)
        {
            // open up the chatbox
            if (!openedChat.activeInHierarchy && !HandleCursorSettings.instance.IsUIOn())
            {
                openedChat.SetActive(true);
                closedChat.SetActive(false);

                // allow cursor movement
                HandleCursorSettings.instance.EnableCursor(true, false);
                PlayerMovement.instance.DisableMovement(true);

                // autofocuses the inputfield
                inputField.Select();
                inputField.ActivateInputField();
            }

            // or close it
            else
            {
                // disable cursor movement
                HandleCursorSettings.instance.EnableCursor(false, true);
                PlayerMovement.instance.DisableMovement(false);

                timeSinceLastMessage = 0;
                closedChat.SetActive(true);
                openedChat.SetActive(false);
            }
        }
    }

    private void HandleChatChannelSwitch()
    {
        // change the chat option
        if (PlayerInputReference.instance.controls.Gameplay.ChatOption.WasPressedThisFrame())
        {
            isChattingGlobally = !isChattingGlobally;
            currentChatOption.text = isChattingGlobally ? "(ALL)" : "(TEAM)";
        }
    }

    private void HandleSpamming()
    {
        if (amountOfTextSent >= maxAmountOfText && !isMuted)
            StartCoroutine(PreventSendingMessage());
    }

    public void HandleTrackingAmountOfTextSent()
    {
        amountOfTextSent++;
        Invoke(nameof(DecreaseAmountOfTextSentInt), 2f);
    }

    private IEnumerator PreventSendingMessage()
    {
        isMuted = true;
        canText = false;
        inputField.interactable = false;

        float startTime = Time.time;
        while (Time.time - startTime <= spamDuration)
        {
            float remainingTime = spamDuration - (Time.time - startTime);
            inputField.text = $"You must wait {Mathf.CeilToInt(remainingTime)} seconds before texting again!";
            yield return null;
        }

        canText = true;
        inputField.interactable = true;
        inputField.text = "";
        amountOfTextSent = 0;
        isMuted = false;
    }

    private void DecreaseAmountOfTextSentInt()
    {
        // this function is called every time a user sends a message
        // essentially, if the user spams a message and reaches the max amount of texts allocated
        // they will be muted
        // but if they dont spam, this allows the user to freely text without any mute
        if (amountOfTextSent > 0)
            amountOfTextSent--;
    }

    public void HandleFormattingTexts(bool isChattingAll, int rankIndex, string teamColor, string username, string position, string text, bool isServer = false)
    {
        // if there isnt a message
        if (string.IsNullOrEmpty(text))
            return;

        GameObject openTextObj = Instantiate(chatTextPrefab, openChatParent);
        GameObject closedTextObj = Instantiate(chatTextPrefab, closedChatParent);

        string formattedText;

        // if the server sent the message
        if (isServer)
            formattedText = $"<color=yellow>{text}";

        else
        {
            string chatOption = isChattingAll ? "(ALL)" : "(TEAM)";
            string nameColor;

            // if we sent out the message
            if (username == HandlePlayerData.instance.GetUsername())
                nameColor = "yellow";

            // if on the same team, make the username text blue
            // #26B5E3 == light blue
            else if (teamColor == PlayerInfo.instance.currentTeam.Value)
                nameColor = "#26B5E3";

            // if not on the same team
            else
                nameColor = "red";

            // add the spaces to give room for the rank image
            formattedText = $"<color=white>{chatOption}       <color={nameColor}>{username}</color> <color=white>({position}):<color=white> {text}";
        }

        if (FBPP.GetInt("ModerateChat") == 1)
            formattedText = ModerateChat(formattedText);

        // open the closed chat agian if it was inactive
        if (!openedChat.activeInHierarchy && !closedChat.activeInHierarchy)
            closedChat.SetActive(true);

        openTextObj.GetComponent<HandleChatBoxTextPrefabs>().SetChatBoxText(rankIndex, formattedText, isChattingAll);
        closedTextObj.GetComponent<HandleChatBoxTextPrefabs>().SetChatBoxText(rankIndex, formattedText, isChattingAll);

        // reset the time since last message
        timeSinceLastMessage = 0;
    }

    public void SendChatMessage(string text)
    {
        // if still in cooldown or nothing is typed
        if (!canText || string.IsNullOrWhiteSpace(text))
            return;

        HandleTrackingAmountOfTextSent();

        SendTextServerRpc(isChattingGlobally, PlayerInfo.instance.rankIndex.Value, PlayerInfo.instance.currentTeam.Value.ToString(), HandlePlayerData.instance.GetUsername(), PlayerInfo.instance.currentPosition.Value.ToString(), text);

        // reset the inputfield text
        if (!Application.isMobilePlatform)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }

        else
        {
            mobileInputField.text = "";
            mobileInputField.DeactivateInputField();
        }
    }

    private string ModerateChat(string text)
    {


        return text;
    }

    private void OnInputSubmit(string text)
    {
        if (FBPP.GetInt("EnableChat") == 0)
            return;

        SendChatMessage(text);
    }

    public void SendMobileChatMessage()
    {
        SendChatMessage(mobileInputField.text);
        mobileInputField.gameObject.SetActive(false);
    }

    [ServerRpc]
    public void SendTextServerRpc(bool isChattingAll, int rankIndex, string teamColor, string username, string position, string text, ServerRpcParams serverRpcParams = default)
    {
        // send to everyone
        if (isChattingAll)
            SendTextClientRpc(isChattingAll, rankIndex, teamColor, username, position, text);

        // send to team only
        else
        {
            var targetClientIds = teamColor.Equals("Blue") ? ServerManager.instance.blueTeamPlayerIds.ToArray() : ServerManager.instance.redTeamPlayerIds.ToArray();

            var rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = targetClientIds
                }
            };

            SendTextClientRpc(isChattingAll, rankIndex, teamColor, username, position, text, false, rpcParams);
        }
    }

    [ClientRpc]
    public void SendTextClientRpc(bool isChattingAll, int rankIndex, string teamColor, string username, string position, string text, bool isServer = false, ClientRpcParams clientRpcParams = default)
    {
        if (FBPP.GetInt("EnableChat") == 0)
            return;

        HandleChatbox chatbox = instance ?? NetworkManager.LocalClient.PlayerObject.GetComponentInChildren<HandleChatbox>();

        chatbox.HandleFormattingTexts(isChattingAll, rankIndex, teamColor, username, position, text, isServer);
    }
}