#!/usr/bin/env bash
# 重新生成CSV格式的预期输出文件
# 用途：将现有测试数据转换为新的CSV格式(词语,拼音,词频)

set -euo pipefail

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"
TEST_CASES_DIR="${REPO_ROOT}/tests/integration/test-cases/1-imports"
EXPECTED_DIR="${TEST_CASES_DIR}/expected"

# CSV格式参数: 词语,拼音 空格分隔,词频
# 213 ,nyyy: 2=词语,1=拼音,3=词频,空格分隔拼音,逗号分隔字段,n=不包含,yyy=都显示
FORMAT_OPTIONS="213 ,nyyy"

# CLI工具路径
CLI_DLL="${REPO_ROOT}/src/ImeWlConverterCmd/bin/Release/net10.0/ImeWlConverterCmd.dll"

# 检查CLI工具是否存在
if [[ ! -f "${CLI_DLL}" ]]; then
    echo "错误: CLI工具不存在,请先构建项目"
    echo "运行: cd ${REPO_ROOT} && dotnet build -c Release"
    exit 1
fi

# 确保输出目录存在
mkdir -p "${EXPECTED_DIR}"

echo "========================================="
echo "重新生成CSV格式预期输出文件"
echo "========================================="
echo "格式: 词语,拼音 空格分隔,词频"
echo "参数: ${FORMAT_OPTIONS}"
echo ""

# 定义测试用例数组 (格式: "测试名称|输入文件|源格式|输出文件")
declare -a TEST_CASES=(
    "T01|唐诗300首【官方推荐】.scel|scel|t01-scel-to-csv.expected"
    "T02|QQPinyin.txt|qqpy|t02-qqtxt-to-csv.expected"
    "T03|成语.qpyd|qpyd|t03-qpyd-to-csv.expected"
    "T04|星际战甲.qcel|qcel|t04-qcel-to-csv.expected"
    "T06|luna_pinyin_export.txt|rime|t06-rime-to-csv.expected"
    "T09|纯汉字.txt|word|t09-word-to-csv.expected"
)

# 处理每个测试用例
for test_case in "${TEST_CASES[@]}"; do
    IFS='|' read -r test_name input_file source_format output_file <<< "${test_case}"
    
    INPUT_PATH="${REPO_ROOT}/src/ImeWlConverterCoreTest/Test/${input_file}"
    OUTPUT_PATH="${EXPECTED_DIR}/${output_file}"
    
    echo "处理: ${test_name}"
    echo "  输入: ${input_file}"
    echo "  格式: ${source_format} -> CSV"
    
    # 检查输入文件是否存在
    if [[ ! -f "${INPUT_PATH}" ]]; then
        echo "  ⚠️  警告: 输入文件不存在,跳过: ${INPUT_PATH}"
        echo ""
        continue
    fi
    
    # 运行转换
    if dotnet "${CLI_DLL}" \
        -i:"${source_format}" "${INPUT_PATH}" \
        -o:self "${OUTPUT_PATH}" \
        "-f:${FORMAT_OPTIONS}" 2>&1 | grep -q "转换完成"; then
        
        # 统计词条数
        word_count=$(wc -l < "${OUTPUT_PATH}" | tr -d ' ')
        echo "  ✓ 成功: 生成 ${word_count} 条词条"
        
        # 显示前3行示例
        echo "  示例:"
        head -3 "${OUTPUT_PATH}" | sed 's/^/    /'
    else
        echo "  ✗ 失败: 转换出错"
    fi
    
    echo ""
done

echo "========================================="
echo "完成! 生成的文件位于:"
echo "${EXPECTED_DIR}"
echo "========================================="
