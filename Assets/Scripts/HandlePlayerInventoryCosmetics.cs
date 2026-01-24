using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class HandlePlayerInventoryCosmetics : NetworkBehaviour
{
    public CosmeticInventory cosmeticInventory;

    [Header("Sockets")]
    [SerializeField] private Transform hairParent;
    [SerializeField] private Transform hairAccessoryParent;
    [SerializeField] private Transform backAccessoryParent;

    private GameObject currentHairObject;
    private GameObject currentAccessoryObject;

    public NetworkVariable<FixedString32Bytes> equippedHairName = new("None", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString32Bytes> equippedAccessoryName = new("None", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        equippedHairName.OnValueChanged += OnHairNameValueChanged;
        equippedAccessoryName.OnValueChanged += OnAccessoryValueChanged;

        RefreshHair(equippedHairName.Value.ToString());
        RefreshAccessory(equippedAccessoryName.Value.ToString());

        if (IsOwner)
        {
            equippedHairName.Value = PlayerPrefs.GetString("HairName", "None");
            equippedAccessoryName.Value = PlayerPrefs.GetString("AccessoryName", "None");
        }
    }

    private void OnHairNameValueChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        RefreshHair(current.ToString());
    }

    private void OnAccessoryValueChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        RefreshAccessory(current.ToString());
    }

    private void RefreshHair(string hairName)
    {
        if (currentHairObject != null) 
            Destroy(currentHairObject);

        if (hairName == "None") 
            return;

        GameObject prefab = cosmeticInventory.GetSelectedHair(hairName);

        if (prefab != null)
        {
            currentHairObject = Instantiate(prefab, hairParent);
            ApplyTransformSettings(currentHairObject);
        }
    }

    private void RefreshAccessory(string accName)
    {
        if (currentAccessoryObject != null) 
            Destroy(currentAccessoryObject);

        if (accName == "None") 
            return;

        GameObject prefab = cosmeticInventory.GetSelectedAccessory(accName);

        if (prefab != null)
        {
            bool isBackAccessory = prefab.GetComponent<AccessoryTranformProperty>().isBackAccessory;

            currentAccessoryObject = Instantiate(prefab, isBackAccessory ? backAccessoryParent : hairAccessoryParent);
            ApplyTransformSettings(currentAccessoryObject);
        }
    }

    private void ApplyTransformSettings(GameObject obj)
    {
        obj.SetActive(true);

        if (obj.TryGetComponent(out AccessoryTranformProperty settings))
        {
            obj.transform.localPosition = settings.GetLocalPosition();
            obj.transform.localRotation = Quaternion.Euler(settings.GetLocalRotation());
            obj.transform.localScale = settings.GetLocalScale();
        }
    }

    public override void OnNetworkDespawn()
    {
        equippedHairName.OnValueChanged -= OnHairNameValueChanged;
        equippedAccessoryName.OnValueChanged -= OnAccessoryValueChanged;
    }
}