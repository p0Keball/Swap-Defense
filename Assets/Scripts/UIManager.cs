using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    #region Inspector
    
    public TextMeshProUGUI swapText;
    [Header("Screens")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public GameObject aboutPanel;
    public TextMeshProUGUI waveText;
    #endregion

    public static UIManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        Instance = this;

    } 

    // Hàm cập nhật số lượt swap lên màn hình
    public void UpdateSwapCount(int remainingSwaps)
    {
        swapText.text = remainingSwaps.ToString();
        
        // Bonus: Đổi màu chữ đỏ khi sắp hết lượt (dưới 3 lượt)
        if (remainingSwaps <= 3)
        {
            swapText.color = Color.red;
        }
        else
        {
            swapText.color = Color.white;
        }
    }

    #region Thanh máu
    public UnityEngine.UI.Image healthBarFill;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null)
        {
            // Tính toán tỷ lệ 0 đến 1
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }
    #endregion

    // Hàm hiện màn hình Start
    public void ShowStartScreen(bool isShow)
    {
        startPanel.SetActive(isShow);
    }

    // Hàm hiện màn hình Game Over
    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true);
    }

    public void OpenAboutScreen()
    {
        aboutPanel.SetActive(true);
    }

// Hàm đóng bảng About
    public void CloseAboutScreen()
    {
        aboutPanel.SetActive(false);
    }

    public void UpdateWaveCount(int currentWave)
{
    if (waveText != null)
    {
        waveText.text = currentWave.ToString();
    }
}
}
