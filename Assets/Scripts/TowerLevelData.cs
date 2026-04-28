using UnityEngine;

[CreateAssetMenu(fileName = "TowerLevelData", menuName = "Scriptable Objects/TowerLevelData")]
public class TowerLevelData : ScriptableObject
{
    public int level;
    public RuntimeAnimatorController animatorController;
    public Sprite StaticSprite; 
    public float damage;
    public float range;
    public float fireRate;
}
