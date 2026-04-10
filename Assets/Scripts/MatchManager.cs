using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class MatchManager : MonoBehaviour
{

    public static MatchManager Instance;
    private GridManager grid;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this; 
        grid = GetComponent<GridManager>(); // Lấy tham chiếu sang GridManager
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #region Refill board

    // làm các ô rơi xuống lấp đầy khoảng trống
    void ApplyGravity()
    {
        // Duyệt qua từng cột (x)
        for (int x = 0; x < grid.width; x++)
        {
            // Duyệt từ dưới lên trên (y)
            for (int y = 0; y < grid.height; y++)
            {
                // Nếu phát hiện một ô trống (ô đã bị xóa sau khi Merge)
                if (grid.gridArray[x, y] == null)
                {
                    // Tìm lên các ô phía trên nó (k)
                    for (int k = y + 1; k < grid.height; k++)
                    {
                        // Nếu thấy có một ô chứa tài nguyên
                        if (grid.gridArray[x, k] != null)
                        {
                            // Lấy ô đó kéo xuống vị trí trống (y)
                            Tile tileToFall = grid.gridArray[x, k];
                            
                            // Cập nhật lại mảng dữ liệu
                            grid.gridArray[x, y] = tileToFall;
                            grid.gridArray[x, k] = null;
                            
                            // Cập nhật tọa độ gridY cho ô đó
                            tileToFall.gridY = y;                           
                            
                            // Cập nhật tên để dễ Debug
                            tileToFall.name = $"{tileToFall.type} Lvl {tileToFall.level} {x},{y}";
                            
                            // Đã tìm thấy và kéo xuống rồi thì dừng việc tìm kiếm ở phía trên, chuyển sang y tiếp theo
                            break; 
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
                // Tìm những ô trống (lúc này chắc chắn đang nằm ở trên cùng)
                if (grid.gridArray[x, y] == null)
                {
                    // Cộng thêm 'height' để nó sinh ra ở tít trên trần nhà
                    Vector2 spawnPosition = new Vector2(x * grid.tileSize, (y + grid.height) * grid.tileSize);
                    
                    // Tạo GameObject mới từ Prefab
                    GameObject spawnedTile = Instantiate(grid.tilePrefab, spawnPosition, Quaternion.identity);
                    spawnedTile.transform.SetParent(this.transform);

                    Tile tileScript = spawnedTile.GetComponent<Tile>();

                    // Random tài nguyên mới (Cấp 1)
                    int randomTypeIndex = Random.Range(0, grid.resourceSprites.Length);
                    ResourceType randomType = (ResourceType)randomTypeIndex;

                    tileScript.Setup(x, y, randomType, grid.resourceSprites[randomTypeIndex]);
                    tileScript.name = $"{randomType} {x},{y}";

                    // Lưu vào mảng
                    grid.gridArray[x, y] = tileScript;
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
            // Nếu không phải là ô đích (ô người chơi vừa kéo tới) thì xóa đi
            if (t != targetTile)
            {
                // Xóa khỏi mảng dữ liệu
                grid.gridArray[t.gridX, t.gridY] = null;
                
                // Phá hủy GameObject trên màn hình
                Destroy(t.gameObject);
            }
        }

        // --- NÂNG CẤP Ô ĐÍCH ---
        targetTile.level++; 
        targetTile.name = $"{targetTile.type} Lvl {targetTile.level} {targetTile.gridX},{targetTile.gridY}";

        // Tạm thời: Đổi màu ô đó sang màu Vàng để bạn dễ nhận biết nó đã lên cấp 2
        targetTile.iconRenderer.color = Color.yellow; 

        if (targetTile.level >= 2)
        {
            // LẤY COMPONENT TOWER TỪ Ô TARGET TILE (Đừng quên chữ targetTile nhé!)
            Tower myTower = targetTile.GetComponent<Tower>(); 
            
            if (myTower != null)
            {
                myTower.isActiveTower = true; // BẬT CHẾ ĐỘ BẮN!
                // Tùy chọn: Level càng cao sát thương càng mạnh
                myTower.damage = targetTile.level * 5; 
                
                Debug.Log($"Tháp ở {targetTile.gridX},{targetTile.gridY} đã ĐƯỢC KÍCH HOẠT! Sát thương: {myTower.damage}");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy script Tower trên Prefab Tile! Bạn đã gắn Tower.cs vào Tile chưa?");
            }
        }

        Debug.Log($"Tuyệt vời! Đã gộp thành công {targetTile.type} lên cấp {targetTile.level}!");
    
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
