using Microsoft.AspNetCore.Mvc;
using ZeroNet.Http.Route;

namespace WebMonitor.Controlers
{
    public class 二维码类型
    {

        const string 动态二维码 = "0";
        const string 静态二维码 = "1";
    }

    public class 性别
    {

        const string 未知的性别 = "0";
        const string 男 = "1";
        const string 女 = "2";
        const string 未说明的性别 = "9";
        
    }

    public class 身份证件类型
    {

        const string 居民身份证 = "1";
        const string 居民户口簿 = "2";
        const string 护照 = "3";
        const string 军官证 = "4";
        const string 驾驶证 = "5";
        const string 港澳居民来往内地通行证 = "6";
        const string 台湾居民来往内地通行证 = "7";
        const string 出生医学证明 = "8";
        const string 其他法定有效证件 = "99";
    }

    public class 民族
    {

        const string 汉族 = "1";
        const string 蒙古族 = "2";
        const string 回族 = "3";
        const string 藏族 = "4";
        const string 维吾尔族 = "5";
        const string 苗族 = "6";
        const string 彝族 = "7";
        const string 壮族 = "8";
        const string 布依族 = "9";
        const string 朝鲜族 = "10";
        const string 满族 = "11";
        const string 侗族 = "12";
        const string 瑶族 = "13";
        const string 白族 = "14";
        const string 土家族 = "15";
        const string 哈尼族 = "16";
        const string 哈萨克族 = "17";
        const string 傣族 = "18";
        const string 黎族 = "19";
        const string 傈僳族 = "20";
        const string 佤族 = "21";
        const string 畲族 = "22";
        const string 高山族 = "23";
        const string 拉祜族 = "24";
        const string 水族 = "25";
        const string 东乡族 = "26";
        const string 纳西族 = "27";
        const string 景颇族 = "28";
        const string 柯尔克孜族 = "29";
        const string 土族 = "30";
        const string 达斡尔族 = "31";
        const string 仫佬族 = "32";
        const string 羌族 = "33";
        const string 布朗族 = "34";
        const string 撒拉族 = "35";
        const string 毛南族 = "36";
        const string 仡佬族 = "37";
        const string 锡伯族 = "38";
        const string 阿昌族 = "39";
        const string 普米族 = "40";
        const string 塔吉克族 = "41";
        const string 怒族 = "42";
        const string 乌孜别克族 = "43";
        const string 俄罗斯族 = "44";
        const string 鄂温克族 = "45";
        const string 德昴族 = "46";
        const string 保安族 = "47";
        const string 裕固族 = "48";
        const string 京族 = "49";
        const string 塔塔尔族 = "50";
        const string 独龙族 = "51";
        const string 鄂伦春族 = "52";
        const string 赫哲族 = "53";
        const string 门巴族 = "54";
        const string 珞巴族 = "55";
        const string 基诺族 = "56";

    }

    public class 国籍
    {

