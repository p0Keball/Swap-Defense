using UnityEngine;

public class Projectile : MonoBehaviour
{

    #region Inspector

    #region Projectile Settings

    [Header("Projectile Settings")]
    public float speed = 10f; // Tốc độ bay của đạn

    #endregion

    #endregion


    public static Projectile Instance;

    private ElementalEffect effect;
    private float effectDuration;
    private float effectValue;

    private Transform target; // Mục tiêu đang theo dõi
    private float damage;       // Sát thương của viên đạn này

    // Hàm này được Tháp gọi khi vừa bắn đạn ra
    public void Seek(Transform _target, float _damage, ElementalEffect _effect, float _duration, float _value)
    {
        target = _target;
        damage = _damage;
        effect = _effect;
        effectDuration = _duration;
        effectValue = _value;
    }

    void HitTarget()
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            
            // Áp dụng hiệu ứng nếu có
            if (effect != ElementalEffect.None)
            {
                enemy.ApplyEffect(effect, effectDuration, effectValue);
            }
        }
        Destroy(gameObject);
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

}