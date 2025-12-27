using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class HandleOffsides : NetworkBehaviour
{
    public static HandleOffsides instance;

    [Header("References")]
    [SerializeField] private Transform mainBall;
    [SerializeField] private BallSync mainBallSync;

    [Header("Settings")]
    [SerializeField] private float midlineXAxis;
    public float restartPlayDelay;

    public List<ulong> potentialOffsideIds = new();

    private ulong lastKickedPlayerId;
    private string lastKickedTeam;

    private void Awake()
    {
        instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheckForOffsidesServerRpc(int kickedPlayerId)
    {
        if (!IsServer || !ServerManager.instance.didStartGame.Value || ServerManager.instance.blueTeamPlayerIds.Count < 2 || ServerManager.instance.redTeamPlayerIds.Count < 2)
            return;

        ClearPotentialOffsidesIdListClientRpc();

        lastKickedPlayerId = (ulong) kickedPlayerId;

        // check for goalkeepers first
        // -2 = blue team
        // -3 = red team
        if (kickedPlayerId < 0)
            lastKickedTeam = kickedPlayerId == -2 ? "Blue" : "Red";

        // check for players
        else
        {
            PlayerInfo lastKickedPlayer = NetworkManager.ConnectedClients[(ulong)kickedPlayerId].PlayerObject.GetComponent<PlayerInfo>();
            lastKickedTeam = lastKickedPlayer.currentTeam.Value.ToString();
        }

        // if the last kicked player was a blue team, check for potential offsides for all the blue players EXCEPT for the kicker
        // a player is offsides when: the player is behind the defense line (determined by the furthest player back that isn't the GK) AND the ball is behind them AND the player didn't kick the ball
        if (lastKickedTeam == "Blue")
            CheckForPotentialOffsideBluePlayers(GetFurthestRedPlayer());

        else
            CheckForPotentialOffsideRedPlayers(GetFurthestBluePlayer());
    }

    public float GetFurthestBluePlayer()
    {
        float furthestXPos = midlineXAxis;

        foreach (ulong id in ServerManager.instance.blueTeamPlayerIds)
        {
            PlayerInfo playerInfo = NetworkManager.ConnectedClients[id].PlayerObject.GetComponent<PlayerInfo>();

            // skip this id if it's a GK
            if (playerInfo.currentPosition.Value.Equals("GK"))
                continue;

            Transform player = playerInfo.playingObj.transform;

            if (player.position.x > furthestXPos)
                furthestXPos = player.position.x;
        }

        return furthestXPos;
    }

    public float GetFurthestRedPlayer()
    {
        float furthestXPos = midlineXAxis;

        foreach (ulong id in ServerManager.instance.redTeamPlayerIds)
        {
            PlayerInfo playerInfo = NetworkManager.ConnectedClients[id].PlayerObject.GetComponent<PlayerInfo>();

            // skip this id if it's a GK
            if (playerInfo.currentPosition.Value.Equals("GK"))
                continue;

            Transform player = playerInfo.playingObj.transform;

            if (player.position.x < furthestXPos)
                furthestXPos = player.position.x;
        }

        return furthestXPos;
    }

    public void CheckForPotentialOffsideBluePlayers(float offsideXPos)
    {
        foreach (ulong id in ServerManager.instance.blueTeamPlayerIds)
        {
            PlayerInfo playerInfo = NetworkManager.ConnectedClients[id].PlayerObject.GetComponent<PlayerInfo>();

            Transform player = playerInfo.playingObj.transform;

            // if the player is BEHIND the defense line AND the player is ahead of the ball AND the player isn't the one who kicked
            if (player.position.x < offsideXPos && player.position.x < mainBall.position.x && id != lastKickedPlayerId)
                SyncPotentialOffsidesIdListClientRpc(id);
        }
    }

    public void CheckForPotentialOffsideRedPlayers(float offsideXPos)
    {
        foreach (ulong id in ServerManager.instance.redTeamPlayerIds)
        {
            PlayerInfo playerInfo = NetworkManager.ConnectedClients[id].PlayerObject.GetComponent<PlayerInfo>();

            Transform player = playerInfo.playingObj.transform;

            // if the player is BEHIND the defense line AND the player is ahead of the ball AND the player isn't the one who kicked
            if (player.position.x > offsideXPos && player.position.x > mainBall.position.x && id != lastKickedPlayerId)
                SyncPotentialOffsidesIdListClientRpc(id);
        }
    }

    [ClientRpc]
    private void SyncPotentialOffsidesIdListClientRpc(ulong id)
    {
        potentialOffsideIds.Add(id);
    }

    [ClientRpc]
    private void ClearPotentialOffsidesIdListClientRpc()
    {
        potentialOffsideIds.Clear();
    }

    public bool IsPlayerOffside(int kickerId)
    {
        return potentialOffsideIds.Contains((ulong) kickerId);
    }

    #region Indirect Kick Handling

    [ServerRpc(RequireOwnership = false)]
    public void StartIndirectionKickServerRpc(Vector3 newPos, string possessionTeam, bool isRestarting = false)
    {
        StartCoroutine(StartIndirectionKick(newPos, possessionTeam, isRestarting));
    }

    private IEnumerator StartIndirectionKick(Vector3 newPos, string possessionTeam, bool isRestarting)
    {
        yield return new WaitForEndOfFrame();

        // avoid duplicate calls
        if (!isRestarting && mainBallSync.isOffside.Value)
            yield break;

        ClearPotentialOffsidesIdListClientRpc();

        PlayAndSyncWhistleSound();

        mainBallSync.isOffside.Value = true;

        yield return new WaitForSeconds(restartPlayDelay);

        ServerManager.instance.HandleWhichTeamHasPossessionInIndirectFreeKickServerRpc(possessionTeam);

        ServerManager.instance.ResetOutOfBoundsTimer();

        mainBallSync.ResetBallServerRpc(newPos);
    }

    #endregion

    #region Whistle Sound
    private void PlayAndSyncWhistleSound()
    {
        SoundManager.instance.PlayWhistleSound();

        SyncWhistleSoundClientRpc();
    }


    [ClientRpc]
    private void SyncWhistleSoundClientRpc()
    {
        if (IsHost)
            return;

        SoundManager.instance.PlayWhistleSound();
    }

    #endregion
}
