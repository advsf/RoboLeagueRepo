using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpawnButtonInfo : NetworkBehaviour
{
    public PlayerDataTypes.Team buttonTeam;
    public PlayerDataTypes.Position buttonPosition;

    [SerializeField] private Rigidbody ballRb;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private Image shirtImage;
    [SerializeField] private GameObject canvaObj;

    public bool isTaken = false;

    public void SpawnPlayer()
    {
        // do not change the position of the player
        if (isTaken)
            return;

        // if an ability is active do NOT allow change of positions
        // it will break the ability

        // or if the GK has the ball in hand
        if (PlayerInfo.instance.playingObj.activeInHierarchy && (PlayerInfo.instance.GetComponentInChildren<HandleAbilities>().IsAbilityActivitated() || PlayerInfo.instance.GetComponentInChildren<HandleGoalkeeperAsPlayer>().isHoldingBall))
            return;

        // if the game is over, don't allow the user to switch
        if (ServerManager.instance.isGameOver.Value)
            return;

        // if the ball is out of bounds
        if (BallManager.instance.mainBallSync.isOutOfPlay.Value)
            return;

        if (PlayerInfo.instance.playingObj.activeInHierarchy)
            ServerManager.instance.RequestChangeOfPositionServerRpc(buttonTeam, buttonPosition);
        else
            ServerManager.instance.RequestSpawnServerRpc(buttonTeam, buttonPosition);

        canvaObj.SetActive(false);
        StartUIManager.instance.EnableDefaultCamera(false);
    } 

    public void SetAsTaken(string username)
    {
        usernameText.text = username;

        // make it more transparent to indicate that its taken
        shirtImage.color = new(shirtImage.color.r, shirtImage.color.g, shirtImage.color.b, 0.44f);

        isTaken = true;
    }

    public void ResetButton()
    {
        usernameText.text = ""; 
        shirtImage.color = new(shirtImage.color.r, shirtImage.color.g, shirtImage.color.b, 1);
        isTaken = false;
    }
}