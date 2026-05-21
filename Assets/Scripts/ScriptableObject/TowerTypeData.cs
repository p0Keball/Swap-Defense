using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TowerTypeData", menuName = "Scriptable Objects/TowerTypeData")]
public class TowerTypeData : ScriptableObject
{
    public ResourceType type; // Loại tài nguyên (Wood, GoldBar...)
    public List<TowerLevelData> levelConfigs; // Danh sách các cấp độ của tháp này
}
