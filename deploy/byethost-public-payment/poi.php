<?php
require_once __DIR__ . '/config.php';

function api_request($method, $path, $payload = null)
{
    $url = rtrim(API_PUBLIC_BASE_URL, '/') . '/' . ltrim($path, '/');
    $headers = [
        'Accept: application/json',
        'Content-Type: application/json'
    ];

    if (function_exists('curl_init')) {
        $ch = curl_init($url);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_CUSTOMREQUEST, strtoupper($method));
        curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
        if ($payload !== null) {
            curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($payload));
        }
        $response = curl_exec($ch);
        $statusCode = (int)curl_getinfo($ch, CURLINFO_HTTP_CODE);
        $error = curl_error($ch);
        curl_close($ch);

        if ($response === false || $statusCode >= 400) {
            return [null, $error ?: ('HTTP ' . $statusCode)];
        }

        return [json_decode($response, true), null];
    }

    $options = [
        'http' => [
            'method' => strtoupper($method),
            'header' => implode("\r\n", $headers),
            'ignore_errors' => true
        ]
    ];

    if ($payload !== null) {
        $options['http']['content'] = json_encode($payload);
    }

    $context = stream_context_create($options);
    $response = @file_get_contents($url, false, $context);
    if ($response === false) {
        return [null, 'Unable to call API'];
    }

    return [json_decode($response, true), null];
}

function ensure_guest_id()
{
    if (!empty($_COOKIE[PUBLIC_GUEST_COOKIE])) {
        return $_COOKIE[PUBLIC_GUEST_COOKIE];
    }

    $guestId = 'web-' . substr(md5(uniqid((string)mt_rand(), true)), 0, 12);
    setcookie(PUBLIC_GUEST_COOKIE, $guestId, time() + 86400 * 30, '/');
    $_COOKIE[PUBLIC_GUEST_COOKIE] = $guestId;
    return $guestId;
}

function asset_url($folder, $fileName)
{
    if (empty($fileName)) {
        return null;
    }

    $base = preg_replace('~/api/?$~', '', rtrim(API_PUBLIC_BASE_URL, '/'));
    return $base . '/' . trim($folder, '/') . '/' . ltrim($fileName, '/');
}

function render_tts_button($title, $description, $lang, $label)
{
    if (empty($description)) {
        return '';
    }

    $escapedDescription = json_encode($description, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    ob_start();
    ?>
    <div class="audio-item">
        <h3><?php echo htmlspecialchars($title); ?></h3>
        <p><?php echo htmlspecialchars($label); ?></p>
        <button type="button" class="button secondary" onclick='speakText("<?php echo htmlspecialchars($lang); ?>", <?php echo $escapedDescription; ?>)'>Phat audio mo ta</button>
    </div>
    <?php
    return ob_get_clean();
}

function render_audio_player($title, $description, $url)
{
    if (empty($url)) {
        return '';
    }

    ob_start();
    ?>
    <div class="audio-item">
        <h3><?php echo htmlspecialchars($title); ?></h3>
        <p><?php echo htmlspecialchars($description); ?></p>
        <audio controls preload="none">
            <source src="<?php echo htmlspecialchars($url); ?>" type="audio/mpeg">
        </audio>
    </div>
    <?php
    return ob_get_clean();
}

$poiId = isset($_GET['id']) ? (int)$_GET['id'] : 0;
$message = isset($_GET['message']) ? $_GET['message'] : '';
$guestId = ensure_guest_id();
$poi = null;
$access = ['hasAccess' => false];
$errorMessage = null;

if ($poiId <= 0) {
    $errorMessage = 'Lien ket QR khong hop le.';
} else {
    [$poi, $poiError] = api_request('GET', 'Food/' . $poiId);
    if (!$poi) {
        $errorMessage = 'Khong tim thay diem POI hoac API chua public.';
    } else {
        [$accessData] = api_request('GET', 'Payment/access?poiId=' . $poiId . '&guestId=' . urlencode($guestId));
        if (is_array($accessData)) {
            $access = $accessData;
        }

        if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['unlock_audio'])) {
            [$checkoutResult, $checkoutError] = api_request('POST', 'Payment/mock-checkout', [
                'poiId' => $poiId,
                'amount' => PUBLIC_PRICE_VND,
                'guestId' => $guestId,
                'provider' => 'ByetHost-Web'
            ]);

            if ($checkoutResult && empty($checkoutError)) {
                header('Location: poi.php?id=' . $poiId . '&message=paid');
                exit;
            }

            $message = 'payment-error';
        }
    }
}

