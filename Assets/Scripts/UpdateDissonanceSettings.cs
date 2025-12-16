using UnityEngine;
using Dissonance;

public class UpdateDissonanceSettings : MonoBehaviour
{
    public static UpdateDissonanceSettings instance;

    [Header("References")]
    [SerializeField] private DissonanceComms comms;
    [SerializeField] private VoiceProximityReceiptTrigger proximityReceiptTrigger;
    [SerializeField] private VoiceProximityBroadcastTrigger proximityBroadcastTrigger;

    private void Start()
    {
        instance = this;
        ApplyAllSettings();
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void ApplyAllSettings()
    {
        string micName = FBPP.GetString("ProxMicName");
        if (!string.IsNullOrEmpty(micName))
            comms.MicrophoneName = micName;

        bool proxEnabled = FBPP.GetInt("ProxEnabled", 1) == 1;
        proximityReceiptTrigger.enabled = proxEnabled;
        proximityBroadcastTrigger.enabled = proxEnabled;

        int mode = FBPP.GetInt("ProxActivationMode", 1);
        if (proximityBroadcastTrigger != null)
            switch (mode)
            {
                case 0: 
                    proximityBroadcastTrigger.Mode = CommActivationMode.None; 
                    break;
                case 1: 
                    proximityBroadcastTrigger.Mode = CommActivationMode.VoiceActivation; 
                    break;
                case 2: 
                    proximityBroadcastTrigger.Mode = CommActivationMode.PushToTalk; 
                    break;
            }
    }
}
