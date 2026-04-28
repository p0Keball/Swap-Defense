using UnityEngine;

// Định nghĩa các loại tài nguyên có trong game
public enum ResourceType
{
    Wood,       
    LimeDust,      
    Leather,  
    IceBar,        
    GoldBar       
}

public class Tile : MonoBehaviour
{

    #region Inspector

    #region Tile Data

    [Header("Tile Data")]
    public ResourceType type;
    public int level = 1; 
    public int gridX;
    public int gridY;

    #endregion

    #region Visuals

    [Header("Visuals")]
    [Tooltip("Kéo thả GameObject con chứa Sprite của vật phẩm vào đây")]
    
    public Vector2 targetPosition; // Vị trí nó cần trượt tới
    public bool isMoving = false;  // Nó có đang trượt không?
    public float fallSpeed = 15f;
    public SpriteRenderer iconRenderer; // Chỉ thay đổi ảnh ở đây, giữ nguyên ảnh nền
    
    #endregion
 
    #endregion

    // Hàm này được gọi từ GameManager.cs
    public void Setup(int x, int y, ResourceType newType, Sprite newSprite)
    {
        gridX = x;
        gridY = y;
        type = newType;
        
        // Gán hình ảnh tài nguyên vào lớp Icon
        if (iconRenderer != null)
        {
            iconRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogError("Bạn chưa kéo thả Icon vào biến iconRenderer trong Prefab kìa!");
        }
    }

    // Hàm có sẵn của Unity, tự động chạy khi chuột click vào Box Collider 2D
    void OnMouseDown()
    {
        // Báo cho GameManager biết ô này vừa bị click
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTileClicked(this);
        }
    }

    // Hàm Update chạy liên tục mỗi khung hình
    void Update()
    {
        if (isMoving)
        {
            // Trượt mượt mà từ vị trí hiện tại đến targetPosition
            transform.position = Vector2.Lerp(transform.position, targetPosition, fallSpeed * Time.deltaTime);

            // Khi đã trượt đến sát đích -> Gắn chặt vào đích và dừng lại
            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
        
        if (GameManager.Instance != null)
        {
            // Tính toán vị trí mà ô này ĐÁNG LẼ phải nằm
            Vector2 targetPosition = new Vector2(gridX * GridManager.Instance.tileSize, gridY * GridManager.Instance.tileSize);
            
            // Lướt nhẹ nhàng từ vị trí hiện tại đến vị trí đích
            transform.position = Vector2.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
        }
    }
}