using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 类型
    /// </summary>
    public enum Type
    {
        [Display(Name = "漫画", Description = "漫画")]
        ComicStrip = 411,

        [Display(Name = "软件", Description = "软件")]
        Software = 417,

        [Display(Name = "摄影", Description = "摄影")]
        Photography = 416,

        [Display(Name = "工具书", Description = "工具书")]
        ReferenceBooks = 415,

        [Display(Name = "杂志", Description = "杂志")]
        Magazine = 414,

        [Display(Name = "图片集", Description = "图片集")]
        PhotoAlbums = 413,

        [Display(Name = "连环画", Description = "连环画")]
        ComicBooks = 412,

        [Display(Name = "插图书", Description = "插图书")]
        IllustratedBooks = 418,

        [Display(Name = "教学", Description = "教学")]
        Teaching = 401,

        [Display(Name = "其他", Description = "其他")]
        Other = 409
    }
}
