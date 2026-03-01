# RuneGames Leaderboard API

Milyonlarca oyuncuya hitap eden, gerçek zamanlı skor takibi ve sıralama yapabilen bir oyun backend servisi. .NET 9, PostgreSQL, Redis ve RabbitMQ üzerine inşa edilmiş, Clean Architecture prensiplerine göre yapılandırılmıştır.

---

## İçindekiler

- [Mimari](#mimari)
- [Teknolojiler](#teknolojiler)
- [Kurulum](#kurulum)
- [Ortam Değişkenleri](#ortam-değişkenleri)
- [API Endpointleri](#api-endpointleri)
- [Sıralama Algoritması](#sıralama-algoritması)
- [Veri Tutarlılığı ve Cache Stratejisi](#veri-tutarlılığı-ve-cache-stratejisi)
- [Güvenlik](#güvenlik)
- [Postman Collection](#postman-collection)

---

## Mimari

Proje, bağımlılıkların içten dışa aktığı Clean Architecture yapısına göre katmanlara ayrılmıştır.

    src/
    ├── RuneGames.Domain          → Entity'ler, domain kuralları, interface tanımları
    ├── RuneGames.Application     → Use case'ler, query'ler, auth bölümleri
    ├── RuneGames.Infrastructure  → PostgreSQL, Redis, RabbitMQ, JWT implementasyonları, Migrationlar
    └── RuneGames.API             → Controller'lar, Middleware

**Veri akışı:**
`Controller → Handler → Repository / Cache → PostgreSQL / Redis`

Her katman yalnızca bir iç katmanı tanır. Infrastructure, Domain ve Application'a bağımlıdır; API ise hepsini bir araya getirir.

---

## Teknolojiler

| Teknoloji | Kullanım Amacı |
|---|---|
| .NET 9 / ASP.NET Core | Web API |
| PostgreSQL 16 | Kalıcı veri depolama |
| Redis 7 | Leaderboard cache katmanı |
| RabbitMQ 3 | Skor olaylarının asenkron yayımlanması |
| Entity Framework Core | ORM ve migration yönetimi |
| JWT Bearer | Kimlik doğrulama |
| Docker / Docker Compose | Migration ve Seeder |
| Swagger / OpenAPI | API dokümantasyonu |

---

## Kurulum

### Gereksinimler

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Git

Başka bir şey kurmanıza gerek yok. .NET, PostgreSQL, Redis ve RabbitMQ hepsi container içinde çalışır. Migration ve Seederlar otomatik çalışmaktadır. 

### Adımlar

**1. Repoyu klonlayın**

    git clone https://github.com/halilomergurgan/LeaderboardAPI.git
    cd LeaderboardAPI

**2. Ortam dosyasını oluşturun**

Proje kök dizinindeki `appsettings.Example.json` dosyasını kopyalayın:

    cp appsettings.Example.json src/RuneGames.API/appsettings.Development.json

    Jwt Secret için termianlde şu komutu çalıştırabilirsiniz.

    openssl rand -base64 32

Açın ve şu alanları doldurun:

    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=postgres;Port=5432;Database=runegames_db;Username=runegames;Password=runegames123",
        "Redis": "redis:6379"
      },
      "Jwt": {
        "Secret": "en-az-32-karakterden-olusan-gizli-anahtar",
        "Issuer": "RuneGames",
        "Audience": "RuneGamesClient"
      }
    }


> Docker Compose ile çalıştırırken host adları `localhost` değil `postgres` ve `redis` olmalıdır.

**3. Uygulamayı başlatın**

    docker compose up --build

İlk çalıştırmada Docker image'ları indirilir, build alınır ve migration'lar otomatik uygulanır. Birkaç dakika sürebilir.

**4. Servislere erişin**

| Servis | Adres |
|---|---|
| API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| RabbitMQ Yönetim Paneli | http://localhost:15672 (runegames / runegames123) |

---

## Ortam Değişkenleri

`appsettings.Example.json` şablonu olarak sunulmuştur.

    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD",
        "Redis": "localhost:6379"
      },
      "Jwt": {
        "Secret": "YOUR_SECRET_KEY_MIN_32_CHARS",
        "Issuer": "RuneGames",
        "Audience": "RuneGamesClient"
      }
    }

---

## API Endpointleri

Tüm endpointler `http://localhost:5000` altında çalışır. Swagger UI için `/swagger` adresini ziyaret edin.

### Auth

#### `POST /api/auth/register`

Yeni kullanıcı kaydı oluşturur.

**Request Body:**

    {
      "username": "player1",
      "password": "Sifre123!",
      "deviceId": "device-uuid-buraya"
    }

**Response:** `201 Created`

    {
      "userId": "uuid",
    }

---

#### `POST /api/auth/login`

Kullanıcı girişi yapar, JWT token döndürür.

**Request Body:**

    {
      "username": "player1",
      "password": "Sifre123!"
    }

**Response:** `200 OK`

    {
      "token": "eyJhbGciOiJIUzI1NiIs..."
    }

---

### Leaderboard

> Aşağıdaki endpointler `Authorization: Bearer <token>` header'ı gerektirir.

#### `POST /api/leaderboard/score`

Maç sonucunu gönderir, skoru günceller.

**Headers:**

    Authorization: Bearer <token>
    Idempotency-Key: <unique-uuid>

**Request Body:**

    {
      "score": 1500,
      "playerLevel": 12,
      "trophyCount": 340
    }

**Response:** `200 OK`

{
    "message": "Score submission received and queued for processing."
}

**Notlar:**
- Skor 0'dan küçük veya 10.000.000'dan büyük olamaz.
- Mevcut skordan düşük bir değer gönderilirse skor değişmez, yalnızca `playerLevel` ve `trophyCount` güncellenir.
- `Idempotency-Key` aynı isteğin iki kez işlenmesini engeller.
- RabbitMQ sıraya alır ve işler. 

---

#### `GET /api/leaderboard/top?count=10`

En yüksek skorlu N oyuncuyu döndürür. ?count değeri gönderilmediği taktirde 100 kayıt döndürür

**Query Parameters:**

| Parametre | Tip | Varsayılan | Açıklama |
|---|---|---|---|
| count | int | 10 | Kaç oyuncu döneceği (max 100) |

**Response:** `200 OK`

    [
      {
        "id": "uuid"
        "rank": 5
        "userId": "uuid",
        "username": "topplayer",
        "score": 9800,
        "playerLevel": 50,
        "trophyCount": 1200,
        "lastUpdated": "date"
      }
    ]

---

#### `GET /api/leaderboard/rank{uuid}`

Giriş yapmış kullanıcının sıralamasını ve etrafındaki oyuncuları döndürür.

**Query Parameters:**

| Parametre | Tip | Varsayılan | Açıklama |
|---|---|---|---|
| surroundingRange | int | 3 | Üst ve alttan kaç oyuncu gösterileceği |

**Response:** `200 OK`

    {
      "rank": 47,
      "surrounding": [
        { "userId": "uuid", "username": "player44", "score": 4200, "rank": 44 },
        { "userId": "uuid", "username": "player45", "score": 4100, "rank": 45 },
        { "userId": "uuid", "username": "me", "score": 4050, "rank": 47 },
        { "userId": "uuid", "username": "player48", "score": 3900, "rank": 48 },
        { "userId": "uuid", "username": "player49", "score": 3800, "rank": 49 }
      ]
    }

---

## Sıralama Algoritması

Sıralama tamamen sunucu tarafında hesaplanır, client'tan gelen veriye doğrudan güvenilmez.

**Temel kural:** Yüksek skor daha iyi sıra demektir.

**Eşitlik durumunda öncelik sırası:**

    1. Score            → Yüksek olan önce
    2. RegistrationDate → Daha önce kayıt olan önce (erken kaydolan avantajlı)
    3. PlayerLevel      → Yüksek olan önce
    4. TrophyCount      → Yüksek olan önce

**Örnek:**

| Oyuncu | Skor | Kayıt Tarihi | Seviye | Kupa | Sıra |
|---|---|---|---|---|---|
| Ali | 5000 | 2024-01-01 | 30 | 500 | 1 |
| Veli | 5000 | 2024-01-03 | 30 | 500 | 2 |
| Ayşe | 5000 | 2024-01-03 | 28 | 500 | 3 |

Ali ve Veli aynı skorda ama Ali daha önce kayıt olduğu için birinci.

**Rank hesaplama:**
Bir oyuncunun sırası, kendisinden daha iyi konumda olan oyuncu sayısı + 1 olarak hesaplanır. Bu işlem PostgreSQL'de tek bir `COUNT` sorgusuyla yapılır.

---

## Veri Tutarlılığı ve Cache Stratejisi

### Redis Cache

- Top 100 oyuncu Redis'te cache'lenir.
- Her skor güncellemesinde cache **tamamen geçersiz kılınır** (`InvalidateAsync`).
- Bir sonraki okuma isteğinde cache yeniden PostgreSQL'den doldurulur.
- `count` parametresi cache'deki eleman sayısından büyükse doğrudan PostgreSQL'e düşülür.

### PostgreSQL

Sıralama sorgularının performanslı çalışması için aşağıdaki index stratejisi uygulanmıştır:

    -- Skor bazlı sıralama için
    CREATE INDEX idx_leaderboard_score ON leaderboard_entries (score DESC);

    -- Kullanıcı bazlı hızlı erişim için
    CREATE INDEX idx_leaderboard_userid ON leaderboard_entries (user_id);

### Idempotency

Her `score` isteği benzersiz bir `Idempotency-Key` header'ı taşır. Aynı key ile gelen ikinci istek işlenmez, böylece ağ hatalarında oluşabilecek çift kayıt sorunu önlenir.

---

## Güvenlik

- Tüm leaderboard endpointleri JWT ile korunur.
- Şifreler BCrypt ile hash'lenerek saklanır, düz metin hiçbir zaman veritabanına yazılmaz.
- Skor değerleri sunucu tarafında doğrulanır: negatif değer ve 10.000.000 üzeri değerler reddedilir.
- Replay attack koruması `Idempotency-Key` mekanizmasıyla sağlanır.
- Client'tan gelen skor doğrudan kabul edilmez; sunucu mevcut skorla karşılaştırır ve yalnızca daha yüksek değeri saklar.

---

## Postman Collection

Tüm endpointleri hazır ortam değişkenleriyle test etmek için Postman collection'ını içe aktarabilirsiniz:

**[Postman Collection Linki →](#)**
*(Linki buraya ekleyin)*

Collection içinde:
- Register ve Login akışı
- Token otomatik olarak sonraki isteklere aktarılır
- Score Push, Leaderboard Top ve Rank endpointleri örnek body'lerle hazır

---

## Lisans

Bu proje Rune Games işe alım süreci kapsamında hazırlanmıştır.
