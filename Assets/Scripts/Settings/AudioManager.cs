using UnityEngine;
using UnityEngine.Audio;

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

        [Header("Audio Mixer")]
        [SerializeField, Tooltip("Tüm seslerin bağlı olduğu ana mixer. Pitch efektleri için gereklidir.")]
        public AudioMixer masterMixer;
        [Tooltip("Mixer üzerinde exposed edilen pitch parametresinin adı.")]
        public string pitchParameterName = "MasterPitch";

        [SerializeField, Tooltip("UI buton tıklamalarında çalınacak varsayılan ses.")]
        private AudioClip clickSound;

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
            musicSource.spatialBlend = 0f; // 2D Ses
            musicSource.ignoreListenerPause = true; // Oyun durduğunda müzik devam etsin
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D Ses
            sfxSource.priority = 64; // Daha yüksek öncelik (0-256, düşük olan önceliklidir)
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
            if (musicSource == null) return;
            
            // Eğer zaten bir klip çalıyorsa ve aynı klipse tekrar oynatma
            AudioClip bgMusic = Resources.Load<AudioClip>("Music/BackgroundMusic");
            if (bgMusic != null)
            {
                if (musicSource.clip == bgMusic && musicSource.isPlaying)
                {
                    return;
                }

                musicSource.clip = bgMusic;
                musicSource.mute = false; // Garanti et
                musicSource.Play();
                // Debug.Log($"<color=cyan>AudioManager:</color> Arka plan müziği başarıyla yüklendi ve başlatıldı: {bgMusic.name}");
            }
            else
            {
                // Debug.LogError("<color=red>AudioManager:</color> Resources/Music/BackgroundMusic yolunda müzik dosyası bulunamadı! Lütfen dosya adını ve yolunu kontrol edin.");
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
                
                // Müzik etkin ve ses var ise ama çalmıyorsa başlat
                if (volume > 0.001f)
                {
                    if (!musicSource.isPlaying)
                    {
                        if (musicSource.clip == null) LoadAndPlayBackgroundMusic();
                        else musicSource.Play();
                    }
                }
                else if (musicSource.isPlaying)
                {
                    musicSource.Pause();
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
        /// Global ses perdesini (pitch) mixer üzerinden değiştirir.
        /// (Önemli: Mixer üzerinde 'MasterPitch' parametresi expose edilmiş olmalıdır.)
        /// </summary>
        public void SetGlobalPitch(float pitch)
        {
            if (masterMixer != null)
            {
                // AudioMixer parametreleri genellikle logaritmik veya lineer olabilir.
                // Pitch için genellikle 0.5 ile 2.0 arası bir değer beklenir.
                masterMixer.SetFloat(pitchParameterName, pitch);
            }
            else
            {
                // Fallback: Sadece mevcut SFX ve Müzik kaynaklarının pitch değerini değiştir
                if (musicSource != null) musicSource.pitch = pitch;
                if (sfxSource != null) sfxSource.pitch = pitch;
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void ResetGlobalPitch()

        {
            SetGlobalPitch(1.0f);
        }
    }
}
