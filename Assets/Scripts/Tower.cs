using UnityEngine;

public class Tower : MonoBehaviour
{

    #region Inspector

    #region Tower Settings

    [Header("Tower Settings")]
    public float range = 3f;           // Tầm bắn
    public float fireRate = 1f;        // Tốc độ bắn (1s bắn 1 viên)
    public int damage = 5;             // Sát thương của tháp
    public bool isActiveTower = false; // Biến này để check xem ô này ĐÃ ĐƯỢC NÂNG CẤP thành tháp chưa

    #endregion

    #region Setup

    [Header("Setup")]
    public GameObject projectilePrefab; // Kéo thả Prefab đạn vào đây

    private float fireCountdown = 0f;

    #endregion

    #endregion

    void Update()
    {
        // Nếu ô này chỉ là tài nguyên cùi (cấp 1) -> Không làm gì cả
        if (!isActiveTower) return;

        // Đếm ngược thời gian bắn
        fireCountdown -= Time.deltaTime;
        
        // Nếu đã đến lúc bắn
        if (fireCountdown <= 0f)
        {
            Transform target = FindClosestEnemy();
            if (target != null)
            {
                Shoot(target);
                fireCountdown = 1f / fireRate; // Reset lại bộ đếm thời gian
            }
        }
    }

    Transform FindClosestEnemy()
    {
        // Tìm TẤT CẢ quái vật đang có trên bản đồ
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        Transform shortestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Kiểm tra xem con nào gần mình nhất
        foreach (Enemy enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                shortestTarget = enemy.transform;
            }
        }

        // Nếu con gần nhất đó nằm TRONG TẦM BẮN -> Chọn nó làm mục tiêu
        if (shortestTarget != null && shortestDistance <= range)
        {
            return shortestTarget;
        }

        return null; // Không có con nào trong tầm ngắm
    }

    void Shoot(Transform target)
    {
        // Sinh ra viên đạn ngay tại vị trí của Tháp
        GameObject bulletGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = bulletGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Seek(target, damage); // Truyền mục tiêu cho viên đạn tự đuổi
        }
    }

    // Hàm này giúp bạn vẽ một vòng tròn màu đỏ trong Scene để dễ canh tầm bắn
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}