        const string 阿富汗 = "4";
        const string 阿尔巴尼亚 = "8";
        const string 南极洲 = "10";
        const string 阿尔及利亚 = "12";
        const string 美属萨摩亚 = "16";
        const string 安道尔 = "20";
        const string 安哥拉 = "24";
        const string 安提瓜与巴布达 = "28";
        const string 阿塞拜疆 = "31";
        const string 阿根廷 = "32";
        const string 澳大利亚 = "36";
        const string 奥地利 = "40";
        const string 巴哈马 = "44";
        const string 巴林 = "48";
        const string 孟加拉国 = "50";
        const string 亚美尼亚 = "51";
        const string 巴巴多斯 = "52";
        const string 比利时 = "56";
        const string 百慕大 = "60";
        const string 不丹 = "64";
        const string 玻利维亚 = "68";
        const string 波黑 = "70";
        const string 博茨瓦纳 = "72";
        const string 布维岛 = "74";
        const string 巴西 = "76";
        const string 伯利兹 = "84";
        const string 英属印度洋领地 = "86";
        const string 所罗门群岛 = "90";
        const string 英属维尔京群岛 = "92";
        const string 文莱 = "96";
        const string 保加利亚 = "100";
        const string 缅甸 = "104";
        const string 布隆迪 = "108";
        const string 白俄罗斯 = "112";
        const string 柬埔寨 = "116";
        const string 喀麦隆 = "120";
        const string 加拿大 = "124";
        const string 佛得角 = "132";
        const string 开曼群岛 = "136";
        const string 中非 = "140";
        const string 斯里兰卡 = "144";
        const string 乍得 = "148";
        const string 智利 = "152";
        const string 中国 = "156";
        const string 台湾 = "158";
        const string 圣诞岛 = "162";
        const string 科科斯_基林_群岛 = "166";
        const string 哥伦比亚 = "170";
        const string 科摩罗 = "174";
        const string 马约特 = "175";
        const string 刚果_布_= "178";
        const string 刚果_金_= "180";
        const string 库克群岛 = "184";
        const string 哥斯达黎加 = "188";
        const string 克罗地亚 = "191";
        const string 古巴 = "192";
        const string 塞浦路斯 = "196";
        const string 捷克 = "203";
        const string 贝宁 = "204";
        const string 丹麦 = "208";
        const string 多米尼克 = "212";
        const string 多米尼加 = "214";
        const string 厄瓜多尔 = "218";
        const string 萨尔瓦多 = "222";
        const string 赤道几内亚 = "226";
        const string 埃塞俄比亚 = "231";
        const string 厄立特里亚 = "232";
        const string 爱沙尼亚 = "233";
        const string 法罗群岛 = "234";
        const string 福克兰群岛_马尔维纳斯_= "238";
        const string 南乔治亚岛和南桑德韦奇岛 = "239";
        const string 斐济 = "242";
        const string 芬兰 = "246";
        const string 法国 = "250";
        const string 法属圭亚那 = "254";
        const string 法属波利尼西亚 = "258";
        const string 法属南部领地 = "260";
        const string 吉布提 = "262";
        const string 加蓬 = "266";
        const string 格鲁吉亚 = "268";
        const string 冈比亚 = "270";
        const string 巴勒斯坦 = "275";
        const string 德国 = "276";
        const string 加纳 = "288";
        const string 直布罗陀 = "292";
        const string 基里巴斯 = "296";
        const string 希腊 = "300";
        const string 格陵兰 = "304";
        const string 格林纳达 = "308";
        const string 瓜德罗普 = "312";
        const string 关岛 = "316";
        const string 危地马拉 = "320";
        const string 几内亚 = "324";
        const string 圭亚那 = "328";
        const string 海地 = "332";
        const string 赫德岛和麦克唐纳岛 = "334";
        const string 梵蒂冈 = "336";
        const string 洪都拉斯 = "340";
        const string 香港 = "344";
        const string 匈牙利 = "348";
        const string 冰岛 = "352";
        const string 印度 = "356";
        const string 印度尼西亚 = "360";
        const string 伊朗 = "364";
        const string 伊拉克 = "368";
        const string 爱尔兰 = "372";
        const string 以色列 = "376";
        const string 意大利 = "380";
        const string 科特迪瓦 = "384";
        const string 牙买加 = "388";
        const string 日本 = "392";
        const string 哈萨克斯坦 = "398";
        const string 约旦 = "400";
        const string 肯尼亚 = "404";
        const string 朝鲜 = "408";
        const string 韩国 = "410";
        const string 科威特 = "414";
        const string 吉尔吉斯斯坦 = "417";
        const string 老挝 = "418";
        const string 黎巴嫩 = "422";
        const string 莱索托 = "426";
        const string 拉脱维亚 = "428";
        const string 利比里亚 = "430";
        const string 利比亚 = "434";
        const string 列支敦士登 = "438";
        const string 立陶宛 = "440";
        const string 卢森堡 = "442";
        const string 澳门 = "446";
        const string 马达加斯加 = "450";
        const string 马拉维 = "454";
        const string 马来西亚 = "458";
        const string 马尔代夫 = "462";
        const string 马里 = "466";
        const string 马耳他 = "470";
        const string 马提尼克 = "474";
        const string 毛里塔尼亚 = "478";
        const string 毛里求斯 = "480";
        const string 墨西哥 = "484";
        const string 摩纳哥 = "492";
        const string 蒙古 = "496";
        const string 摩尔多瓦 = "498";
        const string 蒙特塞拉特 = "500";
        const string 摩洛哥 = "504";
        const string 莫桑比克 = "508";
        const string 阿曼 = "512";
        const string 纳米比亚 = "516";
        const string 瑙鲁 = "520";
        const string 尼泊尔 = "524";
        const string 荷兰 = "528";
        const string 荷属安的列斯 = "530";
        const string 阿鲁巴 = "533";
        const string 新喀里多尼亚 = "540";
        const string 瓦努阿图 = "548";
        const string 新西兰 = "554";
        const string 尼加拉瓜 = "558";
        const string 尼日尔 = "562";
        const string 尼日利亚 = "566";
        const string 纽埃 = "570";
        const string 诺福克岛 = "574";
        const string 挪威 = "578";
        const string 北马里亚纳 = "580";
        const string 美国本土外小岛屿 = "581";
        const string 密克罗尼西亚联邦 = "583";
        const string 马绍尔群岛 = "584";
        const string 帕劳 = "585";
        const string 巴基斯坦 = "586";
        const string 巴拿马 = "591";
        const string 巴布亚新几内亚 = "598";
        const string 巴拉圭 = "600";
        const string 秘鲁 = "604";
        const string 菲律宾 = "608";
        const string 皮特凯恩 = "612";
        const string 波兰 = "616";
        const string 葡萄牙 = "620";
        const string 几内亚比绍 = "624";
        const string 东帝汉 = "626";
        const string 波多黎各 = "630";
        const string 卡塔尔 = "634";
        const string 留尼汪 = "638";
        const string 罗马尼亚 = "642";
        const string 俄罗斯联邦 = "643";
        const string 卢旺达 = "646";
        const string 圣赫勒拿 = "654";
        const string 圣基茨和尼维斯 = "659";
        const string 安圭拉 = "660";
        const string 圣卢西亚 = "662";
        const string 圣皮埃尔和密克隆 = "666";
        const string 圣文森特和格林纳丁斯 = "670";
        const string 圣马力诺 = "674";
        const string 圣多美和普林西比 = "678";
        const string 沙特阿拉伯 = "682";
        const string 塞内加尔 = "686";
        const string 塞舌尔 = "690";
        const string 塞拉利昂 = "694";
        const string 新加坡 = "702";
        const string 斯洛伐克 = "703";
        const string 越南 = "704";
        const string 斯洛文尼亚 = "705";
        const string 索马里 = "706";
        const string 南非 = "710";
        const string 津巴布韦 = "716";
        const string 西班牙 = "724";
        const string 西撒哈拉 = "732";
        const string 苏丹 = "736";
        const string 苏里南 = "740";
        const string 斯瓦尔巴岛和扬马延岛 = "744";
        const string 斯威士兰 = "748";
        const string 瑞典 = "752";
        const string 瑞士 = "756";
        const string 叙利亚 = "760";
        const string 塔吉克斯坦 = "762";
        const string 泰国 = "764";
        const string 多哥 = "768";
        const string 托克劳 = "772";
        const string 汤加 = "776";
        const string 特立尼达和多巴哥 = "780";
        const string 阿联酋 = "784";
        const string 突尼斯 = "788";
        const string 土耳其 = "792";
        const string 土库曼斯坦 = "795";
        const string 特克斯和凯科斯群岛 = "796";
        const string 图瓦卢 = "798";
        const string 乌干达 = "800";
        const string 乌克兰 = "804";
        const string 前南马其顿 = "807";
        const string 埃及 = "818";
        const string 英国 = "826";
        const string 坦桑尼亚 = "834";
        const string 美国 = "840";
        const string 美属维尔京群岛 = "850";
        const string 布基纳法索 = "854";
        const string 乌拉圭 = "858";
        const string 乌兹别克斯坦 = "860";
        const string 委内瑞拉 = "862";
        const string 瓦利斯和富图纳 = "876";
        const string 萨摩亚 = "882";
        const string 也门 = "887";
        const string 南斯拉夫 = "891";
        const string 赞比亚 = "894";
        
    }

