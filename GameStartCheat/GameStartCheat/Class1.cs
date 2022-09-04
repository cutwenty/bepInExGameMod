using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TianDiGuiXuCheat
{
    // [BepInProcess("Maid In Makai.exe")] 如果游戏打开有两个exe比如恋活的工作室需要指定

    // 如果有依赖其他插件
    // 软依赖，如果没有前置插件，依旧继续加载
    //[BepInDependency("com.bepinex.plugin.somedependency", BepInDependency.DependencyFlags.SoftDependency)]
    // 硬依赖，如果没有前置插件，则停止加载
    //[BepInDependency("com.bepinex.plugin.importantdependency", BepInDependency.DependencyFlags.HardDependency)]
    // 省略参数，则默认为硬依赖
    //[BepInDependency("com.bepinex.plugin.anotherimportantone")]

    [BepInPlugin("GameStartCheat", "GameStartCheat", "0.22")]
    public class GameStartCheat : BaseUnityPlugin
    {
        public static Action<string> logInfo;
        public static string[] initItems = new string[0];
        public static string[] initXinFa = new string[0];
        public static string[] initShuFa = new string[0];
        public static Dictionary<string, string[]> npcTemplateEntriesDict = new Dictionary<string, string[]>();
        public static Dictionary<string, string[]> randomNpcEntries = new Dictionary<string, string[]>();
        public static int createTalentPoints = 45;
        public static int initShengWang = 0;
        public static int initGongDe = 0;
        public static int initQiYun = 0;
        public static int extraMoveRange = 0;
        public static float npcExtraPoint = 0;
        public static float playerExtraPoint = 0;
        public static float playerCDReduce = 0;
        public static float playerReadingReduce = 0;
        public static float playerBloodSuck = 0;
        public static float extraEvdRate = 0;
        
        public static Boolean enableRandomNpcEntries = false;
        public static Boolean enablePlayerLiantiLinggen = false;

        void Start()
        {
            GameStartCheat.logInfo = new Action<string>(Logger.LogInfo);
            GameStartCheat.logInfo("plugin start");
            Harmony.CreateAndPatchAll(typeof(GameStartCheat));
            InitConfig();
        }

        void InitConfig()
        {
            GameStartCheat.createTalentPoints = Config.Bind<int>("config", "createTalentPoints", 45, "角色创建时的天赋点，0关闭").Value;
            //GameStartCheat.enablePlayerLiantiLinggen = Config.Bind<Boolean>("config", "enablePlayerLiantiLinggen", false, "开启后，玩家角色如果选的是炼体，会被换成混元灵根").Value;
            GameStartCheat.playerExtraPoint = Config.Bind<float>("config", "playerExtraPoint", 0, "玩家角色每个属性加上这个值").Value;
            GameStartCheat.playerCDReduce = Config.Bind<float>("config", "playerCDReduce", 0, "玩家角色冷却速度，小数，例如1.5，表示冷却-50%").Value;
            GameStartCheat.playerReadingReduce = Config.Bind<float>("config", "playerReadingReduce", 0, "玩家角色吟唱冷却速度，小数，例如1.5，表示冷却-50%").Value;
            GameStartCheat.playerBloodSuck = Config.Bind<float>("config", "playerBloodSuck", 0, "玩家角色吸血，小数，例如0.1").Value;
            GameStartCheat.extraEvdRate = Config.Bind<float>("config", "extraEvdRate", 0, "玩家角色额外闪避，小数，例如0.1").Value;
            GameStartCheat.extraMoveRange = Config.Bind<int>("config", "extraMoveRange", 3, "额外大地图行动点，会和默认行动点相加，总和最大8").Value;

            GameStartCheat.initQiYun = Config.Bind<int>("config", "initQiYun", 0, "初始气运，0关闭，点出四次元仓库要400").Value;
            GameStartCheat.initShengWang = Config.Bind<int>("config", "initShengWang", 0, "初始声望，0关闭").Value;
            GameStartCheat.initGongDe = Config.Bind<int>("config", "initGongDe", 0, "初始功德，0关闭").Value;

            string initItemStr = Config.Bind<string>("config", "initItems", "", "游戏创建时添加的普通物品（不能加心法、法术，只试过丹药和八卦炉，丹药要加“·上品”这种后缀），例如：碎形丹·上品|30&疾行丹·上品|30&八卦炉，格式：物品|数量&物品|数量，不加数量默认1").Value;
            if (initItemStr.Length > 0)
            {
                GameStartCheat.initItems = initItemStr.Split(new char[] { '&' });
            }
            string initXinFaStr = Config.Bind<string>("config", "initXinFa", "", "游戏创建时添加的心法，会添加到藏书中，例如：八九玄功,逍遥游，格式：物品,物品").Value;
            if (initXinFaStr.Length > 0)
            {
                GameStartCheat.initXinFa = initXinFaStr.Split(new char[] { ',' });
            }
            string initShuFaStr = Config.Bind<string>("config", "initShuFa", "", "游戏创建时添加的术法，会添加到藏书中，例如：三昧真火咒法,九幽焱诀，格式：物品,物品").Value;
            if (initShuFaStr.Length > 0)
            {
                GameStartCheat.initShuFa = initShuFaStr.Split(new char[] { ',' });
            }

            string npcTemplateEntries = Config.Bind<string>("config", "npcTemplateEntries", "", "npc模板的天赋，例如：清风|丹道大能,火焰掌控者,丹心凝练,九转丹心&明月|丹道大能,火焰掌控者,丹心凝练,九转丹心&柳一心|飞天神剑,法眼,碎金裂石&金柴|上品道骨,金行者,法眼，格式：人物|特性,特性,特性&人物|特性,特性,特性").Value;
            if (npcTemplateEntries.Length > 0)
            {
                string[] npcItems = npcTemplateEntries.Split(new char[] { '&' });
                foreach (string npcItem in npcItems)
                {
                    string[] details = npcItem.Split('|');
                    if (details.Length <= 1)
                    {
                        continue;
                    }
                    GameStartCheat.npcTemplateEntriesDict.Add(details[0], details[1].Split(','));
                }
            }

            GameStartCheat.enableRandomNpcEntries = Config.Bind<Boolean>("config", "enableRandomNpcEntries", false, "是否开启刷新弟子修改天赋，开启后默认气运之子，开局做好后可以关闭").Value;
            string randomNpcEntriesStr = Config.Bind<string>("config", "randomNpcEntries", "", "随机npc的天赋根据灵根（不包含阴阳混元灵根，现在也刷不出来）、炼体分类，例如：炼体|铁骨,法眼,碎金裂石&金|上品道骨,金行者,法眼&木|上品道骨,木行者,法眼&水|上品道骨,水行者,法眼&火|上品道骨,火行者,法眼&土|上品道骨,土行者,法眼，格式：类型|特性,特性,特性&类型|特性,特性,特性").Value;
            if (randomNpcEntriesStr.Length > 0)
            {
                string[] npcItems = randomNpcEntriesStr.Split(new char[] { '&' });
                foreach (string npcItem in npcItems)
                {
                    string[] details = npcItem.Split('|');
                    if (details.Length <= 1)
                    {
                        continue;
                    }
                    GameStartCheat.randomNpcEntries.Add(details[0], details[1].Split(','));
                }
            }
            GameStartCheat.npcExtraPoint = Config.Bind<float>("config", "npcExtraPoint", 0, "npc每个属性加上这个值，只有上面修改天赋的npc模板和刷新弟子修改天赋才加").Value;
        }

        // 改初始道具
        [HarmonyPatch(typeof(WholeObjects), "GetDropTermItems")]
        [HarmonyPostfix]
        private static List<ItemStored> GetDropTermItems(List<ItemStored> list, WholeObjects __instance, string dropTermID)
        {
            if (dropTermID != "玩家门派初始物品")
            {
                return list;
            }
            GameStartCheat.logInfo("初始物品");
            foreach (string item in GameStartCheat.initItems)
            {
                string[] details = item.Split('|');
                string name = details[0];
                int num = 1;
                if (details.Length > 1)
                {
                    num = Int32.Parse(details[1]);
                }
                if (name.Length <= 0)
                {
                    continue;
                }
                list.Add((ItemStored)typeof(WholeObjects).GetMethod("CreateNormalItem", (BindingFlags)(-1)).Invoke(__instance, new object[]
                {
                        name,
                        "",
                        num,
                        false
                }));
            }
            // 心法书
            foreach (string item in GameStartCheat.initXinFa)
            {
                list.Add(__instance.CreateAXinfaBook(item, "", -1, false));
            }
            // 术法书
            foreach (string item in GameStartCheat.initShuFa)
            {
                list.Add(__instance.CreateAShuFaBooK(item, "", false));
            }

            return list;
        }

        // 改创建角色的点数
        [HarmonyPatch(typeof(CreateChara_RandomEntryPanel), "init")]
        [HarmonyPostfix]
        private static void initRandomEntryPanel(ref int ___CurrentPoint, ref int ___MaxPoint)
        {
            if (GameStartCheat.createTalentPoints > 0)
            {
                GameStartCheat.logInfo("CreateChara_RandomEntryPanel init" + ___CurrentPoint + " " + ___MaxPoint);
                ___CurrentPoint = GameStartCheat.createTalentPoints - (___MaxPoint - ___CurrentPoint);
                ___MaxPoint = GameStartCheat.createTalentPoints;

            }
        }


        // 根据id取模板角色创建前
        [HarmonyPatch(typeof(Chara), "initialize", new Type[] { typeof(string) })]
        [HarmonyPrefix]
        private static void initialize(string ID)
        {
            ANPCCharaTemplate template = AllDictionary.Dic_NPC[ID];
            //GameStartCheat.logInfo("创建角色 initialize string " + ID + template.Name);
            string[] newEntries;
            if (GameStartCheat.npcTemplateEntriesDict.TryGetValue(template.Name, out newEntries))
            {
                GameStartCheat.logInfo(string.Join(", ", new string[] {
                    "修改角色天赋",
                    template.Name,
                    string.Join("|", newEntries)
                }));
                template.Entries = newEntries;

                template.Gengu += GameStartCheat.npcExtraPoint;
                template.TiPo += GameStartCheat.npcExtraPoint;
                template.QiXue += GameStartCheat.npcExtraPoint;
                template.LingMin += GameStartCheat.npcExtraPoint;
                template.WuXing += GameStartCheat.npcExtraPoint;
                template.FuYuan += GameStartCheat.npcExtraPoint;
                template.MeiLi += GameStartCheat.npcExtraPoint;
            }
        }

        // 随机npc改为气运之子
        [HarmonyPatch(typeof(Chara), "initialize", new Type[] { typeof(int), typeof(Inborn) })]
        [HarmonyPrefix]
        private static void initializePrefix(int level, ref Inborn rare, Chara __instance)
        {
            GameStartCheat.logInfo(string.Join(", ", new string[] {
                    "创建角色",
                    level + "",
                    rare + ""
                }));
            if (GameStartCheat.enableRandomNpcEntries)
            {
                rare = Inborn.气运;
                __instance.IsVirtual = true;
            }
        }
        // 返回判断用的灵根string
        private static string getLinggenStr(Chara chara)
        {

            string linggenStr = "";
            switch (chara.linggen)
            {
                case LingGenType.金灵根:
                    linggenStr = "金";
                    break;
                case LingGenType.木灵根:
                    linggenStr = "木";
                    break;
                case LingGenType.水灵根:
                    linggenStr = "水";
                    break;
                case LingGenType.火灵根:
                    linggenStr = "火";
                    break;
                case LingGenType.土灵根:
                    linggenStr = "土";
                    break;
                default:
                    break;
            }
            if (chara.xiulianType == XiuLianType.炼体)
            {
                linggenStr = "炼体";
            }
            return linggenStr;
        }
        // 修改随机角色的特性、属性、灵根
        [HarmonyPatch(typeof(Chara), "initialize", new Type[] { typeof(int), typeof(Inborn) })]
        [HarmonyPostfix]
        private static void initializePost(int level, Inborn rare, Chara __instance)
        {
            if (!GameStartCheat.enableRandomNpcEntries)
            {
                return;
            }
            string linggenStr = GameStartCheat.getLinggenStr(__instance);
            if (linggenStr == "")
            {
                return;
            }
            string[] entries = new string[] { };
            if (GameStartCheat.randomNpcEntries.TryGetValue(linggenStr, out entries))
            {
                //if (linggenStr == "炼体")
                //{
                //    __instance.linggen = LingGenType.混元灵根;
                //    __instance.InitListAndDicInfo();
                //}

                __instance.gengu += GameStartCheat.npcExtraPoint;
                __instance.tipo += GameStartCheat.npcExtraPoint;
                __instance.qixue += GameStartCheat.npcExtraPoint;
                __instance.lingmin += GameStartCheat.npcExtraPoint;
                __instance.wuxing += GameStartCheat.npcExtraPoint;
                __instance.fuyuan += GameStartCheat.npcExtraPoint;
                __instance.meili += GameStartCheat.npcExtraPoint;
            }
        }
        // 如果是随机角色，固定特性
        [HarmonyPatch(typeof(Chara), "AddBaseEntry")]
        [HarmonyPrefix]
        private static bool AddBaseEntry(Chara __instance)
        {
            if (GameStartCheat.enableRandomNpcEntries && __instance.IsVirtual)
            {
                string linggenStr = GameStartCheat.getLinggenStr(__instance);
                if (linggenStr == "")
                {
                    return true;
                }
                string[] entries = new string[] { };
                if (GameStartCheat.randomNpcEntries.TryGetValue(linggenStr, out entries))
                {
                    foreach (string item in entries)
                    {
                        EntryBuilder.AddACharaEntry(__instance, item);
                    }
                } else
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        // 主角门派信息修改、主角信息修改
        [HarmonyPatch(typeof(GameSaveData), "NewGame")]
        [HarmonyPostfix]
        private static void NewGame()
        {
            GameStartCheat.logInfo("创建游戏后修改门派、玩家信息");
            if (GameStartCheat.initShengWang > 0)
            {
                WholeObjects.Instance.GetPlayerSect().shengWang = GameStartCheat.initShengWang;
            }
            if (GameStartCheat.initGongDe > 0)
            {
                WholeObjects.Instance.GetPlayerSect().gongDe = GameStartCheat.initGongDe;
            }
            if (GameStartCheat.initQiYun > 0)
            {
                WholeObjects.Instance.GetPlayerSect().qiYun = GameStartCheat.initQiYun;
            }
            Chara player = WholeObjects.Instance.GetPlayer();
            // 修改玩家属性
            player.extraMoveRange = GameStartCheat.extraMoveRange;
            player.ExtraEvdRate = GameStartCheat.extraEvdRate;
            player.gengu += GameStartCheat.playerExtraPoint;
            player.tipo += GameStartCheat.playerExtraPoint;
            player.qixue += GameStartCheat.playerExtraPoint;
            player.lingmin += GameStartCheat.playerExtraPoint;
            player.wuxing += GameStartCheat.playerExtraPoint;
            player.fuyuan += GameStartCheat.playerExtraPoint;
            player.meili += GameStartCheat.playerExtraPoint;
            //if (GameStartCheat.enablePlayerLiantiLinggen && player.xiulianType == XiuLianType.炼体)
            //{
            //    player.linggen = LingGenType.混元灵根;
            //    player.InitListAndDicInfo();
            //}
            if (GameStartCheat.playerCDReduce > 0)
            {
                player.CDreduce = GameStartCheat.playerCDReduce;
            }
            if (GameStartCheat.playerReadingReduce > 0)
            {
                player.Readingreduce = GameStartCheat.playerReadingReduce;
            }
            if (GameStartCheat.playerBloodSuck > 0)
            {
                player.ExtraBloodSuck = GameStartCheat.playerBloodSuck;
            }
            // 可以改抗性
            //foreach (var item in player.DamageResistIncrease)
            //foreach (var item in player.DamageResistReduce)
        }

    }

}