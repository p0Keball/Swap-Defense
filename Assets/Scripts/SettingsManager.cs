using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI Panels")]
    public GameObject settingsPanel; // Kéo Object 'SettingsPanel' vào đây

    [Header("UI Elements")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle gridToggle;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. Tải lại cấu hình cũ người chơi đã lưu (Nếu chưa có thì mặc định là bật/tối đa)
        bgmSlider.value = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX_Volume", 1f);
        
        // Cấu hình lưới: 1 là hiện, 0 là ẩn (Mặc định ban đầu là 1 - Hiện)
        gridToggle.isOn = PlayerPrefs.GetInt("Show_Grid", 1) == 1; 

        // 2. Lắng nghe sự kiện thay đổi dữ liệu trên giao diện
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        gridToggle.onValueChanged.AddListener(ToggleGridVisibility);

        // Áp dụng ngay lập tức mức âm lượng khi vừa vào game
        SetBGMVolume(bgmSlider.value);
        SetSFXVolume(sfxSlider.value);
        ToggleGridVisibility(gridToggle.isOn);

        // Mới vào game thì ẩn bảng này đi
        settingsPanel.SetActive(false);
    }

    // Hàm mở bảng Settings (Dùng chung cho cả nút ở ngoài Menu chính lẫn nút Pause trong game)
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Hàm đóng bảng Settings
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void SetBGMVolume(float value)
    {
        if (SoundManager.Instance != null && SoundManager.Instance.bgmSource != null)
        {
            SoundManager.Instance.bgmSource.volume = value;
        }
        PlayerPrefs.SetFloat("BGM_Volume", value);
    }

    private void SetSFXVolume(float value)
    {
        if (SoundManager.Instance != null && SoundManager.Instance.sfxSource != null)
        {
            SoundManager.Instance.sfxSource.volume = value;
        }
        PlayerPrefs.SetFloat("SFX_Volume", value);
    }

    private void ToggleGridVisibility(bool isVisible)
    {
        // Lưu lựa chọn của người chơi
        PlayerPrefs.SetInt("Show_Grid", isVisible ? 1 : 0);

        // Gọi sang GridManager để ẩn/hiện đường lưới
        if (GridManager.Instance != null)
        {
            GridManager.Instance.SetGridLinesActive(isVisible);
        }
    }
}