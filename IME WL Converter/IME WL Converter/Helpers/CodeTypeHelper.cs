using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Generaters;

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class CodeTypeHelper
    {
        public static string GetCodeTypeName(CodeType codeType)
        {
            switch (codeType)
            {
                case CodeType.Pinyin:
                    return "拼音";
                case CodeType.Wubi:
                    return "五笔";
                case CodeType.QingsongErbi:
                    return "二笔";
                case CodeType.English:
                    return "英语";
                case CodeType.Yong:
                    return "永码";
                case CodeType.Zhengma:
                    return "郑码";
                case CodeType.InnerCode:
                    return "内码";
                default:
                    return "未知";
            }
        }

        public static IWordCodeGenerater GetGenerater(CodeType codeType)
        {
            switch (codeType)
            {
                case CodeType.Pinyin:
                    return new PinyinGenerater();
                case CodeType.Wubi:
                    return new Wubi86Generater();
                case CodeType.Wubi98:
                    return new Wubi98Generater();
                case CodeType.QingsongErbi:
                    return new QingsongErbiGenerater();
                case CodeType.ChaoqiangErbi:
                    return new ChaoqiangErbiGenerater();
                case CodeType.XiandaiErbi:
                    return new XiandaiErbiGenerater();
                case CodeType.ChaoqingYinxin:
                    return new YingxinErbiGenerater();
                case CodeType.English:
                    return new PinyinGenerater();
                case CodeType.Yong:
                    return new PinyinGenerater();
                case CodeType.Zhengma:
                    return new ZhengmaGenerater();
                case CodeType.TerraPinyin:
                    return new TerraPinyinGenerater();
                case CodeType.Cangjie:
                    return new Cangjie5Generater();
                case CodeType.Chaoyin:
                    return new ChaoyinGenerater();
                case CodeType.UserDefinePhrase:
                    return new PhraseGenerater();
                case CodeType.Zhuyin:
                    return new ZhuyinGenerater();
                case CodeType.NoCode:
                    return null;
                    //case CodeType.UserDefine:
                    //    {
                    //        return SelfDefiningCodeGenerater();
                    //    }
                default:
                    return new SelfDefiningCodeGenerater();
            }
        }

        //private static IWordCodeGenerater SelfDefiningCodeGenerater()
        //{
        //    var UserDefiningPattern = Global.ExportSelfDefiningPattern;
        //    var s = new SelfDefiningCodeGenerater();
        //    s.MutiWordCodeFormat = Global.ExportSelfDefiningPattern.MutiWordCodeFormat;
        //    s.Is1Char1Code = Global.ExportSelfDefiningPattern.IsPinyinFormat;

        //    var dict = UserCodingHelper.GetCodingDict(Global.ExportSelfDefiningPattern.MappingTablePath);
        //    s.MappingDictionary = dict;
        //    return s;
        //}
    }
}