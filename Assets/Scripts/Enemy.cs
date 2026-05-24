using UnityEngine;
using System.Collections;

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
    private float defaultSpeed;
    private bool isBurning = false;
    private bool isFrozen = false;
    private bool isSlowed = false;

    #endregion


    void Start()
    {
        anim = GetComponent<Animator>();
        defaultSpeed = speed;
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeCastleDamage(10f); // Giả sử mỗi con quái gây 10 sát thương
        }
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; 
        
        if (damage <= 0) return; 

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Debug.Log($"{enemyName} đã bị tiêu diệt!");
            anim.SetTrigger("isDead");
            isDead = true;
            Destroy(gameObject, 1f); 
        }
    }


    #region Hệ thống Hiệu ứng (Status Effects)

    public void ApplyEffect(ElementalEffect effect, float duration, float value)
    {
        if (isDead) return;

        switch (effect)
        {
            case ElementalEffect.Freeze:
                StartCoroutine(FreezeRoutine(duration, value));
                break;
            case ElementalEffect.Burn:
                StartCoroutine(BurnRoutine(duration, value));
                break;
            case ElementalEffect.Slow: // Thêm case mới
                StartCoroutine(SlowRoutine(duration, value));
                break;
        }
    }

    private IEnumerator SlowRoutine(float duration, float slowPercent)
    {
        if (isSlowed) yield break; 
        isSlowed = true;

        // Giảm tốc độ
        float speedBeforeSlow = speed;
        speed = defaultSpeed * (1f - slowPercent);
        
        // Đổi sang màu vàng đất hoặc cam để phân biệt với xanh của Freeze
        GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.3f); 

        yield return new WaitForSeconds(duration);

        // Trả lại màu sắc và tốc độ (kiểm tra nếu không bị đóng băng thì mới trả về default)
        if (!isFrozen) {
            speed = defaultSpeed;
            GetComponent<SpriteRenderer>().color = Color.white;
        }
        isSlowed = false;
    }

    // Hiệu ứng Đóng Băng: Giảm tốc độ di chuyển
    private IEnumerator FreezeRoutine(float duration, float slowPercent)
    {
        if (isFrozen) yield break; // Tránh cộng dồn nhiều lần đóng băng cùng lúc
        isFrozen = true;

        // Giảm tốc độ (ví dụ value = 0.6f là làm chậm đi 60%)
        speed = defaultSpeed * (1f - slowPercent);
        
        // Đổi sang màu xanh nhạt để người chơi dễ nhận biết
        GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.8f, 1f); 

        yield return new WaitForSeconds(duration);

        // Hết thời gian thì trả lại tốc độ và màu sắc ban đầu
        speed = defaultSpeed;
        GetComponent<SpriteRenderer>().color = Color.white;
        isFrozen = false;
    }

    // Hiệu ứng Lửa: Gây sát thương theo thời gian (Damage over Time)
    private IEnumerator BurnRoutine(float duration, float damagePerTick)
    {
        if (isBurning) yield break;
        isBurning = true;

        float elapsed = 0;
        
        // Đổi màu đỏ
        GetComponent<SpriteRenderer>().color = new Color(1f, 0.4f, 0.4f);

        while (elapsed < duration)
        {
            if (isDead) yield break; // Nếu quái chết giữa chừng thì ngưng đốt ngay lập tức

            TakeDamage(damagePerTick);
            elapsed += 1f; // Gây sát thương mỗi 1 giây
            
            yield return new WaitForSeconds(1f);
        }

        // Hết thời gian đốt thì trả về màu gốc
        if (!isDead)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
        isBurning = false;
    }

    #endregion

}