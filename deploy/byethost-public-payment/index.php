<?php
require_once __DIR__ . '/config.php';
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?php echo htmlspecialchars(PUBLIC_SITE_NAME); ?></title>
    <style>
        body {
            margin: 0;
            font-family: Arial, sans-serif;
            background: #fffaf5;
            color: #1f2937;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
        }
        .card {
            width: min(560px, calc(100% - 32px));
            background: #fff;
            border-radius: 24px;
            padding: 28px;
            box-shadow: 0 18px 36px rgba(232, 93, 42, 0.12);
        }
        h1 { margin-top: 0; }
        p { color: #6b7280; line-height: 1.7; }
        code {
            display: block;
            margin-top: 14px;
            padding: 12px;
            border-radius: 14px;
            background: #fff7ed;
            overflow-wrap: anywhere;
        }
    </style>
</head>
<body>
    <div class="card">
        <h1><?php echo htmlspecialchars(PUBLIC_SITE_NAME); ?></h1>
        <p>Trang public QR Payment da san sang. Hay quet QR Payment tu CMS hoac mo trang theo dinh dang ben duoi.</p>
        <code>/poi.php?id=1</code>
    </div>
</body>
</html>
