using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI Panels")]
    public GameObject pausePanel;       
    public GameObject pauseButton;    

    [Header("Menu References")]
    public GameObject startPanel;  

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Khi mới bắt đầu game, ẩn bảng pause và đảm bảo thời gian chạy bình thường
        if (pausePanel != null) pausePanel.SetActive(false);
        
        if (pauseButton != null) pauseButton.SetActive(true); 

        Time.timeScale = 1f;
    }

    // 1. Chức năng Tạm dừng game
    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false); // Ẩn nút bấm để giao diện sạch sẽ

        Time.timeScale = 0f; // DỪNG TOÀN BỘ THỜI GIAN TRONG GAME: Quái dừng chạy, tháp dừng bắn, không vuốt được Match-3
    }

    // 2. Chức năng Chơi tiếp
    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);

        Time.timeScale = 1f; // KHÔI PHỤC THỜI GIAN TRONG GAME hoạt động bình thường
    }

    // 3. Chức năng Chơi lại từ đầu (Restart)
    public void RestartGame()
    {
        Time.timeScale = 1f; // LUÔN LUÔN đưa timeScale về 1f trước khi LoadScene để tránh bị lỗi đứng game màn sau
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Tải lại màn chơi hiện tại
    }

    // 4. Chức năng Mở cài đặt (Tái sử dụng SettingsManager cũ)
    public void OpenSettingsFromPause()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OpenSettings(); // Gọi màn hình Settings hiển thị đè lên trên
        }
    }

    // 5. Chức năng Thoát về Menu chính
    public void QuitToMenu()
    {
        Time.timeScale = 1f; // Đảm bảo thời gian trôi bình thường khi quay lại Menu
    
        if (pausePanel != null) pausePanel.SetActive(false); // Ẩn bảng Pause đi
        if (startPanel != null) startPanel.SetActive(true);   // Bật lại màn hình Start Screen của bạn

        // (Tùy chọn) Nếu bạn muốn ẩn nút Pause nhỏ ngoài màn hình khi ở Menu:
        if (pauseButton != null) pauseButton.SetActive(false);

        Debug.Log("Đã quay trở lại màn hình Start Screen!");
    }
}