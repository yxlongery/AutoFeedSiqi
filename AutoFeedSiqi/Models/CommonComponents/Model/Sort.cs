using System.ComponentModel.DataAnnotations;

namespace AutoFeedSiqi.Models.CommonComponents.Model
{
    /// <summary>
    /// 类别
    /// </summary>
    public enum Sort
    {
        [Display(Name = "治愈", Description = "治愈")]
        Healing = 20,

        [Display(Name = "幽默搞笑", Description = "幽默搞笑")]
        HumorousAndFunny = 11,

        [Display(Name = "哲学智慧", Description = "哲学智慧")]
        PhilosophicalWisdom = 19,

        [Display(Name = "热血", Description = "热血")]
        Passion = 18,

        [Display(Name = "历史传记", Description = "历史传记")]
        BiographyAndHistory = 17,

        [Display(Name = "神话传说", Description = "神话传说")]
        MythologyAndLegends = 16,

        [Display(Name = "美食旅游", Description = "美食旅游")]
        FoodAndTourism = 15,

        [Display(Name = "探险冒险", Description = "探险冒险")]
        ExplorationAndAdventure = 14,

        [Display(Name = "奇幻科幻", Description = "奇幻科幻")]
        FantasyAndScienceFiction = 13,

        [Display(Name = "游戏桌游", Description = "游戏桌游")]
        GamesAndBoardGames = 12,

        [Display(Name = "学习辅导", Description = "学习辅导")]
        StudyAndGuidance = 1,

        [Display(Name = "其他", Description = "其他")]
        Other = 10,

        [Display(Name = "超级英雄", Description = "超级英雄")]
        Superhero = 9,

        [Display(Name = "语言学习", Description = "语言学习")]
        LanguageLearning = 8,

        [Display(Name = "浪漫爱情", Description = "浪漫爱情")]
        RomanticLove = 6,

        [Display(Name = "悬疑推理", Description = "悬疑推理")]
        MysteryAndDetectiveStory = 5,

        [Display(Name = "人文艺术", Description = "人文艺术")]
        HumanitiesAndArts = 4,

        [Display(Name = "科技科普", Description = "科技科普")]
        ScienceAndTechnologyPopularization = 3
    }
}
