using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class HandlePlayerJerseyCosmetics : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject jerseyObj;
    [SerializeField] private TextMeshProUGUI[] jerseyName;
    [SerializeField] private TextMeshProUGUI[] jerseyNumber;

    public NetworkVariable<FixedString32Bytes> currentJerseyName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString32Bytes> currentJerseyNumber = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJerseyEnabled = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        currentJerseyName.OnValueChanged += OnJerseyNameValueChanged;
        currentJerseyNumber.OnValueChanged += OnJerseyNumberValueChanged;
        isJerseyEnabled.OnValueChanged += OnJerseyEnabledValueChanged;

        OnJerseyNameValueChanged(default, currentJerseyName.Value);
        OnJerseyNumberValueChanged(default, currentJerseyNumber.Value);
        OnJerseyEnabledValueChanged(default, isJerseyEnabled.Value);

        if (IsOwner)
        {
            currentJerseyName.Value = FBPP.GetString("JerseyName");
            currentJerseyNumber.Value = FBPP.GetString("JerseyNumber");
            isJerseyEnabled.Value = FBPP.GetBool("IsJerseyEnabled");
        }
    }

    public override void OnNetworkDespawn()
    {
        currentJerseyName.OnValueChanged -= OnJerseyNameValueChanged;
        currentJerseyNumber.OnValueChanged -= OnJerseyNumberValueChanged;
        isJerseyEnabled.OnValueChanged -= OnJerseyEnabledValueChanged;
    }

    private void OnJerseyNameValueChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        foreach (TextMeshProUGUI name in jerseyName)
            name.text = current.ToString();
    }

    private void OnJerseyNumberValueChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        foreach (TextMeshProUGUI number in jerseyNumber)
            number.text = current.ToString();
    }

    private void OnJerseyEnabledValueChanged(bool previous, bool current)
    {
        jerseyObj.SetActive(current);
    }
}
