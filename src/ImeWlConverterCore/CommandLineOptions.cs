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

using System.Collections.Generic;

namespace Studyzy.IMEWLConverter;

/// <summary>
/// 命令行选项的强类型模型
/// </summary>
public class CommandLineOptions
{
    /// <summary>
    /// 输入词库格式代码（如 scel, ggpy, qqpy）
    /// </summary>
    public string InputFormat { get; set; } = string.Empty;

    /// <summary>
    /// 输出词库格式代码（如 ggpy, rime, self）
    /// </summary>
    public string OutputFormat { get; set; } = string.Empty;

    /// <summary>
    /// 输出文件路径或目录路径
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// 输入文件路径列表（支持多文件）
    /// </summary>
    public List<string> InputFiles { get; set; } = new();

    /// <summary>
    /// 编码映射文件路径（用于自定义编码）
    /// </summary>
    public string? CodeFile { get; set; }

    /// <summary>
    /// 过滤条件字符串（如 "len:1-100|rm:eng"）
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 自定义格式规范（如 "213, nyyn"）
    /// </summary>
    public string? CustomFormat { get; set; }

    /// <summary>
    /// 词频生成器类型（baidu, google, 或固定数字）
    /// </summary>
    public string? RankGenerator { get; set; }

    /// <summary>
    /// 多字词编码生成规则
    /// </summary>
    public string? MultiCode { get; set; }

    /// <summary>
    /// 编码类型（pinyin, wubi, zhengma, cangjie, zhuyin）
    /// </summary>
    public string? CodeType { get; set; }

    /// <summary>
    /// 目标操作系统（windows, macos, linux）
    /// </summary>
    public string? TargetOS { get; set; }

    /// <summary>
    /// Lingoes ld2 文件编码设置
    /// </summary>
    public string? Ld2Encoding { get; set; }
}
