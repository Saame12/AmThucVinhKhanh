using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinhKhanhFood.App.Models
{
    public class FoodLocation
    {
        public int Id { get; set; }

        // --- TIẾNG VIỆT (Mặc định) ---
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AudioUrl { get; set; }

        // --- TIẾNG ANH ---
        public string? Name_EN { get; set; }
        public string? Description_EN { get; set; }
        public string? AudioUrl_EN { get; set; }

        // --- TIẾNG TRUNG ---
        public string? Name_ZH { get; set; }
        public string? Description_ZH { get; set; }
        public string? AudioUrl_ZH { get; set; }

        // --- CÁC THÔNG SỐ CHUNG ---
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public int? OwnerId { get; set; }

        // =================================================V
        // LOGIC HIỂN THỊ ĐA NGÔN NGỮ (DISPLAY PROPERTIES)
        // =================================================

        // Tự động chọn Tên theo ngôn ngữ hệ thống
        public string DisplayName => App.CurrentLanguage switch
        {
            "en" => !string.IsNullOrWhiteSpace(Name_EN) ? Name_EN : Name,
            "zh" => !string.IsNullOrWhiteSpace(Name_ZH) ? Name_ZH : Name,
            _ => Name // Mặc định là tiếng Việt (vi)
        };

        // Tự động chọn Mô tả
        public string DisplayDescription => App.CurrentLanguage switch
        {
            "en" => !string.IsNullOrWhiteSpace(Description_EN) ? Description_EN : Description,
            "zh" => !string.IsNullOrWhiteSpace(Description_ZH) ? Description_ZH : Description,
            _ => Description
        };

        // Tự động chọn File âm thanh (Cực kỳ quan trọng cho Audio Guide)
        public string? DisplayAudioUrl => App.CurrentLanguage switch
        {
            "en" => !string.IsNullOrWhiteSpace(AudioUrl_EN) ? AudioUrl_EN : AudioUrl,
            "zh" => !string.IsNullOrWhiteSpace(AudioUrl_ZH) ? AudioUrl_ZH : AudioUrl,
            _ => AudioUrl
        };
    }
}
