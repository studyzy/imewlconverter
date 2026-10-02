namespace ImeWlConverter.Formats.LibIMEPinyin;

/// <summary>
/// libime 拼音编码表：由 libime <c>libime/pinyin/pinyindata.cpp</c> 中
/// <c>PinyinFuzzyFlag::None</c> 的规范条目生成。
///
/// 每个拼音音节映射为 libime 内部的 (声母, 韵母) 字符对，与
/// <c>PinyinEncoder::encodeFullPinyinWithFlags(pinyin, VE_UE)</c> 的结果一致。
/// 枚举名与 libime <c>PinyinInitial</c>/<c>PinyinFinal</c> 保持一致。
/// </summary>
internal static class LibimePinyinTable
{
    private enum PinyinInitial
    {
        B = 65, P, M, F, D, T, N, L, G, K, H, J, Q, X, ZH, CH, SH, R, Z, C, S, Y, W, Zero,
    }

    private enum PinyinFinal
    {
        A = 65, AI, AN, ANG, AO, E, EI, EN, ENG, ER, O, ONG, OU, I, IA, IE, IAO, IU,
        IAN, IN, IANG, ING, IONG, U, UA, UO, UAI, UI, UAN, UN, UANG, V, VE, UE, NG,
        Zero, Letter_A, Letter_B, Letter_C, Letter_D, Letter_E, Letter_F, Letter_G,
        Letter_H, Letter_I, Letter_J, Letter_K, Letter_L, Letter_M, Letter_N, Letter_O,
        Letter_P, Letter_Q, Letter_R, Letter_S, Letter_T, Letter_U, Letter_V, Letter_W,
        Letter_X, Letter_Y, Letter_Z,
    }

