namespace Studyzy.IMEWLConverter.Generaters
{
    public interface IWordRankGenerater
    {
        int GetRank(string word);
        /// <summary>
        /// 是否忽略源词库的词频，强制使用新词频？
        /// </summary>
        bool ForceUse { get; set; }
    }
}