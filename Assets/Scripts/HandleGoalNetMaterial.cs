using UnityEngine;

public class HandleGoalNetMaterial : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float distanceFromNetToDecreaseAlpha;
    [SerializeField] private float minimumAlpha;

    [Header("Team Color")]
    [SerializeField] private bool isBlueNet;
    [SerializeField] private bool isRedNet;

    [Header("References")]
    [SerializeField] private MeshRenderer[] goalNetMats;

    private Material[] netMats;

    private Color originalGoalNetColor;
    private Color decreasedAlphaGoalNetColor;

    private bool isTransparent;

    private void Start()
    {
        // this is to prevent memory leaks
        // since calling materials creates a new copy every time
        netMats = new Material[goalNetMats.Length];

        for (int i = 0; i < goalNetMats.Length; i++)
            netMats[i] = goalNetMats[i].material;

        originalGoalNetColor = netMats[0].color;
        decreasedAlphaGoalNetColor = new Color(netMats[0].color.r, netMats[0].color.g, netMats[0].color.b, minimumAlpha);
    }

    private void Update()
    {
        // if we are GK
        if ((isBlueNet && PlayerInfo.instance.currentTeam.Value.Equals("Blue")) || (isRedNet && PlayerInfo.instance.currentTeam.Value.Equals("Red")))
        {
            if (PlayerInfo.instance.currentPosition.Value == "GK")
            {
                if ((Vector3.Distance(PlayerMovement.instance.transform.position, transform.position) <= distanceFromNetToDecreaseAlpha) && !isTransparent)
                {
                    ChangeGoalNetMaterial(true);
                    isTransparent = true;
                }

                else if ((Vector3.Distance(PlayerMovement.instance.transform.position, transform.position) > distanceFromNetToDecreaseAlpha) && isTransparent)
                {
                    ChangeGoalNetMaterial(false);
                    isTransparent = false;
                }
            }

            else if (isTransparent)
            {
                ChangeGoalNetMaterial(false);
                isTransparent = false;
            }
        }

        else if (isTransparent)
        {
            ChangeGoalNetMaterial(false);
            isTransparent = false;
        }
    }

    private void ChangeGoalNetMaterial(bool makeTransparent)
    {
        if (!makeTransparent)
            foreach (Material mat in netMats)
                mat.color = originalGoalNetColor;
        else
            foreach (Material mat in netMats)
                mat.color = decreasedAlphaGoalNetColor;
    }
}
