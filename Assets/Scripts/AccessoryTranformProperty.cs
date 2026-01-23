using UnityEngine;

public class AccessoryTranformProperty : MonoBehaviour
{
    [Header("Lobby Positions")]
    public Vector3 lobbyLocalPosition;
    public Vector3 lobbyLocalRotation;
    public Vector3 lobbyLocalScale;

    [Header("In-Game Positions")]
    public Vector3 inGameLocalPosition;
    public Vector3 inGameLocalRotation;
    public Vector3 inGameLocalScale;

    public Vector3 GetLocalPosition()
    {
        if (PlayerInfo.instance == null)
            return lobbyLocalPosition;
        else
            return inGameLocalPosition;
    }

    public Vector3 GetLocalRotation()
    {
        if (PlayerInfo.instance == null)
            return lobbyLocalRotation;
        else
            return inGameLocalRotation;
    }

    public Vector3 GetLocalScale()
    {
        if (PlayerInfo.instance == null)
            return lobbyLocalScale;
        else
            return inGameLocalScale;
    }
}
