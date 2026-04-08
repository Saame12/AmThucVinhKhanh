using Microsoft.AspNetCore.Razor.TagHelpers;

namespace VinhKhanhFood.Admin.TagHelpers;

[HtmlTargetElement("loc")]
public class LocalizationTagHelper : TagHelper
{
    [HtmlAttributeName("key")]
    public string Key { get; set; } = string.Empty;

    [HtmlAttributeName("lang")]
    public string? Language { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var localizedText = Services.LocalizationService.GetString(Key, Language);
        output.TagName = null;
        output.Content.SetContent(localizedText);
    }
}
