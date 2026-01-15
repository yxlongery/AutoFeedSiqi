//using AutoFeedSiqi.Models.CommonComponents.Model;
//using PdfiumViewer;
//using Serilog;
//using System.Drawing;
//using System.Drawing.Imaging;

//namespace AutoFeedSiqi.Models.CommonComponents
//{
//    public class PdfToImage
//    {
//        /// <summary>
//        ///
//        /// </summary>
//        /// <param name="filePath">pdf文件路径</param>
//        /// <param name="picPath">picture文件路径</param>
//        public static void PdfToPic(string filePath, string picPath)
//        {
//            var pdf = PdfDocument.Load(filePath);
//            var pdfpage = pdf.PageCount;
//            var pagesizes = pdf.PageSizes;

//            for (int i = 1; i <= pdfpage; i++)
//            {
//                var size = new Size
//                {
//                    Height = (int)pagesizes[(i - 1)].Height,
//                    Width = (int)pagesizes[(i - 1)].Width
//                };
//                //可以把".jpg"写成其他形式
//                RenderPage(filePath, i, size, picPath);
//            }
//        }
//        /// <summary>
//        ///
//        /// </summary>
//        /// <param name="filePath">pdf文件路径</param>
//        public static List<string> PdfToPicRandom(string filePath, Feed feed)
//        {
//            var seedId = (int)feed.Id;
//            var path = feed.Path;
//            var picPaths = new List<string>();

//            using var pdf = PdfDocument.Load(filePath);
//            Log.Information($"已选取pdf：{{{nameof(filePath)}}}", filePath);
//            var totalPages = pdf.PageCount;
//            var pagesizes = pdf.PageSizes;

//            var pageNumbers = GetRandomPageNumbers(totalPages);
//            foreach (var pageNumber in pageNumbers)
//            {
//                //如果pageNumber数值小于100，则在前面补0，保证文件名排序正确
//                var picPath = Path.Combine(FeedPtSiqi.Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"preview_page{pageNumber:D3} {{seedid-{seedId}}}.jpg");
//                picPaths.Add(picPath);
//                var size = new Size
//                {
//                    Height = (int)pagesizes[(pageNumber - 1)].Height,
//                    Width = (int)pagesizes[(pageNumber - 1)].Width
//                };
//                //可以把".jpg"写成其他形式
//                RenderPage(filePath, pageNumber, size, picPath);
//                Log.Information($"已获取预览图：{{{nameof(picPath)}}}", picPath);
//            }
//            return picPaths;
//        }

//        public static List<string> PdfToPicRandomForLinux(string filePath, Feed feed)
//        {
//            var seedId = (int)feed.Id;
//            var path = feed.Path;
//            var picPaths = new List<string>();

//            using var pdf = PdfDocument.Load(filePath);
//            Log.Information($"已选取pdf：{{{nameof(filePath)}}}", filePath);
//            var totalPages = pdf.PageCount;
//            var pagesizes = pdf.PageSizes;

//            var pageNumbers = GetRandomPageNumbers(totalPages);
//            foreach (var pageNumber in pageNumbers)
//            {
//                //如果pageNumber数值小于100，则在前面补0，保证文件名排序正确
//                var picPath = Path.Combine(FeedPtSiqi.Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"preview_page{pageNumber:D3} {{seedid-{seedId}}}.jpg");
//                picPaths.Add(picPath);
//                var size = new Size
//                {
//                    Height = (int)pagesizes[(pageNumber - 1)].Height,
//                    Width = (int)pagesizes[(pageNumber - 1)].Width
//                };
//                //可以把".jpg"写成其他形式
//                RenderPage(filePath, pageNumber, size, picPath);
//                Log.Information($"已获取预览图：{{{nameof(picPath)}}}", picPath);
//            }
//            return picPaths;
//        }

//        /// <summary>
//        /// 获得随机页码
//        /// </summary>
//        /// <param name="totalPages"></param>
//        /// <returns></returns>
//        private static List<int> GetRandomPageNumbers(int totalPages)
//        {
//            var pageNumbers = new List<int>() { 1 };
//            for (int count = 1; count <= 3; count++)
//            {
//                var pageNumber = new Random().Next(2, totalPages);
//                if (!pageNumbers.Contains(pageNumber))
//                {
//                    pageNumbers.Add(pageNumber);
//                }
//                else
//                {
//                    count--;
//                }
//            }

//            pageNumbers.Sort();
//            return pageNumbers;
//        }

//        private static void RenderPage(string pdfPath, int pageNumber, Size size, string outputPath, int dpi = 300)
//        {
//            using (var document = PdfDocument.Load(pdfPath))
//            using (var stream = new FileStream(outputPath, FileMode.Create))
//            using (var image = GetPageImage(pageNumber, size, document, dpi))
//            {
//                image.Save(stream, ImageFormat.Jpeg);
//            }
//        }
//        private static Image GetPageImage(int pageNumber, Size size, PdfDocument document, int dpi)
//        {
//            return document.Render(pageNumber - 1, size.Width, size.Height, dpi, dpi, PdfRenderFlags.Annotations);

//        }
//    }
//}
