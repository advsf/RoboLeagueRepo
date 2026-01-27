using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class EscapeMenuHandler : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner)
            return;

        if (PlayerMovement.instance != null)
            if (PlayerMovement.instance.isSliding || PlayerMovement.instance.isMovementDisabled)
                return;

        // just activiate the ui
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // fix right mouse button triggering back button
            if (Application.isMobilePlatform && Mouse.current != null)
                return;

            // if we need to disable the canva obj
            if (ServerManager.instance.IsSpawnSelectionCanvaObjActive())
            {
                HandleCursorSettings.instance.EnableCursor(false, true);

                ServerManager.instance.EnableSpawnSelectionCanvaObj(false);
            }

            else
            {
                HandleCursorSettings.instance.EnableCursor(true, false);

                ServerManager.instance.EnableSpawnSelectionCanvaObj(true);
            }
        }
    }
}