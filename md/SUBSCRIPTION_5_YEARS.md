# Subscription 5 Năm - Hướng Dẫn

## ✅ Đã Hoàn Thành

Hệ thống giờ đây hỗ trợ **Subscription 5 năm**:
- **Thanh toán 1 lần** → Truy cập **TẤT CẢ POIs** trong **5 năm**
- Không cần thanh toán lại cho từng POI
- Hệ thống nhớ người dùng đã thanh toán (qua cookie + database)

---

## Cách Hoạt Động

### **1. Luồng Thanh Toán**

```
Khách vào /public
    ↓
Chọn POI bất kỳ
    ↓
Trang bị làm mờ → Hiển thị Payment Modal
    ↓
"Mua Gói 5 Năm - 50,000 VND"
    ↓
Nhập mã: DEMO-5YEAR (hoặc PAY-5YEAR-...)
    ↓
Kích hoạt subscription
    ↓
Lưu vào database + Set cookie (5 năm)
    ↓
Mở khóa TẤT CẢ POIs
```

### **2. Sau Khi Thanh Toán**

- Cookie `GuestId` được lưu trong 5 năm
- Database lưu subscription với `EndDate = StartDate + 5 năm`
- Mọi POI đều tự động mở khóa
- Không cần thanh toán lại

---

## Database Schema

### **Bảng Subscriptions**

```sql
CREATE TABLE Subscriptions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GuestId TEXT NOT NULL,              -- Unique ID của khách (từ cookie)
    PaymentCode TEXT NOT NULL,          -- Mã thanh toán
    StartDate TEXT NOT NULL,            -- Ngày bắt đầu
    EndDate TEXT NOT NULL,              -- Ngày hết hạn (5 năm sau)
    Status TEXT NOT NULL DEFAULT 'Active', -- Active/Expired
    Amount REAL NOT NULL,               -- 50000
    CreatedAt TEXT NOT NULL             -- Ngày tạo
);
```

**Ví dụ dữ liệu:**
```
Id: 1
GuestId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
PaymentCode: "DEMO-5YEAR"
StartDate: "2026-04-29T07:41:00Z"
EndDate: "2031-04-29T07:41:00Z"
Status: "Active"
Amount: 50000
CreatedAt: "2026-04-29T07:41:00Z"
```

---

## Test Flow

### **Bước 1: Chạy Admin**

```bash
cd "C:\Users\sangl\source\repos\AmThucVinhKhanhnew\VinhKhanhFood.Admin"
dotnet run
```

### **Bước 2: Vào Trang Public**

1. Mở: `http://localhost:5243/public`
2. Thấy thông báo: "💡 Mua gói 5 năm để truy cập không giới hạn tất cả POIs"

### **Bước 3: Chọn POI**

1. Click vào bất kỳ POI nào
2. Trang bị làm mờ
3. Hiển thị modal:
   - **"🔒 Mua Gói 5 Năm"**
   - **"50,000 VND"**
   - **"✨ Truy cập không giới hạn tất cả điểm POI"**
   - **"⏰ Hiệu lực: 5 năm (đến 29/04/2031)"**

### **Bước 4: Thanh Toán**

1. Nhập mã: `DEMO-5YEAR`
2. Click **"Kích Hoạt Gói 5 Năm"**
3. Thấy thông báo: "Đã kích hoạt gói 5 năm! Bạn có thể truy cập tất cả POIs."
4. Trang reload → Nội dung mở khóa

### **Bước 5: Kiểm Tra Subscription**

1. Quay lại: `http://localhost:5243/public`
2. Thấy thông báo: **"✅ Gói 5 Năm Đã Kích Hoạt"**
3. Hiển thị ngày hết hạn: "Bạn có thể truy cập tất cả POIs đến: 29/04/2031"

### **Bước 6: Truy Cập POI Khác**

1. Click vào POI khác
2. **KHÔNG CÓ payment gate** → Nội dung hiển thị ngay
3. Tất cả POI đều mở khóa!

---

## Mã Demo

### **Mã Thanh Toán Demo:**

1. **DEMO-5YEAR** - Mã shortcut (dễ nhớ)
2. **PAY-5YEAR-{timestamp}** - Mã tự động (hiển thị trên màn hình)

**Ví dụ:**
- `DEMO-5YEAR` ✅
- `PAY-5YEAR-20260429074100` ✅
- `PAY-5YEAR-20260429074200` ✅

