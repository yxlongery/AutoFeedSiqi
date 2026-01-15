namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    public class PtSiqiPath
    {
        /// <summary>
        /// 做种Id
        /// </summary>
        public int SeedId { get; set; }

        /// <summary>
        /// 种子名
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 选择的PDF文件
        /// </summary>
        public string? SelectPDF { get; set; }

        /// <summary>
        /// PreviewUrl
        /// </summary>
        public string? PreviewUrl { get; set; }

        /// <summary>
        /// Preview
        /// </summary>
        public string? Preview { get; set; }

        /// <summary>
        /// Preview
        /// </summary>
        public string? Nfo { get; set; }

        /// <summary>
        /// Torrent
        /// </summary>
        public string? Torrent { get; set; }

        /// <summary>
        /// CreatTorrentCmd
        /// </summary>
        public string? CreatTorrentCmd { get; set; }

        /// <summary>
        /// SeedId对应的Paths
        /// </summary>
        public List<string>? Paths { get; set; }
    }
}
