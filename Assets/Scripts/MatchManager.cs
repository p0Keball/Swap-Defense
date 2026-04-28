using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class MatchManager : MonoBehaviour
{

    #region Inspector

    [Header("Tower Data")]
    public List<TowerTypeData> allTowerDatabases; // Kéo thả các Database của Wood, Gold, Ice... vào đây

    #endregion

    private GridManager grid;
    public static MatchManager Instance;
     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this; 
        grid = GetComponent<GridManager>(); // Lấy tham chiếu sang GridManager
    }


    #region Refill board

    // làm các ô rơi xuống lấp đầy khoảng trống
    void ApplyGravity()
    {
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                // Nếu phát hiện một ô trống
                if (grid.gridArray[x, y] == null)
                {
                    // Quét lên trên xem có ô nào để kéo xuống không
                    for (int k = y + 1; k < grid.height; k++)
                    {
                        if (grid.gridArray[x, k] != null)
                        {
                            // Kéo ô đó xuống vị trí y
                            grid.gridArray[x, y] = grid.gridArray[x, k];
                            grid.gridArray[x, k] = null; // Chỗ cũ giờ thành trống

                            // Cập nhật lại tọa độ cho Tile script
                            grid.gridArray[x, y].gridX = x;
                            grid.gridArray[x, y].gridY = y;

                            // Cập nhật lại vị trí hiển thị trên màn hình
                            Vector2 targetPos = new Vector2(x * grid.tileSize, y * grid.tileSize);
                            grid.gridArray[x, y].targetPosition = targetPos;
                            grid.gridArray[x, y].isMoving = true;
                            
                            break; // Xong 1 ô thì thoát vòng lặp k để tìm ô y tiếp theo
                        }
                    }
                }
            }
        }
    }

    //  sinh thêm ô mới ở trên cùng để lấp đầy bàn cờ
    void RefillBoard()
    {
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                // Nếu vẫn còn ô trống (thường là ở trên cùng do đã bị rớt xuống)
                if (grid.gridArray[x, y] == null)
                {
                    // Tọa độ lúc sinh ra: Cộng thêm y vào độ cao để tạo hiệu ứng rơi nối đuôi nhau
                    Vector2 spawnPosition = new Vector2(x * grid.tileSize, (grid.height + y + 1) * grid.tileSize); 
                    
                    // Chỉ sinh materialPrefab
                    GameObject spawnedTile = Instantiate(grid.materialPrefab, spawnPosition, Quaternion.identity);
                    spawnedTile.transform.SetParent(this.transform);

                    Tile tileScript = spawnedTile.GetComponent<Tile>();

                    // Random loại tài nguyên
                    int randomTypeIndex = Random.Range(0, grid.resourceSprites.Length);
                    ResourceType randomType = (ResourceType)randomTypeIndex;

                    // Setup dữ liệu
                    tileScript.Setup(x, y, randomType, grid.resourceSprites[randomTypeIndex]);
                    tileScript.name = $"{randomType} {x},{y}";

                    // Lưu vào mảng
                    grid.gridArray[x, y] = tileScript;

                    // --- SỬA ĐỔI TẠI ĐÂY: KHÔNG DỊCH CHUYỂN TỨC THỜI NỮA ---
                    // Gán tọa độ đích và cho phép viên tài nguyên tự trượt xuống
                    Vector2 targetPos = new Vector2(x * grid.tileSize, y * grid.tileSize);
                    tileScript.targetPosition = targetPos;
                    tileScript.isMoving = true;
                }
            }
        }
    }
    #endregion


    #region Match Logic 

    #region Gộp match

    // Hàm xử lý Gộp tài nguyên
    public void ProcessMerge(List<Tile> matchedTiles, Tile targetTile)
    {
        foreach (Tile t in matchedTiles)
        {
            if (t != targetTile)
            {
                grid.gridArray[t.gridX, t.gridY] = null;
                Destroy(t.gameObject);
            }
        }

        targetTile.level++; 

        Tower myTower = targetTile.GetComponent<Tower>(); 
        if (myTower != null)
        {
            // TÌM DATABASE PHÙ HỢP VỚI LOẠI CỦA Ô NÀY
            TowerTypeData db = allTowerDatabases.Find(d => d.type == targetTile.type);
            
            if (db != null)
            {
                myTower.UpdateStats(targetTile.level, db);
                Debug.Log($"Nâng cấp {targetTile.type} lên cấp {targetTile.level}");
            }
            else
            {
                Debug.LogWarning($"Chưa cấu hình Database cho loại {targetTile.type}");
            }
        }  
    }
    
    // Hàm tìm các ô giống nhau liền kề tạo thành match-3
    public List<Tile> FindMatches(Tile startTile)
    {
        List<Tile> matchedTiles = new List<Tile>();
        List<Tile> horizontalMatches = new List<Tile>();
        List<Tile> verticalMatches = new List<Tile>();

        // 1. Kiểm tra hàng ngang (Trái & Phải)
        horizontalMatches.Add(startTile);

        // Quét sang TRÁI
        for (int x = startTile.gridX - 1; x >= 0; x--)
        {
            Tile nextTile = grid.gridArray[x, startTile.gridY];
            if (nextTile != null && nextTile.type == startTile.type && nextTile.level == startTile.level)
                horizontalMatches.Add(nextTile);
            else break; // Đứt đoạn thì dừng lại
        }
        // Quét sang PHẢI
        for (int x = startTile.gridX + 1; x < grid.width; x++)
        {
            Tile nextTile = grid.gridArray[x, startTile.gridY];
            if (nextTile != null && nextTile.type == startTile.type && nextTile.level == startTile.level)
                horizontalMatches.Add(nextTile);
            else break;
        }

        // Nếu hàng ngang có từ 3 ô trở lên -> Thêm vào danh sách match
        if (horizontalMatches.Count >= 3)
        {
            matchedTiles.AddRange(horizontalMatches);
        }

        // 2. Kiểm tra hàng dọc (Trên & Dưới)
        verticalMatches.Add(startTile);

        // Quét xuống DƯỚI
        for (int y = startTile.gridY - 1; y >= 0; y--)
        {
            Tile nextTile = grid.gridArray[startTile.gridX, y];
            if (nextTile != null && nextTile.type == startTile.type && nextTile.level == startTile.level)
                verticalMatches.Add(nextTile);
            else break;
        }
        // Quét lên TRÊN
        for (int y = startTile.gridY + 1; y < grid.height; y++)
        {
            Tile nextTile = grid.gridArray[startTile.gridX, y];
            if (nextTile != null && nextTile.type == startTile.type && nextTile.level == startTile.level)
                verticalMatches.Add(nextTile);
            else break;
        }

        // Nếu hàng dọc có từ 3 ô trở lên -> Thêm vào danh sách match
        if (verticalMatches.Count >= 3)
        {
            matchedTiles.AddRange(verticalMatches);
        }

        // Loại bỏ các ô bị trùng lặp (ví dụ ô startTile nằm ở cả dọc và ngang)
        return matchedTiles.Distinct().ToList();
    }

    #endregion


    #region Kiểm tra match

    // Hàm quét toàn bộ bàn cờ để tìm các chuỗi gộp tự động
    public bool CheckEntireBoard()
    {
        bool hasMatch = false;

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                Tile currentTile = grid.gridArray[x, y];
                if (currentTile == null) continue;

                List<Tile> matches = FindMatches(currentTile);
                if (matches.Count >= 3)
                {
                    ProcessMerge(matches, currentTile);
                    hasMatch = true;
                }
            }
        }

        return hasMatch; // Trả về true nếu có nổ, false nếu không
    }

    #endregion

    #endregion


    #region Coroutine

    // Coroutine giúp tạo độ trễ giữa các hành động
    public IEnumerator ProcessBoardRoutine()
    {
        // 1. Gọi Gravity để kéo các ô xuống
        ApplyGravity();
        
        // ĐỢI 0.3 GIÂY CHO CHÚNG TRƯỢT XUỐNG XONG
        yield return new WaitForSeconds(0.3f); 

        // 2. Sinh ô mới lấp đầy
        RefillBoard();
        
        // ĐỢI THÊM 0.3 GIÂY CHO Ô MỚI RƠI XUỐNG ĐẾN NƠI
        yield return new WaitForSeconds(0.3f);

        // 3. Quét toàn bộ bàn cờ xem có chuỗi nổ liên hoàn không
        bool hasCombo = CheckEntireBoard();

        if (hasCombo)
        {
            // Nếu có nổ combo, đợi 0.2 giây cho người chơi nhìn thấy
            yield return new WaitForSeconds(0.2f);
            
            // Rồi lặp lại toàn bộ quá trình này một lần nữa
            StartCoroutine(ProcessBoardRoutine());
        }

        else
        {
            WaveManager.Instance.CheckTurnEnd();
        }
    }

    #endregion


}
