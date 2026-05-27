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

    #region Boss Settings
    [Header("Boss Settings")]
    public Enemy bossPrefab;         
    public int bossWaveInterval = 5;  // Cứ sau mỗi 5 wave thì Boss sẽ xuất hiện (Ví dụ: 5, 10, 15...)
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

        UIManager.Instance.UpdateWaveCount(currentWave);
        UIManager.Instance.UpdateSwapCount(turnsLeft);
        
        Debug.Log($"<color=red>--- BÁO ĐỘNG: BẮT ĐẦU WAVE {currentWave}! ---</color>");
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        UIManager.Instance.UpdateSwapCount(turnsLeft);

        // 🔥 LOGIC KIỂM TRA WAVE BOSS
        if (currentWave > 0 && currentWave % bossWaveInterval == 0 && bossPrefab != null)
        {
            Debug.Log($"<color=purple>=== TRẬN CHIẾN BOSS: WAVE {currentWave} BẮT ĐẦU !!! ===</color>");

            // Tính toán máu tăng tiến cho Boss dựa trên số lần xuất hiện (Ví dụ: Wave càng cao Boss càng trâu)
            int bossTier = currentWave / bossWaveInterval; 
            int finalBossHP = bossPrefab.baseHP * bossTier * 3; // Máu boss nhân theo cấp độ (Ví dụ: Cấp 1 = x3, Cấp 2 = x6...)

            // Sinh Boss ra
            Enemy spawnedBoss = Instantiate(bossPrefab);
            
            // Tự động phóng to kích thước của Boss lên một chút cho hoành tráng (Ví dụ: to gấp 1.5 lần)
            spawnedBoss.transform.localScale = bossPrefab.transform.localScale * 2f;

            // Xin đường đi từ PathFinding dựa trên PathType của Boss (Ví dụ: bay đường Ngang hoặc Dọc)
            Transform[] randomPath = PathFinding.Instance.GetRandomPathWaypoints(bossPrefab.pathType);
            
            if (randomPath != null)
            {
                spawnedBoss.Setup(randomPath, finalBossHP);
            }

            // Kết thúc Routine tại đây, không sinh quái thường trong Wave này nữa!
            yield break; 
        }



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