using UnityEngine;

public enum AttackRangeType { Circle, Horizontal, Vertical, Cross }
public enum ElementalEffect { None, Freeze, Burn, Poison }

[CreateAssetMenu(fileName = "TowerLevelData", menuName = "Scriptable Objects/TowerLevelData")]
public class TowerLevelData : ScriptableObject
{

    #region Stats

    [Header("Stats")]
    public int level;
    public float damage;
    public float range;
    public float fireRate;
    public int swapAddAmount;

    #endregion


    #region Tower Modify

    [Header("Tower Modify")]
    public AttackRangeType rangeType; // Loại vùng bắn
    public ElementalEffect effectType; // Hiệu ứng đi kèm
    public float effectDuration; // Thời gian tác dụng (giây)
    public float effectValue; // Giá trị hiệu ứng (ví dụ: giảm 50% tốc độ)

    #endregion


    #region Visuals

    [Header("Visuals")]
    public RuntimeAnimatorController animatorController;
    public Sprite StaticSprite; 

    #endregion

}