$poiImageUrl = !empty($poi['imageUrl'])
    ? (stripos($poi['imageUrl'], 'http') === 0 ? $poi['imageUrl'] : asset_url('images', $poi['imageUrl']))
    : 'https://placehold.co/960x720/FFF7ED/C2410C?text=POI';

$audioVi = !empty($poi['audioUrl']) ? asset_url('audio', $poi['audioUrl']) : null;
$audioEn = !empty($poi['audioUrl_EN']) ? asset_url('audio', $poi['audioUrl_EN']) : null;
$audioZh = !empty($poi['audioUrl_ZH']) ? asset_url('audio', $poi['audioUrl_ZH']) : null;
$hasPaidAccess = !empty($access['hasAccess']);
?>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?php echo !empty($poi['name']) ? htmlspecialchars($poi['name']) : 'POI Payment'; ?></title>
    <style>
        :root {
            --bg: #fffaf5;
            --surface: #ffffff;
            --surface-soft: #fff3e6;
            --line: #f3d9bd;
            --primary: #e85d2a;
            --primary-dark: #bb4318;
            --text: #1f2937;
            --muted: #6b7280;
            --success-bg: #ecfdf5;
            --success-text: #047857;
            --warn-bg: #fff7ed;
            --warn-text: #c2410c;
            --shadow: 0 18px 38px rgba(232, 93, 42, 0.12);
        }

        * { box-sizing: border-box; }
        body {
            margin: 0;
            font-family: Arial, sans-serif;
            background:
                radial-gradient(circle at top right, rgba(255, 211, 185, 0.55), transparent 32%),
                linear-gradient(180deg, #fffdfb 0%, var(--bg) 100%);
            color: var(--text);
        }
        .page { width: min(960px, calc(100% - 32px)); margin: 0 auto; padding: 28px 0 40px; }
        .hero, .card { background: var(--surface); border-radius: 28px; box-shadow: var(--shadow); overflow: hidden; }
        .hero-image-wrap { padding: 18px; background: linear-gradient(180deg, #fff7ed 0%, #ffffff 100%); }
        .hero-image { width: 100%; max-height: 340px; object-fit: contain; border-radius: 20px; background: #fff; display: block; }
        .hero-content, .card-body { padding: 24px; }
        .eyebrow { display: inline-flex; padding: 8px 12px; border-radius: 999px; background: var(--surface-soft); color: var(--primary); font-size: 12px; font-weight: 700; letter-spacing: .04em; text-transform: uppercase; }
        h1 { margin: 14px 0 10px; font-size: clamp(28px, 4vw, 40px); line-height: 1.12; }
        .description, p { color: var(--muted); line-height: 1.7; }
        .grid { display: grid; grid-template-columns: 1.4fr 1fr; gap: 20px; margin-top: 22px; }
        .card { padding: 0; }
        .status { margin-bottom: 16px; padding: 14px 16px; border-radius: 18px; font-size: 14px; line-height: 1.6; }
        .status.success { background: var(--success-bg); color: var(--success-text); }
        .status.warn { background: var(--warn-bg); color: var(--warn-text); }
        .price { display: flex; align-items: baseline; gap: 10px; margin: 16px 0; }
        .price strong { font-size: 32px; line-height: 1; }
        .button { width: 100%; border: none; border-radius: 18px; padding: 16px 18px; font: inherit; font-weight: 700; cursor: pointer; background: var(--primary); color: #fff; box-shadow: 0 14px 28px rgba(232, 93, 42, 0.24); }
        .button.secondary { margin-top: 10px; width: auto; box-shadow: none; }
        .button:hover { background: var(--primary-dark); }
        .meta-list { display: grid; gap: 12px; margin-top: 18px; }
        .meta-item { padding: 14px 16px; background: #fff7ed; border-radius: 18px; }
        .meta-item small { display: block; color: var(--muted); margin-bottom: 6px; }
        .audio-grid { display: grid; gap: 16px; margin-top: 20px; }
        .audio-item { padding: 18px; border: 1px solid var(--line); border-radius: 20px; background: #fffdfb; }
        .audio-item h3 { margin: 0 0 10px; font-size: 17px; }
        audio { width: 100%; margin-top: 10px; }
        @media (max-width: 860px) { .grid { grid-template-columns: 1fr; } }
    </style>
</head>
<body>
    <main class="page">
        <?php if ($errorMessage): ?>
            <section class="card">
                <div class="card-body">
                    <div class="status warn"><?php echo htmlspecialchars($errorMessage); ?></div>
                    <p>Hay kiem tra lai QR Payment hoac public API base URL trong file config.php.</p>
                </div>
            </section>
        <?php else: ?>
            <section class="hero">
                <div class="hero-image-wrap">
                    <img class="hero-image" src="<?php echo htmlspecialchars($poiImageUrl); ?>" alt="<?php echo htmlspecialchars($poi['name'] ?? 'POI'); ?>">
                </div>
                <div class="hero-content">
                    <span class="eyebrow">QR Payment POI</span>
                    <h1><?php echo htmlspecialchars($poi['name'] ?? 'POI'); ?></h1>
                    <p class="description"><?php echo htmlspecialchars($poi['description'] ?? ''); ?></p>
                </div>
            </section>

            <section class="grid">
                <div class="card">
                    <div class="card-body">
                        <h2>Thong tin diem den</h2>
                        <p>Ban co the xem mo ta cua POI nay mien phi ngay tren trang web public. Audio se bi khoa cho den khi hoan tat thanh toan QR.</p>

                        <div class="meta-list">
                            <?php if (!empty($poi['description_EN'])): ?>
                                <div class="meta-item">
                                    <small>Description (English)</small>
                                    <div><?php echo htmlspecialchars($poi['description_EN']); ?></div>
                                </div>
                            <?php endif; ?>

                            <?php if (!empty($poi['description_ZH'])): ?>
                                <div class="meta-item">
                                    <small>Description (Chinese)</small>
                                    <div><?php echo htmlspecialchars($poi['description_ZH']); ?></div>
                                </div>
                            <?php endif; ?>
                        </div>
                    </div>
                </div>

                <div class="card">
                    <div class="card-body">
                        <?php if ($message === 'paid'): ?>
                            <div class="status success">Thanh toan thanh cong. Toan bo audio cua POI nay da duoc mo khoa cho trinh duyet hien tai.</div>
                        <?php elseif ($message === 'payment-error'): ?>
                            <div class="status warn">Khong the xu ly thanh toan luc nay. Vui long thu lai sau.</div>
                        <?php endif; ?>

                        <?php if ($hasPaidAccess): ?>
                            <div class="status success">Audio da duoc mo khoa. Ban co the nghe tat ca audio ben duoi.</div>
                        <?php else: ?>
                            <div class="status warn">Audio hien dang bi khoa. Ban can thanh toan mot lan 10.000 VND de nghe tat ca audio cua diem den nay.</div>
                            <div class="price">
                                <strong><?php echo number_format(PUBLIC_PRICE_VND, 0, ',', '.'); ?></strong>
                                <span>VND / lan quet</span>
                            </div>
                            <form method="post">
                                <button type="submit" class="button" name="unlock_audio" value="1">Thanh toan 10.000 VND de mo khoa audio</button>
                            </form>
                        <?php endif; ?>

                        <div class="meta-list">
                            <div class="meta-item">
                                <small>Ma khach quet</small>
                                <div><?php echo htmlspecialchars($guestId); ?></div>
                            </div>
                            <div class="meta-item">
                                <small>Pham vi mo khoa</small>
                                <div>Tat ca audio cua POI nay tren trang public</div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <section class="card" style="margin-top: 22px;">
                <div class="card-body">
                    <h2>Audio cua POI</h2>

                    <?php if (!$hasPaidAccess): ?>
                        <p>Ban dang o che do xem thong tin. Audio se chi hien sau khi thanh toan thanh cong.</p>
                    <?php else: ?>
                        <div class="audio-grid">
                            <?php echo render_tts_button('Audio mo ta tieng Viet', $poi['description'] ?? '', 'vi-VN', 'Doc noi dung mo ta bang giong doc cua trinh duyet.'); ?>
                            <?php echo render_tts_button('Audio mo ta English', $poi['description_EN'] ?? '', 'en-US', 'Read the English description with browser speech.'); ?>
                            <?php echo render_tts_button('Audio mo ta Chinese', $poi['description_ZH'] ?? '', 'zh-CN', 'Doc mo ta tieng Trung bang giong doc cua trinh duyet.'); ?>

                            <?php echo render_audio_player('Audio file tieng Viet', 'File audio upload cho noi dung tieng Viet.', $audioVi); ?>
                            <?php echo render_audio_player('Audio file English', 'Uploaded audio track for the English content.', $audioEn); ?>
                            <?php echo render_audio_player('Audio file Chinese', 'Uploaded audio track for the Chinese content.', $audioZh); ?>
                        </div>
                    <?php endif; ?>
                </div>
            </section>
        <?php endif; ?>
    </main>
    <script>
        function speakText(language, text) {
            if (!text || !('speechSynthesis' in window)) {
                alert('Trinh duyet hien tai khong ho tro phat audio mo ta.');
                return;
            }

            window.speechSynthesis.cancel();
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = language;
            window.speechSynthesis.speak(utterance);
        }
    </script>
</body>
</html>
