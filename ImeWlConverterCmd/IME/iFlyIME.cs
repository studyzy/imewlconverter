using System;
using System.Collections.Generic;
using System.Text;
using Studyzy.IMEWLConverter.Entities;

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

        public override IList<string> Export(WordLibraryList wlList)
        {
            string headFormat =
                @"###注释部分，请勿修改###
#此文本文件为讯飞输入法用户词库导出所生成
#版本信息:30000008
#词库容量:16384
#词条个数:{0}
#讯飞输入法下载地址:
#http://ime.voicecloud.cn

###格式说明###
#文本编码方式:UTF-8
#词条 类型
#词条部分：限纯中文或纯英文词条，不含空格。中文词条长度上限为16，英文词条长度上限为32
#类型取值：1－联系人，0－其它，若无此项或取值非法，则默认做0处理

###以下为正文内容###";

            const int MAX_COUNT = 16000;
            var result = new List<string>();
            int fileCount = (int) Math.Ceiling(1.0*wlList.Count/MAX_COUNT);
            for (int count = 1; count <= fileCount; count++)
            {
                var sb = new StringBuilder();
                int rowCount = 0;
                for (int i = 0; i < (count == fileCount ? wlList.Count%MAX_COUNT : MAX_COUNT); i++)
                {
                    var wl = wlList[(count-1)*MAX_COUNT+i];
                    if (wl.Word.Length > 1 && wl.Word.Length < 17)
                    {
                        sb.Append(wl.Word);
                        sb.Append("\n");
                        rowCount++;
                    }
                }

                result.Add(string.Format(headFormat, rowCount) + "\n" + sb.ToString());
            }
            return result;
        }

    }
}