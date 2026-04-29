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

    
    void Awake()
    {
        
        anim = GetComponent<Animator>();
    }

 
    void Update()
    {
        if (!isActiveTower) return;

        Transform target = FindClosestEnemy();

        if (anim != null && pendingController != null)
        {
            if (target != null)
            {
                if (anim.runtimeAnimatorController != pendingController)
                {
                    anim.runtimeAnimatorController = pendingController;
                }
                anim.enabled = true;
            }

            else
            {
                // Khi không có địch: Tắt Animator và trả về Sprite tĩnh (StaticSprite)
                anim.enabled = false;
                if (currentData != null && currentData.StaticSprite != null) {
                    GetComponent<SpriteRenderer>().sprite = currentData.StaticSprite;
                }
            }
        }

        if (target != null)
        {
            // Giảm thời gian chờ giữa 2 lần bắn
            fireCountdown -= Time.deltaTime;

            // Nếu đã hết thời gian chờ
            if (fireCountdown <= 0f)
            {
                Shoot(target); // Thực hiện bắn đạn
                
                // Reset lại bộ đếm dựa trên tốc độ bắn (fireRate)
                // Công thức: 1 / fireRate (Ví dụ: fireRate = 2 thì 0.5s bắn 1 lần)
                fireCountdown = 1f / fireRate;
            }
        }
        else
        {
            // Nếu không có địch, có thể reset countdown về 0 để khi địch vừa vào tầm là bắn ngay
            fireCountdown = 0f;
        }
    }
    

    #region Cập nhập chỉ số tháp khi nâng cấp

    private Animator anim;
    private RuntimeAnimatorController pendingController; // Biến tạm lưu Animator Controller mới khi nâng cấp tháp
    

    private TowerLevelData currentData; // Lưu dữ liệu của level hiện tại

    public void UpdateStats(int currentLevel, TowerTypeData db) {
        if (db == null || currentLevel <= 0 || currentLevel > db.levelConfigs.Count) return;
        currentData = db.levelConfigs[currentLevel - 1];

        this.damage = currentData.damage;
        this.range = currentData.range;
        this.fireRate = currentData.fireRate;
        this.pendingController = currentData.animatorController;

        // Hiển thị Sprite tĩnh ban đầu
        if (currentData.StaticSprite != null) {
            GetComponent<SpriteRenderer>().sprite = currentData.StaticSprite;
        }

        isActiveTower = (this.damage > 0);
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
        GameObject bulletGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = bulletGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            // Truyền thêm thông tin hiệu ứng từ currentData
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

        // 1. Xác định dữ liệu sẽ dùng để vẽ
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