    /// <summary>
    /// "拼音:声母:韵母" 条目，取自 libime pinyindata.cpp 的规范拼音表
    /// (<c>PinyinFuzzyFlag::None</c>)，并补充 lue/nue 两个 VE_UE 模糊别名。
    /// </summary>
    private const string Data =
        "zuo:Z:UO,zun:Z:UN,zui:Z:UI,zuan:Z:UAN,zu:Z:U,zou:Z:OU,zong:Z:ONG,zi:Z:I,zhuo:ZH:UO,zhun:ZH:UN,zhui:ZH:UI," +
        "zhuang:ZH:UANG,zhuan:ZH:UAN,zhuai:ZH:UAI,zhua:ZH:UA,zhu:ZH:U,zhou:ZH:OU,zhong:ZH:ONG,zhi:ZH:I,zheng:ZH:ENG," +
        "zhen:ZH:EN,zhei:ZH:EI,zhe:ZH:E,zhao:ZH:AO,zhang:ZH:ANG,zhan:ZH:AN,zhai:ZH:AI,zha:ZH:A,zeng:Z:ENG,zen:Z:EN," +
        "zei:Z:EI,ze:Z:E,zao:Z:AO,zang:Z:ANG,zan:Z:AN,zai:Z:AI,za:Z:A,yun:Y:UN,yue:Y:UE,yuan:Y:UAN,yu:Y:U,you:Y:OU," +
        "yong:Y:ONG,yo:Y:O,ying:Y:ING,yin:Y:IN,yi:Y:I,ye:Y:E,yao:Y:AO,yang:Y:ANG,yan:Y:AN,ya:Y:A,xun:X:UN,xue:X:UE," +
        "xuan:X:UAN,xu:X:U,xiu:X:IU,xiong:X:IONG,xing:X:ING,xin:X:IN,xie:X:IE,xiao:X:IAO,xiang:X:IANG,xian:X:IAN," +
        "xia:X:IA,xi:X:I,wu:W:U,wo:W:O,wong:W:ONG,weng:W:ENG,wen:W:EN,wei:W:EI,wang:W:ANG,wan:W:AN,wai:W:AI,wa:W:A," +
        "tuo:T:UO,tun:T:UN,tui:T:UI,tuan:T:UAN,tu:T:U,tou:T:OU,tong:T:ONG,ting:T:ING,tie:T:IE,tiao:T:IAO,tian:T:IAN," +
        "ti:T:I,teng:T:ENG,tei:T:EI,te:T:E,tao:T:AO,tang:T:ANG,tan:T:AN,tai:T:AI,ta:T:A,suo:S:UO,sun:S:UN,sui:S:UI," +
        "suan:S:UAN,su:S:U,sou:S:OU,song:S:ONG,si:S:I,shuo:SH:UO,shun:SH:UN,shui:SH:UI,shuang:SH:UANG,shuan:SH:UAN," +
        "shuai:SH:UAI,shua:SH:UA,shu:SH:U,shou:SH:OU,shi:SH:I,sheng:SH:ENG,shen:SH:EN,shei:SH:EI,she:SH:E,shao:SH:AO," +
        "shang:SH:ANG,shan:SH:AN,shai:SH:AI,sha:SH:A,seng:S:ENG,sen:S:EN,se:S:E,sao:S:AO,sang:S:ANG,san:S:AN,sai:S:AI," +
        "sa:S:A,rua:R:UA,r:R:Zero,ruo:R:UO,run:R:UN,rui:R:UI,ruan:R:UAN,ru:R:U,rou:R:OU,rong:R:ONG,ri:R:I,reng:R:ENG," +
        "ren:R:EN,re:R:E,rao:R:AO,rang:R:ANG,ran:R:AN,qun:Q:UN,que:Q:UE,quan:Q:UAN,qu:Q:U,qiu:Q:IU,qiong:Q:IONG," +
        "qing:Q:ING,qin:Q:IN,qie:Q:IE,qiao:Q:IAO,qiang:Q:IANG,qian:Q:IAN,qia:Q:IA,qi:Q:I,pu:P:U,pou:P:OU,po:P:O," +
        "ping:P:ING,pin:P:IN,pie:P:IE,piao:P:IAO,pian:P:IAN,pi:P:I,peng:P:ENG,pen:P:EN,pei:P:EI,pao:P:AO,pang:P:ANG," +
        "pan:P:AN,pai:P:AI,pa:P:A,ou:Zero:OU,o:Zero:O,nve:N:VE,nv:N:V,nuo:N:UO,nun:N:UN,nuan:N:UAN,nu:N:U,nou:N:OU," +
        "nong:N:ONG,niu:N:IU,ning:N:ING,nia:N:IA,nin:N:IN,nie:N:IE,niao:N:IAO,niang:N:IANG,nian:N:IAN,ni:N:I," +
        "ng:Zero:NG,neng:N:ENG,nen:N:EN,nei:N:EI,ne:N:E,nao:N:AO,nang:N:ANG,nan:N:AN,nai:N:AI,na:N:A,n:N:Zero,mu:M:U," +
        "mou:M:OU,mo:M:O,miu:M:IU,ming:M:ING,min:M:IN,mie:M:IE,miao:M:IAO,mian:M:IAN,mi:M:I,meng:M:ENG,men:M:EN," +
        "mei:M:EI,me:M:E,mao:M:AO,mang:M:ANG,man:M:AN,mai:M:AI,ma:M:A,m:M:Zero,lve:L:VE,lv:L:V,luo:L:UO,lun:L:UN," +
        "luan:L:UAN,lu:L:U,lou:L:OU,long:L:ONG,lo:L:O,liu:L:IU,ling:L:ING,lin:L:IN,lie:L:IE,liao:L:IAO,liang:L:IANG," +
        "lian:L:IAN,lia:L:IA,li:L:I,leng:L:ENG,lei:L:EI,le:L:E,lao:L:AO,lang:L:ANG,lan:L:AN,lai:L:AI,la:L:A,kuo:K:UO," +
        "kun:K:UN,kui:K:UI,kuang:K:UANG,kuan:K:UAN,kuai:K:UAI,kua:K:UA,ku:K:U,kou:K:OU,kong:K:ONG,keng:K:ENG,ken:K:EN," +
        "kei:K:EI,ke:K:E,kao:K:AO,kang:K:ANG,kan:K:AN,kai:K:AI,ka:K:A,jun:J:UN,jue:J:UE,juan:J:UAN,ju:J:U,jiu:J:IU," +
        "jiong:J:IONG,jing:J:ING,jin:J:IN,jie:J:IE,jiao:J:IAO,jiang:J:IANG,jian:J:IAN,jia:J:IA,ji:J:I,huo:H:UO," +
        "hun:H:UN,hui:H:UI,huang:H:UANG,huan:H:UAN,huai:H:UAI,hua:H:UA,hu:H:U,hou:H:OU,hong:H:ONG,heng:H:ENG,hen:H:EN," +
        "hei:H:EI,he:H:E,hao:H:AO,hang:H:ANG,han:H:AN,hai:H:AI,ha:H:A,guo:G:UO,gun:G:UN,gui:G:UI,guang:G:UANG," +
        "guan:G:UAN,guai:G:UAI,gua:G:UA,gu:G:U,gou:G:OU,gong:G:ONG,geng:G:ENG,gen:G:EN,gei:G:EI,ge:G:E,gao:G:AO," +
        "gang:G:ANG,gan:G:AN,gai:G:AI,ga:G:A,fu:F:U,fou:F:OU,fo:F:O,fiao:F:IAO,feng:F:ENG,fen:F:EN,fei:F:EI,fang:F:ANG," +
        "fan:F:AN,fa:F:A,er:Zero:ER,eng:Zero:ENG,en:Zero:EN,ei:Zero:EI,e:Zero:E,duo:D:UO,dun:D:UN,dui:D:UI,duan:D:UAN," +
        "du:D:U,dou:D:OU,dong:D:ONG,diu:D:IU,ding:D:ING,din:D:IN,die:D:IE,diao:D:IAO,dian:D:IAN,dia:D:IA,di:D:I," +
        "deng:D:ENG,den:D:EN,dei:D:EI,de:D:E,dao:D:AO,dang:D:ANG,dan:D:AN,dai:D:AI,da:D:A,cuo:C:UO,cun:C:UN,cui:C:UI," +
        "cuan:C:UAN,cu:C:U,cou:C:OU,cong:C:ONG,ci:C:I,chuo:CH:UO,chun:CH:UN,chui:CH:UI,chuang:CH:UANG,chuan:CH:UAN," +
        "chuai:CH:UAI,chua:CH:UA,chu:CH:U,chou:CH:OU,chong:CH:ONG,chi:CH:I,cheng:CH:ENG,chen:CH:EN,che:CH:E,chao:CH:AO," +
        "chang:CH:ANG,chan:CH:AN,chai:CH:AI,cha:CH:A,ceng:C:ENG,cen:C:EN,ce:C:E,cao:C:AO,cang:C:ANG,can:C:AN,cai:C:AI," +
        "ca:C:A,bu:B:U,bong:B:ONG,bo:B:O,bing:B:ING,bin:B:IN,bie:B:IE,biao:B:IAO,biang:B:IANG,bian:B:IAN,bi:B:I," +
        "beng:B:ENG,ben:B:EN,bei:B:EI,bao:B:AO,bang:B:ANG,ban:B:AN,bai:B:AI,ba:B:A,ao:Zero:AO,ang:Zero:ANG,an:Zero:AN," +
        "ai:Zero:AI,a:Zero:A,A:Zero:Letter_A,B:Zero:Letter_B,C:Zero:Letter_C,D:Zero:Letter_D,E:Zero:Letter_E," +
        "F:Zero:Letter_F,G:Zero:Letter_G,H:Zero:Letter_H,I:Zero:Letter_I,J:Zero:Letter_J,K:Zero:Letter_K," +
        "L:Zero:Letter_L,M:Zero:Letter_M,N:Zero:Letter_N,O:Zero:Letter_O,P:Zero:Letter_P,Q:Zero:Letter_Q," +
        "R:Zero:Letter_R,S:Zero:Letter_S,T:Zero:Letter_T,U:Zero:Letter_U,V:Zero:Letter_V,W:Zero:Letter_W," +
        "X:Zero:Letter_X,Y:Zero:Letter_Y,Z:Zero:Letter_Z,lue:L:VE,nue:N:VE,"
;

