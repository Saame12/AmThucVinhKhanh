# 📋 Vinh Khanh Food - Hướng Dẫn Sử Dụng Nhanh

## 🔐 Đăng Nhập

### Admin
- **URL**: `http://localhost:[port]/Account/Login`
- **Tab**: Quản Trị Viên
- **Username**: `admin`
- **Password**: `admin123`

### Owner (Chủ Hàng)
- **Tab**: Chủ Nhà Hàng
- **Có 2 lựa chọn**:
  1. **Đăng Nhập**: Dành cho owner đã có tài khoản
  2. **Đăng Ký**: Form đăng ký (chờ duyệt admin)

---

## 📱 Các Trang Chính

### Owner Screens

| Trang | URL | Mô Tả |
|-------|-----|-------|
| **Dashboard** | `/Home/Index` | Trạng thái tài khoản, nút nhanh |
| **Thêm Quán** | `/Owner/Create` | Form thêm POI với bản đồ |
| **Danh Sách Quán** | `/Owner/FoodLocations` | Xem/xóa quán của mình |
| **Hồ Sơ** | `/Owner/Profile` | Chỉnh sửa thông tin |

### Admin Screens

| Trang | URL | Mô Tả |
|-------|-----|-------|
| **Dashboard** | `/Home/Admin` | Thống kê & nút nhanh |
| **Duyệt Owner** | `/Home/PendingOwners` | Duyệt/từ chối đơn đăng ký owner |
| **Duyệt POI** | `/FoodLocation/PendingApprovals` | Duyệt/từ chối quán ăn |

---

## 🔌 API Endpoints

### Owner APIs
- `POST /api/owner/register` - Đăng ký owner
- `POST /api/owner/login` - Đăng nhập owner
- `GET /api/owner/{id}` - Lấy thông tin owner
- `GET /api/owner/{id}/foodlocations` - Danh sách quán của owner
- `POST /api/owner/{id}/foodlocation` - Thêm quán
- `PUT /api/owner/{id}` - Cập nhật owner

### Admin APIs
- `GET /api/admin/pending-owners` - Danh sách owner chờ duyệt
- `GET /api/admin/all-owners` - Tất cả owner
- `POST /api/admin/approve-owner/{id}` - Duyệt owner
- `POST /api/admin/reject-owner/{id}` - Từ chối owner
- `GET /api/admin/pending-foodlocations` - POI chờ duyệt
- `GET /api/admin/foodlocations/{status}` - POI theo trạng thái
- `POST /api/admin/approve-foodlocation/{id}` - Duyệt POI
- `POST /api/admin/reject-foodlocation/{id}` - Từ chối POI
- `GET /api/admin/statistics` - Thống kê

---

## 📊 Dữ Liệu Mẫu

### 3 Chủ Hàng (Owners)

| Tên | Username | Email | Trạng Thái |
|-----|----------|-------|-----------|
| Nguyễn Văn An | `owner_pho_vinh` | `owner1@phuvinh.com` | ✅ Approved |
| Trần Thị Bình | `owner_com_tam` | `owner2@comtam.com` | ✅ Approved |
| Lê Minh Khoa | `owner_banh_mi` | `owner3@banhmi.com` | ⏳ Pending |

### 2 POI (Quán Ăn)

| Tên | Chủ Hàng | Trạng Thái |
|-----|----------|-----------|
| Phở Đặc Biệt | Nguyễn Văn An | ✅ Approved |
| Sườn Nướng Cơm Tấm | Trần Thị Bình | ✅ Approved |

---

## 🗺️ Chức Năng Chính

### Thêm Quán (Create Food Location)
1. **Thông Tin Cơ Bản**
   - Tên quán (Tiếng Việt, English, 中文)
   - Mô tả

2. **Chọn Vị Trí**
   - Bản đồ tương tác (Leaflet)
   - Nhấp vào bản đồ để lấy toạ độ
   - Nút "Dùng Vị Trí Hiện Tại" (GPS)
   - Hiển thị Latitude/Longitude

3. **Hình Ảnh & Audio**
   - URL hình ảnh
   - URL audio (multiple languages)

4. **Gửi Yêu Cầu**
   - POI chờ duyệt admin
   - Hiển thị trong danh sách chờ duyệt

### Duyệt Đơn (Admin)
- Xem danh sách chờ duyệt
- Xem chi tiết đầy đủ
- Duyệt hoặc từ chối
- Nhập lý do (nếu từ chối)

---

## 🚀 Chạy Ứng Dụng

```bash
# Start API (port 5020)
cd VinhKhanhFood.API
dotnet run

# Start Admin (port 5000)
cd VinhKhanhFood.Admin
dotnet run
```

### URLs
- **Admin**: `http://localhost:5000`
- **API**: `http://localhost:5020/scalar/v1`

---

## ✅ Trạng Thái Implementation

| Tính Năng | Trạng Thái |
|-----------|-----------|
| Owner Registration | ✅ Hoàn thành |
| Owner Login | ✅ Hoàn thành |
| Add Food Location (Create) | ✅ Hoàn thành |
| Food Location List | ✅ Hoàn thành |
| Admin Approve Owner | ✅ Hoàn thành |
| Admin Approve POI | ✅ Hoàn thành |
| Map Integration (Leaflet) | ✅ Hoàn thành |
| Statistics Dashboard | ✅ Hoàn thành |
| Database Seeding | ✅ Hoàn thành |

---

## 📝 Notes

- Mật khẩu **CHƯA hash** - Dùng bcrypt hoặc hashing trong production
- Bản đồ dùng **Leaflet + OpenStreetMap** (free, open-source)
- Database: **SQLite** (development)
- Dữ liệu tự động seed khi API khởi động