    public class APP申请方式{

    const string APP在线申请 = "1";
    const string 医疗卫生机构自助机申请 = "2";
    const string 医疗卫生机构窗口申请 = "3";
    const string 批量预生成 = "4";

    }

    public class 支付_收款账户类型{

    const string 无 = "0";
    const string 微信 = "1";
    const string 支付宝 = "2";
    const string 银联 = "3";
    const string 银行 = "4";
    const string 统一支付平台 = "5";
    const string 其他 = "9";
    }public class 付款方式
    {

        const string 无支付 = "0";
        const string 银行卡支付 = "1";
        const string 院内预缴金支付 = "2";
        const string 现金 = "3";
        const string 其他 = "4";

    }

    public class 刷卡终端类型
    {

        const string 人工窗口 = "1";
        const string 自助终端 = "2";
        const string 其他 = "99";
    }

    public class 就诊类型
    {

        const string 住院 = "I";
        const string 门诊 = "O";
        const string 急诊 = "E";
        const string 体检 = "H";
        const string 其它 = "T";
    }

    public class 科室类型
    {

        const string 挂号科室 = "1";
        const string 收费科室 = "2";
        const string 门诊科室 = "3";
        const string 住院科室 = "4";
        const string 检查科室 = "5";
        const string 检验科室 = "6";
        const string 处置治疗科室 = "7";
        const string 药房 = "8";
        const string 手术麻醉 = "9";
        const string 其它科室 = "10";
    }

    public class 诊疗环节代码
    {
        const string 诊疗环节名称 = "诊疗环节代码";
        const string 挂号 = "10101";
        const string 诊断 = "10102";
        const string 取药 = "10103";
        const string 检查 = "10104";
        const string 收费 = "10105";
        const string 开方 = "10106";
        const string 手术 = "10107";
        const string 其他 = "0";

    }

