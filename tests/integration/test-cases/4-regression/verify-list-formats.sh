#!/usr/bin/env bash
# 验证 --list-formats 输出包含所有预期格式
# 修复 Issue #395: 确保命令行模式下能全量支持所有输入法格式

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../../../.." && pwd)"
BUILD_CONFIG="${DOTNET_CONFIG:-Debug}"
CLI_PATH="${REPO_ROOT}/src/ImeWlConverterCmd/bin/${BUILD_CONFIG}/net10.0/ImeWlConverterCmd.dll"

# 检查 CLI 是否存在
if [[ ! -f "${CLI_PATH}" ]]; then
    echo "错误: CLI 工具不存在: ${CLI_PATH}" >&2
    echo "请先构建项目: dotnet build src/ImeWlConverterCmd" >&2
    exit 1
fi

# 运行 --list-formats
OUTPUT=$(dotnet "${CLI_PATH}" --list-formats 2>&1)

# 定义所有应该存在的输入格式
EXPECTED_IMPORTS=(
    bcd bdict bdpy bdpybin bdsj bdsje bing cjpt emoji fit gboard ggpy
    ifly jd jdmb jdzm ld2 libimebin libimetxt libpy mspy plist pyim pyjj
    qcel qpyd qqpy qqpye qqsj qqwb rime rimedb scel self sgpy
    sgpybin sxpy uwl wb86 wb98 wbnewage win10mspy win10mspyss
    win10mswb word xiaoxiao xlpy xywb yahoo zgpy
)

# 定义所有应该存在的输出格式
EXPECTED_EXPORTS=(
    bdpy bdsj bdsje bing cjpt cysl dy erbi fit gboard ggpy ifly
    jd jdzm libimebin libimetxt libpy mspy plist pyim pyjj qqpy qqpye qqsj
    qqwb rime scel self sgpy sxpy wb86 wb98 wbnewage win10mspy
    win10mspyss win10mswb word xiaoxiao xlpy xywb yahoo zgpy
)

FAILED=0

echo "验证 --list-formats 输出..."
echo ""

# 验证输入格式
echo "检查输入格式 (预期 ${#EXPECTED_IMPORTS[@]} 种):"
MISSING_IMPORTS=()
for fmt in "${EXPECTED_IMPORTS[@]}"; do
    if ! echo "${OUTPUT}" | grep -q "^  ${fmt} "; then
        MISSING_IMPORTS+=("${fmt}")
    fi
done

if [[ ${#MISSING_IMPORTS[@]} -eq 0 ]]; then
    echo "  PASS: 所有 ${#EXPECTED_IMPORTS[@]} 种输入格式均已注册"
else
    echo "  FAIL: 缺少 ${#MISSING_IMPORTS[@]} 种输入格式:"
    for fmt in "${MISSING_IMPORTS[@]}"; do
        echo "    - ${fmt}"
    done
    FAILED=1
fi

# 验证输出格式
echo ""
echo "检查输出格式 (预期 ${#EXPECTED_EXPORTS[@]} 种):"
MISSING_EXPORTS=()
for fmt in "${EXPECTED_EXPORTS[@]}"; do
    if ! echo "${OUTPUT}" | grep -q "^  ${fmt} "; then
        MISSING_EXPORTS+=("${fmt}")
    fi
done

if [[ ${#MISSING_EXPORTS[@]} -eq 0 ]]; then
    echo "  PASS: 所有 ${#EXPECTED_EXPORTS[@]} 种输出格式均已注册"
else
    echo "  FAIL: 缺少 ${#MISSING_EXPORTS[@]} 种输出格式:"
    for fmt in "${MISSING_EXPORTS[@]}"; do
        echo "    - ${fmt}"
    done
    FAILED=1
fi

# 特别验证 Issue #395 中提到的格式
echo ""
echo "验证 Issue #395 核心格式:"
ISSUE_FORMATS=("word" "wb98" "wb86" "wbnewage" "jd" "xywb" "qqwb" "win10mswb")
for fmt in "${ISSUE_FORMATS[@]}"; do
    if echo "${OUTPUT}" | grep -q "^  ${fmt} "; then
        echo "  PASS: ${fmt}"
    else
        echo "  FAIL: ${fmt} 缺失!"
        FAILED=1
    fi
done

echo ""
if [[ ${FAILED} -eq 0 ]]; then
    echo "=== 全部验证通过 ==="
    exit 0
else
    echo "=== 存在格式缺失，验证失败 ==="
    exit 1
fi
