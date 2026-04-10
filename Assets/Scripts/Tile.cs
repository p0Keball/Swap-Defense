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

    #endregion

     
    #endregion

    public SpriteRenderer iconRenderer; // Chỉ thay đổi ảnh ở đây, giữ nguyên ảnh nền

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
        if (GameManager.Instance != null)
        {
            // Tính toán vị trí mà ô này ĐÁNG LẼ phải nằm
            Vector2 targetPosition = new Vector2(gridX * GridManager.Instance.tileSize, gridY * GridManager.Instance.tileSize);
            
            // Lướt nhẹ nhàng từ vị trí hiện tại đến vị trí đích
            transform.position = Vector2.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
        }
    }
}