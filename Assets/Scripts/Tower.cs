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
    private float fireCountdown = 0f;

    #endregion

    #endregion

    private Animator anim;

    void Awake()
    {
        
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isActiveTower) return;

        // Tìm kẻ địch gần nhất trong tầm  
        Transform target = FindClosestEnemy();

        // Logic xử lý Animator 
        HandleAnimation(target);

        if (target != null)
        {
            fireCountdown -= Time.deltaTime;

            if (fireCountdown <= 0f)
            {
                AudioClip shootSoundToPlay = null;

                // 1. Kiểm tra cấp độ hiện tại (currentData) xem có nhạc bắn riêng không
                if (currentData != null)
                {
                    shootSoundToPlay = currentData.shootSFX;
                }
            
                // 2. MẸO TỰ ĐỘNG: Nếu cấp hiện tại trống nhạc, tự lục lại cấu hình Cấp 1 (phần tử 0) để lấy nhạc mặc định
                if (shootSoundToPlay == null && levelConfigs != null && levelConfigs.Count > 0 && levelConfigs[0] != null)
                {
                    shootSoundToPlay = levelConfigs[0].shootSFX;
                }
            
                // 3. Ra lệnh cho SoundManager phát nhạc
                if (shootSoundToPlay != null)
                {
                    SoundManager.Instance.PlaySFX(shootSoundToPlay);
                }

                  

                // KIỂM TRA: Nếu có Prefab đạn thì bắn, nếu không thì gây hiệu ứng trực tiếp
                if (currentData.projectilePrefab != null)
                {
                    Shoot(target);
                }
                else
                {
                    // Đây là Bẫy hoặc Tháp hào quang: Gây sát thương/hiệu ứng trực tiếp
                    ApplyTrapEffect(target);
                }

                fireCountdown = 1f / fireRate;
            }
        }
        else
        {
            fireCountdown = 0f;
        }
    }

    // Hàm bổ sung để xử lý sát thương trực tiếp cho Bẫy
    void ApplyTrapEffect(Transform target)
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Gây sát thương trực tiếp (damage có thể bằng 0 nếu chỉ muốn áp dụng hiệu ứng)
            enemy.TakeDamage(damage);

            // Áp dụng hiệu ứng nguyên tố (nếu có)
            if (currentData.effectType != ElementalEffect.None)
            {
                enemy.ApplyEffect(currentData.effectType, currentData.effectDuration, currentData.effectValue);
            }
            
            Debug.Log($"Bẫy {gameObject.name} đã kích hoạt lên {enemy.enemyName}");
        }
    }

    // Tách logic Animation ra cho gọn  
    void HandleAnimation(Transform target)
    {
        if (anim != null && pendingController != null)
        {
            if (target != null)
            {
                if (anim.runtimeAnimatorController != pendingController)
                    anim.runtimeAnimatorController = pendingController;
                anim.enabled = true;
            }
            else
            {
                anim.enabled = false;
                if (currentData != null && currentData.StaticSprite != null)
                    GetComponent<SpriteRenderer>().sprite = currentData.StaticSprite;
            }
        }
    }
    

    #region Cập nhập chỉ số tháp khi nâng cấp
    
    private RuntimeAnimatorController pendingController; // Biến tạm lưu Animator Controller mới khi nâng cấp tháp

    private TowerLevelData currentData; // Lưu dữ liệu của level hiện tại

    public void UpdateStats(int currentLevel, TowerTypeData db) {
    if (db == null || currentLevel <= 0 || currentLevel > db.levelConfigs.Count) return;
    currentData = db.levelConfigs[currentLevel - 1];

    this.damage = currentData.damage;
    this.range = currentData.range;
    this.fireRate = currentData.fireRate;
    this.pendingController = currentData.animatorController;

    if (currentData.StaticSprite != null) {
        GetComponent<SpriteRenderer>().sprite = currentData.StaticSprite;
    }

    // Kích hoạt tháp dựa trên RangeType như bạn muốn
    isActiveTower = (currentData.rangeType != AttackRangeType.None);
    
    if (anim != null) anim.enabled = false;
    }
    
    #endregion


    #region Click chest

    private Vector3 mousePosStart;
    private float clickTimeStart;

    private void OnMouseDown() {
        mousePosStart = Input.mousePosition;
        clickTimeStart = Time.time;
    }

    private void OnMouseUp() {
        // Tính khoảng cách chuột đã di chuyển
        float moveDistance = Vector3.Distance(mousePosStart, Input.mousePosition);
        float clickDuration = Time.time - clickTimeStart;

        // Nếu di chuyển rất ít (< 10 pixel) và thả tay nhanh (< 0.2s) -> Đó là CLICK
        if (moveDistance < 10f && clickDuration < 0.2f) {
            if (currentData != null && currentData.swapAddAmount > 0) {
                AddSwapTurns();
            }
        }
        // Nếu di chuyển nhiều hơn -> Tile.cs sẽ tự xử lý logic Drag/Swap của nó
    }

    public void AddSwapTurns()
    {
        // logic cộng lượt 
        WaveManager.Instance.turnsLeft += currentData.swapAddAmount;
        UIManager.Instance.UpdateSwapCount(WaveManager.Instance.turnsLeft);

        Material tile = GetComponent<Material>();
        if (tile != null)
        {
            //  Phải gán null trong mảng trước để MatchManager nhận diện ô trống
            GridManager.Instance.gridArray[tile.gridX, tile.gridY] = null;
        }

        // QUAN TRỌNG: Gọi Coroutine thông qua MatchManager.Instance
        // Như vậy Coroutine sẽ chạy trên MatchManager (không bị xóa) thay vì Tower (sắp bị xóa)
        if (MatchManager.Instance != null) {
            MatchManager.Instance.StartCoroutine(MatchManager.Instance.ProcessBoardRoutine());
        }

        // Xóa rương
        Destroy(gameObject);
    }

    #endregion


    #region Tìm kiếm mục tiêu và bắn đạn

    Transform FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Transform shortestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            bool isInRange = false;

            // Kiểm tra loại vùng bắn
            switch (currentData.rangeType)
            {
                case AttackRangeType.Circle:
                    isInRange = distanceToEnemy <= range;
                    break;
                case AttackRangeType.Horizontal:
                    // Sai số y nhỏ (0.5) để chấp nhận các địch nằm cùng hàng
                    isInRange = Mathf.Abs(enemy.transform.position.y - transform.position.y) < 0.5f 
                                && distanceToEnemy <= range;
                    break;
                case AttackRangeType.Vertical:
                    isInRange = Mathf.Abs(enemy.transform.position.x - transform.position.x) < 0.5f 
                                && distanceToEnemy <= range;
                    break;
            }

            if (isInRange && distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                shortestTarget = enemy.transform;
            }
        }
        return shortestTarget;
    }

    // Cập nhật hàm Shoot để truyền hiệu ứng  
    void Shoot(Transform target)
    {
        // Kiểm tra xem data hiện tại có đạn không để tránh lỗi
        if (currentData == null || currentData.projectilePrefab == null) 
        {
            Debug.LogWarning("Tháp chưa được gắn Prefab đạn trong TowerLevelData!");
            return;
        }

        // Sinh ra viên đạn dựa trên dữ liệu của cấp độ HIỆN TẠI
        GameObject bulletGO = Instantiate(currentData.projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = bulletGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            // Truyền mục tiêu và sát thương (cộng thêm các hiệu ứng nếu có) cho viên đạn tự đuổi
            projectile.Seek(target, damage, currentData.effectType, currentData.effectDuration, currentData.effectValue);
        }
    }

    #endregion


    #region Vẽ phạm vi tấn công trong Scene View

    void OnDrawGizmosSelected()
    {
        // Chỉnh màu đỏ hơi trong suốt để không bị rối mắt
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); 

        float drawRange = range;
        AttackRangeType drawType = AttackRangeType.Circle; // Mặc định là hình tròn

        // Xác định dữ liệu sẽ dùng để vẽ
        if (currentData != null)
        {
            // Nếu game đang chạy và tháp đã được gán data
            drawRange = currentData.range;
            drawType = currentData.rangeType;
        }
        else if (levelConfigs != null && levelConfigs.Count > 0 && levelConfigs[0] != null)
        {
            // Nếu ở Edit Mode (chưa Play game), lấy tạm thông số Level 1 để hiển thị
            drawRange = levelConfigs[0].range;
            drawType = levelConfigs[0].rangeType;
        }

        // 2. Vẽ hình dáng vùng tấn công tương ứng
        switch (drawType)
        {
            case AttackRangeType.None:
                // Không vẽ gì nếu không có loại vùng bắn
                break;

            case AttackRangeType.Circle:
                // Vẽ hình tròn bán kính = drawRange
                Gizmos.DrawWireSphere(transform.position, drawRange);
                break;

            case AttackRangeType.Horizontal:
                // Vẽ hình hộp chữ nhật nằm ngang
                // Chiều dài (X) = drawRange * 2 (trái và phải)
                // Chiều cao (Y) = 1f (tương ứng với sai số 0.5f lên và xuống mà ta check ở FindClosestEnemy)
                Gizmos.DrawWireCube(transform.position, new Vector3(drawRange * 2f, 1f, 0f));
                break;

            case AttackRangeType.Vertical:
                // Vẽ hình hộp chữ nhật nằm dọc
                // Chiều rộng (X) = 1f, Chiều cao (Y) = drawRange * 2
                Gizmos.DrawWireCube(transform.position, new Vector3(1f, drawRange * 2f, 0f));
                break;

            case AttackRangeType.Cross:
                // Vẽ hình chữ thập (kết hợp cả ngang và dọc)
                Gizmos.DrawWireCube(transform.position, new Vector3(drawRange * 2f, 1f, 0f));
                Gizmos.DrawWireCube(transform.position, new Vector3(1f, drawRange * 2f, 0f));
                break;
        }
    }

    #endregion

}