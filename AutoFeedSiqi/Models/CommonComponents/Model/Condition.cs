using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 状态
    /// </summary>
    public enum Condition
    {
        [Display(Name = "单本完", Description = "单本完")]
        CompleteSingleVolume = 10,

        [Display(Name = "部分", Description = "部分")]
        Part = 9,

        [Display(Name = "合集完", Description = "合集完")]
        CompleteCollection = 8,

        [Display(Name = "精选集", Description = "精选集")]
        SelectedCollection = 6,

        [Display(Name = "丛书", Description = "丛书")]
        SeriesOfBooks = 5
    }
}
