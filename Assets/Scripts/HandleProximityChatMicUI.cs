using UnityEngine;
using Unity.Netcode;
using Dissonance;
using Dissonance.Integrations.Unity_NFGO;

public class HandleProximityChatMicUI : NetworkBehaviour
{
    [SerializeField] private GameObject micObj;
    [SerializeField] private bool shouldEnableForOwner; // determines if this should only be shown to the owner or not
    [SerializeField] private NfgoPlayer proxPlayer;

    public DissonanceComms comms;
    public bool isSpeaking;
    private VoicePlayerState _voiceState;

    private void Start()
    {
        if (ServerManager.instance.isTutorialServer || ServerManager.instance.isPracticeServer)
        {
            micObj.SetActive(false);

            enabled = false;
            return;
        }

        micObj.SetActive(false);

        comms = proxPlayer._comms;

        _voiceState = comms.FindPlayer(proxPlayer.PlayerId);
    }

    private void Update()
    {
        if (_voiceState == null)
            _voiceState = comms.FindPlayer(proxPlayer.PlayerId);

        if ((IsOwner && !shouldEnableForOwner) || (!IsOwner && shouldEnableForOwner))
        {
            micObj.SetActive(false);
            return;
        }

        isSpeaking = _voiceState.IsSpeaking;
        micObj.SetActive(isSpeaking);
    }
}
