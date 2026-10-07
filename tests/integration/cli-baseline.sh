#!/usr/bin/env bash
# CLI 退出码基线测试（characterization）
#
# 目的：在重构前固化 CLI 的错误处理行为（退出码 + 错误消息风格）。
# Phase 4 计划将退出码从 0/1 细分为 0-4 并新增 --json 输出；
# 届时此脚本的断言应同步更新，更新前的差异即回归。
#
# 用法：./cli-baseline.sh （需先 dotnet build src/ImeWlConverterCmd）

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
CLI_DLL="${REPO_ROOT}/src/ImeWlConverterCmd/bin/Debug/net10.0/ImeWlConverterCmd.dll"
TEST_DATA="${REPO_ROOT}/src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel"
TMP_DIR="$(mktemp -d)"

if [[ ! -f "${CLI_DLL}" ]]; then
    echo "错误: CLI 不存在: ${CLI_DLL}"
    echo "请先构建: dotnet build src/ImeWlConverterCmd"
    exit 1
fi

PASS=0
FAIL=0

# assert_exit <描述> <期望退出码> <命令...>
assert_exit() {
    local desc="$1" expected="$2"
    shift 2
    local actual=0
    "$@" >/dev/null 2>&1 || actual=$?
    if [[ "${actual}" == "${expected}" ]]; then
        echo "  PASS: ${desc} (exit=${actual})"
        PASS=$((PASS + 1))
    else
        echo "  FAIL: ${desc} 期望 exit=${expected} 实际 exit=${actual}"
        FAIL=$((FAIL + 1))
    fi
}

# assert_error_message <描述> <期望包含的文本> <命令...>
assert_error_message() {
    local desc="$1" expected_text="$2"
    shift 2
    local output
    output="$("$@" 2>&1 >/dev/null)" || true
    if [[ "${output}" == *"${expected_text}"* ]]; then
        echo "  PASS: ${desc} (错误消息包含 \"${expected_text}\")"
        PASS=$((PASS + 1))
    else
        echo "  FAIL: ${desc} 错误消息不包含 \"${expected_text}\"，实际输出: ${output}"
        FAIL=$((FAIL + 1))
    fi
}

# assert_error_not_contains <描述> <不应包含的文本> <命令...>
assert_error_not_contains() {
    local desc="$1" forbidden="$2"
    shift 2
    local output
    output="$("$@" 2>&1 >/dev/null)" || true
    if [[ "${output}" != *"${forbidden}"* ]]; then
        echo "  PASS: ${desc} (错误消息不含 \"${forbidden}\")"
        PASS=$((PASS + 1))
    else
        echo "  FAIL: ${desc} 错误消息泄漏了 \"${forbidden}\"，实际输出: ${output}"
        FAIL=$((FAIL + 1))
    fi
}

echo "== CLI 退出码基线测试 =="
CLI() { dotnet "${CLI_DLL}" "$@"; }

# 1. 成功转换 → exit 0
assert_exit "正常转换(scel→self)退出码 0" 0 \
    CLI -i scel -o self -O "${TMP_DIR}/ok.txt" -F "213 ,nyyy" "${TEST_DATA}"

# 2. 未知输入格式 → exit 1
assert_exit "未知输入格式退出码 1" 1 \
    CLI -i no-such-format -o self -O "${TMP_DIR}/x.txt" "${TEST_DATA}"

# 3. 缺少必填参数 → exit 1
assert_exit "缺少输入格式退出码 1" 1 \
    CLI -o self -O "${TMP_DIR}/x.txt" "${TEST_DATA}"

# 4. 输入文件不存在 → exit 2（Phase 4 退出码契约：输入错误类）
assert_exit "输入文件不存在退出码 2" 2 \
    CLI -i scel -o self -O "${TMP_DIR}/x.txt" "${TMP_DIR}/no-such-file.scel"
