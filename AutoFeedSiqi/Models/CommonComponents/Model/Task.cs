using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 发种任务
    /// </summary>
    public enum FeedTask
    {
        [Display(Name = "未开始", Description = "未开始")]
        None,

        //[Display(Name = "上传数据", Description = "上传数据")]
        //UploadData,

        [Display(Name = "获得预览图", Description = "获得预览图")]
        GetPreviews,

        [Display(Name = "获得预览图链接", Description = "获得预览图链接")]
        GetPreviewsUrls,

        [Display(Name = "生成NFO", Description = "生成NFO")]
        CreateNfo,

        [Display(Name = "制种", Description = "制种")]
        CreateSeed,

        [Display(Name = "发种", Description = "发种")]
        Feed,

        [Display(Name = "做种", Description = "做种")]
        Seed
    }
}
