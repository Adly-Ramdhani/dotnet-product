# 📦 ProductManagementApp

ProductManagementApp adalah aplikasi web berbasis ASP.NET Core MVC yang digunakan untuk mengelola produk, termasuk fitur otentikasi pengguna (registrasi & login), CRUD produk.

---

## 🧰 Tech Stack

- [.NET Framework](https://dotnet.microsoft.com/)
- ASP.NET MVC
- Razor View Engine
- Entity Framework
- PostgreSQL 
- Bootstrap 
- C#

## 📦 Instalasi & Menjalankan Proyek

### 1. Clone Proyek
git clone https://github.com/Adly-Ramdhani/dotnet-product.git

### 2. Ganti konfiguras databases yang ada di appsettings.json dan data/ApplicationDbContextFactory 
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=db_product2;Username=postgres;Password=nabati321"
  },
  Sesuaikan saja

### 3. dotnet ef database update
### 4. dotnet clean
### 5. rm -rf obj/ bin/  
### 5. donret build
### 6. dotnet run



