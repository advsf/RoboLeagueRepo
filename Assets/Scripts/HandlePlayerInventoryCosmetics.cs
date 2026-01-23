using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class HandlePlayerInventoryCosmetics : NetworkBehaviour
{
    public CosmeticInventory cosmeticInventory;

    [Header("Sockets")]
    [SerializeField] private Transform hairParent;
    [SerializeField] private Transform accessoryParent;

    // Keep track of the currently spawned objects so we can destroy them later
    private GameObject currentHairObject;
    private GameObject currentAccessoryObject;

    public NetworkVariable<FixedString32Bytes> equippedHairName = new("None", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString32Bytes> equippedAccessoryName = new("None", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        equippedHairName.OnValueChanged += OnHairNameValueChanged;
        equippedAccessoryName.OnValueChanged += OnAccessoryValueChanged;

        // Initial load for players already in the session
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

        // 2. Get the prefab from our Blue Cube (ScriptableObject)
        GameObject prefab = cosmeticInventory.GetSelectedHair(hairName);

        if (prefab != null)
        {
            // 3. Spawn and Parent it
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
            currentAccessoryObject = Instantiate(prefab, accessoryParent);
            ApplyTransformSettings(currentAccessoryObject);
        }
    }

    private void ApplyTransformSettings(GameObject obj)
    {
        if (obj.TryGetComponent(out AccessoryTranformProperty settings))
        {
            obj.transform.localPosition = settings.GetLocalPosition();
            obj.transform.localRotation = Quaternion.Euler(settings.GetLocalRotation());
            obj.transform.localScale = settings.GetLocalScale();
        }
    }

    public override void OnNetworkDespawn()
    {
        // Clean up events to prevent memory leaks
        equippedHairName.OnValueChanged -= OnHairNameValueChanged;
        equippedAccessoryName.OnValueChanged -= OnAccessoryValueChanged;
    }
}