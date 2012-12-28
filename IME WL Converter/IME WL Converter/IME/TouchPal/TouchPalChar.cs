using System;
using System.IO;
using System.Text;

namespace Studyzy.IMEWLConverter.IME.TouchPal
{
    /// <summary>
    /// 一个字，包含拼音本字的顺序和拼音2字节，词频4，下字4，跳转4，上字4，真实上字4，未知4，共占有空间26字节
    /// </summary>
    internal class TouchPalChar
    {
        private int beginPosition;
        private int countPosition;
        private int jumpToPosition;
        private int nextCharPosition;
        private int pinyinCode;
        private int prevCharPosition;
        private int prevValidCharPosition;
        private int unknown;
        private int wordIndex;

        public int Unknown
        {
            get { return unknown; }
        }

        public int PrevValidCharPosition
        {
            get { return prevValidCharPosition; }
            set { prevValidCharPosition = value; }
        }

        public int PrevCharPosition
        {
            get { return prevCharPosition; }
            set { prevCharPosition = value; }
        }

        public int JumpToPosition
        {
            get { return jumpToPosition; }
            set { jumpToPosition = value; }
        }

        public int NextCharPosition
        {
            get { return nextCharPosition; }
            set { nextCharPosition = value; }
        }

        public int CountPosition
        {
            get { return countPosition; }
            set { countPosition = value; }
        }

        /// <summary>
        /// 前2个字节，表示字的顺序和字的拼音编码，前5位是字的顺序，后面11位是拼音编码
        /// </summary>
        public short IndexAndPinYin
        {
            get { return (short) ((wordIndex << 11) + pinyinCode); }
            set
            {
                wordIndex = value >> 11;
                pinyinCode = value & 0x7FF;
            }
        }

        public int BeginPosition
        {
            get { return beginPosition; }
            set { beginPosition = value; }
        }

        /// <summary>
        /// 下一个字
        /// </summary>
        public TouchPalChar NextChar { get; set; }

        /// <summary>
        /// 跳转到的字
        /// </summary>
        public TouchPalChar JumpToChar { get; set; }

        /// <summary>
        /// 直接上一个字
        /// </summary>
        public TouchPalChar PrevChar { get; set; }

        /// <summary>
        /// 词语上的直接上一个字（忽略中间的跳转）
        /// </summary>
        public TouchPalChar PrevValidChar { get; set; }

        /// <summary>
        /// 字关联的词，只有该字是最后一个字的时候才有这个属性
        /// </summary>
        public TouchPalWord Word { get; set; }

        /// <summary>
        /// 该字在词中的顺序，从1开始计数
        /// </summary>
        public int WordIndex
        {
            get { return wordIndex; }
            set { wordIndex = value; }
        }

        /// <summary>
        /// 拼音的编码，通过GlobalCache.PinyinIndexMapping可查得
        /// </summary>
        public int PinyinCode
        {
            get { return pinyinCode; }
            set { pinyinCode = value; }
        }

        public string PinyinString
        {
            get { return GlobalCache.PinyinMapping[PinyinCode]; }
        }

        /// <summary>
        /// 真实对应的汉字，在导出时使用
        /// </summary>
        public char Char { get; set; }

        /// <summary>
        /// 这个词在内存中占用的字节数，如果是最后一个字，词频汉字也算这个字的占用，在导出时使用
        /// </summary>
        public int MemeryLength
        {
            get
            {
                if (Word == null)
                {
                    return 26;
                }
                else
                {
                    return 26 + 2*Word.ChineseWord.Length + 5;
                }
            }
        }

        public static TouchPalChar Load(FileStream fs, int position = 0)
        {
            if (position > 0)
            {
                fs.Position = position;
            }
            if (GlobalCache.CharList.ContainsKey((int) fs.Position))
            {
                return GlobalCache.CharList[(int) fs.Position];
            }

            var c = new TouchPalChar();
            c.beginPosition = (int) fs.Position;
            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            c.IndexAndPinYin = BitConverter.ToInt16(temp, 0);
            temp = new byte[4];
            fs.Read(temp, 0, 4);
            c.countPosition = BitConverter.ToInt32(temp, 0);
            fs.Read(temp, 0, 4);
            c.nextCharPosition = BitConverter.ToInt32(temp, 0);
            fs.Read(temp, 0, 4);
            c.jumpToPosition = BitConverter.ToInt32(temp, 0);
            fs.Read(temp, 0, 4);
            c.prevCharPosition = BitConverter.ToInt32(temp, 0);
            fs.Read(temp, 0, 4);
            c.prevValidCharPosition = BitConverter.ToInt32(temp, 0);
            fs.Read(temp, 0, 4);
            c.unknown = BitConverter.ToInt32(temp, 0);
            GlobalCache.CharList.Add(c.beginPosition, c);
            return c;
        }

        /// <summary>
        /// 是否是词中的最后一个字
        /// </summary>
        /// <returns></returns>
        public bool IsLastChar()
        {
            return countPosition > 0;
        }

        public byte[] ToBinary()
        {
            var mem = new byte[MemeryLength];
            BitConverter.GetBytes(IndexAndPinYin).CopyTo(mem, 0);
            BitConverter.GetBytes(CountPosition).CopyTo(mem, 2);
            BitConverter.GetBytes(NextCharPosition).CopyTo(mem, 6);
            BitConverter.GetBytes(JumpToPosition).CopyTo(mem, 10);
            BitConverter.GetBytes(PrevCharPosition).CopyTo(mem, 14);
            BitConverter.GetBytes(PrevValidCharPosition).CopyTo(mem, 18);
            BitConverter.GetBytes(Unknown).CopyTo(mem, 22);
            if (Word != null)
            {
                Word.ToBinary().CopyTo(mem, 26);
            }
            return mem;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Position:{0}\t", beginPosition);
            sb.AppendFormat("WordIndex:{0}\t", wordIndex);
            sb.AppendFormat("PinyinString:{0}\t", PinyinString);
            sb.AppendFormat("CountPosition:{0}\t", CountPosition);
            sb.AppendFormat("NextPosition:{0}\t", NextCharPosition);
            sb.AppendFormat("JumpPosition:{0}\t", JumpToPosition);
            sb.AppendFormat("PrePosition:{0}\t", PrevCharPosition);
            sb.AppendFormat("PreVailidPosition:{0}\t", PrevValidCharPosition);
            sb.AppendFormat("UnknowPosition:{0}", Unknown);
            return sb.ToString();
        }
    }
}