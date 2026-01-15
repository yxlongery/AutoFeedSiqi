using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 颜色
    /// </summary>
    public enum Color
    {
        [Display(Name = "全彩", Description = "全彩")]
        Colorful = 4,

        [Display(Name = "黑白", Description = "黑白")]
        BlackAndWhite = 5,

        [Display(Name = "混合", Description = "混合")]
        Mixing = 8
    }
}
