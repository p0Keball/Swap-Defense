using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{   

    #region Inspector

     

    
    #endregion
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isGameOver = false;
    
    //Singleton
    public static GameManager Instance;


    #region Chuyển cảnh
    public static bool isDirectPlay = false;
    void Start() 
    {
        currentHealth = maxHealth;
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        // Kiểm tra xem có phải là chơi thẳng luôn không (Restart) hay là mới mở game (Menu)
        if (isDirectPlay)
        {
            StartGame(); // Gọi hàm bắt đầu game luôn
        }
        else
        {
            Time.timeScale = 0; 
            UIManager.Instance.ShowStartScreen(true);
        }
    }

    // Nút Play sẽ gọi hàm này
    public void StartGame()
    {
        isDirectPlay = true;
        Time.timeScale = 1; // Chạy game
        UIManager.Instance.ShowStartScreen(false);
    }

    // Nút Restart sẽ gọi hàm này
    public void RestartGame()
    {
        Time.timeScale = 1;
        // Load lại Scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Nút Menu sẽ gọi hàm này
    public void BackToMenu()
    {
        isDirectPlay = false;
        // Ở đây đơn giản là load lại scene, Start() sẽ tự hiện Menu
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("<color=orange>!!! GAME OVER !!!</color>");
        
        Time.timeScale = 0; // Dừng game lại
        UIManager.Instance.ShowGameOverScreen(); // HIỆN MÀN HÌNH KẾT THÚC
    }

    #endregion
    //Thanh máu
    #region Thanh máu
    

    public void TakeCastleDamage(float damage)
    {
        if (isGameOver) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Đảm bảo máu không âm

        // Gọi UI cập nhật
        UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    
    #endregion
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    #region Xử lý Drag to Swap

    private Material draggingMaterial; // Ô bắt đầu nhấn
    private Material targetMaterial;   // Ô đang được rê chuột tới

    public void OnTileDown(Material clickedMaterial)
    {
        // Nếu board đang xử lý logic thì không cho thao tác
        // if (MatchManager.Instance.isProcessing) return;

        draggingMaterial = clickedMaterial;
        draggingMaterial.PlaySelectEffect(true); // Hiệu ứng nhấn cho ô gốc
    }

    public void OnTileOver(Material hoveredMaterial)
    {
        if (draggingMaterial == null) return;

        // Nếu di chuột sang một ô khác và ô đó nằm cạnh ô gốc
        if (hoveredMaterial != draggingMaterial && IsAdjacent(draggingMaterial, hoveredMaterial))
        {
            // Nếu trước đó đã có targetMaterial khác, reset hiệu ứng của nó
            if (targetMaterial != null && targetMaterial != hoveredMaterial)
            {
                targetMaterial.PlaySelectEffect(false);
            }

            targetMaterial = hoveredMaterial;
            targetMaterial.PlaySelectEffect(true); // Gợi ý ô sẽ được đổi
        }
        else if (hoveredMaterial == draggingMaterial)
        {
            // Nếu quay lại ô cũ, hủy chọn ô target hiện tại
            if (targetMaterial != null)
            {
                targetMaterial.PlaySelectEffect(false);
                targetMaterial = null;
            }
        }
    }

    void Update()
    {
        // Khi người dùng THẢ chuột
        if (Input.GetMouseButtonUp(0))
        {
            if (draggingMaterial != null)
            {
                // CHỈ XÁC NHẬN ĐỔI KHI CÓ TARGET TILE HỢP LỆ
                if (targetMaterial != null)
                {
                    SwapTiles(draggingMaterial, targetMaterial);
                }
                else
                {
                    Debug.Log("Đã hủy bỏ: Trả về vị trí cũ hoặc kéo ra ngoài vùng hợp lệ.");
                }

                // Dọn dẹp hiệu ứng và biến tạm
                draggingMaterial.PlaySelectEffect(false);
                if (targetMaterial != null) targetMaterial.PlaySelectEffect(false);
                
                draggingMaterial = null;
                targetMaterial = null;
            }
        }
    }
    // Hàm kiểm tra kề cạnh 
    bool IsAdjacent(Material material1, Material material2)
    {
        int dx = Mathf.Abs(material1.gridX - material2.gridX);
        int dy = Mathf.Abs(material1.gridY - material2.gridY);
        return (dx + dy) == 1;
    }

    #endregion

    
    #region Swap

    // Hàm thực hiện tráo đổi 2 ô
    public void SwapTiles(Material material1, Material material2)
    {
        // 1. Đổi vị trí Transform (Hiện thị trên màn hình)
        Vector3 tempPosition = material1.transform.position;
        material1.transform.position = material2.transform.position;
        material2.transform.position = tempPosition;

        // 2. Đổi toạ độ Grid X, Y trong class Tile
        int tempX = material1.gridX;
        int tempY = material1.gridY;
        
        material1.gridX = material2.gridX;
        material1.gridY = material2.gridY;
        
        material2.gridX = tempX;
        material2.gridY = tempY;

        // 3. Cập nhật lại vị trí của chúng trong mảng gridArray
        GridManager.Instance.gridArray[material1.gridX, material1.gridY] = material1;
        GridManager.Instance.gridArray[material2.gridX, material2.gridY] = material2;

        // Đổi tên để dễ debug
        material1.name = $"{material1.type} {material1.gridX},{material1.gridY}";
        material2.name = $"{material2.type} {material2.gridX},{material2.gridY}";

        List<Material> allMatches = new List<Material>();

        bool hasMergeHappened = false; // Thêm biến này để theo dõi

        // Quét ô thứ 1
        List<Material> match1 = MatchManager.Instance.FindMatches(material1);
        if (match1.Count >= 3)
        {
            MatchManager.Instance.ProcessMerge(match1, material1);
            hasMergeHappened = true; // Báo hiệu là có gộp
        }

        // Quét ô thứ 2
        List<Material> match2 = MatchManager.Instance.FindMatches(material2);
        if (match2.Count >= 3)
        {
            MatchManager.Instance.ProcessMerge(match2, material2);
            hasMergeHappened = true; // Báo hiệu là có gộp
        }
        
        WaveManager.Instance.turnsLeft--;
        UIManager.Instance.UpdateSwapCount(WaveManager.Instance.turnsLeft);
        
        // Nếu CÓ xảy ra gộp tài nguyên
        if (hasMergeHappened)
        {
            // Bắt đầu chuỗi kịch bản rơi - lấp đầy - quét combo có thời gian chờ
            StartCoroutine(MatchManager.Instance.ProcessBoardRoutine());
        }

        else 
        {
            Debug.Log("Không có match nào xảy ra.");
            WaveManager.Instance.CheckTurnEnd();
        }
    }

    #endregion


}