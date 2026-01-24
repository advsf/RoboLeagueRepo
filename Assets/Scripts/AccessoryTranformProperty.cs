using UnityEngine;

public class AccessoryTranformProperty : MonoBehaviour
{
    [Header("Lobby Positions")]
    [SerializeField] private Vector3 lobbyLocalPosition;
    [SerializeField] private Vector3 lobbyLocalRotation;
    [SerializeField] private Vector3 lobbyLocalScale;

    [Header("In-Game Positions")]
    [SerializeField] private Vector3 inGameLocalPosition;
    [SerializeField] private Vector3 inGameLocalRotation;
    [SerializeField] private Vector3 inGameLocalScale;

    [Header("Other Information")]
    public bool isBackAccessory = false;

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
