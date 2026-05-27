using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources (Cửa phát thanh)")]
    public AudioSource bgmSource; 
    public AudioSource sfxSource; 

    [Header("Audio Clips (Kho chứa file nhạc)")]
    public AudioClip normalBGM; // Nhạc nền thường
    public AudioClip bossBGM;   // Nhạc nền Boss (Dùng chung cho mọi Boss)
    public AudioClip mergeSFX;  // Tiếng gộp ô (Dùng chung cho mọi tài nguyên)
    public AudioClip hitSFX;    // Tiếng đạn bắn trúng địch (Dùng chung)

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        PlayNormalBGM();
    }

    // Hàm phát nhạc nền thường
    public void PlayNormalBGM()
    {
        if (normalBGM != null && bgmSource != null)
        {
            if (bgmSource.clip == normalBGM) return;
            bgmSource.clip = normalBGM;
            bgmSource.Play();
        }
    }

    // Hàm phát nhạc nền Boss
    public void PlayBossBGM()
    {
        if (bossBGM != null && bgmSource != null)
        {
            if (bgmSource.clip == bossBGM) return;
            bgmSource.clip = bossBGM;
            bgmSource.Play();
        }
    }

    // Hàm dùng chung để phát hiệu ứng âm thanh ngắn (SFX)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}