    public class 标准科室代码
    {
        const string 科室名称 = "科室代码";
        const string 预防保健科 = "100";
        const string 全科医疗科 = "200";
        const string 内科 = "300";
        const string 呼吸内科专业 = "301";
        const string 消化内科专业 = "302";
        const string 神经内科专业 = "303";
        const string 心血管内科专业 = "304";
        const string 血液内科专业 = "305";
        const string 肾病学专业 = "306";
        const string 内分泌专业 = "307";
        const string 免疫学专业 = "308";
        const string 变态反应专业 = "309";
        const string 老年病专业 = "310";
        const string 其他内科 = "311";
        const string 外科 = "400";
        const string 普通外科专业 = "401";
        const string 神经外科专业 = "402";
        const string 骨科专业 = "403";
        const string 泌尿外科专业 = "404";
        const string 胸外科专业 = "405";
        const string 心脏大血管外科专业 = "406";
        const string 烧伤科专业 = "407";
        const string 整形外科专业 = "408";
        const string 其他外科 = "409";
        const string 妇产科 = "500";
        const string 妇科专业 = "501";
        const string 产科专业 = "502";
        const string 计划生育专业 = "503";
        const string 优生学专业 = "504";
        const string 生殖健康与不孕症专业 = "505";
        const string 其他妇产科 = "506";
        const string 妇女保健科 = "600";
        const string 青春期保健专业 = "601";
        const string 围产期保健专业 = "602";
        const string 更年期保健专业 = "603";
        const string 妇女心理卫生专业 = "604";
        const string 妇女营养专业 = "605";
        const string 其他妇女保健科 = "606";
        const string 儿科 = "700";
        const string 新生儿专业 = "701";
        const string 小儿传染病专业 = "702";
        const string 小儿消化专业 = "703";
        const string 小儿呼吸专业 = "704";
        const string 小儿心脏病专业 = "705";
        const string 小儿肾病专业 = "706";
        const string 小儿血液病专业 = "707";
        const string 小儿神经病学专业 = "708";
        const string 小儿内分泌专业 = "709";
        const string 小儿遗传病专业 = "710";
        const string 小儿免疫专业 = "711";
        const string 其他儿科 = "712";
        const string 小儿外科 = "800";
        const string 小儿普通外科专业 = "801";
        const string 小儿骨科专业 = "802";
        const string 小儿泌尿外科专业 = "803";
        const string 小儿胸心外科专业 = "804";
        const string 小儿神经外科专业 = "805";
        const string 其他小儿外科 = "806";
        const string 儿童保健科 = "900";
        const string 儿童生长发育专业 = "901";
        const string 儿童营养专业 = "902";
        const string 儿童心理卫生专业 = "903";
        const string 儿童五官保健专业 = "904";
        const string 儿童康复专业 = "905";
        const string 其他儿童保健科 = "906";
        const string 眼科 = "1000";
        const string 耳鼻咽喉科 = "1100";
        const string 耳科专业 = "1101";
        const string 鼻科专业 = "1102";
        const string 咽喉科专业 = "1103";
        const string 其他耳鼻咽喉科 = "1104";
        const string 口腔科 = "1200";
        const string 口腔科专业 = "1201";
        const string 口腔颌面外科专业 = "1202";
        const string 正畸专业 = "1203";
        const string 口腔修复专业 = "1204";
        const string 口腔预防保健专业 = "1205";
        const string 其他口腔科 = "1206";
        const string 皮肤科 = "1300";
        const string 皮肤专业 = "1301";
        const string 性传播疾病专业 = "1302";
        const string 其他皮肤科 = "1303";
        const string 医疗美容科 = "1400";
        const string 精神科 = "1500";
        const string 精神病专业 = "1501";
        const string 精神卫生专业 = "1502";
        const string 药物依赖专业 = "1503";
        const string 精神康复专业 = "1504";
        const string 社区防治专业 = "1505";
        const string 临床心理专业 = "1506";
        const string 司法精神专业 = "1507";
        const string 其他精神科 = "1508";
        const string 传染科 = "1600";
        const string 肠道传染病专业 = "1601";
        const string 呼吸道传染病专业 = "1602";
        const string 肝炎专业 = "1603";
        const string 虫媒传染病专业 = "1604";
        const string 动物源性传染病专业 = "1605";
        const string 蠕虫病专业 = "1606";
        const string 其他传染专业 = "1607";
        const string 结核病专业 = "1700";
        const string 地方病专业 = "1800";
        const string 肿瘤科 = "1900";
        const string 急诊医学科 = "2000";
        const string 康复医学科 = "2100";
        const string 运动医学科 = "2200";
        const string 职业病科 = "2300";
        const string 职业中毒专业 = "2301";
        const string 尘肺专业 = "2302";
        const string 放射病专业 = "2303";
        const string 物理因素损伤专业 = "2304";
        const string 职业健康监护专业 = "2305";
        const string 其他职业病科 = "2306";
        const string 临终关怀科 = "2400";
        const string 特种医学与军事医学科 = "2500";
        const string 麻醉科 = "2600";
        const string 医学检验科 = "3000";
        const string 临床体液_血液专业 = "3001";
        const string 临床微生物学专业 = "3002";
        const string 临床生化检验专业 = "3003";
        const string 临床免疫_血清学专业 = "3004";
        const string 其他医学检验科 = "3005";
        const string 病理科 = "3100";
        const string 医学影像科 = "3200";
        const string Ｘ线诊断专业 = "3201";
        const string ＣＴ诊断专业 = "3202";
        const string 磁共振成像诊断专业 = "3203";
        const string 核医学专业 = "3204";
        const string 超声诊断专业 = "3205";
        const string 心电诊断专业 = "3206";
        const string 脑电及脑血流图诊断专业 = "3207";
        const string 神经肌肉电图专业 = "3208";
        const string 介入放射学专业 = "3209";
        const string 放射治疗专业 = "3210";
        const string 其他医学影像科 = "3211";
        const string 中医科 = "5000";
        const string 内科 = "5001";
        const string 外科 = "5002";
        const string 妇产科 = "5003";
        const string 儿科 = "5004";
        const string 皮肤科 = "5005";
        const string 眼科 = "5006";
        const string 耳鼻咽喉科 = "5007";
        const string 内科药房 = "5405";
        const string 肿瘤科 = "5009";
        const string 骨伤科 = "5010";
        const string 肛肠科 = "5011";
        const string 老年病科 = "5012";
        const string 针灸科 = "5013";
        const string 推拿科 = "5014";
        const string 康复医学 = "5015";
        const string 急诊科 = "5016";
        const string 预防保健科 = "5017";
        const string 其他中医科 = "5018";
        const string 民族医学科 = "5100";
        const string 维吾尔医学 = "5101";
        const string 藏医学 = "5102";
        const string 蒙医学 = "5103";
        const string 彝医学 = "5104";
        const string 傣医学 = "5105";
        const string 其他民族医学科 = "5106";
        const string 中西医结合科 = "5200";
        const string 药库 = "5300";
        const string 中成药库 = "5301";
        const string 草药库 = "5302";
        const string 西药库 = "5303";
        const string 药房 = "5400";
        const string 中草药房 = "5401";
        const string 疫苗药房 = "5402";
        const string 西成药房 = "5403";
        const string 挂号处 = "5500";
        const string 收费处 = "5600";
        const string 注射室 = "5700";
        const string 住院处 = "5800";
        const string 护士站 = "5900";
        const string 住院药房 = "5404";
        const string 普外科住院部 = "5901";
        const string 感染科住院部 = "5902";
        const string 重症监护室 = "5903";
        const string 小儿科住院部 = "5904";
        const string 产科住院部 = "5905";
        const string 院办 = "9901";
        const string 财务 = "9902";
        const string 其他科室 = "9900";
        const string 材料库 = "5304";
        const string 体检科 = "101";
        const string 门诊输液室 = "6000";
        const string 卫生局 = "9903";
        const string 医生报表 = "9904";
        const string 外科药房 = "5406";
        const string 办公室药房 = "5407";
        const string 二层护士站 = "5906";
        const string 三层护士站 = "5907";
        const string 四层护士站 = "5908";
        const string 手术室层护士站 = "5909";
        const string 注射室护士站 = "5910";
        const string 材料药房 = "5408";
        const string 办公耗材 = "5305";
        const string 计划免疫 = "102";


    }

