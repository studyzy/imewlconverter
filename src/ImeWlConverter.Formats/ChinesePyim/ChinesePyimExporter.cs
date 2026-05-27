namespace ImeWlConverter.Formats.ChinesePyim;

using System.Text;
using ImeWlConverter.Abstractions;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Formats.Shared;

/// <summary>Chinese-pyim dictionary exporter (text format). Format: pin-yin word</summary>
[FormatPlugin("pyim", "Chinese-pyim", 177)]
public sealed partial class ChinesePyimExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => Encoding.UTF8;

    protected override string LineEnding => "\n";
    protected override string? GetHeader() => ";; -*- coding: utf-8 -*--";

    protected override string? FormatEntry(WordEntry entry)
    {
        var pinyin = entry.Code?.GetPrimaryCode("-") ?? "";
        if (string.IsNullOrEmpty(pinyin))
            return null;
        return $"{pinyin} {entry.Word}";
    }
}
