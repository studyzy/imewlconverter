using System;
using System.Collections.Generic;
using System.Diagnostics;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Generaters
{


    public class ChaoyinGenerater : IWordCodeGenerater
    {


        protected  PinyinGenerater pinyinGenerater = new PinyinGenerater();

        public bool Is1Char1Code
        {
            get { return false; }
        }


        public bool Is1CharMutiCode
        {
            get { return true; }
        }


        public Code GetCodeOfString(string str)
        {
            var pyCode = pinyinGenerater.GetCodeOfString(str);
            var pyList = pyCode.GetDefaultCode();
            return new Code(ChaoyinHelper.GetChaoyin(pyList));
        }

        public void GetCodeOfWordLibrary(WordLibrary wl)
        {
            if (wl.CodeType == CodeType.Pinyin)
            {
                var code = ChaoyinHelper.GetChaoyin(wl.PinYin);
                wl.SetCode(CodeType.Chaoyin, code);
            }
            else
            {
                wl.SetCode(CodeType.Chaoyin, GetCodeOfString(wl.Word));
            }
        }

        public IList<string> GetAllCodesOfChar(char str)
        {
            var pyCode = pinyinGenerater.GetAllCodesOfChar(str);
            var result=new List<string>();
            foreach (var py in pyCode)
            {
                result.Add(ChaoyinHelper.GetChaoyin(py));
            }
            return result;
        }
    }
}