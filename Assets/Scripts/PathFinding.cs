using UnityEngine;

public class PathFinding : MonoBehaviour
{

    #region Inspector

    [Header("Path Setting")]
    public Transform[] pathList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    #endregion

    public static PathFinding Instance;

    void Start()
    {
        Instance=this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Hàm này trả về danh sách các điểm (waypoints) của 1 con đường ngẫu nhiên
    public Transform[] GetRandomPathWaypoints()
    {
        // Kiểm tra xem bạn đã kéo thả Path vào chưa
        if (pathList == null || pathList.Length == 0)
        {
            Debug.LogError("Chưa có đường đi nào trong danh sách pathList!");
            return null;
        }

        // 1. Chọn ngẫu nhiên 1 vị trí (index) từ 0 đến tổng số lượng đường đi
        int randomIndex = Random.Range(0, pathList.Length);
        Transform selectedPath = pathList[randomIndex];

        // 2. Lấy tất cả các điểm con (waypoints) của con đường vừa chọn
        Transform[] waypoints = new Transform[selectedPath.childCount];
        for (int i = 0; i < selectedPath.childCount; i++)
        {
            waypoints[i] = selectedPath.GetChild(i);
        }

        return waypoints;
    }
}
