/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.IME
{
    /// <summary>
    /// Win10微软拼音自学习词库
    /// </summary>
    [ComboBoxShow(
        ConstantString.WIN10_MS_PINYIN_SELF_STUDY,
        ConstantString.WIN10_MS_PINYIN_SELF_STUDY_C,
        131
    )]
    public class Win10MsPinyinSelfStudy : IWordLibraryExport, IWordLibraryImport
    {
        #region Pinyin Map
        private static Dictionary<string, short> pinyinMap;
        private Dictionary<string, short> PinyinMap
        {
            get
            {
                if (pinyinMap == null)
                {
                    pinyinMap = new Dictionary<string, short>();
                    for (short i = 0; i < pinyinIndex.Length; i++)
                    {
                        pinyinMap[pinyinIndex[i]] = i;
                    }
                }
                return pinyinMap;
            }
        }
        private static string[] pinyinIndex = new string[]
        {
            "a",
            "ai",
            "an",
            "ang",
            "ao",
            "ba",
            "bai",
            "ban",
            "bang",
            "bao",
            "bei",
            "ben",
            "beng",
            "bi",
            "bian",
            "biao",
            "bie",
            "bin",
            "bing",
            "bo",
            "bu",
            "ca",
            "cai",
            "can",
            "cang",
            "cao",
            "ce",
            "cen",
            "ceng",
            "cha",
            "chai",
            "chan",
            "chang",
            "chao",
            "che",
            "chen",
            "cheng",
            "chi",
            "chong",
            "chou",
            "chu",
            "chua",
            "chuai",
            "chuan",
            "chuang",
            "chui",
            "chun",
            "chuo",
            "ci",
            "cong",
            "cou",
            "cu",
            "cuan",
            "cui",
            "cun",
            "cuo",
            "da",
            "dai",
            "dan",
            "dang",
            "dao",
            "de",
            "dei",
            "den",
            "deng",
            "di",
            "dia",
            "dian",
            "diao",
            "die",
            "ding",
            "diu",
            "dong",
            "dou",
            "du",
            "duan",
            "dui",
            "dun",
            "duo",
            "e",
            "ei",
            "en",
            "eng",
            "er",
            "fa",
            "fan",
            "fang",
            "fei",
            "fen",
            "feng",
            "fiao",
            "fo",
            "fou",
            "fu",
            "ga",
            "gai",
            "gan",
            "gang",
            "gao",
            "ge",
            "gei",
            "gen",
            "geng",
            "gong",
            "gou",
            "gu",
            "gua",
            "guai",
            "guan",
            "guang",
            "gui",
            "gun",
            "guo",
            "ha",
            "hai",
            "han",
            "hang",
            "hao",
            "he",
            "hei",
            "hen",
            "heng",
            "hong",
            "hou",
            "hu",
            "hua",
            "huai",
            "huan",
            "huang",
            "hui",
            "hun",
            "huo",
            "ji",
            "jia",
            "jian",
            "jiang",
            "jiao",
            "jie",
            "jin",
            "jing",
            "jiong",
            "jiu",
            "ju",
            "juan",
            "jue",
            "jun",
            "ka",
            "kai",
            "kan",
            "kang",
            "kao",
            "ke",
            "kei",
            "ken",
            "keng",
            "kong",
            "kou",
            "ku",
            "kua",
            "kuai",
            "kuan",
            "kuang",
            "kui",
            "kun",
            "kuo",
            "la",
            "lai",
            "lan",
            "lang",
            "lao",
            "le",
            "lei",
            "leng",
            "li",
            "lia",
            "lian",
            "liang",
            "liao",
            "lie",
            "lin",
            "ling",
            "liu",
            "lo",
            "long",
            "lou",
            "lu",
            "luan",
            "lue",
            "lun",
            "luo",
            "lv",
            "ma",
            "mai",
            "man",
            "mang",
            "mao",
            "me",
            "mei",
            "men",
            "meng",
            "mi",
            "mian",
            "miao",
            "mie",
            "min",
            "ming",
            "miu",
            "mo",
            "mou",
            "mu",
            "na",
            "nai",
            "nan",
            "nang",
            "nao",
            "ne",
            "nei",
            "nen",
            "neng",
            "ni",
            "nian",
            "niang",
            "niao",
            "nie",
            "nin",
            "ning",
            "niu",
            "nong",
            "nou",
            "nu",
            "nuan",
            "nue",
            "nun",
            "nuo",
            "nv",
            "o",
            "ou",
            "pa",
            "pai",
            "pan",
            "pang",
            "pao",
            "pei",
            "pen",
            "peng",
            "pi",
            "pian",
            "piao",
            "pie",
            "pin",
            "ping",
            "po",
            "pou",
            "pu",
            "qi",
            "qia",
            "qian",
            "qiang",
            "qiao",
            "qie",
            "qin",
            "qing",
            "qiong",
            "qiu",
            "qu",
            "quan",
            "que",
            "qun",
            "ran",
            "rang",
            "rao",
            "re",
            "ren",
            "reng",
            "ri",
            "rong",
            "rou",
            "ru",
            "rua",
            "ruan",
            "rui",
            "run",
            "ruo",
            "sa",
            "sai",
            "san",
            "sang",
            "sao",
            "se",
            "sen",
            "seng",
            "sha",
            "shai",
            "shan",
            "shang",
            "shao",
            "she",
            "shei",
            "shen",
            "sheng",
            "shi",
            "shou",
            "shu",
            "shua",
            "shuai",
            "shuan",
            "shuang",
            "shui",
            "shun",
            "shuo",
            "si",
            "song",
            "sou",
            "su",
            "suan",
            "sui",
            "sun",
            "suo",
            "ta",
            "tai",
            "tan",
            "tang",
            "tao",
            "te",
            "tei",
            "teng",
            "ti",
            "tian",
            "tiao",
            "tie",
            "ting",
            "tong",
            "tou",
            "tu",
            "tuan",
            "tui",
            "tun",
            "tuo",
            "wa",
            "wai",
            "wan",
            "wang",
            "wei",
            "wen",
            "weng",
            "wo",
            "wu",
            "xi",
            "xia",
            "xian",
            "xiang",
            "xiao",
            "xie",
            "xin",
            "xing",
            "xiong",
            "xiu",
            "xu",
            "xuan",
            "xue",
            "xun",
            "ya",
            "yan",
            "yang",
            "yao",
            "ye",
            "yi",
            "yin",
            "ying",
            "yo",
            "yong",
            "you",
            "yu",
            "yuan",
            "yue",
            "yun",
            "za",
            "zai",
            "zan",
            "zang",
            "zao",
            "ze",
            "zei",
            "zen",
            "zeng",
            "zha",
            "zhai",
            "zhan",
            "zhang",
            "zhao",
            "zhe",
            "zhei",
            "zhen",
            "zheng",
            "zhi",
            "zhong",
            "zhou",
            "zhu",
            "zhua",
            "zhuai",
            "zhuan",
            "zhuang",
            "zhui",
            "zhun",
            "zhuo",
            "zi",
            "zong",
            "zou",
            "zu",
            "zuan",
            "zui",
            "zun",
            "zuo",
        };
        #endregion

        public event Action<string> ImportLineErrorNotice;

        public Win10MsPinyinSelfStudy()
        {
            this.CodeType = CodeType.Pinyin;
            this.PinyinType = PinyinType.FullPinyin;
        }

        public PinyinType PinyinType { get; set; }
        public Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        public int CountWord { get; set; }
        public int CurrentStatus { get; set; }

        public bool IsText => false;

        public CodeType CodeType { get; set; }

        /// <summary>
        /// 小端数字到int
        /// </summary>
        /// <param name="src">数组</param>
        /// <param name="offset">从数组offset开始</param>
        /// <param name="len">长len个字节</param>
        /// <returns></returns>
        private int bytesToIntLittle(byte[] src, int offset, int len)
        {
            int value = 0,
                sf = 0;
            for (; len > 0; offset++, len--, sf += 8)
                value |= (src[offset] & 0xFF) << sf;
            return value;
        }

        public WordLibraryList Import(string path)
        {
            WordLibraryList re = new WordLibraryList();
            FileStream fp = File.OpenRead(path);
            int user_word_base = 0x2400;
            //get word num
            byte[] bytes = new byte[50];
            fp.Seek(12, SeekOrigin.Begin);
            fp.Read(bytes, 0, 4);
            int cnt = bytesToIntLittle(bytes, 0, 4);
            //get each word
            for (int i = 0; i < cnt; i++)
            {
                int cur_idx = user_word_base + i * 60;
                //get word len
                fp.Seek(cur_idx + 10, SeekOrigin.Begin);
                fp.Read(bytes, 0, 1);
                int wordLen = bytesToIntLittle(bytes, 0, 1);
                //get word
                fp.Seek(cur_idx + 12, SeekOrigin.Begin);
                fp.Read(bytes, 0, wordLen * 2);
                string word = Encoding.Unicode.GetString(bytes, 0, wordLen * 2);
                //get pinyin
                var pinyin = new string[wordLen];
                for (var j = 0; j < wordLen; j++)
                {
                    var byte2 = new byte[2];
                    fp.Read(byte2, 0, 2);
                    var pyIndex = BitConverter.ToInt16(byte2, 0);
                    if (pyIndex >= 0 && pyIndex < pinyinIndex.Length)
                        pinyin[j] = pinyinIndex[pyIndex];
                }
                re.Add(
                    new WordLibrary()
                    {
                        Word = word,
                        CodeType = this.CodeType,
                        PinYin = pinyin
                    }
                );
            }
            fp.Close();
            return re;
        }

        public WordLibraryList ImportLine(string str)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }

        public string ExportFilePath { get; set; }
        public event Action<string> ExportErrorNotice;

        protected void SendExportErrorNotice(string msg)
        {
            ExportErrorNotice?.Invoke(msg);
        }

        /// <summary>
        /// 最多支持2W条一个dat文件
        /// </summary>
        /// <param name="wlList"></param>
        /// <returns></returns>
        public IList<string> Export(WordLibraryList wlList)
        {
            //Win10拼音对词条长度有限制
            wlList = Filter(wlList);
            var list = new List<WordLibraryList>();
            if (wlList.Count > 20000)
            {
                SendExportErrorNotice(
                    "微软拼音自学习词库最多支持2万条记录的导入，当前词条数为：" + wlList.Count + "，超过限制，请设置过滤条件或者更换词库源。"
                );
                //以后微软拼音放开2W限制了，再把这个异常取消吧。
                var item20000 = new WordLibraryList();
                for (var i = 0; i < wlList.Count; i++)
                {
                    item20000.Add(wlList[i]);
                    if (i % 19999 == 0 && i != 0)
                    {
                        list.Add(item20000);
                        item20000 = new WordLibraryList();
                    }
                }
                if (item20000.Count != 0)
                {
                    list.Add(item20000);
                }
            }
            else
            {
                list.Add(wlList);
            }
            var fileList = "";
            for (var i = 0; i < list.Count; i++)
            {
                string tempPath = Path.Combine(
                    FileOperationHelper.GetCurrentFolderPath(),
                    "Win10微软拼音自学习词库" + i + ".dat"
                );
                if (!string.IsNullOrEmpty(this.ExportFilePath)) //For test
                {
                    tempPath = this.ExportFilePath;
                }
                fileList += tempPath + "\r\n";
                ExportTo1File(tempPath, list[i]);
            }
            return new List<string>() { "词库文件在：" + fileList };
        }

        private void ExportTo1File(string tempPath, WordLibraryList wlList)
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            var fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(HexStringToByteArray("55AA88810200600055AA55AA")); //Unknown

            bw.Write(BitConverter.GetBytes((long)wlList.Count)); //phrase_count
            bw.Write(BitConverter.GetBytes((int)DateTime.Now.Ticks)); //timestamp
            for (var i = 0; i < 9192; i++)
            {
                bw.Write((byte)0);
            }
            //0x2400词条开始
            for (var i = 0; i < wlList.Count; i++)
            {
                var wl = wlList[i];
                try
                {
                    // bw.Write(new byte[] { 0x6D, 0x1B });
                    bw.Write(BitConverter.GetBytes((Int16)(i + 0x6D1B))); //Unknown，怀疑是词频
                    bw.Write(new byte[] { 0x1A, 0x26 }); //Unknown
                    bw.Write(new byte[] { 0x00, 0x00, 0x00 }); //前3个字的拼音？
                    bw.Write(new byte[] { 0x00, 0x00, 0x04 });
                    bw.Write((byte)wl.Word.Length);
                    bw.Write((byte)0x5A);
                    bw.Write(Encoding.Unicode.GetBytes(wl.Word));
                    foreach (string py1 in wl.PinYin)
                    {
                        var py1Index = PinyinMap[py1];
                        bw.Write(py1Index);
                    }
                    var used = 12 + 4 * wl.Word.Length;
                    //一个词条60字节，剩下的补0
                    for (var j = used; j < 60; j++)
                    {
                        bw.Write((byte)0);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            //最后一堆0,补到nK (n>=10)
            var k = (int)Math.Ceiling(fs.Position / 1024.0);
            while (fs.Position < k * 1024)
            {
                bw.Write((byte)0);
            }

            fs.Close();
        }

        private WordLibraryList Filter(WordLibraryList wlList)
        {
            var result = new WordLibraryList();

            foreach (var wl in wlList)
            {
                if (wl.Word.Length > 12 || wl.Word.Length == 1) //最多支持12个字
                    continue;

                result.Add(wl);
            }
            return result;
        }

        public string ExportLine(WordLibrary wl)
        {
            throw new NotImplementedException("二进制文件不支持单个词汇的转换");
        }

        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }

            return buffer;
        }
    }
}