    private static readonly Dictionary<string, (byte Initial, byte Final)> Map = Build();

    private static readonly string[] InitialStrings = BuildInitialStrings();

    private static readonly string[] FinalStrings = BuildFinalStrings();

    private static Dictionary<string, (byte Initial, byte Final)> Build()
    {
        var map = new Dictionary<string, (byte Initial, byte Final)>(450, StringComparer.Ordinal);
        foreach (var token in Data.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = token.Split(':');
            map[parts[0]] = ((byte)Enum.Parse<PinyinInitial>(parts[1]), (byte)Enum.Parse<PinyinFinal>(parts[2]));
        }

        return map;
    }

    private static string[] BuildInitialStrings()
    {
        var strings = new string[(int)PinyinInitial.Zero - (int)PinyinInitial.B + 1];
        for (var value = (int)PinyinInitial.B; value <= (int)PinyinInitial.Zero; value++)
        {
            var name = ((PinyinInitial)value).ToString();
            strings[value - (int)PinyinInitial.B] = name == nameof(PinyinInitial.Zero) ? "" : name.ToLowerInvariant();
        }

        return strings;
    }

    private static string[] BuildFinalStrings()
    {
        var strings = new string[(int)PinyinFinal.Letter_Z - (int)PinyinFinal.A + 1];
        for (var value = (int)PinyinFinal.A; value <= (int)PinyinFinal.Letter_Z; value++)
        {
            var name = ((PinyinFinal)value).ToString();
            strings[value - (int)PinyinFinal.A] = name switch
            {
                nameof(PinyinFinal.Zero) => "",
                _ when name.StartsWith("Letter_", StringComparison.Ordinal) => name["Letter_".Length..],
                _ => name.ToLowerInvariant(),
            };
        }

        return strings;
    }

