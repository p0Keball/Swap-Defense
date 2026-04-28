using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    #region Inspector
    
    public TextMeshProUGUI swapText;

    #endregion

    public static UIManager Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    } 

    // Hàm cập nhật số lượt swap lên màn hình
    public void UpdateSwapCount(int remainingSwaps)
    {
        swapText.text = "Swaps: " + remainingSwaps.ToString();
        
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
}
