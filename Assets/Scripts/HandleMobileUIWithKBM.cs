using UnityEngine;
using Unity.Netcode;

public class HandleMobileUIWithKBM : NetworkBehaviour
{
    [SerializeField] private GameObject[] UIToEnableWithKBM;
    [SerializeField] private GameObject[] UIToEnableWithoutKBM;

    private void Start()
    {
        if (!IsOwner)
            return;

        HandleKBMSupport.OnInputChanged += ChangeUI;

        ChangeUI();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner)
            return;

        HandleKBMSupport.OnInputChanged -= ChangeUI;
    }

    private void ChangeUI()
    {
        bool isUsingKBM = HandleKBMSupport.instance.IsUsingKBM;

        if (UIToEnableWithKBM.Length > 0)
            foreach (GameObject obj in UIToEnableWithKBM)
                obj.SetActive(isUsingKBM);

        if (UIToEnableWithoutKBM.Length > 0)
            foreach (GameObject obj in UIToEnableWithoutKBM)
                obj.SetActive(!isUsingKBM);
    }
}