    /// <summary>
    /// 尝试把 ASCII 拼音音节编码为 libime 的 (声母, 韵母) 字符对。
    /// 除规范拼写外，还接受 imewlconverter 常见的 "lue"/"nue" (libime VE_UE 模糊别名)。
    /// </summary>
    public static bool TryEncode(string syllable, out byte initial, out byte final)
    {
        if (Map.TryGetValue(syllable, out var pair))
        {
            initial = pair.Initial;
            final = pair.Final;
            return true;
        }

        if (syllable.EndsWith("ue", StringComparison.Ordinal))
        {
            var canonical = string.Concat(syllable.AsSpan(0, syllable.Length - 2), "ve");
            if (Map.TryGetValue(canonical, out pair))
            {
                initial = pair.Initial;
                final = pair.Final;
                return true;
            }
        }

        initial = 0;
        final = 0;
        return false;
    }

    /// <summary>把 libime 的 (声母, 韵母) 字符对还原为 ASCII 拼音音节；无法识别时返回空串。</summary>
    public static string DecodeSyllable(byte initial, byte final)
    {
        var initialString = initial >= (byte)PinyinInitial.B && initial <= (byte)PinyinInitial.Zero
            ? InitialStrings[initial - (byte)PinyinInitial.B]
            : "";
        var finalString = final >= (byte)PinyinFinal.A && final <= (byte)PinyinFinal.Letter_Z
            ? FinalStrings[final - (byte)PinyinFinal.A]
            : "";
        return initialString + finalString;
    }

    /// <summary>把 libime 多音节拼音字节串（每音节 2 字节，无分隔）解码为 ' 分隔的拼音。</summary>
    public static string DecodeFullPinyin(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            return "";

        var syllables = new string[data.Length / 2];
        for (var i = 0; i < syllables.Length; i++)
            syllables[i] = DecodeSyllable(data[i * 2], data[(i * 2) + 1]);
        return string.Join('\'', syllables);
    }
}
