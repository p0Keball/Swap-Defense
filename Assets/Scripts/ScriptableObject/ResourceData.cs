using UnityEngine;

[CreateAssetMenu(fileName = "NewResource", menuName = "Scriptable Objects/ResourceData")]
public class ResourceData : ScriptableObject
{
    [Header("Resource Info")]
    public string resourceName;   // Tên tài nguyên  
    public Sprite resourceSprite; // Hình ảnh tài nguyên
}