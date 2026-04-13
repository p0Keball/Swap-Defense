using UnityEngine;

public class GridManager : MonoBehaviour
{

    #region Inspector

    #region References

    [Header("References")]
    public GameObject tilePrefab;
    public GameObject materialPrefab; 
    
    // Tạo một mảng chứa hình ảnh (Kéo thả trong Unity Editor)
    public Sprite[] resourceSprites; 

    #endregion

    #region Grid Settings

    [Header("Grid Settings")]
    public int width = 12;
    public int height = 6;
    public float tileSize = 1f;

    #endregion

    #endregion


    public static GridManager Instance;

    void Awake()
    {
        Instance=this;
    }

    void Start()
    {
        GenerateGrid();
    }


    #region Khởi tạo ô lưới

    public Tile[,] gridArray;

    void GenerateGrid()
    {
        gridArray = new Tile[width, height];
  
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 spawnPosition = new Vector2(x * tileSize, y * tileSize);

                if (tilePrefab != null)
                {
                    GameObject bgTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
                    bgTile.transform.SetParent(this.transform); // Gom vào GridManager cho gọn
                    bgTile.name = $"BG {x},{y}";
                }

                GameObject spawnedTile = Instantiate(materialPrefab, spawnPosition, Quaternion.identity);
                spawnedTile.transform.SetParent(this.transform);

                // Lấy component Tile
                Tile tileScript = spawnedTile.GetComponent<Tile>();

                // Chọn ngẫu nhiên 1 loại tài nguyên từ Enum
                int randomTypeIndex = Random.Range(0, resourceSprites.Length);
                ResourceType randomType = (ResourceType)randomTypeIndex;

                // Cài đặt dữ liệu cho ô
                tileScript.Setup(x, y, randomType, resourceSprites[randomTypeIndex]);
                tileScript.name = $"{randomType} {x},{y}";

                // Lưu vào mảng 2D
                gridArray[x, y] = tileScript;
            }
        }

        // Căn giữa camera  
        CenterCamera();

        // Quét bàn cờ ngay khi vừa sinh ra. 
        // Nếu phát hiện có Match (hàm trả về true), thì khởi động chuỗi hiệu ứng Rơi & Lấp đầy!
        if (MatchManager.Instance.CheckEntireBoard())
        {
            StartCoroutine(MatchManager.Instance.ProcessBoardRoutine());
        }
    }

    void CenterCamera()
    {
        Camera.main.transform.position = new Vector3((width * tileSize) / 2f - (tileSize / 2f), 
                                                     (height * tileSize) / 2f - (tileSize / 2f), -10);
    }

    #endregion

}