#!/usr/bin/env bash
# 重新生成2-exports测试套件的预期输出文件
# 用途：将CSV源数据导出为各种输入法格式的预期输出

set -euo pipefail

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
TEST_CASES_DIR="${REPO_ROOT}/tests/integration/test-cases/2-exports"
EXPECTED_DIR="${TEST_CASES_DIR}/expected"
SOURCE_CSV="${TEST_CASES_DIR}/source/source-data-csv.txt"
SCEL_SOURCE="${REPO_ROOT}/src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel"
ARRAY30_CODE="${REPO_ROOT}/src/ImeWlConverterCoreTest/Test/array30.txt"

# CLI工具路径（根据平台选择）
CLI_DLL="${REPO_ROOT}/src/ImeWlConverterCmd/bin/Release/net10.0/ImeWlConverterCmd.dll"
CLI_EXE="${REPO_ROOT}/src/ImeWlConverterCmd/bin/Release/net10.0/ImeWlConverterCmd"
CLI_CMD=""

# 检查CLI工具是否存在（支持.dll和可执行文件）
if [[ -f "${CLI_DLL}" ]]; then
    CLI_CMD="dotnet \"${CLI_DLL}\""
elif [[ -f "${CLI_EXE}" ]]; then
    CLI_CMD="\"${CLI_EXE}\""
else
    echo "错误: CLI工具不存在"
    echo "请先构建项目: cd ${REPO_ROOT} && dotnet build -c Release"
    exit 1
fi

# 检查源文件是否存在
if [[ ! -f "${SOURCE_CSV}" ]]; then
    echo "错误: CSV源文件不存在: ${SOURCE_CSV}"
    exit 1
fi

# 确保输出目录存在
mkdir -p "${EXPECTED_DIR}"

echo "========================================="
echo "重新生成2-exports测试预期输出文件"
echo "========================================="
echo "源CSV: ${SOURCE_CSV}"
echo "输出目录: ${EXPECTED_DIR}"
echo ""

# 计数器
success_count=0
fail_count=0

# E01: CSV到搜狗拼音文本
echo "生成 E01-CSV到搜狗拼音文本..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:sgpy "${EXPECTED_DIR}/e01-csv-to-sgpy.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E02: CSV到QQ拼音文本 (UTF-16LE编码)
echo "生成 E02-CSV到QQ拼音文本..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:qqpy "${EXPECTED_DIR}/e02-csv-to-qqpy.expected" 2>&1 | grep -q "转换完成"; then
    # 转换为UTF-16LE编码
    iconv -f UTF-8 -t UTF-16LE "${EXPECTED_DIR}/e02-csv-to-qqpy.expected" > "${EXPECTED_DIR}/e02-csv-to-qqpy.tmp"
    mv "${EXPECTED_DIR}/e02-csv-to-qqpy.tmp" "${EXPECTED_DIR}/e02-csv-to-qqpy.expected"
    echo "  ✓ 成功 (已转换为UTF-16LE)"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E03: CSV到Rime格式
echo "生成 E03-CSV到Rime格式..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:rime "${EXPECTED_DIR}/e03-csv-to-rime.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E04: CSV到微软拼音 (已禁用，跳过)
echo "跳过 E04-CSV到微软拼音 (已禁用)"

# E06: CSV到谷歌拼音
echo "生成 E06-CSV到谷歌拼音..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:ggpy "${EXPECTED_DIR}/e06-csv-to-ggpy.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E07: CSV到百度拼音
echo "生成 E07-CSV到百度拼音..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:bdpy "${EXPECTED_DIR}/e07-csv-to-bdpy.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E08: CSV到拼音加加
echo "生成 E08-CSV到拼音加加..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:pyjj "${EXPECTED_DIR}/e08-csv-to-pyjj.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E09: CSV到紫光拼音
echo "生成 E09-CSV到紫光拼音..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:zgpy "${EXPECTED_DIR}/e09-csv-to-zgpy.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E14: CSV到libpinyin
echo "生成 E14-CSV到libpinyin..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:libpy "${EXPECTED_DIR}/e14-csv-to-libpy.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E15: CSV到FIT输入法
echo "生成 E15-CSV到FIT输入法..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:fit "${EXPECTED_DIR}/e15-csv-to-fit.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E16: CSV到Mac简体拼音
echo "生成 E16-CSV到Mac简体拼音..."
if ${CLI_CMD} -i:self "${SOURCE_CSV}" -o:plist "${EXPECTED_DIR}/e16-csv-to-plist.expected" 2>&1 | grep -q "转换完成"; then
    echo "  ✓ 成功"
    success_count=$((success_count + 1))
else
    echo "  ✗ 失败"
    fail_count=$((fail_count + 1))
fi

# E10-E13, E05: 往返测试 (已禁用，跳过)
echo "跳过往返测试用例 (E05, E10-E13, 已禁用)"

# E17: CSV到行列30自定义编码
echo "生成 E17-CSV到行列30自定义编码..."
if [[ -f "${SCEL_SOURCE}" ]] && [[ -f "${ARRAY30_CODE}" ]]; then
    if ${CLI_CMD} \
        -i:scel "${SCEL_SOURCE}" \
        "-c:${ARRAY30_CODE}" \
        "-mc:code_e2=p11+p12+p21+p22,code_e3=p11+p21+p31+p32,code_a4=p11+p21+p31+n11" \
        "-f:213 ,nyyy" \
        -o:self "${EXPECTED_DIR}/e17-csv-to-array30.expected" 2>&1 | grep -q "转换完成"; then
        echo "  ✓ 成功"
        success_count=$((success_count + 1))
    else
        echo "  ✗ 失败"
        fail_count=$((fail_count + 1))
    fi
else
    echo "  ⚠️  跳过 (源文件或编码文件不存在)"
fi

echo ""
echo "========================================="
echo "完成!"
echo "成功: ${success_count} 个"
echo "失败: ${fail_count} 个"
echo "生成的文件位于: ${EXPECTED_DIR}"
echo "========================================="

# 返回退出码
if [[ ${fail_count} -eq 0 ]]; then
    exit 0
else
    exit 1
fi
