# 🎯 Hướng Dẫn Chi Tiết - Vinh Khanh Food

## 📋 Danh Sách Tính Năng Được Thực Hiện

### ✅ Owner (Chủ Hàng)

- [x] **Đăng Ký** - Form đầy đủ, chờ duyệt admin
  - Username, Password, Email, SĐT
  - Tên chủ hàng, tên quán, mô tả
  - CMND/CCCD, Mã số thuế
  
- [x] **Đăng Nhập** - Kiểm tra tài khoản & trạng thái
  - Chỉ login được nếu status = "Approved"
  
- [x] **Dashboard** - Xem trạng thái & nút nhanh
  - Hiển thị trạng thái tài khoản (Pending/Approved/Rejected)
  - Nút: Quản Lý POI, Thêm Quán, Hồ Sơ
  
- [x] **Thêm Quán Ăn** - Form chi tiết với bản đồ
  - Tên (VI, EN, ZH), Mô tả (VI, EN, ZH)
  - **Bản đồ tương tác** (Leaflet + OSM)
    - Click trên bản đồ → lấy toạ độ
    - Nút GPS → dùng vị trí hiện tại
  - URL Hình ảnh, Audio
  - Chờ duyệt admin
  
- [x] **Danh Sách Quán Của Tôi**
  - Hiển thị: Tên, Mô tả, Vị trí, Ngày tạo, Trạng thái
  - Nút: Xem Chi Tiết, Xóa
  - Badges: ✅ Đã Duyệt, ⏳ Chờ, ❌ Từ Chối
  
- [x] **Hồ Sơ Của Tôi**
  - Xem & chỉnh sửa thông tin cá nhân
  - Xem & chỉnh sửa thông tin quán
  - Hiển thị trạng thái, lý do từ chối (nếu có)

---

### ✅ Admin (Quản Trị Viên)

- [x] **Đăng Nhập** - Username/password cứng
  - `admin` / `admin123`
  
- [x] **Dashboard** - Thống kê tổng quát
  - Số owner chờ duyệt
  - Số owner đã duyệt
  - Số POI chờ duyệt
  - Số POI đã duyệt
  - Link nhanh tới các trang quản lý
  
- [x] **Duyệt Owner**
  - Danh sách owner chờ duyệt
  - Xem chi tiết đầy đủ
  - Nút: Duyệt, Từ Chối (với lý do)
  
- [x] **Duyệt POI**
  - Danh sách POI chờ duyệt
  - Tab: Chờ Duyệt, Đã Duyệt, Từ Chối
  - Xem chi tiết + owner info
  - Nút: Duyệt, Từ Chối

---

## 🔌 API Hoàn Thiện

### Owner APIs
```
POST /api/owner/register        - Đăng ký chủ hàng
POST /api/owner/login           - Đăng nhập chủ hàng
GET  /api/owner/{id}            - Lấy thông tin owner
PUT  /api/owner/{id}            - Cập nhật owner
GET  /api/owner/{id}/foodlocations           - Danh sách quán
POST /api/owner/{id}/foodlocation            - Thêm quán mới
```

### Admin APIs
```
GET  /api/admin/pending-owners               - Owner chờ duyệt
GET  /api/admin/all-owners                   - Tất cả owner
GET  /api/admin/owners/{status}              - Filter by status
POST /api/admin/approve-owner/{id}           - Duyệt owner
POST /api/admin/reject-owner/{id}            - Từ chối owner

GET  /api/admin/pending-foodlocations        - POI chờ duyệt
GET  /api/admin/foodlocations/{status}       - Filter by status
POST /api/admin/approve-foodlocation/{id}    - Duyệt POI
POST /api/admin/reject-foodlocation/{id}     - Từ chối POI

GET  /api/admin/statistics                   - Thống kê
```

---

## 🗺️ Công Nghệ Sử Dụng

