using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;

public class HandlePlayerData : MonoBehaviour
{
    public static HandlePlayerData instance;

    [Header("Rank Images References")]
    [SerializeField] private Sprite[] rankSprites;

    public bool isPlayerMaxRank = false;
    public bool isPlayerLowestRank = false;

    private Dictionary<string, string> cachedData = new Dictionary<string, string>();
    private bool isDataLoaded = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private async void Start()
    {
        try
        {
            await HandlePlayerAuthentication.instance.EnsureAuthentication();
            await InitializeAndDownloadData();

            await AuthenticationService.Instance.UpdatePlayerNameAsync(GetUsername().Replace(" ", ""));
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing player data: {e}");
        }
    }

    private void OnEnable()
    {
        if (isDataLoaded)
        {
            IncreaseRankEXP(0);
            RefreshRankFlags();
        }
    }

    public async Task InitializeAndDownloadData()
    {
        try
        {
            var cloudData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            bool hasCloudData = cloudData.Count > 0;

            // if cloud data exists
            if (hasCloudData)
            {
                foreach (var item in cloudData)
                {
                    try
                    {
                        cachedData[item.Key] = item.Value.Value.GetAs<string>();
                    }
                    catch
                    {
                        cachedData[item.Key] = item.Value.Value.ToString();
                    }
                }

                SyncCloudToLocal();
            }
            else
            {
                // no cloud data
                // but there's already a pre-existing local data
                // so sync it to cloud
                if (FBPP.HasKey("Username"))
                {
                    await SyncLocalToCloud();
                }
                // no cloud data and no previous data
                // create default data
                else
                {
                    await CreateDefaultData();
                }
            }

            isDataLoaded = true;
            RefreshRankFlags();

            StartCoroutine(HandlePlayerStatsUI.instance.InitializeUIAndAnimate());
        }

        catch (Exception e)
        {
            Debug.LogError(e);

            // if cloud fails fall back to local data
            InitializeDatas();
            isDataLoaded = true;
        }
    }

    private async Task CreateDefaultData()
    {
        var defaultData = new Dictionary<string, object>
        {
            { "Username", "Guest" + UnityEngine.Random.Range(1000, 9999) },
            { "Goals", 0 },
            { "Assists", 0 },
            { "Saves", 0 },
            { "RankIndex", 0 },
            { "RankXP", 0f }
        };

        // save to cloud
        await CloudSaveService.Instance.Data.Player.SaveAsync(defaultData);

        // update authentication name
        await AuthenticationService.Instance.UpdatePlayerNameAsync(defaultData["Username"].ToString().Replace(" ", ""));

        // update cache
        foreach (var kvp in defaultData)
        {
            cachedData[kvp.Key] = Convert.ToString(kvp.Value, CultureInfo.InvariantCulture);
        }

        SyncCloudToLocal();
    }

    private void InitializeDatas()
    {
        if (!FBPP.HasKey("Username"))
        {
            FBPP.SetString("Username", "Guest" + UnityEngine.Random.Range(1000, 9999));
            FBPP.SetInt("Goals", 0);
            FBPP.SetInt("Assists", 0);
            FBPP.SetInt("Saves", 0);
            FBPP.SetInt("RankIndex", 0);
            FBPP.SetFloat("RankXP", 0);
            FBPP.Save();
        }
    }

    private void SyncCloudToLocal()
    {
        FBPP.SetString("Username", GetString("Username", "Guest"));
        FBPP.SetInt("Goals", GetInt("Goals", 0));
        FBPP.SetInt("Assists", GetInt("Assists", 0));
        FBPP.SetInt("Saves", GetInt("Saves", 0));
        FBPP.SetInt("RankIndex", Mathf.Clamp(GetInt("RankIndex", 0), 0, rankSprites.Length - 1));
        FBPP.SetFloat("RankXP", GetFloat("RankXP", 0f));
        FBPP.Save();
    }

