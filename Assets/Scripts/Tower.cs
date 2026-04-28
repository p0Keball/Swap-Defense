using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{

    #region Inspector

    #region Tower Settings

    [Header("Tower Settings")]

    [HideInInspector] public float range = 3f;           // Tầm bắn
    [HideInInspector] public float fireRate = 1f;        // Tốc độ bắn (1s bắn 1 viên)
    [HideInInspector] public float damage = 5f;             // Sát thương của tháp

    public bool isActiveTower = false; // Biến này để check xem ô này ĐÃ ĐƯỢC NÂNG CẤP thành tháp chưa
    public List<TowerLevelData> levelConfigs; // Danh sách data cho từng cấp

    #endregion

    #region Setup

    [Header("Setup")]
    public GameObject projectilePrefab; // Kéo thả Prefab đạn vào đây

    private float fireCountdown = 0f;

    #endregion

    #endregion

    private Animator anim;
    private SpriteRenderer iconRenderer;
    private RuntimeAnimatorController pendingController; // Biến tạm lưu Animator Controller mới khi nâng cấp tháp

    void Awake()
    {
        iconRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    public void UpdateStats(int currentLevel, TowerTypeData db)
    {
        if (db == null) return;

        if (currentLevel > 0 && currentLevel <= db.levelConfigs.Count)
        {
            TowerLevelData data = db.levelConfigs[currentLevel - 1];
            
            this.damage = data.damage;
            this.range = data.range;
            this.fireRate = data.fireRate;

            // 1. ĐỔI HÌNH ẢNH HIỂN THỊ NGAY LẬP TỨC KHI GHÉP
            if (data.StaticSprite != null)
            {
                iconRenderer.sprite = data.StaticSprite;
            }
            
            // 2. LƯU ANIMATION VÀO BIẾN TẠM (CHƯA CHẠY)
            pendingController = data.animatorController;

            // Tắt animator để không ghi đè lên Sprite tĩnh vừa đổi
            if (anim != null) anim.enabled = false;

            isActiveTower = (data.damage > 0);
        }
    }

    void Update()
    {
        if (!isActiveTower) return;

        Transform target = FindClosestEnemy();

        // LOGIC ĐIỀU KHIỂN ANIMATION THEO ENEMY
        if (anim != null && pendingController != null)
        {
            if (target != null)
            {
                // Có địch: Bật Animator để chạy hành động tấn công
                if (anim.runtimeAnimatorController != pendingController)
                {
                    anim.runtimeAnimatorController = pendingController;
                }
                anim.enabled = true;
            }
            else
            {
                // Không có địch: Tắt Animator để hiện lại Sprite tĩnh (levelSprite)
                anim.enabled = false;
            }
        }

        // Logic bắn đạn...
        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f && target != null)
        {
            Shoot(target);
            fireCountdown = 1f / fireRate;
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