using UnityEngine;
using UnityEngine.InputSystem;

public class ResetAllKeyBindsToDefault : MonoBehaviour
{
    public void ResetAllKeybinds()
    {
        PlayerInputReference.instance.controls.asset.RemoveAllBindingOverrides();

        FBPP.DeleteKey("rebinds");
        FBPP.Save();
    }
}
