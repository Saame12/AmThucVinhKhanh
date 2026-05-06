# ByetHost Public Payment Page

Day la goi file PHP de upload len `https://vinkkhanhqrpay.byethost4.com/`.

## File can upload

- `index.php`
- `poi.php`
- `config.php`  
  Tao file nay bang cach copy `config.example.php` va doi gia tri `API_PUBLIC_BASE_URL`

## Cach upload

1. Dang nhap ByetHost / File Manager / FTP.
2. Mo thu muc web root, thuong la `htdocs/`.
3. Upload 3 file vao dung thu muc root do:
   - `htdocs/index.php`
   - `htdocs/poi.php`
   - `htdocs/config.php`

## Config bat buoc

Trong `config.php`, sua:

```php
define('API_PUBLIC_BASE_URL', 'https://your-public-api-domain.example.com/api');
```

Luu y:

- Khong duoc de `localhost`
- Khong duoc de IP noi bo
- API `.NET` cua ban phai public tren internet de file PHP goi duoc

## Link public

Sau khi upload xong, trang public co dang:

- `https://vinkkhanhqrpay.byethost4.com/`
- `https://vinkkhanhqrpay.byethost4.com/poi.php?id=1`

## Logic hoat dong

- Mo ta POI duoc xem mien phi
- Audio bi khoa
- Bam nut thanh toan mock `10.000 VND`
- PHP goi API `POST /api/Payment/mock-checkout`
- Giao dich duoc luu vao `QR Payment Dashboard`
- Trang public mo khoa audio cho guest cookie hien tai
