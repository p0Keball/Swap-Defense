using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{

    #region Inspector

    #region Game State

    [Header("Game State")]
    public int currentWave = 0;      
    public int turnsLeft = 5;        
    public int maxTurnsPerWave = 5;  

    #endregion
    
    #region Enemy Settings

    [Header("Enemy Settings")]
    [Tooltip("Chỉ cần kéo thả trực tiếp các Prefab quái vật vào đây")]
    public Enemy[] enemyPrefabs; // Mảng này giờ chỉ chứa script Enemy từ Prefab
    public float spawnDelay = 1.5f; 

    #endregion

    #endregion

    public static WaveManager Instance;

    void Start()
    {
        Instance = this;
    }

    public void CheckTurnEnd() 
    {
        if (turnsLeft <= 0) 
        {
            TriggerNextWave();
        }
    }

    void TriggerNextWave()
    {
        currentWave++;
        turnsLeft = maxTurnsPerWave; 
        
        Debug.Log($"<color=red>--- BÁO ĐỘNG: BẮT ĐẦU WAVE {currentWave}! ---</color>");
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        // Duyệt qua từng Prefab Quái Vật bạn đã kéo thả vào
        foreach (Enemy enemyPrefab in enemyPrefabs)
        {
            // Đọc hồ sơ của nó ngay trên Prefab: Đã đến Wave mở khóa chưa?
            if (currentWave >= enemyPrefab.unlockAtWave)
            {
                // Tính toán hệ số cường hóa
                int activeWaves = currentWave - enemyPrefab.unlockAtWave; 
                int spawnAmount = enemyPrefab.baseAmount + activeWaves; 
                int finalHP = enemyPrefab.baseHP + (activeWaves * 5); 

                for (int i = 0; i < spawnAmount; i++)
                {
                    // Đẻ quái ra
                    Enemy spawnedEnemy = Instantiate(enemyPrefab);
                    
                    // Xin đường đi dựa vào thông tin "pathType" lưu trên chính con quái
                    Transform[] randomPath = PathFinding.Instance.GetRandomPathWaypoints(enemyPrefab.pathType);
                    
                    if (randomPath != null)
                    {
                        // Truyền đường đi và lượng máu đã nâng cấp
                        spawnedEnemy.Setup(randomPath, finalHP);
                    }

                    yield return new WaitForSeconds(spawnDelay);
                }
            }
        }
    }
}