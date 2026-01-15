using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 来源
    /// </summary>
    public enum Source
    {
        [Display(Name = "自购", Description = "自购")]
        SelfPurchase = 7,

        [Display(Name = "自制", Description = "自制")]
        SelfMade = 8,

        [Display(Name = "转载", Description = "转载")]
        Repost = 1,

        [Display(Name = "个人收集", Description = "个人收集")]
        PersonalCollection = 2
    }
}