    public class 医院级别代码
    {

        const string 一级 = "10";
        const string 一级甲等 = "12";
        const string 一级乙等 = "13";
        const string 一级丙等 = "14";
        const string 一级未评 = "19";
        const string 二级 = "20";
        const string 二级甲等 = "22";
        const string 二级乙等 = "23";
        const string 二级丙等 = "24";
        const string 二级未评 = "29";
        const string 三级 = "30";
        const string 三级甲等 = "32";
        const string 三级乙等 = "33";
        const string 三级丙等 = "34";
        const string 三级未评 = "39";
    }

    public class 医疗卫生机构类别代码
    {

        const string 医院 = "A";
        const string 综合医院 = "A100";
        const string 中医医院 = "A2";
        const string 中医_综合_医院 = "A210";
        const string 中医专科医院 = "A220";
        const string 肛肠医院 = "A221";
        const string 骨伤医院 = "A222";
        const string 针灸医院 = "A223";
        const string 按摩医院 = "A224";
        const string 其他中医专科医院 = "A229";
        const string 中西医结合医院 = "A300";
        const string 民族医院 = "A4";
        const string 蒙医院 = "A411";
        const string 藏医院 = "A412";
        const string 维医院 = "A413";
        const string 傣医院 = "A414";
        const string 其他民族医院 = "A419";
        const string 专科医院 = "A5";
        const string 口腔医院 = "A511";
        const string 眼科医院 = "A512";
        const string 耳鼻喉科医院 = "A513";
        const string 肿瘤医院 = "A514";
        const string 心血管病医院 = "A515";
        const string 胸科医院 = "A516";
        const string 血液病医院 = "A517";
        const string 妇产_科_医院 = "A518";
        const string 儿童医院 = "A519";
        const string 精神病医院 = "A520";
        const string 传染病医院 = "A521";
        const string 皮肤病医院 = "A522";
        const string 结核病医院 = "A523";
        const string 麻风病医院 = "A524";
        const string 职业病医院 = "A525";
        const string 骨科医院 = "A526";
        const string 康复医院 = "A527";
        const string 整形外科医院 = "A528";
        const string 美容医院 = "A529";
        const string 其他专科医院 = "A539";
        const string 疗养院 = "A600";
        const string 护理院_站_= "A7";
        const string 护理院 = "A710";
        const string 护理站 = "A720";
        const string 社区卫生服务中心（站） = "B";
        const string 社区卫生服务中心 = "B100";
        const string 社区卫生服务站 = "B200";
        const string 卫生院 = "C";
        const string 街道卫生院 = "C100";
        const string 乡镇卫生院 = "C2";
        const string 中心卫生院 = "C210";
        const string 乡卫生院 = "C220";
        const string 门诊部_诊所_医务室_村卫生室 = "D";
        const string 门诊部 = "D1";
        const string 综合门诊部 = "D110";
        const string 中医门诊部 = "D120";
        const string 中医_综合_门诊部 = "D121";
        const string 中医专科门诊部 = "D122";
        const string 中西医结合门诊部 = "D130";
        const string 民族医门诊部 = "D140";
        const string 专科门诊部 = "D150";
        const string 普通专科门诊部 = "D151";
        const string 口腔门诊部 = "D152";
        const string 眼科门诊部 = "D153";
        const string 医疗美容门诊部 = "D154";
        const string 精神卫生门诊部 = "D155";
        const string 其他专科门诊部 = "D159";
        const string 诊所 = "D2";
        const string 普通诊所 = "D211";
        const string 中医诊所 = "D212";
        const string 中西医结合诊所 = "D213";
        const string 民族医诊所 = "D214";
        const string 口腔诊所 = "D215";
        const string 医疗美容诊所 = "D216";
        const string 精神卫生诊所 = "D217";
        const string 其他诊所 = "D229";
        const string 卫生所_室_= "D300";
        const string 医务室 = "D400";
        const string 中小学卫生保健所 = "D500";
        const string 村卫生室 = "D600";
        const string 急救中心（站） = "E";
        const string 急救中心 = "E100";
        const string 急救中心站 = "E200";
        const string 急救站 = "E300";
        const string 采供血机构 = "F";
        const string 血站 = "F1";
        const string 血液中心 = "F110";
        const string 中心血站 = "F120";
        const string 基层血站_中心血库 = "F130";
        const string 单采血浆站 = "F200";
        const string 妇幼保健院_所_站_= "G";
        const string 妇幼保健院 = "G100";
    }

