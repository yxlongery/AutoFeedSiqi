using AutoFeedSiqi.Models.CommonComponents.Model;
using Serilog;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO.Compression;

namespace AutoFeedSiqi.Models.CommonComponents
{
    public class CBZToImage
    {
        public static List<string> ToPicRandom1(string filePath, Feed feed)
        {
            var seedId = (int)feed.Id;
            var picPaths = new List<string>();

            using ZipArchive archive = ZipFile.OpenRead(filePath);
            Log.Information($"已选取cbz：{{{nameof(filePath)}}}", filePath);

            var archiveEntries = archive.Entries;
            //去除entries中length为0的项
            var entries = archiveEntries.Where(e => e.Length > 0).ToList();
            var totalPages = entries.Count;
            var pageNumbers = PdfToImage.GetRandomPageNumbers(totalPages);
            var count = 0;
            //只提取pageNumbers指定的页码
            foreach (var entry in entries)
            {
                if (pageNumbers.Contains(count))
                {
                    var pageNumber = count;
                    //如果pageNumber数值小于100，则在前面补0，保证文件名排序正确
                    //pageNumber+1是因为pageNumber是从0开始的，而页码是从1开始的
                    var picPath = Path.Combine(FeedPtSiqi.Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"preview_page{pageNumber + 1:D3} {{seedid-{seedId}}}.jpg");
                    picPaths.Add(picPath);
                    entry.ExtractToFile(picPath + ".tmp", true);
                    PdfToImage.ImageCompression(picPath + ".tmp", picPath);
                    Log.Information($"已获取预览图：{{{nameof(picPath)}}}", picPath);
                }
                count++;
            }
            return picPaths;
        }
        public static async Task<List<string>> ToPicRandom(string filePath, Feed feed)
        {
            var seedId = (int)feed.Id;
            var picPaths = new List<string>();

            using var archive = ArchiveFactory.Open(filePath);
            Log.Information($"已选取cbz：{{{nameof(filePath)}}}", filePath);

            // 配置解压选项
            var extractOptions = new ExtractionOptions
            {
                ExtractFullPath = false, // 不保留压缩包内的目录结构（直接解压文件到目标目录）
                Overwrite = true         // 覆盖已存在的同名文件
            };
            var archiveEntries = archive.Entries;
            //去除entries中length为0的项
            var entries = archiveEntries.Where(e => e.IsDirectory == false).ToList();
            var totalPages = entries.Count;
            var pageNumbers = PdfToImage.GetRandomPageNumbers(totalPages);
            var count = 0;
            //只提取pageNumbers指定的页码
            foreach (var entry in entries)
            {
                if (pageNumbers.Contains(count))
                {
                    var pageNumber = count;
                    //如果pageNumber数值小于100，则在前面补0，保证文件名排序正确
                    //pageNumber+1是因为pageNumber是从0开始的，而页码是从1开始的
                    var picPath = Path.Combine(FeedPtSiqi.Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"preview_page{pageNumber + 1:D3} {{seedid-{seedId}}}.jpg");
                    picPaths.Add(picPath);
                    // 执行解压
                    await entry.WriteToFileAsync(picPath + ".tmp", extractOptions);
                    PdfToImage.ImageCompression(picPath + ".tmp", picPath);
                    Log.Information($"已获取预览图：{{{nameof(picPath)}}}", picPath);
                }
                count++;
            }
            return picPaths;
        }
    }
}
