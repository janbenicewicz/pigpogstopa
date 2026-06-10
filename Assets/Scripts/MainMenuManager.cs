using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Sceny")]
    public string playScene = "New AI";
    public string testScene = "Test";

    [Header("Panele UI")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Opcje - dźwięk")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Opcje - rozgrywka")]
    public TMP_Dropdown difficultyDropdown;
    public Toggle fullscreenToggle;

    const string PREF_MASTER = "vol_master";
    const string PREF_SFX = "vol_sfx";
    const string PREF_DIFFICULTY = "difficulty";
    const string PREF_FULLSCREEN = "fullscreen";

    void Start()
    {
        Time.timeScale = 1f;

        ShowMain();
        LoadPreferences();
        ApplyPreferences();
    }

    // ── Nawigacja ────────────────────────────────────────────────────

    public void ShowMain()
    {
        SetPanel(mainPanel, true);
        SetPanel(optionsPanel, false);
        SetPanel(creditsPanel, false);
    }

    public void ShowOptions()
    {
        SetPanel(mainPanel, false);
        SetPanel(optionsPanel, true);
        SetPanel(creditsPanel, false);
    }

    public void ShowCredits()
    {
        SetPanel(mainPanel, false);
        SetPanel(optionsPanel, false);
        SetPanel(creditsPanel, true);
    }

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    // ── Akcje przyciskow ─────────────────────────────────────────────

    public void PlayGame()
    {
        LoadScene(playScene);
    }

    public void PlayTest()
    {
        LoadScene(testScene);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[MainMenuManager] Brak nazwy sceny do zaladowania!");
            return;
        }

        Debug.Log("[MainMenuManager] Laduje scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        SavePreferences();
        Debug.Log("[MainMenuManager] Wyjscie z gry.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Opcje ────────────────────────────────────────────────────────

    public void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(PREF_MASTER, value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(PREF_SFX, value);
    }

    public void OnDifficultyChanged(int index)
    {
        PlayerPrefs.SetInt(PREF_DIFFICULTY, index);
    }

    public void OnFullscreenChanged(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt(PREF_FULLSCREEN, value ? 1 : 0);
    }

    public void ApplyOptions()
    {
        SavePreferences();
        ShowMain();
    }

    // ── PlayerPrefs ──────────────────────────────────────────────────

    void LoadPreferences()
    {
        float master = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
        float sfx = PlayerPrefs.GetFloat(PREF_SFX, 1f);
        int diff = PlayerPrefs.GetInt(PREF_DIFFICULTY, 1);
        bool full = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;

        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
        if (difficultyDropdown != null) difficultyDropdown.value = diff;
        if (fullscreenToggle != null) fullscreenToggle.isOn = full;
    }

    void ApplyPreferences()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
        Screen.fullScreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
    }

    void SavePreferences()
    {
        PlayerPrefs.Save();
    }
}
