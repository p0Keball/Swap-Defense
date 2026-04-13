using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour
{   

    #region Inspector

     

    
    #endregion
    
    //Singleton
    public static GameManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    #region xử lý click chuột

    // BIẾN LƯU TRỮ Ô ĐANG ĐƯỢC CHỌN 
    private Tile firstSelectedTile;

    public void OnTileClicked(Tile clickedTile)
    {
        // Nếu chưa chọn ô nào, thì ô vừa click sẽ là ô thứ 1
        if (firstSelectedTile == null)
        {
            firstSelectedTile = clickedTile;
            // (Tuỳ chọn) Bạn có thể thêm code đổi màu ô ở đây để báo hiệu đang chọn
        }

        else
        {
            // Nếu click lại chính ô đó -> Bỏ chọn
            if (firstSelectedTile == clickedTile)
            {
                firstSelectedTile = null;
                return;
            }

            // Nếu đã có ô thứ 1, kiểm tra xem ô thứ 2 có nằm CẠNH NHAU không
            if (IsAdjacent(firstSelectedTile, clickedTile))
            {
                SwapTiles(firstSelectedTile, clickedTile);
            }
            
            // Dù swap thành công hay thất bại, đều reset lại ô đã chọn
            firstSelectedTile = null; 
        }
    }

    // Kiểm tra 2 ô có kề sát nhau (trên, dưới, trái, phải) không
    bool IsAdjacent(Tile tile1, Tile tile2)
    {
        int dx = Mathf.Abs(tile1.gridX - tile2.gridX);
        int dy = Mathf.Abs(tile1.gridY - tile2.gridY);

        // Nằm cạnh nhau khi tổng khoảng cách X và Y bằng 1
        return (dx + dy) == 1;
    }

    #endregion


    #region Swap

    // Hàm thực hiện tráo đổi 2 ô
    public void SwapTiles(Tile tile1, Tile tile2)
    {
        // 1. Đổi vị trí Transform (Hiện thị trên màn hình)
        Vector3 tempPosition = tile1.transform.position;
        tile1.transform.position = tile2.transform.position;
        tile2.transform.position = tempPosition;

        // 2. Đổi toạ độ Grid X, Y trong class Tile
        int tempX = tile1.gridX;
        int tempY = tile1.gridY;
        
        tile1.gridX = tile2.gridX;
        tile1.gridY = tile2.gridY;
        
        tile2.gridX = tempX;
        tile2.gridY = tempY;

        // 3. Cập nhật lại vị trí của chúng trong mảng gridArray
        GridManager.Instance.gridArray[tile1.gridX, tile1.gridY] = tile1;
        GridManager.Instance.gridArray[tile2.gridX, tile2.gridY] = tile2;

        // Đổi tên để dễ debug
        tile1.name = $"{tile1.type} {tile1.gridX},{tile1.gridY}";
        tile2.name = $"{tile2.type} {tile2.gridX},{tile2.gridY}";

        List<Tile> allMatches = new List<Tile>();

        bool hasMergeHappened = false; // Thêm biến này để theo dõi

        // Quét ô thứ 1
        List<Tile> match1 = MatchManager.Instance.FindMatches(tile1);
        if (match1.Count >= 3)
        {
            MatchManager.Instance.ProcessMerge(match1, tile1);
            hasMergeHappened = true; // Báo hiệu là có gộp
        }

        // Quét ô thứ 2
        List<Tile> match2 = MatchManager.Instance.FindMatches(tile2);
        if (match2.Count >= 3)
        {
            MatchManager.Instance.ProcessMerge(match2, tile2);
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