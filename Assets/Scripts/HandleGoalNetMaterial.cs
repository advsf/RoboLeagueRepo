using UnityEngine;
using UnityEngine.Rendering;

public class HandleGoalNetMaterial : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float distanceFromNetToDecreaseAlpha;
    [SerializeField] private float minimumAlpha;

    [Header("Team Color")]
    [SerializeField] private bool isBlueNet;
    [SerializeField] private bool isRedNet;

    [Header("References")]
    [SerializeField] private MeshRenderer netRenderer;
    [SerializeField] private MeshRenderer postRenderer;

    [Header("Net Materials")]
    [SerializeField] private Material netOpaqueMaterial;
    [SerializeField] private Material netTransparentMaterial;

    [Header("Post Materials")]
    [SerializeField] private Material postOpaqueMaterial;
    [SerializeField] private Material postTransparentMaterial;

    private bool isTransparent;

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

    private void ChangeGoalNetMaterial(bool isTransparent) 
    {
        if (isTransparent)
        {
            netRenderer.material = netTransparentMaterial;
            postRenderer.material = postTransparentMaterial;
        }

        else
        {
            netRenderer.material = netOpaqueMaterial;
            postRenderer.material = postOpaqueMaterial;
        }
    }
}
