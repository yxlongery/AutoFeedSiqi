using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 制作组
    /// </summary>
    public enum Team
    {
        [Display(Name = "SQB", Description = "SQB")]
        SQB = 6,

        [Display(Name = "其他", Description = "其他")]
        Other = 5
    }
}
