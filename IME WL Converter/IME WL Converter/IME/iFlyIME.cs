using System.Text;

namespace Studyzy.IMEWLConverter.IME
{
    [ComboBoxShow(ConstantString.IFLY_IME, ConstantString.IFLY_IME_C, 1050)]
    public class iFlyIME : NoPinyinWordOnly
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override WordLibraryList ImportLine(string line)
        {
            if (line.Length == 0 || line[0] == '#')
            {
                return null;
            }
            return base.ImportLine(line.Split(' ')[0]);
        }

        public override string Export(WordLibraryList wlList)
        {
            string head =
                @"###注释部分，请勿修改###
#此文本文件为讯飞输入法用户词库导出所生成
#版本信息:30000004
#词库容量:{0}
#词条个数:{1}
#讯飞输入法下载地址:
#http://ime.voicecloud.cn

###格式说明###
#文本编码方式:UTF-8
#词条 类型
#词条部分：限纯中文或纯英文词条，不含空格。中文词条长度上限为10，英文词条长度上限为20
#类型取值：1－联系人，0－其它，若无此项或取值非法，则默认做0处理

###以下为正文内容###
";
            head = string.Format(head, wlList.Count*10, wlList.Count);

            return head + base.Export(wlList);
        }
    }
}