    private async Task SyncLocalToCloud()
    {
        var localData = new Dictionary<string, object>
        {
            { "Username", FBPP.GetString("Username", "Guest") },
            { "Goals", FBPP.GetInt("Goals", 0) },
            { "Assists", FBPP.GetInt("Assists", 0) },
            { "Saves", FBPP.GetInt("Saves", 0) },
            { "RankIndex", FBPP.GetInt("RankIndex", 0) },
            { "RankXP", FBPP.GetFloat("RankXP", 0f) }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(localData);

        foreach (var kvp in localData)
        {
            cachedData[kvp.Key] = Convert.ToString(kvp.Value, CultureInfo.InvariantCulture);
        }
    }

    #region Player Username

    public string GetUsername()
    {
        return GetString("Username", FBPP.GetString("Username", "Guest"));
    }

    public async Task SetUsername(string newUsername)
    {
        await SaveSingleValue("Username", newUsername);

        await AuthenticationService.Instance.UpdatePlayerNameAsync(newUsername.Replace(" ", ""));

        FBPP.SetString("Username", newUsername);
        FBPP.Save();
    }

    #endregion

    #region Helper Functions

    private async Task SaveSingleValue(string key, object value)
    {
        try
        {
            cachedData[key] = Convert.ToString(value, CultureInfo.InvariantCulture);

            // save to cloud
            var data = new Dictionary<string, object> { { key, value } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving {key} to cloud: {e}");
        }
    }

    private async Task SaveMultipleValues(Dictionary<string, object> values)
    {
        try
        {
            foreach (var kvp in values)
            {
                cachedData[kvp.Key] = Convert.ToString(kvp.Value, CultureInfo.InvariantCulture);
            }

            await CloudSaveService.Instance.Data.Player.SaveAsync(values);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving multiple values to cloud: {e}");
        }
    }

    private int GetInt(string key, int defaultValue = 0)
    {
        if (!cachedData.TryGetValue(key, out string val))
            return defaultValue;

        if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            return result;

        Debug.LogWarning($"Failed to parse {key} as int. Value: '{val}'. Using default: {defaultValue}");
        return defaultValue;
    }

    private float GetFloat(string key, float defaultValue = 0f)
    {
        if (!cachedData.TryGetValue(key, out string val))
            return defaultValue;

        if (float.TryParse(val, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float result))
            return result;

        Debug.LogWarning($"Failed to parse {key} as float. Value: '{val}'. Using default: {defaultValue}");
        return defaultValue;
    }

    private string GetString(string key, string defaultValue = "")
    {
        return cachedData.TryGetValue(key, out string val) ? val : defaultValue;
    }

    #endregion

    #region Player Game Stats Data

    public async void UpdateGoalsCount()
    {
        int newGoals = GetInt("Goals") + 1;

        FBPP.SetInt("Goals", newGoals);
        FBPP.Save();

        await SaveSingleValue("Goals", newGoals);
    }

    public int GetGoalsCount()
    {
        return GetInt("Goals", FBPP.GetInt("Goals", 0));
    }

    public async void UpdateAssistsCount()
    {
        int newAssists = GetInt("Assists") + 1;

        FBPP.SetInt("Assists", newAssists);
        FBPP.Save();

        await SaveSingleValue("Assists", newAssists);
    }

    public int GetAssistsCount()
    {
        return GetInt("Assists", FBPP.GetInt("Assists", 0));
    }

    public async void UpdateSavesCount()
    {
        int newSaves = GetInt("Saves") + 1;

        FBPP.SetInt("Saves", newSaves);
        FBPP.Save();

        await SaveSingleValue("Saves", newSaves);
    }

    public int GetSavesCount()
    {
        return GetInt("Saves", FBPP.GetInt("Saves", 0));
    }

    private void RefreshRankFlags()
    {
        int index = GetInt("RankIndex", FBPP.GetInt("RankIndex", 0));
        index = Mathf.Clamp(index, 0, rankSprites.Length - 1);

        isPlayerMaxRank = index == rankSprites.Length - 1;
        isPlayerLowestRank = index == 0;
    }

    #endregion

    #region Player Rank Data

    public Sprite GetRankSprite()
    {
        int index = GetInt("RankIndex", FBPP.GetInt("RankIndex", 0));
        index = Mathf.Clamp(index, 0, rankSprites.Length - 1);
        return rankSprites[index];
    }

    public Sprite GetRankSprite(int index)
    {
        index = Mathf.Clamp(index, 0, rankSprites.Length - 1);
        return rankSprites[index];
    }

    public async void IncreaseRankEXP(float xp)
    {
        float newXP = GetFloat("RankXP", FBPP.GetFloat("RankXP", 0)) + xp;
        int rankIndex = GetInt("RankIndex", FBPP.GetInt("RankIndex", 0));

        while (true)
        {
            // at the highest rank
            // can only rank down
            if (rankIndex >= rankSprites.Length - 1)
            {
                if (newXP < 0)
                {
                    rankIndex = Mathf.Max(0, rankIndex - 1);
                    newXP = 100 + newXP;
                }
                else
                {
                    newXP = Mathf.Clamp(newXP, 0, 100);
                    break;
                }
            }
            // at the lowest rank
            // can only rank up
            else if (rankIndex <= 0)
            {
                if (newXP >= 100)
                {
                    rankIndex = Mathf.Min(rankSprites.Length - 1, rankIndex + 1);
                    newXP -= 100;
                }
                else
                {
                    newXP = Mathf.Clamp(newXP, 0, 100);
                    break;
                }
            }
            // can rank up or down
            else
            {
                if (newXP >= 100)
                {
                    newXP -= 100;
                    rankIndex++;

                    // Stop if reached max
                    if (rankIndex >= rankSprites.Length - 1)
                    {
                        rankIndex = rankSprites.Length - 1;
                        newXP = Mathf.Clamp(newXP, 0, 100);
                        break;
                    }
                }
                else if (newXP < 0)
                {
                    // rank down and carry over the left over xp
                    newXP = 100 + newXP;
                    rankIndex--;

                    // Stop if reached lowest
                    if (rankIndex <= 0)
                    {
                        rankIndex = 0;
                        newXP = Mathf.Clamp(newXP, 0, 100);
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // stop to prevent xp overflow
            if (newXP >= 0 && newXP < 100 && rankIndex > 0 && rankIndex < rankSprites.Length - 1)
                break;
        }

        FBPP.SetInt("RankIndex", rankIndex);
        FBPP.SetFloat("RankXP", newXP);
        FBPP.Save();

        var rankData = new Dictionary<string, object>
        {
            { "RankIndex", rankIndex },
            { "RankXP", newXP }
        };
        await SaveMultipleValues(rankData);

        isPlayerMaxRank = rankIndex == rankSprites.Length - 1;
        isPlayerLowestRank = rankIndex == 0;

        // update UI if in main lobby
        if (ServerManager.instance == null)
            HandlePlayerStatsUI.instance.UpdateStatsUI();
    }

    public int GetRankIndex()
    {
        return GetInt("RankIndex", FBPP.GetInt("RankIndex", 0));
    }

    public float GetRankXP()
    {
        return GetFloat("RankXP", FBPP.GetFloat("RankXP", 0f));
    }

    #endregion

    #region Manual Sync Methods

    public async Task PullFromCloud()
    {
        try
        {
            var cloudData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            foreach (var item in cloudData)
            {
                try
                {
                    cachedData[item.Key] = item.Value.Value.GetAs<string>();
                }
                catch
                {
                    // Fallback for complex types or nulls
                    cachedData[item.Key] = item.Value.Value.ToString();
                }
            }

            SyncCloudToLocal();
            RefreshRankFlags();

            Debug.Log("Successfully pulled data from cloud");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error pulling from cloud: {e}");
        }
    }

    public async Task PushToCloud()
    {
        try
        {
            await SyncLocalToCloud();
            Debug.Log("Successfully pushed data to cloud");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error pushing to cloud: {e}");
        }
    }

    public async Task ForceSync()
    {
        await PullFromCloud();
    }

    #endregion

    private void OnApplicationQuit()
    {
        FBPP.Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            FBPP.Save();
    }
}