# 注：错误消息断言使用 ASCII 锚点 —— Windows 下 CLI 中文输出走 OEM 代码页(GBK)，
# 与本脚本的 UTF-8 字符串无法直接字节比较；退出码断言已锁定行为本身。
assert_error_message "输入文件不存在输出可读错误" "no-such-file.scel" \
    CLI -i scel -o self -O "${TMP_DIR}/x.txt" "${TMP_DIR}/no-such-file.scel"

# 5. 非法过滤参数（字母数字）→ exit 1 且不抛裸 FormatException
assert_exit "非法过滤参数退出码 1" 1 \
    CLI -i scel -o self -O "${TMP_DIR}/x.txt" -f "len:abc-10" "${TEST_DATA}"
assert_error_message "非法过滤参数输出可读错误" "len:abc-10" \
    CLI -i scel -o self -O "${TMP_DIR}/x.txt" -f "len:abc-10" "${TEST_DATA}"

# 6. 错误消息不泄漏完整堆栈（默认模式；IMEWL_DEBUG=1 时才输出堆栈）
assert_error_not_contains "默认模式错误消息不含堆栈" "   at " \
    CLI -i no-such-format -o self -O "${TMP_DIR}/x.txt" "${TEST_DATA}"

# 6.5 -r 固定词频选项（Issue #425 回归：-r 曾被当作输入文件名导致 exit 2）
assert_exit "-r 数字固定词频退出码 0" 0 \
    CLI -i scel -o self -O "${TMP_DIR}/rank.txt" -r 100 "${TEST_DATA}"
assert_exit "-r 非数字值退出码 1" 1 \
    CLI -i scel -o self -O "${TMP_DIR}/x.txt" -r baidu "${TEST_DATA}"
JSON_RANK_ERR="$(CLI --json -i scel -o self -O "${TMP_DIR}/x.txt" -r baidu "${TEST_DATA}" 2>/dev/null)"
if [[ "${JSON_RANK_ERR}" == *'"code": "invalid-rank"'* ]]; then
    echo "  PASS: --json -r 非数字值输出 error.code=invalid-rank"
    PASS=$((PASS + 1))
else
    echo "  FAIL: --json -r 错误输出异常: ${JSON_RANK_ERR}"
    FAIL=$((FAIL + 1))
fi

# 7. --json 成功输出（schema=1, ok=true）
JSON_OUTPUT="$(CLI --json -i scel -o self -O "${TMP_DIR}/ok.json.txt" -F "213 ,nyyy" "${TEST_DATA}" 2>/dev/null)"
if [[ "${JSON_OUTPUT}" == *'"ok": true'* && "${JSON_OUTPUT}" == *'"schema": 1'* ]]; then
    echo "  PASS: --json 成功输出包含 schema=1 与 ok=true"
    PASS=$((PASS + 1))
else
    echo "  FAIL: --json 成功输出异常: ${JSON_OUTPUT}"
    FAIL=$((FAIL + 1))
fi

# 8. --json 失败输出（error.code 契约）
JSON_ERROR="$(CLI --json -i no-such-format -o self -O "${TMP_DIR}/x.txt" "${TEST_DATA}" 2>/dev/null)"
if [[ "${JSON_ERROR}" == *'"ok": false'* && "${JSON_ERROR}" == *'"code": "unknown-format"'* ]]; then
    echo "  PASS: --json 错误输出包含 error.code=unknown-format"
    PASS=$((PASS + 1))
else
    echo "  FAIL: --json 错误输出异常: ${JSON_ERROR}"
    FAIL=$((FAIL + 1))
fi

# 9. --list-formats --json 输出格式清单
JSON_LIST="$(CLI --list-formats --json 2>/dev/null)"
if [[ "${JSON_LIST}" == *'"importFormats"'* && "${JSON_LIST}" == *'"exportFormats"'* ]]; then
    echo "  PASS: --list-formats --json 输出格式清单"
    PASS=$((PASS + 1))
else
    echo "  FAIL: --list-formats --json 输出异常: ${JSON_LIST:0:200}"
    FAIL=$((FAIL + 1))
fi

rm -rf "${TMP_DIR}"

echo ""
echo "结果: 通过 ${PASS}，失败 ${FAIL}"
[[ ${FAIL} -eq 0 ]] || exit 1
