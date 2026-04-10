using UnityEngine;

public class WaveManager : MonoBehaviour
{

    #region Inspector

    #region Game State

    [Header("Game State")]
    public int currentWave = 0;      // Wave hiện tại  
    public int turnsLeft = 5;        // Số lượt còn lại trước khi Wave ập đến
    public int maxTurnsPerWave = 5;  // Số lượt tối đa mỗi Wave

    #endregion
    
    #region Enemy Settings

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    [Tooltip("Kéo thả TẤT CẢ các Object Path (đường đi) của bạn vào đây")]
     
    public int baseEnemyCount = 3;    // Số quái gốc ở Wave 1

    #endregion

    #endregion

    public static WaveManager Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance=this;
    }

    #region gọi màn tiếp theo 

    public void CheckTurnEnd() {
        if (turnsLeft <= 0) {
            TriggerNextWave();
        }
    }

    // Hàm gọi khi hết lượt chơi
    void TriggerNextWave()
    {
        currentWave++;
        turnsLeft = maxTurnsPerWave; 
        
        Debug.Log($"<color=red>--- BÁO ĐỘNG: BẮT ĐẦU WAVE {currentWave}! ---</color>");
        
        // Gọi Coroutine sinh quái
        StartCoroutine(SpawnWaveRoutine());
    }

    // Coroutine sinh quái vật
    System.Collections.IEnumerator SpawnWaveRoutine()
    {
        // Tính toán: Wave càng cao, số lượng quái và máu quái càng tăng
        int enemiesToSpawn = baseEnemyCount + (currentWave - 1); 
        int enemyHP = 10 + (currentWave * 5); 

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Sinh quái
            GameObject enemyObj = Instantiate(enemyPrefab);
            Enemy enemyScript = enemyObj.GetComponent<Enemy>();
            
            Transform[] randomPath = PathFinding.Instance.GetRandomPathWaypoints();
            // Truyền mảng đường đi và máu cho nó
            enemyScript.Setup(randomPath, enemyHP);

            // Chờ 1.5 giây rồi mới sinh con tiếp theo
            yield return new WaitForSeconds(1.5f);
        }
    }

    #endregion

}
