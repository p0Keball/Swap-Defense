using UnityEngine;

// 1. Khai báo các "Nhóm đường đi" (Bạn có thể thêm tùy ý)
public enum PathType 
{ 
    Horizontal, // Đường hàng ngang (cho Kỵ sĩ)
    Vertical    // Đường cột dọc (cho Dơi)
}

public class PathFinding : MonoBehaviour
{    

    #region Inspector

    [Header("Path List")]
    public Transform[] horizontalPaths; // Lát nữa kéo các đường ngang vào đây
    public Transform[] verticalPaths;   // Lát nữa kéo các đường dọc vào đây

    #endregion
   
    public static PathFinding Instance;
    
    void Start()
    {
        Instance = this;
    }

    // Nâng cấp: Truyền loại đường muốn lấy vào hàm
    public Transform[] GetRandomPathWaypoints(PathType requestedType)
    {
        Transform[] selectedPool = null;

        // 2. Tùy vào yêu cầu mà bốc đúng "giỏ" đường đi
        if (requestedType == PathType.Horizontal) 
        {
            selectedPool = horizontalPaths;
        }
        else if (requestedType == PathType.Vertical) 
        {
            selectedPool = verticalPaths;
        }

        if (selectedPool == null || selectedPool.Length == 0)
        {
            Debug.LogError($"Chưa có đường đi nào trong nhóm {requestedType}!");
            return null;
        }

        // 3. Lấy ngẫu nhiên 1 đường trong đúng cái "giỏ" đó
        int randomIndex = Random.Range(0, selectedPool.Length);
        Transform selectedPath = selectedPool[randomIndex];

        // 4. Lấy các Waypoint (điểm nối) như cũ
        Transform[] waypoints = new Transform[selectedPath.childCount];
        for (int i = 0; i < selectedPath.childCount; i++)
        {
            waypoints[i] = selectedPath.GetChild(i);
        }

        return waypoints;
    }
}