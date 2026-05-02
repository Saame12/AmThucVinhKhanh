# QR Public Web Flow - Hướng Dẫn Sử Dụng (Cập Nhật)

## Tổng Quan

Hệ thống có **2 loại QR riêng biệt**:

### 1. **Audio QR** (Cho App)
- Dành cho người dùng trong app
- **Mỗi POI có 1 Audio QR riêng**
- Quét QR → Mở POI trong app → Phát audio tự động
- Không cần thanh toán
- Quản lý trong: **Manage POI** → Click "Audio QR" ở mỗi POI

### 2. **QR Public Web** (Cho Web Công Khai) ✨
- Dành cho khách quét từ bên ngoài (camera điện thoại, app bên thứ 3)
- **CHỈ CÓ DUY NHẤT 1 QR** cho toàn bộ hệ thống
- Hiển thị ở: **Trang chủ Admin**
- Quét QR → Vào trang web công khai → Xem danh sách POIs → Chọn POI → Thanh toán → Mở khóa nội dung

---

## Luồng Hoạt Động QR Public Web

### **Bước 1: Xem QR Public Web**

1. Đăng nhập vào Admin Web: `http://localhost:5243`
   - Username: `admin` / Password: `admin`
   - Hoặc: `octhao` / `123`

2. Sau khi login, bạn sẽ thấy **trang chủ** với:
   - **QR Public Web** (duy nhất) ở giữa màn hình
   - Label: `VK-PUBLIC-WEB`
   - URL: `http://localhost:5243/public`
   - Nút "Mở trang công khai"

3. Click **"Mở trang công khai"** để test

---

### **Bước 2: Quét QR (Từ Bên Ngoài)**

Khi khách quét QR Public Web từ camera điện thoại hoặc app bên thứ 3:

1. **Mở trang danh sách POIs**: `http://localhost:5243/public`
   - Hiển thị tất cả POIs đã được Approved
   - Giao diện đẹp với gradient background
   - Mỗi POI có: Hình ảnh, Tên, Mô tả ngắn, Badge "🎧 Audio Guide Available"

---

### **Bước 3: Chọn POI**

1. Khách click vào POI muốn xem
2. Chuyển đến: `http://localhost:5243/public/poi/{id}`

---

### **Bước 4: Payment Gate (Trang Bị Làm Mờ)**

Trang chi tiết POI hiển thị:

**Phần bị blur (làm mờ):**
- Hình ảnh POI
- Mô tả chi tiết
- Audio guide

**Phần hiển thị rõ (ở giữa màn hình):**
- Modal thanh toán
- QR Payment (trong demo là QR code giả)
- Số tiền: 50,000 VND
- Ô nhập mã thanh toán
- Nút "Verify Payment"

---

### **Bước 5: Thanh Toán (Demo Mode)**

**Trong demo mode**, có 2 cách nhập mã:

1. **Mã tự động** (hiển thị trên màn hình):
   - Format: `PAY-{poiId}-{timestamp}`
   - Ví dụ: `PAY-1-20260429071528`

2. **Mã shortcut**:
   - Format: `DEMO-{poiId}`
   - Ví dụ: `DEMO-1`, `DEMO-2`, `DEMO-3`

**Cách test:**
1. Copy mã demo hiển thị trên màn hình
2. Paste vào ô "Enter payment code to verify"
3. Click "Verify Payment"
4. Trang reload với `?payment={code}` trong URL

---

### **Bước 6: Nội Dung Được Mở Khóa**

Sau khi thanh toán thành công:
- Blur overlay biến mất
- Hiển thị đầy đủ:
  - Hình ảnh POI
  - Tên và mô tả chi tiết
  - Audio guide (sẵn sàng phát)
- Có nút "← Quay lại danh sách" để xem POI khác

---

## So Sánh 2 Loại QR

| Tính Năng | Audio QR | QR Public Web |
|-----------|----------|---------------|
| **Đối tượng** | Người dùng trong app | Khách từ bên ngoài |
| **Quét từ** | App (tab Scan QR) | Camera điện thoại / App bên thứ 3 |
| **Số lượng** | Mỗi POI 1 QR | Duy nhất 1 QR cho toàn hệ thống |
| **Vị trí** | Manage POI → Audio QR | Trang chủ Admin |
| **Đích đến** | Mở POI trong app | Trang web công khai (danh sách POIs) |
| **Thanh toán** | Không cần | Cần thanh toán 50,000 VND/POI |
| **Nội dung** | Audio tự động phát | Hình + Mô tả + Audio |
| **Label** | `VK-AUDIO-{id}` | `VK-PUBLIC-WEB` |

