using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReference : MonoBehaviour
{
    public static PlayerInputReference instance;

    public PlayerControls controls;

    private const string RebindsKey = "rebinds";

    private void Awake()
    {
        if (instance == null)
            instance = this;

        controls = new PlayerControls();

        // load saved rebinds
        var rebinds = FBPP.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            controls.asset.LoadBindingOverridesFromJson(rebinds);
        }
    }

    public void SaveBindingOverrides()
    {
        var rebinds = controls.asset.SaveBindingOverridesAsJson();
        FBPP.SetString(RebindsKey, rebinds);
        FBPP.Save();
    }
}
