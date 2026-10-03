# 🚀 TaskMaster Pro

TaskMaster Pro, Trello benzeri kurumsal seviyede bir Kanban görev yönetim sistemidir. Endüstri standartlarındaki **Clean Architecture** prensipleriyle inşa edilmiş sağlam bir backend'i, ve modern **React** teknolojileriyle kurgulanmış dinamik bir frontend'i bir araya getirir. 

Proje, katmanların birbirinden tamamen izole (Loosely Coupled) olduğu, test edilebilir, ölçeklenebilir ve bağımlılıkların (dependency) en aza indirgendiği bir yapı sunar.

## 🏗️ Mimari Tasarım (Clean Architecture)

Proje, iş mantığını dış bağımlılıklardan korumak için 4 temel katmana ayrılmıştır:

1. **Domain:** Çekirdek katmandır. Projenin kalbi olan Entity'ler (Board, Column, Task, vb.) ve arayüzler (Interface) burada yer alır. Hiçbir dış kütüphaneye bağımlılığı yoktur.
2. **Application:** İş kurallarının (Use Cases), DTO'ların, FluentValidation kurallarının ve servis katmanının (Service Pattern) bulunduğu katmandır. Veritabanı veya framework'ten bağımsızdır.
3. **Infrastructure:** Entity Framework Core yapılandırmaları, Repository implementasyonları, JWT üretim mekanizmaları ve veritabanı bağlantılarının yapıldığı dış dünya katmanıdır.
4. **API:** İstemci ile iletişimi sağlayan Controller'ların, Global Exception Middleware'in ve konfigürasyonların (Dependency Injection) ayarlandığı sunum katmanıdır.

Bu sayede; veritabanı değiştiğinde (örneğin PostgreSQL'den MSSQL'e geçişte) veya bir araç değiştiğinde, Domain ve Application katmanlarında tek satır kod değiştirilmesine gerek kalmaz.

## 💻 Kullanılan Teknolojiler & Modüller

### Backend (.NET Core)
* **C# .NET 8 / Web API**
* **Veritabanı:** PostgreSQL
* **ORM:** Entity Framework Core (Code-First)
* **Tasarım Kalıpları:** Generic Repository, Unit of Work, Service Pattern
* **Kimlik Doğrulama:** JWT (JSON Web Tokens) & BCrypt Şifreleme
* **Doğrulama:** FluentValidation
* **Loglama & Hata Yönetimi:** Global Exception Middleware ile merkezi hata formatlaması.

### Frontend (React)
* **Framework:** React 18, Vite, TypeScript
* **Stil:** Tailwind CSS ile Modern, Dark Mode ve Glassmorphism odaklı tasarım.
* **Sürükle & Bırak (DnD):** `@dnd-kit/core` kullanılarak sütunlar arası ve içi akıcı görev taşıma deneyimi.
* **API Yönetimi:** Axios Interceptors kullanılarak tüm isteklere otomatik JWT eklenmesi ve 401 hatalarında otomatik yönlendirme.
* **State Management:** React Hooks ve Context API tabanlı yetkilendirme yönetimi.

## 🐳 Docker ve Konteynerizasyon

Proje, tamamen izole edilmiş mikroservis mantığıyla Docker üzerinde çalışacak şekilde tasarlanmıştır. `docker-compose.yml` aracılığıyla tüm bileşenler tek seferde orkestre edilir.

* **taskmasterpro_db:** PostgreSQL 16 veritabanı konteyneri. Veri kaybını önlemek için Volume bağlamaları yapılmıştır.
* **taskmasterpro_api:** Multi-stage Dockerfile ile optimize edilmiş .NET API konteyneri. Veritabanının hazır olmasını (`healthcheck`) bekleyecek şekilde konfigüre edilmiştir.
* **taskmasterpro_frontend:** Vite projesinin statik olarak derlenip **Nginx** üzerinden sunulduğu, bağımsız frontend konteyneri. API istekleri için ortam değişkenleri (`.env`) build aşamasında Docker tarafından enjekte edilir.

---

## 🚀 Kurulum ve Çalıştırma

Projeyi tek bir komutla ayağa kaldırmak için **Docker** kullanılmıştır. Bilgisayarınızda Docker ve Docker Compose yüklü olması yeterlidir.

1. Projeyi bilgisayarınıza klonlayın.
2. Ana dizinde terminali açın.
3. Sistem için gereken şifreleri belirlemek adına `.env.example` dosyasına bakarak kendi `.env` dosyanızı oluşturun.
4. Aşağıdaki komutu çalıştırın:

```bash
docker compose up -d --build
```

Bu komut:
* Veritabanını hazırlar.
* API'yi derleyip, veritabanı göçlerini (Migrations) otomatik uygular.
* Frontend'i derleyerek Nginx üzerinde yayına alır.

Tüm servisler ayağa kalktığında tarayıcınızdan **`http://localhost:3000`** adresine giderek uygulamayı kullanmaya başlayabilirsiniz! (API uç noktalarına Swagger üzerinden `http://localhost:8080/swagger` adresinden erişebilirsiniz.)
