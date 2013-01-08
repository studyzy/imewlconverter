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
                case CodeType.Erbi:
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
                case CodeType.Erbi:
                    return new ErbiGenerater();
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
                default:
                    return new SelfDefiningCodeGenerater();
            }
        }
    }
}