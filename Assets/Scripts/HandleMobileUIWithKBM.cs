using UnityEngine;

public class HandleMobileUIWithKBM : MonoBehaviour
{
    [SerializeField] private GameObject[] UIToEnableWithKBM;
    [SerializeField] private GameObject[] UIToEnableWithoutKBM;

    private void Start()
    {
        HandleKBMSupport.OnInputChanged += ChangeUI;
        ChangeUI();
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
