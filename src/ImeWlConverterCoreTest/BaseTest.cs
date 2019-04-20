using System;
using System.IO;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Test
{
    public abstract class BaseTest
    {
        /// <summary>
        /// 深蓝测试
        /// </summary>
        protected WordLibrary WlData = new WordLibrary
            {Rank = 10, PinYin = new[] {"shen", "lan", "ce", "shi"}, Word = "深蓝测试"};

        protected IWordLibraryExport exporter;
        protected IWordLibraryImport importer;
        protected abstract string StringData { get; }

        /// <summary>
        /// 深蓝测试
        /// 词库转换
        /// </summary>
        protected WordLibraryList WlListData
        {
            get
            {
                var wordLibrary = new WordLibrary
                    {Rank = 80, PinYin = new[] {"ci", "ku", "zhuan", "huan"}, Word = "词库转换"};
                return new WordLibraryList {WlData, wordLibrary};
            }
        }

        public abstract void InitData();

        protected string GetFullPath(string fileName)
        {
            return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Test", fileName)
            ;
        }
    }
}