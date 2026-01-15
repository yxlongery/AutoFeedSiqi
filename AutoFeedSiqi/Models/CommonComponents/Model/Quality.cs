using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 品质
    /// </summary>
    public enum Quality
    {
        [Display(Name = "超高质量", Description = "超高质量")]
        SuperHighQuality = 1,

        [Display(Name = "高质量", Description = "高质量")]
        HighQuality = 2,

        [Display(Name = "一般质量", Description = "一般质量")]
        GeneralQuality = 3
    }
}
