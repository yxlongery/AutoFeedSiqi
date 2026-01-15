using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 语言
    /// </summary>
    public enum Language
    {
        [Display(Name = "中简", Description = "中简")]
        SimplifiedChinese = 1,

        [Display(Name = "中繁", Description = "中繁")]
        TraditionalChinese = 6,

        [Display(Name = "中英", Description = "中英")]
        ChineseAndEnglish = 7,

        [Display(Name = "英语", Description = "英语")]
        English = 2,

        [Display(Name = "日语", Description = "日语")]
        Japanese = 3,

        [Display(Name = "韩语", Description = "韩语")]
        Korean = 4,

        [Display(Name = "其他", Description = "其他")]
        Other = 5
    }
}
