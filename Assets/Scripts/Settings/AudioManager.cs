/**
 * @file AudioManager.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Oyun genelindeki seslerin (Müzik ve SFX) merkezi yönetimini sağlayan Singleton sınıfıdır.
 */

using UnityEngine;

namespace Settings
{
    /// <summary>
    /// Tüm ses kaynaklarını yöneten, sahneler arası geçişte silinmeyen ses yöneticisi.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
    /// <summary> AudioManager'ın Singleton örneği. </summary>
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Sahnede var mı diye bak
                _instance = Object.FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);

                // Yoksa yeni bir tane oluştur
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager (Auto-Generated)");
                    _instance = go.AddComponent<AudioManager>();
                    // SetupAudioSources Awake içinde çağrılacak ama burada da garanti edelim
                    _instance.SetupAudioSources();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Ses Kaynakları")]
    [SerializeField, Tooltip("Arka plan müziği için kullanılan kaynak.")]
    private AudioSource musicSource;
    
    [SerializeField, Tooltip("Ses efektleri için kullanılan kaynak.")]
    private AudioSource sfxSource;

    [Header("Özel Sesler")]
    [SerializeField] private AudioClip clickSound;

    private bool isInitialized = false;

    private void Awake()
    {
        // Singleton pattern uygulaması
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            SetupAudioSources();
            LoadSpecialSounds();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Ses gecikmesi (Latency) problemi için kod üzerinden AudioSettings.Reset() kullanmak
        // oyunların build edildikten sonra (özellikle Android ve Standalone) log vermeden,
        // sessizce çökmesine (silent crash) neden olur.
        // DSP Buffer Size'ı (Best Latency vs) Edit -> Project Settings -> Audio altından yapmanız gerekir.
    }

    private void LoadSpecialSounds()
    {
        if (clickSound == null)
        {
            clickSound = Resources.Load<AudioClip>("SFX/click");
        }
    }

    /// <summary>
    /// Buton tıklama sesini çalar.
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            PlaySFX(clickSound);
        }
    }

    /// <summary>
    /// Ses kaynaklarını hazırlar ve temel ayarlarını yapar.
    /// </summary>
    public void SetupAudioSources()
    {
        if (isInitialized) return;

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        isInitialized = true;
        // Başlangıç ses seviyelerini PlayerPrefs'ten yükle
        ApplyInitialSettings();
        LoadAndPlayBackgroundMusic();
    }

    /// <summary>
    /// PlayerPrefs'ten kaydedilmiş ses ayarlarını yükler ve uygular.
    /// </summary>
    public void ApplyInitialSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(SettingsModel.MusicVolumeKey, 1f);
        bool musicEnabled = PlayerPrefs.GetInt(SettingsModel.MusicEnabledKey, 1) == 1;
        float sfxVol = PlayerPrefs.GetFloat(SettingsModel.SFXVolumeKey, 1f);
        bool sfxEnabled = PlayerPrefs.GetInt(SettingsModel.SFXEnabledKey, 1) == 1;

        SetMusicVolume(musicEnabled ? musicVol : 0f);
        SetSFXVolume(sfxEnabled ? sfxVol : 0f);
    }

    /// <summary>
    /// Resources klasöründen arka plan müziğini yükler ve çalmaya başlar.
    /// </summary>
    public void LoadAndPlayBackgroundMusic()
    {
        // Eğer zaten bir klip çalıyorsa ve aynı klipse tekrar oynatma
        AudioClip bgMusic = Resources.Load<AudioClip>("Music/BackgroundMusic");
        if (bgMusic != null)
        {
            if (musicSource.clip == bgMusic && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = bgMusic;
            musicSource.Play();
            // Debug.Log("AudioManager: Arka plan müziği başlatıldı.");
        }
        else
        {
            Debug.LogWarning("AudioManager: Resources/Music/BackgroundMusic yolunda müzik dosyası bulunamadı!");
        }
    }

    /// <summary>
    /// Müzik kaynağının ses seviyesini günceller.
    /// </summary>
    /// <param name="volume">Ses seviyesi (0.0 - 1.0).</param>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
            
            // Eğer ses çok düşükse veya 0 ise, performansı artırmak için duraklatabiliriz
            if (volume <= 0.001f && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
            else if (volume > 0.001f && !musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
        }
    }

    /// <summary>
    /// SFX kaynağının ses seviyesini günceller.
    /// </summary>
    /// <param name="volume">Ses seviyesi (0.0 - 1.0).</param>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Belirtilen ses efektini tek seferlik çalar.
    /// </summary>
    /// <param name="clip">Çalınacak ses klibi.</param>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
}