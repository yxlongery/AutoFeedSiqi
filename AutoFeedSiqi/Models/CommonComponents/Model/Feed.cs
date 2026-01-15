using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 思齐PT站的发种记录
    /// </summary>
    public class Feed
    {
        /// <summary>
        /// Id
        /// </summary>
        [Display(Name = "Id")]
        public uint Id { get; set; }

        /// <summary>
        /// SiqiId
        /// </summary>
        [Display(Name = "思齐Id")]
        public uint SiqiId { get; set; }

        /// <summary>
        /// 书名
        /// </summary>
        [Display(Name = "书名")]
        [Required(ErrorMessage = "书名是必填项")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// ISBN
        /// </summary>
        [Display(Name = "ISBN")]
        [RegularExpression(@"\d{10,13}|", ErrorMessage = "ISBN长度应为10或13位数字,或者留空")]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// 作者
        /// </summary>
        [Display(Name = "作者")]
        public string Authors { get; set; } = string.Empty;

        /// <summary>
        /// 出版社
        /// </summary>
        [Display(Name = "出版社")]
        public string Press { get; set; } = string.Empty;

        /// <summary>
        /// 出版日期
        /// </summary>
        [Display(Name = "出版日期")]
        public DateTime Date { get; set; } = DateTime.UnixEpoch;

        /// <summary>
        /// 内容简介
        /// </summary>
        [Display(Name = "内容简介")]
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// 豆瓣Id
        /// </summary>
        [Display(Name = "豆瓣Id")]
        public int DoubanId { get; set; }

        /// <summary>
        /// 类型：连环画、插图画、漫画等等
        /// </summary>
        [Display(Name = "类型")]
        public Type Type { get; set; } = Type.ComicStrip;

        /// <summary>
        /// 类别：历史传记、人文艺术、科普科技等等
        /// </summary>
        [Display(Name = "类别")]
        public Sort Sort { get; set; } = Sort.BiographyAndHistory;

        /// <summary>
        /// 来源：自制、个人收集、自购、转载等等
        /// </summary>
        [Display(Name = "来源")]
        public Source Source { get; set; } = Source.PersonalCollection;

        /// <summary>
        /// 状态：单本完、部分、合集完、精选集、丛书等等
        /// </summary>
        [Display(Name = "状态")]
        public Condition Condition { get; set; } = Condition.CompleteSingleVolume;

        /// <summary>
        /// 颜色：黑白、全彩、混合等等
        /// </summary>
        [Display(Name = "颜色")]
        public Color Color { get; set; } = Color.BlackAndWhite;

        /// <summary>
        /// 格式：pdf、mobi等等
        /// </summary>
        [Display(Name = "格式")]
        public Extension Extension { get; set; } = Extension.PDF;

        /// <summary>
        /// 语言：中简、中繁、英语等等
        /// </summary>
        [Display(Name = "语言")]
        public Language Language { get; set; } = Language.SimplifiedChinese;

        /// <summary>
        /// 品质：一般质量、高质量、超高质量等
        /// </summary>
        [Display(Name = "品质")]
        public Quality Quality { get; set; } = Quality.HighQuality;

        /// <summary>
        /// 制作组：SQB，其他
        /// </summary>
        [Display(Name = "制作组")]
        public Team Team { get; set; } = Team.Other;

        /// <summary>
        /// 预览图url，用分号分割
        /// </summary>
        [Display(Name = "预览图URL")]
        [JsonIgnore]
        public string PreviewUrl { get; set; } = string.Empty;

        /// <summary>
        /// ID对应的文件目录
        /// </summary>
        [Display(Name = "文件目录")]
        [JsonIgnore]
        [Required(ErrorMessage = "文件目录是必填项")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// ID对应的当前任务
        /// </summary>
        [Display(Name = "发种任务")]
        [JsonIgnore]
        public FeedTask Task { get; set; } = FeedTask.None;

        /// <summary>
        /// ID对应的当前任务完成情况
        /// </summary>
        [Display(Name = "完成情况")]
        [JsonIgnore]
        public bool TaskState { get; set; } = false;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        public DateTime CreateAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Display(Name = "最后更新时间")]
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
