using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFeedSiqi.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configurations",
                columns: table => new
                {
                    id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    token = table.Column<string>(type: "TEXT", nullable: false),
                    excel_path = table.Column<string>(type: "TEXT", nullable: false),
                    auto_data_path = table.Column<string>(type: "TEXT", nullable: false),
                    isky_host = table.Column<string>(type: "TEXT", nullable: false),
                    isky_email = table.Column<string>(type: "TEXT", nullable: false),
                    isky_password = table.Column<string>(type: "TEXT", nullable: false),
                    isky_token = table.Column<string>(type: "TEXT", nullable: false),
                    siqi_track = table.Column<string>(type: "TEXT", nullable: false),
                    siqi_host = table.Column<string>(type: "TEXT", nullable: false),
                    siqi_cookie = table.Column<string>(type: "TEXT", nullable: false),
                    siqi_passkey = table.Column<string>(type: "TEXT", nullable: false),
                    siqi_uploader_token = table.Column<string>(type: "TEXT", nullable: false),
                    qb_host = table.Column<string>(type: "TEXT", nullable: false),
                    qb_user_name = table.Column<string>(type: "TEXT", nullable: false),
                    qb_password = table.Column<string>(type: "TEXT", nullable: false),
                    qb_tags = table.Column<string>(type: "TEXT", nullable: false),
                    invisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    create_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feeds",
                columns: table => new
                {
                    id = table.Column<uint>(type: "INTEGER", nullable: false, comment: "Id")
                        .Annotation("Sqlite:Autoincrement", true),
                    siqi_id = table.Column<uint>(type: "INTEGER", nullable: false, comment: "思齐Id"),
                    title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "书名"),
                    isbn = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false, comment: "ISBN"),
                    authors = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "作者"),
                    press = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "出版社"),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "出版日期"),
                    summary = table.Column<string>(type: "TEXT", nullable: false, comment: "内容简介"),
                    douban_id = table.Column<int>(type: "INTEGER", nullable: false, comment: "豆瓣Id"),
                    type = table.Column<int>(type: "INTEGER", nullable: false, comment: "类型：连环画、插图画、漫画等等"),
                    sort = table.Column<int>(type: "INTEGER", nullable: false, comment: "类别：历史传记、人文艺术、科普科技等等"),
                    source = table.Column<int>(type: "INTEGER", nullable: false, comment: "来源：自制、个人收集、自购、转载等等"),
                    condition = table.Column<int>(type: "INTEGER", nullable: false, comment: "状态：单本完、部分、合集完、精选集、丛书等等"),
                    color = table.Column<int>(type: "INTEGER", nullable: false, comment: "颜色：黑白、全彩、混合等等"),
                    extension = table.Column<int>(type: "INTEGER", nullable: false, comment: "格式：pdf、mobi等等"),
                    language = table.Column<int>(type: "INTEGER", nullable: false, comment: "语言：中简、中繁、英语等等"),
                    quality = table.Column<int>(type: "INTEGER", nullable: false, comment: "品质：一般质量、高质量、超高质量等"),
                    team = table.Column<int>(type: "INTEGER", nullable: false, comment: "制作组：SQB，其他"),
                    preview_url = table.Column<string>(type: "TEXT", nullable: false, comment: "预览图url，用分号分割"),
                    path = table.Column<string>(type: "TEXT", nullable: false, comment: "ID对应的文件目录"),
                    task = table.Column<int>(type: "INTEGER", nullable: false, comment: "ID对应的当前任务"),
                    task_state = table.Column<bool>(type: "INTEGER", nullable: false, comment: "ID对应的当前任务完成情况"),
                    create_at = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "创建时间"),
                    last_updated_at = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "最后更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feeds", x => x.id);
                },
                comment: "思齐PT站的发种记录");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configurations");

            migrationBuilder.DropTable(
                name: "feeds");
        }
    }
}
