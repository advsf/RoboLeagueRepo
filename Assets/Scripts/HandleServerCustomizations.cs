using UnityEngine;

public class HandleServerCustomizations : MonoBehaviour
{
    public static HandleServerCustomizations instance;

    public int serverGameDuration = 600; // in seconds
    public int serverMapHash; // add different maps later
    public float serverKickMultiplier = 1;
    public float serverCurveMultiplier = 1;
    public float serverSpeedMultiplier = 1;
    public float serverJumpMultiplier = 1;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
            Destroy(gameObject);
    }


}
