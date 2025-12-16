using UnityEngine;
using Unity.Netcode;
using Dissonance;

public class SceneReferenceManager : NetworkBehaviour
{
    public static SceneReferenceManager instance;

    public Rigidbody ballRb;
    public GameObject ball;
    public BallSync ballSync;
    public GameObject positionBoundary;
    public BoxCollider AntiGriefBox;
    public DissonanceComms comms;

    public override void OnNetworkSpawn()
    {
        instance = this;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        instance = null;

        base.OnNetworkDespawn();
    }
}
