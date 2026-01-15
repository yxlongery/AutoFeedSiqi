using AutoFeedSiqi.Models.CommonComponents.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration
{
    public class FeedEntityTypeConfiguration : IEntityTypeConfiguration<Feed>
    {
        public void Configure(EntityTypeBuilder<Feed> entity)
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("feeds", tb => tb.HasComment("思齐PT站的发种记录"));

            entity.Property(e => e.Id)
                .HasComment("Id");
            entity.Property(e => e.SiqiId)
                .HasComment("思齐Id");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("书名");
            entity.Property(e => e.Isbn)
                .HasMaxLength(13)
                .HasComment("ISBN");
            entity.Property(e => e.Authors)
                .HasMaxLength(100)
                .HasComment("作者");
            entity.Property(e => e.Press)
                .HasMaxLength(100)
                .HasComment("出版社");
            entity.Property(e => e.Date)
                .HasComment("出版日期");
            entity.Property(e => e.Summary)
                .HasComment("内容简介");
            entity.Property(e => e.DoubanId)
                .HasComment("豆瓣Id");
            entity.Property(e => e.Type)
                .HasComment("类型：连环画、插图画、漫画等等");
            entity.Property(e => e.Sort)
                .HasComment("类别：历史传记、人文艺术、科普科技等等");
            entity.Property(e => e.Source)
                .HasComment("来源：自制、个人收集、自购、转载等等");
            entity.Property(e => e.Condition)
                .HasComment("状态：单本完、部分、合集完、精选集、丛书等等");
            entity.Property(e => e.Color)
                .HasComment("颜色：黑白、全彩、混合等等");
            entity.Property(e => e.Extension)
                .HasComment("格式：pdf、mobi等等");
            entity.Property(e => e.Language)
                .HasComment("语言：中简、中繁、英语等等");
            entity.Property(e => e.Quality)
                .HasComment("品质：一般质量、高质量、超高质量等");
            entity.Property(e => e.Team)
                .HasComment("制作组：SQB，其他");
            entity.Property(e => e.PreviewUrl)
                .HasComment("预览图url，用分号分割");
            entity.Property(e => e.Path)
                .HasComment("ID对应的文件目录");
            entity.Property(e => e.Task)
                .HasComment("ID对应的当前任务");
            entity.Property(e => e.TaskState)
                .HasComment("ID对应的当前任务完成情况");
            entity.Property(e => e.CreateAt)
                .HasComment("创建时间");
            entity.Property(e => e.LastUpdatedAt)
                .HasComment("最后更新时间");
        }
    }
}
