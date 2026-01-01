using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class HandleHalftime : NetworkBehaviour
{
    public static HandleHalftime instance;

    [Header("References")]
    [SerializeField] private Transform stadium;
    [SerializeField] private BallSync mainBallSync;

    [Header("Halftime 1 Transform References")]
    [SerializeField] private Vector3 halftime1Pos;
    [SerializeField] private Vector3 halftime1Rot;

    [Header("Halftime 2 Transform References")]
    [SerializeField] private Vector3 halftime2Pos;
    [SerializeField] private Vector3 halftime2Rot;

    public NetworkVariable<float> halftimeDuration = new(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool isAfterHalftime = false; // false if in halftime 1, true if in halftime 2

    private bool isInHalftime = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        instance = this;
    }

    public IEnumerator TurnOnHalftime()
    {
        // if the ball is kinematic (from a GK holding the ball or something, don't call halftime -> call it after)
        if (isInHalftime || mainBallSync.GetRigidbody().isKinematic)
            yield break;

        isInHalftime = true;

        ServerManager.instance.isInHalftime.Value = true;

        PlayWhistleSoundClientRpc();

        // start countdown
        StartCoroutine(CountDownHalftimeDuration());

        yield return new WaitForSeconds(halftimeDuration.Value);

        // reset the clock
        ServerManager.instance.matchTime.Value = 0;

        // rotate stadium
        // give the illusion to the player that they switched sides
        ChangeIntoStadiumHalf2ClientRpc();

        // spawn ball in the middle
        // red team starts
        ServerManager.instance.RestartRound("Blue", false);

        ServerManager.instance.isInHalftime.Value = false;

        isInHalftime = false;
        isAfterHalftime = true;
    }

    [ClientRpc]
    public void ChangeIntoStadiumHalf1ClientRpc()
    {
        stadium.SetPositionAndRotation(halftime1Pos, Quaternion.Euler(halftime1Rot));
    }

    [ClientRpc]
    public void ChangeIntoStadiumHalf2ClientRpc()
    {
        stadium.SetPositionAndRotation(halftime2Pos, Quaternion.Euler(halftime2Rot));
    }

    [ClientRpc]
    private void PlayWhistleSoundClientRpc()
    {
        SoundManager.instance.PlayWhistleSound();
    }

    private IEnumerator CountDownHalftimeDuration()
    {
        // initialize
        halftimeDuration.Value = 5;

        while (halftimeDuration.Value >= 0)
        {
            halftimeDuration.Value -= Time.deltaTime;
            yield return null;
        }
    }
}
