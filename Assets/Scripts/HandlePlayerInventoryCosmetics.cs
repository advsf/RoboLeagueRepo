using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class HandlePlayerInventoryCosmetics : NetworkBehaviour
{
    [Header("Hair References")]
    [SerializeField] private GameObject[] hairs;

    [Header("Accessory References")]
    [SerializeField] private GameObject[] accessories;

    public NetworkVariable<FixedString32Bytes> equippedHairName = new("None", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString32Bytes> equippedAccessoryName = new("None", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        equippedHairName.OnValueChanged += OnHairNameValueChanged;
        equippedAccessoryName.OnValueChanged += OnAccessoryValueChanged;

        OnHairNameValueChanged(default, equippedHairName.Value);
        OnAccessoryValueChanged(default, equippedAccessoryName.Value);

        if (IsOwner)
        {
            equippedHairName.Value = PlayerPrefs.GetString("HairName", "None");
            equippedAccessoryName.Value = PlayerPrefs.GetString("AccessoryName", "None");
        }
    }

    private void OnHairNameValueChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        foreach (GameObject hair in hairs)
            hair.SetActive(hair.name == current.ToString());
    }

    private void OnAccessoryValueChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        foreach (GameObject accessory in accessories)
            accessory.SetActive(accessory.name == current.ToString());
    }
}
