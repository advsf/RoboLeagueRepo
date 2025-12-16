using UnityEngine;

public class HandleInventoryUI : MonoBehaviour
{
    [Header("Sub-Menu References")]
    [SerializeField] private GameObject hairSubMenu;
    [SerializeField] private GameObject accessioriesSubMenu;
    [SerializeField] private GameObject emotesSubMenu;
    [SerializeField] private GameObject ballsSubMenu;
    [SerializeField] private GameObject trailsSubMenu;

    [Header("General References")]
    [SerializeField] private GameObject rowPrefab;

    [Header("Hair Button Prefab References")]
    [SerializeField] private GameObject[] hairButtonPrefabs;
    [SerializeField] private GameObject hairDefaultButtonPrefab;
    [SerializeField] private Transform hairButtonContentPrefab;

    [Header("Accessory Button Prefab References")]
    [SerializeField] private GameObject[] accessoryButtonPrefabs;
    [SerializeField] private GameObject accessoryDefaultButtonPrefab;
    [SerializeField] private Transform accessoryButtonContentParent;

    [Header("Ball Button Prefab References")]
    [SerializeField] private GameObject[] ballButtonPrefabs;
    [SerializeField] private GameObject ballDefaultButtonPrefab;
    [SerializeField] private Transform ballButtonContentParent;

    [Header("Trail Button Prefab References")]
    [SerializeField] private GameObject[] trailButtonPrefabs;
    [SerializeField] private GameObject trailDefaultButtonPrefab;
    [SerializeField] private Transform trailButtonContentParent;

    private void Start()
    {
        EnableHairSubMenu();
    }

    #region Sub Menu

    public void EnableHairSubMenu()
    {
        hairSubMenu.SetActive(true);
        accessioriesSubMenu.SetActive(false);
        emotesSubMenu.SetActive(false);
        ballsSubMenu.SetActive(false);
        trailsSubMenu.SetActive(false);

        SetUpHairButtonPrefabs();
    }

    public void EnableAccessoriesSubMenu()
    {
        accessioriesSubMenu.SetActive(true);
        hairSubMenu.SetActive(false);
        emotesSubMenu.SetActive(false);
        ballsSubMenu.SetActive(false);
        trailsSubMenu.SetActive(false);

        SetUpAccessoryButtonPrefabs();
    }

    public void EnableEmotesSubMenu()
    {
        emotesSubMenu.SetActive(true);
        accessioriesSubMenu.SetActive(false);
        hairSubMenu.SetActive(false);
        ballsSubMenu.SetActive(false);
        trailsSubMenu.SetActive(false);
    }

    public void EnableBallsSubMenu()
    {
        ballsSubMenu.SetActive(true);
        emotesSubMenu.SetActive(false);
        accessioriesSubMenu.SetActive(false);
        hairSubMenu.SetActive(false);
        trailsSubMenu.SetActive(false);

        SetUpBallButtonPrefabs();
    }

    public void EnableTrailsSubMenu()
    {
        trailsSubMenu.SetActive(true);
        ballsSubMenu.SetActive(false);
        emotesSubMenu.SetActive(false);
        accessioriesSubMenu.SetActive(false);
        hairSubMenu.SetActive(false);

        SetUpTrailButtonPrefabs();
    }

    #endregion

    #region Enabling Hair Button Prefabs

    private void SetUpHairButtonPrefabs()
    {
        foreach (Transform row in hairButtonContentPrefab)
            Destroy(row.gameObject);

        Transform currentRow = null;
        int spanwedItems = 0;
        for (int i = 0; i < hairButtonPrefabs.Length; i++)
        {
            // set the row
            if (spanwedItems == 0 || spanwedItems % 3 == 0)
            {
                GameObject row = Instantiate(rowPrefab, hairButtonContentPrefab);
                currentRow = row.transform;
            }

            if (i == 0)
            {
                Instantiate(hairDefaultButtonPrefab, currentRow);
                spanwedItems++;
                continue;
            }

            else
            {
                // if we own the item
                if (PlayerPrefs.HasKey(hairButtonPrefabs[i].name))
                {
                    
                }

                Instantiate(hairButtonPrefabs[i], currentRow);
                spanwedItems++;
            }
        }

        // make an empty row
        // so that we can show the second last row fully
        Instantiate(rowPrefab, hairButtonContentPrefab);
    }

    #endregion

    #region Enabling Accessory Button Prefabs

    private void SetUpAccessoryButtonPrefabs()
    {
        foreach (Transform row in accessoryButtonContentParent)
            Destroy(row.gameObject);

        Transform currentRow = null;
        int spanwedItems = 0;
        for (int i = 0; i < accessoryButtonPrefabs.Length; i++)
        {
            // set the row
            if (spanwedItems == 0 || spanwedItems % 3 == 0)
            {
                GameObject row = Instantiate(rowPrefab, accessoryButtonContentParent);
                currentRow = row.transform;
            }

            if (i == 0)
            {
                Instantiate(accessoryDefaultButtonPrefab, currentRow);
                spanwedItems++;
                continue;
            }

            else
            {
                // if we own the item
                if (PlayerPrefs.HasKey(accessoryButtonPrefabs[i].name))
                {

                }

                Instantiate(accessoryButtonPrefabs[i], currentRow);
                spanwedItems++;
            }
        }

        // make an empty row
        // so that we can show the second last row fully
        Instantiate(rowPrefab, accessoryButtonContentParent);
    }

    #endregion

    #region Enabling Ball Button Prefabs

    private void SetUpBallButtonPrefabs()
    {
        foreach (Transform row in ballButtonContentParent)
            Destroy(row.gameObject);

        Transform currentRow = null;
        int spanwedItems = 0;
        for (int i = 0; i < ballButtonPrefabs.Length; i++)
        {
            // set the row
            if (spanwedItems == 0 || spanwedItems % 3 == 0)
            {
                GameObject row = Instantiate(rowPrefab, ballButtonContentParent);
                currentRow = row.transform;
            }

            if (i == 0)
            {
                Instantiate(ballDefaultButtonPrefab, currentRow);
                spanwedItems++;
                continue;
            }

            else
            {
                // if we own the item
                if (PlayerPrefs.HasKey(ballButtonPrefabs[i].name))
                {

                }

                Instantiate(ballButtonPrefabs[i], currentRow);
                spanwedItems++;
            }
        }

        // make an empty row
        // so that we can show the second last row fully
        Instantiate(rowPrefab, ballButtonContentParent);
    }

    #endregion

    #region Enabling Trail Button Prefabs

    private void SetUpTrailButtonPrefabs()
    {
        foreach (Transform row in trailButtonContentParent)
            Destroy(row.gameObject);

        Transform currentRow = null;
        int spanwedItems = 0;
        for (int i = 0; i < trailButtonPrefabs.Length; i++)
        {
            // set the row
            if (spanwedItems == 0 || spanwedItems % 3 == 0)
            {
                GameObject row = Instantiate(rowPrefab, trailButtonContentParent);
                currentRow = row.transform;
            }

            if (i == 0)
            {
                Instantiate(trailDefaultButtonPrefab, currentRow);
                spanwedItems++;
                continue;
            }

            else
            {
                // if we own the item
                if (PlayerPrefs.HasKey(trailButtonPrefabs[i].name))
                {

                }

                Instantiate(trailButtonPrefabs[i], currentRow);
                spanwedItems++;
            }
        }

        // make an empty row
        // so that we can show the second last row fully
        Instantiate(rowPrefab, trailButtonContentParent);
    }

    #endregion
}
