using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 格式
    /// </summary>
    public enum Extension
    {
        [Display(Name = "PDF", Description = "PDF")]
        PDF = 5,

        [Display(Name = "CBZ/CBR", Description = "CBZ/CBR")]
        CBZOrCBR = 1,

        [Display(Name = "EPUB", Description = "EPUB")]
        EPUB = 3,

        [Display(Name = "图片格式", Description = "图片格式")]
        ImageFormat = 7,

        [Display(Name = "MOBI", Description = "MOBI")]
        MOBI = 6,

        [Display(Name = "混合", Description = "混合")]
        Mixing = 8,

        [Display(Name = "其他", Description = "其他")]
        Other = 4
    }
}
