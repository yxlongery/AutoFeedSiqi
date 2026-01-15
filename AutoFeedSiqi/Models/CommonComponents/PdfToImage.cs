using AutoFeedSiqi.Models.CommonComponents.Model;
using PDFtoImage;
using Serilog;
using SkiaSharp;
using System.Runtime.Versioning;

namespace AutoFeedSiqi.Models.CommonComponents
{
    public class PdfToImage
    {
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        public static List<string> ToPicRandom(string filePath, Feed feed)
        {
            var seedId = (int)feed.Id;
            var picPaths = new List<string>();
            Log.Information($"已选取pdf：{{{nameof(filePath)}}}", filePath);

            // 将输入文件读取到内存中
            //var bytes = File.ReadAllBytes(filePath); 
            //var pdfAsBase64String = Convert.ToBase64String(bytes); // 将读取的数据进行Base64编码后写入输出文件中
            //using var pdfFileStream = File.OpenRead(filePath);

            // 先获取总页数
            int totalPages;
            using (var pdfFileStream = File.OpenRead(filePath))
            {
                totalPages = Conversion.GetPageCount(pdfFileStream);
            }

            // 再生成预览图
            var pageNumbers = GetRandomPageNumbers(totalPages);
            using (var pdfFileStream = File.OpenRead(filePath))
            {
                var bitmaps = Conversion.ToImages(pdfFileStream, pageNumbers, options: new PDFtoImage.RenderOptions() { Dpi = 72 });//直接返回图片数组 
                var count = 0;
                foreach (var bitmap in bitmaps)
                {
                    var pageNumber = pageNumbers[count++];
                    //如果pageNumber数值小于100，则在前面补0，保证文件名排序正确
                    //pageNumber+1是因为pageNumber是从0开始的，而页码是从1开始的
                    var picPath = Path.Combine(FeedPtSiqi.Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"preview_page{pageNumber + 1:D3} {{seedid-{seedId}}}.jpg");
                    picPaths.Add(picPath);
                    using (bitmap)
                    {
                        using var fileStream = new FileStream(picPath + ".tmp", FileMode.Create, FileAccess.Write);
                        var format = SKEncodedImageFormat.Jpeg;
                        bitmap.Encode(fileStream, format, 80);
                    }
                    ImageCompression(picPath + ".tmp", picPath);
                    Log.Information($"已获取预览图：{{{nameof(picPath)}}}", picPath);
                }
            }
            return picPaths;
        }

        /// <summary>
        /// 获得3个随机页码,包含封面页0，共4页，排序后返回，页码从0开始，即第一页是0，第二页是1，以此类推，最后一页是totalPages-1
        /// 如果totalPages小于等于4，则返回所有页码
        /// </summary>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        public static List<int> GetRandomPageNumbers(int totalPages)
        {
            if (totalPages <= 4)
            {
                var allPageNumbers = new List<int>();
                for (int i = 0; i < totalPages; i++)
                {
                    allPageNumbers.Add(i);
                }
                return allPageNumbers;
            }
            var pageNumbers = new List<int>() { 0 };
            for (int count = 1; count <= 3; count++)
            {
                var pageNumber = new Random().Next(1, totalPages);
                if (!pageNumbers.Contains(pageNumber))
                {
                    pageNumbers.Add(pageNumber);
                }
                else
                {
                    count--;
                }
            }

            pageNumbers.Sort();
            return pageNumbers;
        }

        /// <summary>
        /// 压缩图片，大小在500KB以内，并且删除原图，保留压缩后的图片为原图名称
        /// </summary>
        /// <param name="picPathTmp"></param>
        /// <param name="picPath"></param>
        public static void ImageCompression(string picPathTmp, string picPath)
        {
            using FileStream input = File.OpenRead(picPathTmp);
            ImageCompression(input, picPath);
            File.Delete(picPathTmp);
        }

        /// <summary>
        /// 压缩图片，大小在500KB以内
        /// </summary>
        /// <param name="picPathTmp"></param>
        /// <param name="picPath"></param>
        public static void ImageCompression(FileStream picPathTmp, string picPath)
        {
            using SKBitmap original = SKBitmap.Decode(picPathTmp);
            var quality = 90;
            using SKImage image = SKImage.FromBitmap(original);
            using FileStream output = File.OpenWrite(picPath);
            var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            while (data.Size > 500 * 1024 && quality > 10)
            {
                quality -= 10;
                data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            }
            data.SaveTo(output);
        }
    }
}
