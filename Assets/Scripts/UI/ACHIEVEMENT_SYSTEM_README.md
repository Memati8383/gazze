# Başarım Bildirim Sistemi

## Özellikler

✨ **Animasyonlu Bildirimler**: Sağdan kayan, yumuşak animasyonlu bildirimler
🎨 **Özel Temalar**: Her başarım için benzersiz renk teması ve emoji ikonu
🌈 **Renkli Tasarım**: 
   - 🏁 Uzun Yol/Maraton: Mavi tema (Mesafe başarımları)
   - 💝 Yardımsever/Cömert Kalp: Mor tema (Yardım başarımları)
   - ⚡ Kıl Payı/Tehlike Avcısı: Kırmızı tema (Risk başarımları)
   - 🚀 Hız Tutkunu/Süpersonik: Pembe tema (Hız başarımları)
   - 🌟 İlk Adım: Yeşil tema (İlk başarım)
⚡ **Gerçek Zamanlı**: Oyun sırasında anlık başarım bildirimleri
📊 **Çoklu Bildirim**: Aynı anda birden fazla bildirim gösterme desteği
💾 **Kalıcı Kayıt**: PlayerPrefs ile başarım kaydetme
✨ **Görsel Efektler**: Parçacık efektleri, glow animasyonları, icon bounce

## Kurulum

### Otomatik Kurulum (Önerilen)

1. Unity Editor'de menüden: `Gazze > UI > Create Achievement Notification Prefab`
2. Ardından: `Gazze > UI > Setup Achievement Notification Manager`

### Manuel Kurulum

1. `AchievementNotificationPrefab` prefab'ını sahnede bir Canvas'a ekleyin
2. `AchievementNotificationManager` objesini sahnede oluşturun
3. Manager'a prefab referansını atayın

## Kullanım

### Kod İçinden Başarım Gösterme

```csharp
using Gazze.UI;

// Basit kullanım
AchievementNotificationManager.Instance.ShowAchievement(
    "Başarım Adı", 
    "Başarım açıklaması"
);

// Icon ile (opsiyonel)
AchievementNotificationManager.Instance.ShowAchievement(
    "Başarım Adı", 
    "Başarım açıklaması",
    mySprite
);
```

### Test Etme

1. Sahneye `AchievementTestHelper` component'ini ekleyin
2. Play mode'da:
   - `1` tuşu: "Uzun Yol" başarımı
   - `2` tuşu: "Yardımsever" başarımı
   - `3` tuşu: "Kıl Payı" başarımı
   - `4` tuşu: "Hız Tutkunu" başarımı
   - `5` tuşu: "Süpersonik" başarımı

3. Inspector'da sağ tık:
   - `Test Multiple Achievements`: Birden fazla bildirim test et
   - `Clear All Achievements`: Tüm başarımları sıfırla

## Mevcut Başarımlar

### Oyun İçi Gerçek Zamanlı Başarımlar

- **Uzun Yol**: 1000+ puan kazanın
- **Maraton**: 5000+ puan kazanın
- **Yardımsever**: 10+ yardım toplayın
- **Cömert Kalp**: 50+ yardım toplayın
- **Kıl Payı**: 5+ kıl payı kaçış yapın
- **Tehlike Avcısı**: 20+ kıl payı kaçış yapın
- **Hız Tutkunu**: 45+ hıza ulaşın
- **Süpersonik**: 60+ hıza ulaşın

### Oyun Sonu Başarımları

- **İlk Adım**: İlk oyununuzu tamamlayın

## Özelleştirme

### Animasyon Ayarları

`AchievementNotification` component'inde:
- `Slide In Duration`: Giriş animasyon süresi (varsayılan: 0.5s)
- `Display Duration`: Ekranda kalma süresi (varsayılan: 3s)
- `Slide Out Duration`: Çıkış animasyon süresi (varsayılan: 0.4s)
- `Slide Curve`: Animasyon eğrisi

### Pozisyon Ayarları

- `Off Screen Offset`: Ekran dışı mesafesi (varsayılan: 400px)
- `On Screen Position X`: Ekrandaki X pozisyonu (varsayılan: -50px)

### Manager Ayarları

`AchievementNotificationManager` component'inde:
- `Max Simultaneous Notifications`: Aynı anda gösterilebilecek maksimum bildirim sayısı (varsayılan: 3)
- `Vertical Spacing`: Bildirimler arası dikey boşluk (varsayılan: 120px)
- `Top Margin`: Üst kenar boşluğu (varsayılan: 100px)

## Teknik Detaylar

### Dosya Yapısı

```
Assets/
├── Scripts/
│   └── UI/
│       ├── AchievementNotification.cs          # Bildirim UI component
│       ├── AchievementNotificationManager.cs   # Singleton manager
│       ├── AchievementNotificationEffects.cs   # Görsel efektler
│       ├── AchievementTheme.cs                 # Tema sistemi
│       └── AchievementTestHelper.cs            # Test yardımcısı
├── Editor/
│   └── AchievementNotificationBuilder.cs       # Editor script
└── Prefabs/
    └── UI/
        └── AchievementNotificationPrefab.prefab # Bildirim prefab
```

### Tema Sistemi

Her başarım otomatik olarak kendi temasını alır:

- **Mesafe Başarımları** (Uzun Yol, Maraton): Mavi ton, 🏁 bayrak ikonu
- **Yardım Başarımları** (Yardımsever, Cömert Kalp): Mor ton, 💝 kalp ikonu
- **Risk Başarımları** (Kıl Payı, Tehlike Avcısı): Kırmızı ton, ⚡ şimşek ikonu
- **Hız Başarımları** (Hız Tutkunu, Süpersonik): Pembe ton, 🚀 roket ikonu
- **İlk Başarım** (İlk Adım): Yeşil ton, 🌟 yıldız ikonu

Yeni başarım eklerken `AchievementTheme.cs` dosyasındaki `GetTheme()` metoduna yeni case ekleyebilirsiniz.

### Entegrasyon

Sistem `PlayerController.cs` ile entegre edilmiştir:
- `CheckRealtimeAchievements()`: Her frame'de başarım kontrolü
- `CalculateAchievements()`: Oyun sonu başarım hesaplama
- `ShowAchievementNotification()`: Bildirim gösterme ve kaydetme

## Sorun Giderme

### Bildirimler Görünmüyor

1. `AchievementNotificationManager` sahneye eklenmiş mi kontrol edin
2. Manager'ın `notificationPrefab` referansı atanmış mı kontrol edin
3. Canvas'ın `Render Mode` ayarını kontrol edin (ScreenSpace-Overlay önerilir)

### Animasyon Çalışmıyor

1. `Time.timeScale` değerinin 0 olmadığından emin olun
2. Bildirim objesinin `CanvasGroup` component'ine sahip olduğunu kontrol edin

### Başarımlar Kaybolmuyor

1. `Clear All Achievements` context menüsünü kullanın
2. Veya PlayerPrefs'i manuel temizleyin

## Gelecek Geliştirmeler

- [ ] Ses efektleri ekleme
- [ ] Parçacık efektleri
- [ ] Farklı başarım seviyeleri (bronz, gümüş, altın)
- [ ] Başarım ikonları
- [ ] Başarım geçmişi UI'ı
- [ ] Steam/Google Play başarım entegrasyonu
