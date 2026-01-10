using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class HandleSettings : MonoBehaviour
{
    public static HandleSettings instance;

    [Header("General Reference")]
    [SerializeField] private GameObject settingUI;
    [SerializeField] private GameObject gameplayMenuSetting;
    [SerializeField] private GameObject screenMenuSetting;
    [SerializeField] private GameObject controlMenuSetting;
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Volume postVolume;

    [Header("Sub-Menu Reference")]
    [SerializeField] private GameObject videoSubMenu;
    [SerializeField] private GameObject graphicsSubMenu;
    [SerializeField] private GameObject audioSubMenu;

    [Header("Mobile UI Readjust References")]
    [SerializeField] private GameObject mobileReadjustCanvaObj;

    [Header("Mandatory Mouse DPI")]
    [SerializeField] private GameObject mandatoryMouseDPISettingObj;
    [SerializeField] private Slider _mouseDPISlider;
    [SerializeField] private TMP_InputField _mouseDPIInputField;

    [Header("Mouse DPI Reference")]
    [SerializeField] private Slider mouseDPISlider;
    [SerializeField] private TMP_InputField mouseDPIInputField;

    [Header("Sens Reference")]
    [SerializeField] private Slider sensSlider;
    [SerializeField] private TMP_InputField sensInputField;

    [Header("Mouse Control Reference")]
    [SerializeField] private Toggle invertVerticalToggle;
    [SerializeField] private Toggle invertHorizontalToggle;

    [Header("Enable HUD Reference")]
    [SerializeField] private Toggle enableHudToggle;

    [Header("Camera Distance Reference")]
    [SerializeField] private Slider cameraDistanceSlider;
    [SerializeField] private TMP_InputField cameraDistanceInputField;

    [Header("Camera Y Offset Reference")]
    [SerializeField] private Slider cameraYOffsetSlider;
    [SerializeField] private TMP_InputField cameraYOffsetInputField;

    [Header("Enable Ball Dot Reference")]
    [SerializeField] private Toggle enableBallDotToggle;

    [Header("Ball Dot Min. Size")]
    [SerializeField] private Slider ballDotMinSizeSlider;
    [SerializeField] private TMP_InputField ballDotMinSizeInputField;

    [Header("Ball Dot Max Size")]
    [SerializeField] private Slider ballDotMaxSizeSlider;
    [SerializeField] private TMP_InputField ballDotMaxSizeInputField;

    [Header("Volume Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider playerVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider emoteVolumeSlider;
    [SerializeField] private Slider crowdVolumeSlider;

    [Header("Proximity Chat Settings")]
    [SerializeField] private Toggle enableProximityChatToggle;
    [SerializeField] private TextMeshProUGUI proximityActivationText;
    [SerializeField] private TextMeshProUGUI inputDeviceText;
    private int currentProximityActivationIndex = 1;
    private string[] proximityActivationTexts = { "None", "Voice Activation", "Push To Talk" };

    [Header("Resolution Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;

    [Header("Fullscreen Settings")]
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TextMeshProUGUI fullscreenModeText;
    private int currentFullscreenModeIndex = 0; // 0 = Exclusive Fullscreen, 1 = Fullscreen Window, 2 = Maximized Window, 3 = Windowed

    [Header("Vsync Settings")]
    [SerializeField] private Toggle vSyncToggle;

    [Header("Limit FPS Settings")]
    [SerializeField] private TextMeshProUGUI limitFPSText;
    private int currentFPSSettings = 0;
    private int[] fpsOptions = { 30, 60, 144, 240, -1 };
    private int fpsIndex = 2; // default to 144 (index 2)

    [Header("Quality Settings")]
    [SerializeField] private TextMeshProUGUI qualitySettingText;
    private int currentQualitySettings = 0; // 0 - fancy, 1 - balanced, 2 - performance

    [Header("Sens Reference")]
    [SerializeField] private Slider renderScaleSlider;
    [SerializeField] private TMP_InputField renderScaleInputField;

    [Header("Anti-Aliasing Setting")]
    [SerializeField] private TextMeshProUGUI antiAliasingText;
    private int currentAntiAliasingSettings; // 0 = off, 1 = 2x, 2 = 4x, 3 = 8x

    [Header("Motion Blur Settings")]
    [SerializeField] private Toggle motionBlurToggle;
    [SerializeField] private float motionBlurIntensity = 0.5f;
    private MotionBlur motionBlur;

    [Header("Tonemapping Settings")]
    [SerializeField] private TextMeshProUGUI tonemappingText;
    private int currentTonemappingSetting; // 0 = none, 1 = neutral, 2 = ACES
    private Tonemapping tonemapping;

    [Header("Other References")]
    [SerializeField] private SessionHolder sessionHolder;

    public static event Action<bool> OnHUDToggled;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseSettingsUI();
    }

    private void Awake()
    {
        // to be able to use FBPP for steam cloud save
        var config = new FBPPConfig()
        {
            SaveFileName = "roboLeague-save-file.txt",
            AutoSaveData = false,
            ScrambleSaveData = true,
            EncryptionSecret = "my-secret",
            SaveFilePath = Application.persistentDataPath
        };

        // pass it to FBPP
        FBPP.Start(config);

        if (Application.isMobilePlatform)
        {
            QualitySettings.vSyncCount = 0;
            
            // dynamically set fps depending on the fresh rate of the device
            float nativeRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

            if (nativeRefreshRate > 0)
                Application.targetFrameRate = (int)nativeRefreshRate;
            else
                Application.targetFrameRate = 60;
        }
    }

    private void Start()
    {
        if (instance == null)
            instance = this;

        settingUI.SetActive(true);
        screenMenuSetting.SetActive(true);
        controlMenuSetting.SetActive(true);

        videoSubMenu.SetActive(true);
        graphicsSubMenu.SetActive(true);
        audioSubMenu.SetActive(true);

        mobileReadjustCanvaObj.SetActive(false);

        // get all resolutions
        resolutions = Screen.resolutions.Distinct().ToArray();
        PopulateResolutionDropdown();

        // get all the post processing
        GetAllPostProcessing();

        if (!Application.isMobilePlatform)
            mandatoryMouseDPISettingObj.SetActive(!PlayerPrefs.HasKey("MouseDPI"));

        // create or initialize data
        if (!FBPP.HasKey("IsDefaultCreated"))
            CreateDefaultSettings();
        else
            InitializeGameplaySettings();

        TurnOnSubVideoMenu();
        TurnOnGameplayMenu();
        settingUI.SetActive(false);
    }

    private void CreateDefaultSettings()
    {
        // create sensSlider value
        sensSlider.value = 200;
        UpdateSensitivityThroughSlider();

        // create mouse invert control values
        invertVerticalToggle.isOn = false;
        invertHorizontalToggle.isOn = false;
        InvertVerticalMouse();
        InvertHorizontalMouse();

        // create hub control values
        enableHudToggle.isOn = true;
        EnableHud(true);

        // create camera distance values
        cameraDistanceSlider.value = 12;
        UpdateCameraDistanceThroughSlider();

        // create camera y offset values
        cameraYOffsetSlider.value = 3f;
        UpdateCameraYOffsetThroughSlider();

        // create ball dot setting
        enableBallDotToggle.isOn = true;
        EnableBallDot();

        // create ball dot size setting
        ballDotMinSizeSlider.value = 0.05f;
        UpdateBallMinSizeThroughSlider();

        ballDotMaxSizeSlider.value = 0.4f;
        UpdateBallMaxSizeThroughSlider();

        // create volume control values
        masterVolumeSlider.value = 1;
        playerVolumeSlider.value = 1;
        musicVolumeSlider.value = 1;
        emoteVolumeSlider.value = 1;
        crowdVolumeSlider.value = 1;
        UpdateMasterVolume();
        UpdatePlayerVolume();
        UpdateMusicVolume();
        UpdateEmoteVolume();
        UpdateCrowdVolume();

        // dissonance settings
        enableProximityChatToggle.isOn = true;
        UpdateProximityChatToggle();

        currentProximityActivationIndex = 1; 
        UpdateProximityActivationMode();

        FBPP.SetString("ProxMicName", "");

        // resolutions
        currentResolutionIndex = resolutions.Length - 1;
        ApplyResolution();

        // fullscreen
        fullScreenToggle.isOn = true;
        UpdateFullscreen();

        // fullscreen mode
        PlayerPrefs.SetInt("FullscreenMode", 0);
        UpdateFullscreenMode();

        // vsync 
        vSyncToggle.isOn = false;
        UpdateVsync();

        // fps limit
        int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

        fpsIndex = Array.IndexOf(fpsOptions, refreshRate);

        if (fpsIndex == -1)
        {
            int closest = fpsOptions
                .Where(fps => fps != -1)
                .OrderBy(fps => Mathf.Abs(fps - refreshRate))
                .First();
            fpsIndex = Array.IndexOf(fpsOptions, closest);
        }

        UpdateFPSSettings();

        // quality setting
        currentQualitySettings = !Application.isMobilePlatform ? 0 : 1; // pc default to Fancy graphics, mobile default to performance graphics
        UpdateQuality();

        // render scale
        renderScaleSlider.value = !Application.isMobilePlatform ? 1 : 0.5f;
        UpdateRenderScaleThroughSlider();

        // anti aliasing
        currentAntiAliasingSettings = 0;
        UpdateAntiAliasing();

        // motion blur
        motionBlurToggle.isOn = false;
        UpdateMotionBlur();

        // tonemapping
        currentTonemappingSetting = 2;
        UpdateTonemappingSetting();

        PlayerPrefs.Save();
        FBPP.Save();

        FBPP.SetInt("IsDefaultCreated", 1);
    }

    private void InitializeGameplaySettings()
    {
        // mouse DPI UI
        mouseDPISlider.value = PlayerPrefs.GetFloat("MouseDPI");
        mouseDPIInputField.text = PlayerPrefs.GetFloat("MouseDPI").ToString("F2");

        // sens UI
        sensSlider.value = FBPP.GetFloat("Sensitivity");
        sensInputField.text = FBPP.GetFloat("Sensitivity").ToString("F2");

        // mouse invert UI
        invertVerticalToggle.isOn = FBPP.GetInt("InvertVerticalMouse") == -1;
        invertHorizontalToggle.isOn = FBPP.GetInt("InvertHorizontalMouse") == -1;

        // hud enable UI
        enableHudToggle.isOn = FBPP.GetInt("EnableHud") == 1;
        EnableHud(enableHudToggle.isOn);

        // camera distance
        cameraDistanceSlider.value = FBPP.GetFloat("CameraDistance");
        cameraDistanceInputField.text = FBPP.GetFloat("CameraDistance").ToString("F2");

        // camera y offset
        cameraYOffsetSlider.value = FBPP.GetFloat("CameraYOffset");
        cameraYOffsetInputField.text = FBPP.GetFloat("CameraYOffset").ToString("F2");

        enableBallDotToggle.isOn = FBPP.GetInt("EnableBallDot") == 1;

        // ball dot size setting
        ballDotMinSizeSlider.value = FBPP.GetFloat("BallDotMinSize");
        UpdateBallMinSizeThroughSlider();

        ballDotMaxSizeSlider.value = FBPP.GetFloat("BallDotMaxSize");
        UpdateBallMaxSizeThroughSlider();

        // volume
        masterVolumeSlider.value = FBPP.GetFloat("MasterVolume");
        playerVolumeSlider.value = FBPP.GetFloat("PlayerVolume");
        musicVolumeSlider.value = FBPP.GetFloat("MusicVolume");
        emoteVolumeSlider.value = FBPP.GetFloat("EmoteVolume");
        crowdVolumeSlider.value = FBPP.GetFloat("CrowdVolume");
        UpdateMasterVolume();
        UpdatePlayerVolume();
        UpdateMusicVolume();
        UpdateEmoteVolume();
        UpdateCrowdVolume();

        // dissonance
        enableProximityChatToggle.isOn = FBPP.GetInt("ProxEnabled") == 1; // 1 = on, 0 = off

        currentProximityActivationIndex = FBPP.GetInt("ProxActivationMode");

        proximityActivationText.text = proximityActivationTexts[currentProximityActivationIndex];

        string savedMic = FBPP.GetString("ProxMicName");
        if (string.IsNullOrEmpty(savedMic))
            inputDeviceText.text = "Default";
        else
            inputDeviceText.text = savedMic;

        // resolution
        currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
        ApplyResolution();

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // fullscreen
        fullScreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen") == 0;
        UpdateFullscreen();

        // fullscreen mode
        currentFullscreenModeIndex = PlayerPrefs.GetInt("FullscreenMode");
        UpdateFullscreenMode();

        // vsync
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync") == 1;
        UpdateVsync();

        // fps
        int savedFPS = PlayerPrefs.GetInt("FPS", 60); // default to 60
        fpsIndex = Array.IndexOf(fpsOptions, savedFPS);

        if (fpsIndex == -1)
            fpsIndex = 2;

        UpdateFPSSettings();

        // quality
        currentQualitySettings = FBPP.GetInt("Quality");
        UpdateQuality();

        // anti aliasing setting
        currentAntiAliasingSettings = FBPP.GetInt("AntiAliasing");
        UpdateAntiAliasing();

        // motion blur
        motionBlurToggle.isOn = FBPP.GetInt("MotionBlur") == 1;
        UpdateMotionBlur();

        // tonemapping
        currentTonemappingSetting = FBPP.GetInt("Tonemapping");
        UpdateTonemappingSetting();
    }

    private void GetAllPostProcessing()
    {
        postVolume.profile.TryGet(out motionBlur);
        postVolume.profile.TryGet(out tonemapping);
    }

    #region Mobile Readjust UI

    public void OpenMobileReadjustUI()
    {
        mobileReadjustCanvaObj.SetActive(true);
    }

    public void CloseMobileReadjustUI()
    {
        mobileReadjustCanvaObj.SetActive(false);

        if (PlayerInfo.instance != null)
            HandleMobileUI.instance.HandleUpdatingAllUICustomization();
    }

    #endregion

    #region Mouse DPI Settings

    public void MandatoryUpdateMouseDPIThroughSlider()
    {
        if (_mouseDPISlider.value <= 0)
            _mouseDPISlider.value = 1;

        PlayerPrefs.SetFloat("MouseDPI", _mouseDPISlider.value);
        PlayerPrefs.Save();

        _mouseDPIInputField.text = _mouseDPISlider.value.ToString("F2");
    }

    public void MandatoryUpdateMouseDPIThroughInputField()
    {
        if (float.TryParse(_mouseDPIInputField.text.ToString(), out float DPI))
        {
            if (DPI <= 0)
                DPI = 1;

            PlayerPrefs.SetFloat("MouseDPI", DPI);
            PlayerPrefs.Save();

            _mouseDPISlider.value = DPI;
        }
    }

    public void UpdateMouseDPIThroughSlider()
    {
        if (mouseDPISlider.value <= 0)
            mouseDPISlider.value = 1;

        PlayerPrefs.SetFloat("MouseDPI", mouseDPISlider.value);
        PlayerPrefs.Save();

        mouseDPIInputField.text = mouseDPISlider.value.ToString("F2");
    }

    public void UpdateMouseDPIThroughInputField()
    {
        if (float.TryParse(mouseDPIInputField.text.ToString(), out float DPI))
        {
            if (DPI <= 0)
                DPI = 1;

            PlayerPrefs.SetFloat("MouseDPI", DPI);
            PlayerPrefs.Save();

            mouseDPISlider.value = DPI;
        }
    }

    #endregion

    #region Sensitivity Settings
    public void UpdateSensitivityThroughSlider()
    {
        FBPP.SetFloat("Sensitivity", sensSlider.value);
        FBPP.Save();

        sensInputField.text = sensSlider.value.ToString("F2");
    }

    public void UpdateSensitivityThroughInputField()
    {
        if (float.TryParse(sensInputField.text.ToString(), out float sens))
        {
            FBPP.SetFloat("Sensitivity", sens);
            FBPP.Save();

            sensSlider.value = sens;
        }
    }

    #endregion

    #region Mouse Settings

    public void InvertVerticalMouse()
    {
        FBPP.SetInt("InvertVerticalMouse", invertVerticalToggle.isOn ? -1 : 1);
        FBPP.Save();
    }

    public void InvertHorizontalMouse()
    {
        FBPP.SetInt("InvertHorizontalMouse", invertHorizontalToggle.isOn ? -1 : 1);
        FBPP.Save();
    }

    #endregion

    #region Hud Settings

    public void EnableHud(bool isEnabled)
    {
        FBPP.SetInt("EnableHud", isEnabled ? 1 : 0);
        FBPP.Save();

        OnHUDToggled?.Invoke(isEnabled);
    }

    #endregion

    #region Camera Distance Settings

    public void UpdateCameraDistanceThroughSlider()
    {
        FBPP.SetFloat("CameraDistance", cameraDistanceSlider.value);
        FBPP.Save();

        cameraDistanceInputField.text = cameraDistanceSlider.value.ToString("F2");
    }

    public void UpdateCameraDistanceThroughInputField()
    {
        if (float.TryParse(cameraDistanceInputField.text.ToString(), out float distance))
        {
            FBPP.SetFloat("CameraDistance", distance);
            FBPP.Save();

            cameraDistanceSlider.value = distance;
        }
    }

    #endregion

    #region Camera Distance Settings

    public void UpdateCameraYOffsetThroughSlider()
    {
        FBPP.SetFloat("CameraYOffset", cameraYOffsetSlider.value);
        FBPP.Save();

        cameraYOffsetInputField.text = cameraYOffsetSlider.value.ToString("F2");
    }

    public void UpdateCameraYOffsetThroughInputField()
    {
        if (float.TryParse(cameraYOffsetInputField.text.ToString(), out float offset))
        {
            FBPP.SetFloat("CameraYOffset", offset);
            FBPP.Save();

            cameraYOffsetSlider.value = offset;
        }
    }

    #endregion

    #region Ball Dot Setting (Toggle)

    public void EnableBallDot()
    {
        FBPP.SetInt("EnableBallDot", enableBallDotToggle.isOn ? 1 : -1);
        FBPP.Save();
    }

    #endregion

    #region Ball Dot Min. Setting
    public void UpdateBallMinSizeThroughSlider()
    {
        FBPP.SetFloat("BallDotMinSize", ballDotMinSizeSlider.value);
        FBPP.Save();

        ballDotMinSizeInputField.text = ballDotMinSizeSlider.value.ToString("F2");
    }

    public void UpdateBallMinSizeThroughInputField()
    {
        if (float.TryParse(ballDotMinSizeInputField.text.ToString(), out float minSize))
        {
            minSize = Mathf.Clamp(minSize, ballDotMinSizeSlider.minValue, ballDotMinSizeSlider.maxValue);

            FBPP.SetFloat("BallDotMinSize", minSize);
            FBPP.Save();

            ballDotMinSizeSlider.value = minSize;
        }
    }

    #endregion

    #region Ball Dot Max Setting
    public void UpdateBallMaxSizeThroughSlider()
    {
        FBPP.SetFloat("BallDotMaxSize", ballDotMaxSizeSlider.value);
        FBPP.Save();

        ballDotMaxSizeInputField.text = ballDotMaxSizeSlider.value.ToString("F2");
    }

    public void UpdateBallMaxSizeThroughInputField()
    {
        if (float.TryParse(ballDotMaxSizeInputField.text.ToString(), out float maxSize))
        {
            maxSize = Mathf.Clamp(maxSize, ballDotMaxSizeSlider.minValue, ballDotMaxSizeSlider.maxValue);

            FBPP.SetFloat("BallDotMaxSize", maxSize);
            FBPP.Save();

            ballDotMaxSizeSlider.value = maxSize;
        }
    }

    #endregion

    #region Volume Settings

    public void UpdateMasterVolume()
    {
        float volume = masterVolumeSlider.value;

        FBPP.SetFloat("MasterVolume", volume);
        FBPP.Save();

        masterMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }

    public void UpdatePlayerVolume()
    {
        float volume = playerVolumeSlider.value;

        FBPP.SetFloat("PlayerVolume", volume);
        FBPP.Save();

        masterMixer.SetFloat("Player", Mathf.Log10(volume) * 20);
    }

    public void UpdateMusicVolume()
    {
        float volume = musicVolumeSlider.value;

        FBPP.SetFloat("MusicVolume", volume);
        FBPP.Save();

        masterMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }

    public void UpdateEmoteVolume()
    {
        float volume = emoteVolumeSlider.value;

        FBPP.SetFloat("EmoteVolume", volume);
        FBPP.Save();

        masterMixer.SetFloat("Emote", Mathf.Log10(volume) * 20);
    }

    public void UpdateCrowdVolume()
    {
        float volume = crowdVolumeSlider.value;

        FBPP.SetFloat("CrowdVolume", volume);
        FBPP.Save();

        masterMixer.SetFloat("Crowd", Mathf.Log10(volume) * 20);
    }

    #endregion

    #region Proximity Settings

    public void UpdateProximityChatToggle()
    {
        // 1 = enabled, 0 = disabled
        int value = enableProximityChatToggle.isOn ? 1 : 0;
        FBPP.SetInt("ProxEnabled", value);
        FBPP.Save();

        if (UpdateDissonanceSettings.instance != null)
            UpdateDissonanceSettings.instance.ApplyAllSettings();
    }

    public void UpdateProximityActivationMode()
    {
        proximityActivationText.text = proximityActivationTexts[currentProximityActivationIndex];

        // 0 = none, 1 = open, 2 = push to talk
        FBPP.SetInt("ProxActivationMode", currentProximityActivationIndex);
        FBPP.Save();

        if (UpdateDissonanceSettings.instance != null)
            UpdateDissonanceSettings.instance.ApplyAllSettings();
    }

    public void GoNextActivationMode()
    {
        currentProximityActivationIndex++;
        if (currentProximityActivationIndex >= proximityActivationTexts.Length)
            currentProximityActivationIndex = 0;

        UpdateProximityActivationMode();
    }

    public void GoPreviousActivationMode()
    {
        currentProximityActivationIndex--;
        if (currentProximityActivationIndex < 0)
            currentProximityActivationIndex = proximityActivationTexts.Length - 1;

        UpdateProximityActivationMode();
    }

    public void GoNextInputDevice()
    {
        string[] devices = Microphone.devices;

        if (devices.Length == 0) 
            return;

        string currentMic = FBPP.GetString("ProxMicName");
        int currentIndex = Array.IndexOf(devices, currentMic);

        int nextIndex = currentIndex + 1;
        if (nextIndex >= devices.Length) 
            nextIndex = 0;

        string newMicName = devices[nextIndex];
        inputDeviceText.text = newMicName;

        FBPP.SetString("ProxMicName", newMicName);
        FBPP.Save();

        if (UpdateDissonanceSettings.instance != null)
            UpdateDissonanceSettings.instance.ApplyAllSettings();
    }

    public void GoPreviousInputDevice()
    {
        string[] devices = Microphone.devices;

        if (devices.Length == 0)
            return;

        string currentMic = FBPP.GetString("ProxMicName");
        int currentIndex = Array.IndexOf(devices, currentMic);

        int prevIndex = currentIndex - 1;
        
        if (prevIndex < 0) 
            prevIndex = devices.Length - 1;

        string newMicName = devices[prevIndex];
        inputDeviceText.text = newMicName;

        FBPP.SetString("ProxMicName", newMicName);
        FBPP.Save();

        if (UpdateDissonanceSettings.instance != null)
            UpdateDissonanceSettings.instance.ApplyAllSettings();
    }

    #endregion

    #region Resolution Settings

    public void SetResolution(int resolutionIndex)
    {
        currentResolutionIndex = resolutionIndex;
    }

    private void ApplyResolution()
    {
        // do NOT do this for mobile - it will cause issues with the resolution
        if (Application.isMobilePlatform)
            return;

        Resolution res = resolutions[currentResolutionIndex];

        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);

        Debug.Log(currentResolutionIndex);

        PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
        PlayerPrefs.Save();
    }

    private void PopulateResolutionDropdown()
    {
        resolutions = Screen.resolutions.Distinct().ToArray();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int refreshRate = Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value);
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + refreshRate + "Hz";
            options.Add(option);

            Resolution screenRes = Screen.currentResolution;
            int screenRefresh = Mathf.RoundToInt((float)screenRes.refreshRateRatio.value);
            if (resolutions[i].width == screenRes.width &&
                resolutions[i].height == screenRes.height &&
                refreshRate == screenRefresh)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    #endregion

    #region Fullscreen Settings

    public void UpdateFullscreen()
    {
        // 0 = full screen, 1 = not fullscreen
        Screen.fullScreen = fullScreenToggle.isOn;
        PlayerPrefs.SetInt("Fullscreen", fullScreenToggle.isOn ? 0 : 1);
        PlayerPrefs.Save();
    }

    public void UpdateFullscreenMode()
    {
        // handle the text
        switch (currentFullscreenModeIndex)
        {
            case 0:
                fullscreenModeText.text = "Exclusive Fullscreen";
                break;
            case 1:
                fullscreenModeText.text = "Fullscreen Window";
                break;
            case 2:
                fullscreenModeText.text = "Maximized Window";
                break;
            case 3:
                fullscreenModeText.text = "Windowed";
                break;
        }

        Screen.fullScreenMode = (FullScreenMode)currentFullscreenModeIndex;
        PlayerPrefs.SetInt("FullscreenMode", currentFullscreenModeIndex);
        PlayerPrefs.Save();
    }

    public void GoToNextFullscreenMode()
    {
        currentFullscreenModeIndex++;

        if (currentFullscreenModeIndex > 3)
            currentFullscreenModeIndex = 0;

        UpdateFullscreenMode();
    }

    public void GoToPreviousFullscreenMode()
    {
        currentFullscreenModeIndex--;

        if (currentFullscreenModeIndex < 0)
            currentFullscreenModeIndex = 3;

        UpdateFullscreenMode();
    }

    #endregion

    #region Vsync Settings

    public void UpdateVsync()
    {
        PlayerPrefs.SetInt("VSync", vSyncToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        // 0 = off, 1 = on
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
    }

    #endregion

    #region Limit FPS Settings

    public void UpdateFPSSettings()
    {
        currentFPSSettings = fpsOptions[fpsIndex];

        if (currentFPSSettings == -1)
            limitFPSText.text = "Unlimited";
        else
            limitFPSText.text = currentFPSSettings.ToString();

        Application.targetFrameRate = currentFPSSettings;

        PlayerPrefs.SetInt("FPS", currentFPSSettings);
        PlayerPrefs.Save();
    }

    public void GoNextFPSLimit()
    {
        fpsIndex++;

        if (fpsIndex >= fpsOptions.Length)
            fpsIndex = 0;

        UpdateFPSSettings();
    }

    public void GoBackFPSLimit()
    {
        fpsIndex--;

        if (fpsIndex < 0)
            fpsIndex = fpsOptions.Length - 1;

        UpdateFPSSettings();
    }

    #endregion

    #region Quality Settings

    private void UpdateQuality()
    {
        QualitySettings.SetQualityLevel(currentQualitySettings, true);

        qualitySettingText.text = QualitySettings.names[currentQualitySettings];

        FBPP.SetInt("Quality", currentQualitySettings);
        FBPP.Save();
    }

    public void GoToNextQuality()
    {
        currentQualitySettings++;

        if (currentQualitySettings > 2)
            currentQualitySettings = 0;

        UpdateQuality();
    }

    public void GoToPreviousQuality()
    {
        currentQualitySettings--;

        if (currentQualitySettings < 0)
            currentQualitySettings = 2;

        UpdateQuality();
    }

    #endregion

    #region Render Scale Settings

    public void UpdateRenderScaleThroughSlider()
    {
        FBPP.SetFloat("RenderScale", renderScaleSlider.value);
        FBPP.Save();

        renderScaleInputField.text = renderScaleSlider.value.ToString("F2");
        UpdateRenderScale();
    }

    public void UpdateRenderScaleThroughInputField()
    {
        if (float.TryParse(renderScaleInputField.text.ToString(), out float renderScale))
        {
            FBPP.SetFloat("RenderScale", renderScale);
            FBPP.Save();

            renderScaleSlider.value = renderScale;
            UpdateRenderScale();
        }
    }

    private void UpdateRenderScale()
    {
        var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        urp.renderScale = FBPP.GetFloat("RenderScale");
    }

    #endregion

    #region Anti-Aliasing Setting

    private void UpdateAntiAliasing()
    {
        switch (currentAntiAliasingSettings)
        {
            case 0: // off
                QualitySettings.antiAliasing = 0;
                antiAliasingText.text = "Off";
                break;
            case 1: // 2x
                QualitySettings.antiAliasing = 2;
                antiAliasingText.text = "2x";
                break;
            case 2: // 4x
                QualitySettings.antiAliasing = 4;
                antiAliasingText.text = "4x";
                break;
            case 3: // 8x
                QualitySettings.antiAliasing = 8;
                antiAliasingText.text = "8x";
                break;
        }

        PlayerPrefs.SetInt("AntiAliasing", currentAntiAliasingSettings);
        PlayerPrefs.Save();
    }

    public void GoToNextAntiAliasingSetting()
    {
        currentAntiAliasingSettings++;

        if (currentAntiAliasingSettings > 3)
            currentAntiAliasingSettings = 0;

        UpdateAntiAliasing();
    }

    public void GoToPreviousAntiAliasingSetting()
    {
        currentAntiAliasingSettings--;

        if (currentAntiAliasingSettings < 0)
            currentAntiAliasingSettings = 3;

        UpdateAntiAliasing();
    }

    #endregion

    #region Motion Blur Settings

    public void UpdateMotionBlur()
    {
        // 0 = off, 1 = on
        if (motionBlurToggle.isOn)
        {
            motionBlur.active = true;
            motionBlur.intensity.value = motionBlurIntensity;

            FBPP.SetInt("MotionBlur", 1);
            FBPP.Save();
        }

        else
        {
            motionBlur.active = false;
            motionBlur.intensity.value = 0;

            FBPP.SetInt("MotionBlur", 0);
            FBPP.Save();
        }
    }

    #endregion

    #region Tonemapping Settings

    private void UpdateTonemappingSetting()
    {
        switch (currentTonemappingSetting)
        {
            case 0: // none
                tonemapping.mode.value = TonemappingMode.None;
                tonemappingText.text = "None";
                break;
            case 1: // default
                tonemapping.mode.value = TonemappingMode.Neutral;
                tonemappingText.text = "Neutral";
                break;
            case 2: // ACES
                tonemapping.mode.value = TonemappingMode.ACES;
                tonemappingText.text = "ACES";
                break;
        }

        FBPP.SetInt("Tonemapping", currentTonemappingSetting);
        FBPP.Save();
    }

    public void GoToNextTonemappingSetting()
    {
        currentTonemappingSetting++;

        if (currentTonemappingSetting > 2)
            currentTonemappingSetting = 0;

        UpdateTonemappingSetting();
    }

    public void GoToPreviousTonemappingSetting()
    {
        currentTonemappingSetting--;

        if (currentTonemappingSetting < 0)
            currentTonemappingSetting = 2;

        UpdateTonemappingSetting();
    }

    #endregion 

    #region General Functions
    public void OpenSettingsUI()
    {
        settingUI.SetActive(true);
    }

    public void CloseSettingsUI()
    {
        settingUI.SetActive(false);
    }

    public void TurnOnGameplayMenu()
    {
        gameplayMenuSetting.SetActive(true);
        screenMenuSetting.SetActive(false);
        controlMenuSetting.SetActive(false);
    }

    public void TurnOnScreenMenu()
    {
        screenMenuSetting.SetActive(true);
        gameplayMenuSetting.SetActive(false);
        controlMenuSetting.SetActive(false);
    }

    public void TurnOnControlMenu()
    {
        controlMenuSetting.SetActive(true);
        screenMenuSetting.SetActive(false);
        gameplayMenuSetting.SetActive(false);
    }

    public void TurnOnSubVideoMenu()
    {
        videoSubMenu.SetActive(true);
        graphicsSubMenu.SetActive(false);
        audioSubMenu.SetActive(false);
    }

    public void TurnOnSubGraphicsMenu()
    {
        graphicsSubMenu.SetActive(true);
        videoSubMenu.SetActive(false);
        audioSubMenu.SetActive(false);
    }

    public void TurnOnSubAudioMenu()
    {
        audioSubMenu.SetActive(true);
        graphicsSubMenu.SetActive(false);
        videoSubMenu.SetActive(false);
    }

    public void CloseMandatoryMouseDPIUI()
    {
        mandatoryMouseDPISettingObj.SetActive(!PlayerPrefs.HasKey("MouseDPI"));
    }

    public void ResetAllSettings()
    {
        DeleteAllSettingsRelatedData();

        CreateDefaultSettings();
        InitializeGameplaySettings();
    }

    private void DeleteAllSettingsRelatedData()
    {
        FBPP.DeleteKey("Sensitivity");
        FBPP.DeleteKey("InvertVerticalMouse");
        FBPP.DeleteKey("InvertHorizontalMouse");
        FBPP.DeleteKey("EnableHUD");
        FBPP.DeleteKey("CameraDistance");
        FBPP.DeleteKey("CameraYOffset");
        FBPP.DeleteKey("EnableBallDot");
        FBPP.DeleteKey("BallDotMinSize");
        FBPP.DeleteKey("BallDotMaxSize");

        FBPP.DeleteKey("MasterVolume");
        FBPP.DeleteKey("PlayerVolume");
        FBPP.DeleteKey("MusicVolume");
        FBPP.DeleteKey("EmoteVolume");
        FBPP.DeleteKey("CrowdVolume");

        FBPP.DeleteKey("ProxEnabled");
        FBPP.DeleteKey("ProxActivationMode");
        FBPP.DeleteKey("ProxMicName");

        FBPP.DeleteKey("ResolutionIndex");
        FBPP.DeleteKey("Fullscreen");
        FBPP.DeleteKey("FullscreenMode");
        FBPP.DeleteKey("VSync");
        FBPP.DeleteKey("FPS");

        FBPP.DeleteKey("Quality");
        FBPP.DeleteKey("AntiAliasing");
        FBPP.DeleteKey("MotionBlur");
        FBPP.DeleteKey("Tonemapping");

        PlayerPrefs.Save();
        FBPP.Save();
    }

    #endregion

    // only because this object is persistent
    public void BanSession() => sessionHolder.BanSession(sessionHolder.ActiveSession.Id);
}