    public class 医疗卫生机构性质代码
    {

        const string 非营利性医疗卫生机构 = "1";
        const string 营利性医疗卫生机构 = "2";
        const string 其他医疗卫生机构 = "9";
    }

    public class 绑定的或者支持的支付账户类型
    {

        const string 无 = "0";
        const string 微信 = "1";
        const string 支付宝 = "2";
        const string 银联 = "3";
        const string 银行 = "4";
        const string 统一支付平台 = "5";
        const string 其他 = "9";
    }

    public class 健康卡状态
    {

        const string 正常 = "1";
        const string 挂失 = "2";
        const string 注销 = "3";
        const string 制卡确认 = "4";
        const string 未发卡 = "5";
    }

    public class 卡类型
    {

        const string 电子健康卡 = "0";
        const string 实体健康卡 = "1";
    }

    public class 金融机构编号
    {

        const string 中国邮政储蓄银行 = "100";
        const string 中国工商银行 = "102";
        const string 中国农业银行 = "103";
        const string 中国银行 = "104";
        const string 中国建设银行 = "105";
        const string 交通银行 = "301";
        const string 中信银行 = "302";
        const string 丹东银行 = "446";
        const string 锦州银行 = "439";
        const string 汉口银行 = "414";
        const string 鞍山银行 = "456";
        const string 烟台市商业银行 = "404";
        const string 镇江市商业银行 = "407";
        const string 焦作市商业银行 = "411";
        const string 苏州市商业银行 = "421";
        const string 乌鲁木齐市商业银行 = "427";
        const string 宜昌市商业银行 = "432";
        const string 葫芦岛市商业银行 = "433";
        const string 无锡市商业银行 = "445";
        const string 吉林市商业银行 = "451";
        const string 南通市商业银行 = "452";
        const string 扬州市商业银行 = "453";
        const string 秦皇岛市商业银行 = "457";
        const string 盐城市商业银行 = "460";
        const string 泉州市商业银行 = "464";
        const string 营口市商业银行 = "465";
        const string 常州市商业银行 = "468";
        const string 淮安市商业银行 = "469";
        const string 嘉兴市商业银行 = "470";
        const string 芜湖市商业银行 = "471";
        const string 马鞍山市商业银行 = "477";
        const string 连云港市商业银行 = "480";
        const string 威海市商业银行 = "481";
        const string 淮北市商业银行 = "482";
        const string 攀枝花市商业银行 = "483";
        const string 安庆市商业银行 = "484";
        const string 绵阳市商业银行 = "485";
        const string 泸州市商业银行 = "486";
        const string 大同市商业银行 = "487";
        const string 湛江市商业银行 = "489";
        const string 张家口市商业银行 = "490";
        const string 徐州市商业银行 = "494";
        const string 柳州市商业银行 = "495";
        const string 南充市商业银行 = "496";
        const string 德阳市商业银行 = "498";
        const string 唐山市商业银行 = "499";
        const string 六盘水市商业银行 = "500";
        const string 曲靖市商业银行 = "502";
        const string 西宁市商业银行 = "504";
        const string 洛阳市商业银行 = "509";
        const string 长治市商业银行 = "512";
        const string 遵义市商业银行 = "516";
        const string 邯郸市商业银行 = "517";
        const string 安顺市商业银行 = "519";
        const string 平凉市商业银行 = "523";
        const string 玉溪市商业银行 = "524";
        const string 东营市商业银行 = "527";
        const string 泰安市商业银行 = "528";
        const string 襄樊市商业银行 = "529";
        const string 自贡市商业银行 = "532";
        const string 许昌市商业银行 = "536";
        const string 牡丹江市商业银行 = "538";
        const string 铁岭市商业银行 = "539";
        const string 乐山市商业银行 = "540";
        const string 盘锦市商业银行 = "544";
        const string 遂宁市商业银行 = "551";
        const string 保定市商业银行 = "552";
        const string 凉山州商业银行 = "555";
        const string 漯河市商业银行 = "556";
        const string 达州市商业银行 = "557";
        const string 晋中市商业银行 = "559";
        const string 驻马店市商业银行 = "560";
        const string 衡水市商业银行 = "561";
        const string 周口市商业银行 = "562";
        const string 阳泉市商业银行 = "563";
        const string 宜宾市商业银行 = "564";
        const string 库尔勒市商业银行 = "565";
        const string 雅安市商业银行 = "566";
        const string 商丘市商业银行 = "567";
        const string 安阳市商业银行 = "568";
        const string 濮阳市商业银行 = "579";
        const string 上海农村商业银行 = "1401";
        const string 昆山农村商业银行 = "1402";
        const string 常熟农村商业银行 = "1403";
        const string 深圳农村商业银行 = "1404";
        const string 广州农村商业银行 = "1405";
        const string 佛山顺德农村商业银行 = "1408";
        const string 江阴农村商业银行 = "1412";
        const string 重庆农村商业银行 = "1413";
        const string 东莞农村商业银行 = "1415";
        const string 张家港农村商业银行 = "1416";
        const string 北京农村商业银行 = "1418";
        const string 天津农村商业银行 = "1419";
        const string 成都农村商业银行 = "1422";
        const string 江门新会农村商业银行 = "1425";
        const string 吴江农村商业银行 = "1428";
        const string 江苏太仓农村商业银行 = "1433";
        const string 山西尧都农村商业银行 = "1434";
        const string 江苏锡州农村商业银行 = "1437";
        const string 宁夏黄河农村商业银行 = "1446";
        const string 天津滨海农村商业银行 = "1456";
        const string 武汉农村商业银行 = "1459";
        const string 江南农村商业银行 = "1460";
        const string 运城市农村信用合作社联合社 = "518";
        const string 杭州市萧山区农村信用合作社联合社 = "1406";
        const string 佛山市南海区农村信用合作社联合社 = "1407";
        const string 云南省农村信用社联合社 = "1409";
        const string 湖北省农村信用社联合社 = "1410";
        const string 徐州市市郊信用合作社联合社 = "1411";
        const string 山东省农村信用社联合社 = "1414";
        const string 福建省农村信用社联合社 = "1417";
        const string 佛山市三水区农村信用合作社联合社 = "1421";
        const string 沧州市农村信用合作社联合社 = "1423";
        const string 江苏省农村信用社联合社 = "1424";
        const string 高要市农村信用合作社联合社 = "1426";
        const string 佛山市禅城区农村信用合作社联合社 = "1427";
        const string 浙江省农村信用社联合社 = "1429";
        const string 珠海市农村信用合作社联合社 = "1431";
        const string 贵州省农村信用社联合社 = "1436";
        const string 湖南省农村信用社联合社 = "1438";
        const string 江西省农村信用社联合社 = "1439";
        const string 河南省农村信用社联合社 = "1440";
        const string 河北省农村信用社联合社 = "1441";
        const string 陕西省农村信用社联合社 = "1442";
        const string 广西壮族自治区农村信用社联合社 = "1443";
        const string 新疆维吾尔自治区农村信用社联合社 = "1444";
        const string 吉林省农村信用社联合社 = "1445";
        const string 安徽省农村信用社联合社 = "1447";
        const string 海南省农村信用社联合社01 = "1448";
        const string 青海省农村信用社联合社 = "1449";
        const string 广东省农村信用社联合社 = "1450";
        const string 内蒙古自治区农村信用社联合社 = "1451";
        const string 四川省农村信用社联合社 = "1452";
        const string 甘肃省农村信用社联合社 = "1453";
        const string 辽宁省农村信用社联合社 = "1454";
        const string 山西省农村信用社联合社 = "1455";
        const string 黑龙江省农村信用社联合社 = "1457";
        const string 宁波鄞州农村合作银行 = "1420";
        const string 三门峡市城市信用社 = "488";
        const string 象山县绿叶城市信用社 = "550";
        const string 浙江泰隆商业银行 = "473";
        const string 浙江民泰商业银行 = "525";
        const string 浙江稠州商业银行 = "530";
        const string 江苏长江商业银行 = "493";
        const string 景德镇商业银行 = "573";
        const string 齐齐哈尔商行 = "416";
        const string 中国光大银行 = "303";
        const string 华夏银行 = "304";
        const string 中国民生银行 = "305";
        const string 广发银行 = "306";
        const string 深圳发展银行 = "307";
        const string 招商银行 = "308";
        const string 兴业银行01 = "309";
        const string 上海浦东发展银行 = "310";
        const string 恒丰银行 = "311";
        const string 浙商银行 = "316";
        const string 渤海银行 = "317";
        const string 上海银行 = "401";
        const string 北京银行 = "403";
        const string 厦门银行 = "402";
        const string 福建海峡银行 = "405";
        const string 吉林银行 = "406";
        const string 宁波银行 = "408";
        const string 齐鲁银行 = "409";
        const string 平安银行 = "410";
        const string 温州银行 = "412";
        const string 广州银行 = "413";
        const string 盛京银行 = "417";
        const string 洛阳银行 = "418";
        const string 辽阳银行 = "419";
        const string 大连银行 = "420";
        const string 河北银行 = "422";
        const string 杭州银行 = "423";
        const string 南京银行 = "424";
        const string 东莞银行 = "425";
        const string 金华银行 = "426";
        const string 绍兴银行 = "428";
        const string 成都银行 = "429";
        const string 抚顺银行 = "430";
        const string 临商银行 = "431";
        const string 天津银行 = "434";
        const string 郑州银行 = "435";
        const string 宁夏银行 = "436";
        const string 珠海华润银行 = "437";
        const string 齐商银行 = "438";
        const string 徽商银行 = "440";
        const string 重庆银行 = "441";
        const string 哈尔滨银行 = "442";
        const string 贵阳银行 = "443";
        const string 西安银行 = "444";
        const string 兰州银行 = "447";
        const string 南昌银行 = "448";
        const string 晋商银行 = "449";
        const string 青岛银行 = "450";
        const string 九江银行 = "454";
        const string 日照银行 = "455";
        const string 青海银行 = "458";
        const string 长沙银行 = "461";
        const string 潍坊银行 = "462";
        const string 赣州银行 = "463";
        const string 富滇银行 = "466";
        const string 阜新银行 = "467";
        const string 廊坊银行 = "472";
        const string 内蒙古银行 = "474";
        const string 湖州银行 = "475";
        const string 沧州银行 = "476";
        const string 广西北部湾银行 = "478";
        const string 包商银行 = "479";
        const string 桂林银行 = "491";
        const string 龙江银行 = "492";
        const string 莱商银行 = "497";
        const string 晋城银行 = "503";
        const string 江苏银行 = "508";
        const string 承德银行 = "513";
        const string 德州银行 = "515";
        const string 上饶银行 = "526";
        const string 乌海银行 = "531";
        const string 七台河城信 = "533";
        const string 鄂尔多斯银行 = "534";
        const string 鹤壁银行 = "535";
        const string 长安银行 = "541";
        const string 重庆三峡银行 = "542";
        const string 石嘴山银行 = "543";
        const string 昆仑银行 = "547";
        const string 平顶山银行 = "548";
        const string 朝阳银行 = "549";
        const string 黄石银行 = "553";
        const string 邢台银行 = "554";
        const string 新乡银行 = "558";
        const string 信阳银行 = "569";
        const string 华融湘江银行 = "570";
        const string 营口沿海银行 = "572";
        const string 哈密商行 = "574";
        const string 湖北银行 = "575";
        const string 西藏银行 = "576";
        const string 新疆汇和银行 = "577";
        const string 华兴银行 = "578";
        const string 苏州银行 = "1430";
        const string 济宁银行 = "537";



    }

    public class 数据来源方式
    {

        const string 医疗卫生机构 = "0";
        const string 支付宝小程序 = "1";
        const string 地市健康卡平台 = "2";
        const string 其他第三方App = "3";

    }
}