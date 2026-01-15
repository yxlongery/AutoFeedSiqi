namespace AutoFeedSiqi.Models.BookInfoExtractor
{
    /// <summary>
    /// 图书信息类
    /// </summary>
    public class BookInfo1
    {
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Producer { get; set; }
        public string OriginalTitle { get; set; }
        public List<string> Translators { get; set; } = new List<string>();
        public string PublicationYear { get; set; }
        public string Pages { get; set; }
        public string Price { get; set; }
        public string Binding { get; set; }
        public string Series { get; set; }
        public string ISBN { get; set; }

        /// <summary>
        /// 转换为字符串表示
        /// </summary>
        public override string ToString()
        {
            var translators = Translators.Count > 0 ? $"；{string.Join(" ", Translators)} 翻译" : "";
            var publicationYear = DateTime.TryParse(PublicationYear, out var pubYear) ? pubYear : DateTime.MinValue;

            return $"\t{ISBN}\t{Author}{translators}\t{Publisher}\t{publicationYear:yyyy-MM}\t";
        }
    }
}
