using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 发种上传思齐PT站的相关配置
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Id
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// AppToken
        /// </summary>
        [Display(Name = "AppToken", Description = "AppToken")]
        [Required(ErrorMessage = "Token是必填项")]
        [RegularExpression(@"siqi2025", ErrorMessage = "AppToken不正确")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// ExcelPath
        /// </summary>
        [Display(Name = "Excel路径", Description = "Excel路径")]
        public string ExcelPath { get; set; } = string.Empty;

        /// <summary>
        /// AutoDataPath
        /// </summary>
        [Display(Name = "自动数据目录", Description = "自动数据目录")]
        public string AutoDataPath { get; set; } = $"{Path.Combine("data", "autodata")}";

        /// <summary>
        /// IskyHost https://img.si-qi.xyz/api/v1
        /// </summary>
        [Display(Name = "Isky网址", Description = "Isky网址")]
        [Url(ErrorMessage = "不是有效的网址")]
        public string IskyHost { get; set; } = "https://img.si-qi.xyz/api/v1";

        /// <summary>
        /// IskyEmail
        /// </summary>
        [Display(Name = "IskyEmail", Description = "IskyEmail")]
        [Required(ErrorMessage = "IskyEmail是必填项")]
        [EmailAddress(ErrorMessage = "不是有效的邮箱")]
        public string IskyEmail { get; set; } = string.Empty;

        /// <summary>
        /// IskyPassword
        /// </summary>
        [Display(Name = "Isky密码", Description = "Isky密码")]
        [Required(ErrorMessage = "IskyPassword是必填项")]
        public string IskyPassword { get; set; } = string.Empty;

        /// <summary>
        /// IskyToken
        /// </summary>
        [Display(Name = "IskyToken", Description = "IskyToken")]
        [RegularExpression(@"^Bearer .*$", ErrorMessage = "IskyToken以Bearer开头")]
        public string IskyToken { get; set; } = string.Empty;

        /// <summary>
        /// 思齐track https://si-qi.xyz/announce.php
        /// </summary>
        [Display(Name = "思齐track", Description = "思齐track")]
        [Url(ErrorMessage = "不是有效的网址")]
        public string SiqiTrack { get; set; } = "https://si-qi.xyz/announce.php";

        /// <summary>
        /// 思齐网址Host https://si-qi.xyz
        /// </summary>
        [Display(Name = "思齐网址", Description = "思齐网址")]
        [Url(ErrorMessage = "不是有效的网址")]
        public string SiqiHost { get; set; } = "https://si-qi.xyz";

        /// <summary>
        /// 思齐cookie
        /// </summary>
        [Display(Name = "思齐cookie", Description = "思齐cookie")]
        [Required(ErrorMessage = "SiqiCookie是必填项")]
        public string SiqiCookie { get; set; } = string.Empty;

        /// <summary>
        /// 思齐Passkey
        /// </summary>
        [Display(Name = "思齐Passkey", Description = "思齐Passkey")]
        [Required(ErrorMessage = "SiqiPasskey是必填项")]
        public string SiqiPasskey { get; set; } = string.Empty;

        /// <summary>
        /// 思齐发种员权限Token
        /// </summary>
        [Display(Name = "发种员Token", Description = "发种员Token")]
        [RegularExpression(@"d6k55a0lvancydsnnrgumxf7u8n56g52", ErrorMessage = "发种员权限Token不正确")]
        public string SiqiUploaderToken { get; set; } = string.Empty;

        /// <summary>
        /// qBittorrent webUI 网址
        /// </summary>
        [Display(Name = "Qb网址", Description = "Qb网址")]
        [Required(ErrorMessage = "QbHost是必填项")]
        [Url(ErrorMessage = "不是有效的网址")]
        public string QbHost { get; set; } = string.Empty;

        /// <summary>
        /// qBittorrent webUI 用户名
        /// </summary>
        [Display(Name = "Qb用户名", Description = "Qb用户名")]
        [Required(ErrorMessage = "QbUserName是必填项")]
        public string QbUserName { get; set; } = string.Empty;

        /// <summary>
        /// qBittorrent webUI 密码
        /// </summary>
        [Display(Name = "Qb密码", Description = "Qb密码")]
        [Required(ErrorMessage = "QbPassword是必填项")]
        public string QbPassword { get; set; } = string.Empty;

        /// <summary>
        /// qBittorrent webUI 标签，可以多个，用,分割
        /// </summary>
        [Display(Name = "Qb标签", Description = "qBittorrent webUI 标签，可以多个，用,分割")]
        public string QbTags { get; set; } = string.Empty;

        /// <summary>
        /// 发种是否匿名
        /// </summary>
        [Display(Name = "发种是否匿名", Description = "发种是否匿名")]
        public bool Invisible { get; set; } = true;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Display(Name = "创建时间", Description = "创建时间")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Display(Name = "最后更新时间", Description = "最后更新时间")]
        public DateTime LastUpdatedAt { get; set; }
    }
}
