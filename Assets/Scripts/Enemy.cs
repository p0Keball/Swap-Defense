using UnityEngine;

public class Enemy : MonoBehaviour
{

    #region Inspector

    [Header("Stats")]
    public float speed = 2f;
    public int maxHealth = 10;
    private int currentHealth;

    private Transform[] path;
    private int currentWaypointIndex = 0;

    #endregion
    
    // Hàm này được GridManager gọi khi vừa sinh ra quái
    public void Setup(Transform[] waypoints, int hp)
    {
        path = waypoints;
        maxHealth = hp;
        currentHealth = maxHealth;

        // Đưa quái vật vào vị trí xuất phát (Point 0)
        if (path != null && path.Length > 0)
        {
            transform.position = path[0].position;
        }
    }

    void Update()
    {
        // Nếu chưa có đường đi hoặc đã đi đến nơi -> Dừng lại
        if (path == null || currentWaypointIndex >= path.Length) return;

        // 1. Di chuyển về phía Waypoint hiện tại
        Transform targetWaypoint = path[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // 2. Nếu đã đi đến nơi (khoảng cách cực nhỏ) -> Chuyển mục tiêu sang Waypoint tiếp theo
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;
            
            // Nếu đã đi hết đường (đến lâu đài)
            if (currentWaypointIndex >= path.Length)
            {
                ReachBase();
            }
        }
    }

    void ReachBase()
    {
        Debug.Log("Quái vật đã vào lâu đài! Mất 1 máu!");
        // TODO: Trừ máu người chơi ở đây
        
        Destroy(gameObject); // Tự hủy
    }

    // Tạm thời viết sẵn hàm nhận sát thương để lát nữa Tháp bắn
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Debug.Log("Quái vật đã bị tiêu diệt!");
            Destroy(gameObject);
        }
    }

}