---

## Cookie & Session

### **Cookie `GuestId`**

```javascript
// Được set sau khi thanh toán thành công
Response.Cookies.Append("GuestId", guestId, new CookieOptions
{
    Expires = DateTimeOffset.UtcNow.AddYears(5), // 5 năm
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Lax
});
```

**Đặc điểm:**
- Tự động tạo nếu chưa có
- Lưu trong 5 năm
- HttpOnly (bảo mật)
- Dùng để nhận diện người dùng đã thanh toán

---

## Logic Kiểm Tra Subscription

### **Trong PublicController:**

```csharp
// 1. Lấy GuestId từ cookie
var guestId = Request.Cookies["GuestId"];

// 2. Tìm subscription trong database
var subscription = await _context.Subscriptions
    .Where(s => s.GuestId == guestId && s.Status == "Active")
    .OrderByDescending(s => s.EndDate)
    .FirstOrDefaultAsync();

// 3. Kiểm tra còn hạn không
if (subscription != null && subscription.EndDate > DateTime.UtcNow)
{
    hasPaid = true; // Mở khóa tất cả POIs
}
```

---

## So Sánh Trước & Sau

| | Trước | Sau |
|---|---|---|
| **Thanh toán** | Mỗi POI 1 lần | **1 lần cho tất cả** |
| **Giá** | 50k/POI | **50k cho 5 năm** |
| **Thời hạn** | Không có | **5 năm** |
| **Nhớ người dùng** | Không | **Cookie + Database** |
| **Truy cập POI khác** | Phải trả lại | **Tự động mở khóa** |

---

## Lợi Ích

✅ **Trải nghiệm tốt hơn** - Chỉ thanh toán 1 lần
✅ **Giá trị cao hơn** - Truy cập tất cả POIs trong 5 năm
✅ **Tiện lợi** - Không cần thanh toán lại
✅ **Nhớ người dùng** - Cookie + Database
✅ **Dễ quản lý** - Xem subscription trong database

---

## Quản Lý Subscription

### **Xem Subscriptions Trong Database:**

```bash
cd "C:\Users\sangl\source\repos\AmThucVinhKhanhnew\CheckUsers"
dotnet run
```

Hoặc query trực tiếp:

```sql
SELECT * FROM Subscriptions WHERE Status = 'Active';
```

### **Kiểm Tra Subscription Hết Hạn:**

```sql
SELECT * FROM Subscriptions 
WHERE Status = 'Active' 
AND EndDate < datetime('now');
```

### **Xóa Subscription (Test):**

```sql
DELETE FROM Subscriptions WHERE GuestId = 'xxx';
```

---

## Production Notes

### **Trong Production:**

1. **Tích hợp Payment Gateway:**
   - Momo
   - VNPay
   - Chuyển khoản ngân hàng

2. **Webhook để verify payment:**
   - Payment gateway gọi webhook
   - Tạo subscription sau khi verify thành công

3. **Email confirmation:**
   - Gửi email xác nhận sau khi mua
   - Gửi email nhắc nhở trước khi hết hạn

4. **Admin dashboard:**
   - Xem danh sách subscriptions
   - Gia hạn thủ công
   - Hủy subscription

---

## Files Đã Thay Đổi

### **Tạo mới:**
1. `Models/Subscription.cs` - Model cho subscription
2. `create_subscriptions_table.sql` - SQL tạo bảng

### **Chỉnh sửa:**
1. `AppDbContext.cs` - Thêm DbSet<Subscription>
2. `DbInitializer.cs` - Tự động tạo bảng Subscriptions
3. `PublicController.cs` - Logic kiểm tra subscription + tạo subscription
4. `Views/Public/Index.cshtml` - Hiển thị trạng thái subscription
5. `Views/Public/Poi.cshtml` - UI "Mua Gói 5 Năm"

---

## Kết Luận

Hệ thống giờ đây:
- ✅ **Subscription 5 năm** thay vì thanh toán per POI
- ✅ **Thanh toán 1 lần** → Mở khóa tất cả POIs
- ✅ **Nhớ người dùng** qua Cookie + Database
- ✅ **Trải nghiệm tốt hơn** cho khách hàng
- ✅ **Sẵn sàng cho production** (chỉ cần tích hợp payment gateway)

**Test ngay với mã: DEMO-5YEAR** 🚀
