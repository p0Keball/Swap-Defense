using UnityEngine;

public class Enemy : MonoBehaviour
{

    #region Inspector

    #region enemy profile

    [Header("Enemy Profile")]
    public string enemyName = "Quái Vật";
    public PathType pathType;       // Quái này đi đường ngang hay dọc?
    
    #endregion

    #region Conditions

    [Header("Conditions")]
    public int unlockAtWave = 1;    // Wave mấy thì bắt đầu xuất hiện?
    public int baseAmount = 3;      // Lúc mới mở khóa thì xuất hiện mấy con?
    
    #endregion
    
    #region Stats

    [Header("Chỉ số (Stats)")]
    public int baseHP = 10;         // Máu gốc ban đầu
    public float speed = 2f;        // Tốc độ di chuyển
    
    #endregion

    #endregion


    #region Biến Ẩn (Không hiện trên Inspector)
    
    private float currentHealth;
    private Transform[] path;
    private int currentWaypointIndex = 0;
    private float maxHealth; // Máu sau khi đã được WaveManager tính toán nâng cấp 
    private Animator anim;
    private bool isDead = false; // Biến để kiểm tra xem quái đã chết chưa, tránh gọi hàm TakeDamage sau khi đã chết
    
    #endregion


     void Start()
    {
        anim = GetComponent<Animator>();
    }
    
    // WaveManager sẽ gọi hàm này và truyền Máu Đã Nâng Cấp vào
    public void Setup(Transform[] waypoints, int finalHP)
    {
        path = waypoints;
        maxHealth = finalHP;
        currentHealth = maxHealth; // Đổ đầy máu

        // Đưa quái vật vào vị trí xuất phát (Point 0)
        if (path != null && path.Length > 0)
        {
            transform.position = path[0].position;
        }
    }

    void Update()
    {
        if (isDead) return; // Nếu đã chết thì không làm gì nữa

        // Nếu chưa có đường đi hoặc đã đi đến nơi -> Dừng lại
        if (path == null || currentWaypointIndex >= path.Length) return;

        // 1. Di chuyển về phía Waypoint hiện tại
        Transform targetWaypoint = path[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // 2. Nếu đã đi đến nơi -> Chuyển mục tiêu sang Waypoint tiếp theo
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= path.Length)
            {
                ReachBase();
            }
        }
    }

    void ReachBase()
    {
        Debug.Log($"{enemyName} đã vào lâu đài! Mất 1 máu!");
        // TODO: Trừ máu người chơi ở đây
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Debug.Log($"{enemyName} đã bị tiêu diệt!");
            anim.SetTrigger("isDead");
            isDead = true;
            Destroy(gameObject,1f);
        }
    }
}