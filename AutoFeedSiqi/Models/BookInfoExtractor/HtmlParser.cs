using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace AutoFeedSiqi.Models.BookInfoExtractor
{
    /// <summary>
    /// HTML解析器类
    /// </summary>
    public static class HtmlParser
    {
        /// <summary>
        /// 从HTML内容中提取图书信息
        /// </summary>
        /// <param name="html">HTML内容</param>
        /// <returns>解析后的图书信息对象</returns>
        public static BookInfo1 ParseBookInfo(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("HTML内容不能为空");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var bookInfo = new BookInfo1();

            // 提取作者
            doc.LoadHtml(Regex.Match(html, @"<span.*作者.*?<br>", RegexOptions.Singleline).Value);
            var authorNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'作者')]/following-sibling::a");
            if (authorNode != null)
                bookInfo.Author = Regex.Replace(authorNode.InnerText.Trim().Replace("\n", " "), @"\s+", " ");
            else
            {
                authorNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'作者')]/following-sibling::text()");
                if (authorNode != null)
                    bookInfo.Author = Regex.Replace(authorNode.InnerText.Trim().Replace("\n", " "), @"\s+", " ");
            }

            // 提取出版社
            doc.LoadHtml(Regex.Match(html, @"<span.*出版社.*?<br>", RegexOptions.Singleline).Value);
            var publisherNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'出版社')]/following-sibling::a");
            if (publisherNode != null)
                bookInfo.Publisher = Regex.Replace(publisherNode.InnerText.Trim().Replace("\n", " "), @"\s+", " ");
            else
            {
                publisherNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'出版社')]/following-sibling::text()");
                if (publisherNode != null)
                    bookInfo.Publisher = Regex.Replace(publisherNode.InnerText.Trim().Replace("\n", " "), @"\s+", " ");
            }

            // 提取出品方、
            doc.LoadHtml(Regex.Match(html, @"<span.*出品方.*?<br>", RegexOptions.Singleline).Value);
            var producerNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'出品方')]/following-sibling::a");
            if (producerNode != null)
                bookInfo.Producer = Regex.Replace(producerNode.InnerText.Trim().Replace("\n", " "), @"\s+", " ");
            else
            {
                producerNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'出品方')]/following-sibling::text()");
                if (producerNode != null)
                    bookInfo.Producer = Regex.Replace(producerNode.InnerText.Trim().Replace("\n", " "), @"\s+", " ");
            }

            // 提取原作名
            doc.LoadHtml(Regex.Match(html, @"<span.*原作名.*?<br>", RegexOptions.Singleline).Value);
            var originalTitleNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'原作名')]/following-sibling::text()");
            if (originalTitleNode != null)
                bookInfo.OriginalTitle = originalTitleNode.InnerText.Trim();

            // 提取译者（可能有多个）
            doc.LoadHtml(Regex.Match(html, @"<span.*译者.*?<br>", RegexOptions.Singleline).Value);
            var translatorNodes = doc.DocumentNode.SelectNodes("//span[contains(@class,'pl') and contains(text(),'译者')]/following-sibling::a");
            if (translatorNodes != null)
            {
                foreach (var node in translatorNodes)
                {
                    bookInfo.Translators.Add(Regex.Replace(node.InnerText.Trim().Replace("\n", " "), @"\s+", " "));
                }
            }
            else
            {
                var translatorTextNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'译者')]/following-sibling::text()");
                if (translatorTextNode != null)
                    bookInfo.Translators.Add(Regex.Replace(translatorTextNode.InnerText.Trim().Replace("\n", " "), @"\s+", " "));
            }


            // 提取出版年
            doc.LoadHtml(Regex.Match(html, @"<span.*出版年.*?<br>", RegexOptions.Singleline).Value);
            var yearNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'出版年')]/following-sibling::text()");
            if (yearNode != null)
                bookInfo.PublicationYear = Regex.Replace(yearNode.InnerText.Trim().Replace("\n", ""), @"\s+", "");

            // 提取页数
            doc.LoadHtml(Regex.Match(html, @"<span.*页数.*?<br>", RegexOptions.Singleline).Value);
            var pagesNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'页数')]/following-sibling::text()");
            if (pagesNode != null)
                bookInfo.Pages = pagesNode.InnerText.Trim();

            // 提取定价
            doc.LoadHtml(Regex.Match(html, @"<span.*定价.*?<br>", RegexOptions.Singleline).Value);
            var priceNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'定价')]/following-sibling::text()");
            if (priceNode != null)
                bookInfo.Price = priceNode.InnerText.Trim();

            // 提取装帧
            doc.LoadHtml(Regex.Match(html, @"<span.*装帧.*?<br>", RegexOptions.Singleline).Value);
            var bindingNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'装帧')]/following-sibling::text()");
            if (bindingNode != null)
                bookInfo.Binding = bindingNode.InnerText.Trim();

            // 提取丛书
            doc.LoadHtml(Regex.Match(html, @"<span.*丛书.*?<br>", RegexOptions.Singleline).Value);
            var seriesNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'丛书')]/following-sibling::a");
            if (seriesNode != null)
                bookInfo.Series = seriesNode.InnerText.Trim();

            // 提取ISBN
            doc.LoadHtml(Regex.Match(html, @"<span.*ISBN.*?<br>", RegexOptions.Singleline).Value);
            var isbnNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'pl') and contains(text(),'ISBN')]/following-sibling::text()");
            if (isbnNode != null)
                bookInfo.ISBN = Regex.Replace(isbnNode.InnerText.Trim().Replace("\n", ""), @"\s+", "");

            return bookInfo;
        }
    }
}
