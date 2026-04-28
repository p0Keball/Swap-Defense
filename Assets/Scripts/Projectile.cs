using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Tốc độ bay của đạn
    private Transform target; // Mục tiêu đang theo dõi
    private float damage;       // Sát thương của viên đạn này

    // Hàm này được Tháp gọi khi vừa bắn đạn ra
    public void Seek(Transform _target, float _damage)
    {
        target = _target;
        damage = _damage;
    }

    void Update()
    {
        // Nếu quái đã chết trước khi đạn bay tới -> Hủy viên đạn luôn
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Tính khoảng cách và hướng bay tới mục tiêu
        Vector2 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Nếu khoảng cách đến quái nhỏ hơn quãng đường bay trong 1 khung hình -> Đã trúng mục tiêu!
        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // Bay về phía mục tiêu
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        // Lấy script Enemy và gọi hàm trừ máu mà chúng ta đã viết từ trước
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Hiệu ứng nổ nhỏ (nếu có thể thêm sau)
        // Bắn trúng xong thì tự hủy viên đạn
        Destroy(gameObject);
    }
}