---

## Cấu Trúc Menu Admin

```
Trang Chủ (Home/Index)
├── 🌐 QR Public Web (duy nhất)
├── 📍 Manage POIs
├── 👥 Users
└── 📊 Usage Dashboard

Manage POI (Poi/Index)
└── Mỗi POI có:
    ├── Audio QR (cho app)
    ├── Edit
    └── Delete
```

---

## URLs Quan Trọng

| Trang | URL | Mô Tả |
|-------|-----|-------|
| Admin Login | `http://localhost:5243` | Đăng nhập admin |
| **Trang Chủ** | `http://localhost:5243/Home` | **QR Public Web duy nhất ở đây** |
| Manage POI | `http://localhost:5243/Poi` | Quản lý POIs + Audio QR |
| Audio QR | `http://localhost:5243/Poi/Qr/{id}` | Xem Audio QR của POI |
| **Public - Danh sách** | `http://localhost:5243/public` | Trang công khai - Danh sách POIs |
| **Public - Chi tiết** | `http://localhost:5243/public/poi/{id}` | Trang công khai - Chi tiết POI |

---

## Test Flow Hoàn Chỉnh

### **Test Nhanh:**

```bash
# 1. Start API
cd "C:\Users\sangl\source\repos\AmThucVinhKhanhnew\VinhKhanhFood.API"
dotnet run

# 2. Start Admin (terminal mới)
cd "C:\Users\sangl\source\repos\AmThucVinhKhanhnew\VinhKhanhFood.Admin"
dotnet run

# 3. Test trong browser
```

**Các bước test:**
1. Mở: `http://localhost:5243`
2. Login: `admin` / `admin`
3. Bạn sẽ thấy **trang chủ** với QR Public Web duy nhất
4. Click **"Mở trang công khai"**
5. Chọn POI từ danh sách
6. Nhập mã: `DEMO-1`
7. Click "Verify Payment"
8. Xem nội dung đã mở khóa

---

## Lưu Ý Quan Trọng

1. **QR Public Web là DUY NHẤT**: 
   - Chỉ có 1 QR cho toàn bộ hệ thống
   - Hiển thị ở trang chủ admin
   - QR dẫn đến trang danh sách POIs, không phải POI cụ thể

2. **Audio QR riêng cho mỗi POI**:
   - Mỗi POI có 1 Audio QR riêng
   - Quản lý trong Manage POI
   - Dành cho app, không cần thanh toán

3. **Payment Gate**:
   - Mỗi POI cần thanh toán riêng
   - Thanh toán POI A không mở khóa POI B

4. **Demo Mode**:
   - Hiện tại đang ở chế độ demo
   - Chấp nhận mã thanh toán giả
   - Trong production sẽ tích hợp Momo/VNPay/Banking

5. **Chỉ POIs Approved**:
   - Trang công khai chỉ hiển thị POIs có Status = "Approved"
   - POIs Pending/Rejected không hiển thị

---

## Các File Đã Thay Đổi

### **Đã Tạo:**
1. **Views/Public/Index.cshtml** - Trang danh sách POIs công khai

### **Đã Chỉnh Sửa:**
1. **PublicController.cs** - Thêm action Index() để hiển thị danh sách POIs
2. **Views/Public/Poi.cshtml** - Thêm nút quay lại danh sách
3. **Views/Home/Index.cshtml** - Thêm QR Public Web duy nhất ở trang chủ
4. **Views/Poi/Index.cshtml** - Xóa nút "Public Web QR" khỏi mỗi POI
5. **PoiController.cs** - Xóa action PaymentQr (không còn cần)
6. **AccountController.cs** - Redirect đến Home/Index sau login

### **Đã Xóa:**
1. **Views/Poi/PaymentQr.cshtml** - Không còn cần thiết

---

## Kết Luận

Flow QR Public Web đã hoàn chỉnh với:
- ✅ 1 QR duy nhất ở trang chủ admin
- ✅ Audio QR riêng cho mỗi POI (trong Manage POI)
- ✅ Trang danh sách POIs công khai
- ✅ Payment gate trên từng POI
- ✅ Demo mode với mã thanh toán giả
- ✅ Giao diện đẹp, responsive
- ✅ Sẵn sàng cho production (chỉ cần tích hợp payment gateway thật)