| Công Nghệ | Mục Đích | Link |
|-----------|---------|------|
| **Leaflet.js** | Bản đồ tương tác | https://leafletjs.com |
| **OpenStreetMap** | Tile map | https://www.openstreetmap.org |
| **Bootstrap 5** | Giao diện | https://getbootstrap.com |
| **SQLite** | Database | Embedded |
| **Entity Framework Core** | ORM | .NET |
| **ASP.NET Core** | Backend | Web Framework |

---

## 🚀 Hướng Dẫn Chạy

### 1. Khởi động API
```bash
cd VinhKhanhFood.API
dotnet run
# Lắng nghe tại: http://localhost:5020
# Database: auto-seed 3 owners + 2 POIs
```

### 2. Khởi động Admin Portal
```bash
cd VinhKhanhFood.Admin
dotnet run
# Truy cập tại: http://localhost:5000
# Đăng nhập: admin / admin123
```

### 3. Xem API Documentation
```
http://localhost:5020/scalar/v1
```

---

## 🧪 Test Scenarios

### Scenario 1: Owner Đăng Ký & Thêm Quán
1. Vào trang Login
2. Chọn tab "Chủ Nhà Hàng"
3. Nhấp "Đăng Ký Ngay"
4. Điền form đầy đủ
5. Gửi yêu cầu
6. **→ Chờ Admin duyệt**
7. Admin duyệt → Owner được phép đăng nhập
8. Owner thêm quán với bản đồ
9. **→ POI chờ duyệt**
10. Admin duyệt POI → Hiển thị công khai

### Scenario 2: Admin Duyệt Đơn
1. Admin login
2. Dashboard → "Duyệt Owner"
3. Xem danh sách chờ
4. Xem chi tiết từng owner
5. Duyệt hoặc từ chối
6. Lặp tại "Duyệt POI" cho quán ăn

### Scenario 3: Owner Quản Lý Quán
1. Owner login
2. Vào "Danh Sách Quán"
3. Xem các quán đã thêm
4. Click "Thêm Quán Mới"
5. Sử dụng bản đồ để chọn vị trí
6. Gửi yêu cầu

---

## 📊 Database Schema

### Owners Table
```
Id (int, PK)
Username (string, unique)
Password (string) ⚠️ UNHASHED - TODO: bcrypt
Email (string, unique)
PhoneNumber (string)
FullName (string)
BusinessName (string)
BusinessDescription (nullable)
Address (nullable)
Latitude/Longitude (double?)
IdentificationNumber (nullable)
TaxNumber (nullable)
Status (Pending/Approved/Rejected)
RegistrationDate (datetime)
ApprovedDate (datetime?)
RejectionReason (nullable)
IsActive (bool)
```

### FoodLocations Table
```
Id (int, PK)
Name, Description (string)
Name_EN, Description_EN
Name_ZH, Description_ZH
Latitude, Longitude (double)
ImageUrl, AudioUrl (nullable)
OwnerId (int, FK → Owners)
Status (Pending/Approved/Rejected)
CreatedDate (datetime)
ApprovedDate (datetime?)
```

---

## ⚠️ TODO / Future Improvements

- [ ] Hash passwords (bcrypt)
- [ ] JWT authentication
- [ ] Email verification
- [ ] Admin roles & permissions
- [ ] Audit logs
- [ ] Image upload (S3 or local storage)
- [ ] Multi-language support
- [ ] Mobile app (MAUI)
- [ ] Analytics dashboard
- [ ] Notification system

---

## 👥 Sample Data

**Owners:**
- owner_pho_vinh (Approved) - Phở Vĩnh Khánh
- owner_com_tam (Approved) - Cơm Tấm Sài Gòn
- owner_banh_mi (Pending) - Bánh Mì Thọ

**POIs:**
- Phở Đặc Biệt (owner_pho_vinh) - Approved
- Sườn Nướng Cơm Tấm (owner_com_tam) - Approved

---

## 📞 Support

Nếu có vấn đề:
1. Kiểm tra logs: `VinhKhanhFood.API/bin/Debug/net9.0`
2. Xóa database: `rm VinhKhanhFood.db`
3. Reset migrations: `dotnet ef database drop`
4. Rebuild: `dotnet build`

---

**Last Updated**: 2024
**Version**: 1.0
