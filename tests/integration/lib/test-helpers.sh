#!/usr/bin/env bash
# 测试辅助函数库
# 用途：提供词库转换集成测试所需的核心功能函数
# 包括：转换器调用、文件比较、错误处理等

set -euo pipefail

# 防止重复加载
if [[ -n "${TEST_HELPERS_LOADED:-}" ]]; then
    return 0
fi
TEST_HELPERS_LOADED=true

# ============================================================================
# 全局变量
# ============================================================================

# CLI工具路径（相对于仓库根目录）
BUILD_CONFIG="${DOTNET_CONFIG:-Debug}"
readonly CONVERTER_CLI="src/ImeWlConverterCmd/bin/${BUILD_CONFIG}/net10.0/ImeWlConverterCmd.dll"

# 超时时间（秒）
readonly DEFAULT_TIMEOUT=30

# diff选项检测（用于跨平台换行符兼容）
DIFF_STRIP_TRAILING_CR_SUPPORTED=false
if diff --help 2>&1 | grep -q -- '--strip-trailing-cr'; then
    DIFF_STRIP_TRAILING_CR_SUPPORTED=true
fi

# ============================================================================
# 工具函数
# ============================================================================

# 函数：获取脚本所在目录的绝对路径
# 返回：tests/integration 目录的绝对路径
get_integration_dir() {
    local script_dir
    script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
    echo "${script_dir}"
}

# 函数：获取仓库根目录的绝对路径
# 返回：仓库根目录的绝对路径
get_repo_root() {
    local integration_dir
    integration_dir="$(get_integration_dir)"
    echo "$(cd "${integration_dir}/../.." && pwd)"
}

# 函数：确保目录存在
# 参数：$1 - 目录路径
ensure_dir() {
    local dir_path="$1"
    if [[ ! -d "${dir_path}" ]]; then
        mkdir -p "${dir_path}"
    fi
}

# 函数：检查文件是否存在
# 参数：$1 - 文件路径
# 返回：0=存在, 1=不存在
file_exists() {
    local file_path="$1"
    [[ -f "${file_path}" ]]
}

# ============================================================================
# 转换器相关函数
# ============================================================================

# 函数：运行词库转换器
# 参数：
#   $1 - 输入文件路径
#   $2 - 源格式代码
#   $3 - 目标格式代码
#   $4 - 输出文件路径
#   $5 - 超时时间（可选，默认30秒）
#   $6 - 格式选项（可选，用于自定义格式，如"213 ,nyyy"）
#   $@ - 额外参数（第7个参数开始，每个参数作为单独的位置参数）
# 返回：0=成功, 非0=失败
run_converter() {
    local input_file="$1"
    local source_format="$2"
    local target_format="$3"
    local output_file="$4"
    local timeout="${5:-${DEFAULT_TIMEOUT}}"
    local format_options="${6:-}"
    shift 6  # 移除前6个参数，剩下的都是额外参数
    
    local repo_root
    repo_root="$(get_repo_root)"
    local cli_path="${repo_root}/${CONVERTER_CLI}"
    
    # 检查CLI工具是否存在
    if [[ ! -f "${cli_path}" ]]; then
        echo "错误: CLI工具不存在: ${cli_path}" >&2
        echo "请先构建项目: dotnet build" >&2
        return 1
    fi
    
    # 检查输入文件是否存在
    if [[ ! -f "${input_file}" ]]; then
        echo "错误: 输入文件不存在: ${input_file}" >&2
        return 1
    fi
    
    # 确保输出目录存在
    local output_dir
    output_dir="$(dirname "${output_file}")"
    ensure_dir "${output_dir}"
    
    # 构建命令参数
    local cmd_args=(-i:"${source_format}" "${input_file}")
    
    # 添加额外参数（在输出参数之前）
    while [[ $# -gt 0 ]]; do
        cmd_args+=("$1")
        shift
    done
    
    # 添加输出参数
    cmd_args+=(-o:"${target_format}" "${output_file}")
    
    # 如果提供了格式选项，添加-f参数
    if [[ -n "${format_options}" ]]; then
        cmd_args+=("-f:${format_options}")
    fi
    
    # 调用转换器（使用timeout命令限制执行时间）
    # CLI工具格式：-i:格式 输入文件 [额外参数] -o:格式 输出文件 [-f:格式选项]
    # 注意：macOS和Linux的timeout命令可能不同，需要处理兼容性
    if command -v timeout >/dev/null 2>&1; then
        # Linux/Git Bash有timeout命令
        timeout "${timeout}s" dotnet "${cli_path}" "${cmd_args[@]}" 2>&1
    elif command -v gtimeout >/dev/null 2>&1; then
        # macOS可能需要安装coreutils: brew install coreutils
        gtimeout "${timeout}s" dotnet "${cli_path}" "${cmd_args[@]}" 2>&1
    else
        # 如果没有timeout命令，直接运行（无超时控制）
        dotnet "${cli_path}" "${cmd_args[@]}" 2>&1
    fi
    
    return $?
}

# ============================================================================
# 文件比较函数
# ============================================================================

# 函数：比较两个文件是否相同
# 参数：
#   $1 - 预期文件路径
#   $2 - 实际文件路径
# 返回：0=相同, 1=不同
# 输出：如果不同，输出diff结果
compare_files() {
    local expected_file="$1"
    local actual_file="$2"
    
    # 检查文件是否存在
    if [[ ! -f "${expected_file}" ]]; then
        echo "错误: 预期文件不存在: ${expected_file}" >&2
        return 1
    fi
    
    if [[ ! -f "${actual_file}" ]]; then
        echo "错误: 实际文件不存在: ${actual_file}" >&2
        return 1
    fi
    
    # 使用diff比较文件
    # -u: 统一格式输出
    # -w: 忽略空白字符的差异
    # --label: 为输出添加标签，使其更易读
    local -a diff_options=(-u -w \
        --label="预期输出" "${expected_file}" \
        --label="实际输出" "${actual_file}")

    if [[ "${DIFF_STRIP_TRAILING_CR_SUPPORTED}" == "true" ]]; then
        diff_options+=(--strip-trailing-cr)
    fi

    if diff "${diff_options[@]}" >/dev/null 2>&1; then
        return 0
    else
        # 文件不同，输出差异
        echo "文件差异详情："
        diff "${diff_options[@]}" || true
        return 1
    fi
}

# 函数：比较微软拼音格式文件（处理动态GUID）
# 参数：
#   $1 - 预期文件路径
#   $2 - 实际文件路径
# 返回：0=相同, 1=不同
compare_mspy_files() {
    local expected_file="$1"
    local actual_file="$2"
    
    # 检查文件是否存在
    if [[ ! -f "${expected_file}" ]]; then
        echo "错误: 预期文件不存在: ${expected_file}" >&2
        return 1
    fi
    
    if [[ ! -f "${actual_file}" ]]; then
        echo "错误: 实际文件不存在: ${actual_file}" >&2
        return 1
    fi
    
    # 创建临时文件，将GUID替换为固定值进行比较
    local temp_expected
    temp_expected=$(mktemp)
    local temp_actual
    temp_actual=$(mktemp)
    
    # 处理预期文件：将GUID替换为固定值
    sed 's/<ns1:DictionaryGUID>{.*}<\/ns1:DictionaryGUID>/<ns1:DictionaryGUID>{FIXED-GUID}<\/ns1:DictionaryGUID>/g' "${expected_file}" > "${temp_expected}"
    
    # 处理实际文件：将GUID替换为固定值
    sed 's/<ns1:DictionaryGUID>{.*}<\/ns1:DictionaryGUID>/<ns1:DictionaryGUID>{FIXED-GUID}<\/ns1:DictionaryGUID>/g' "${actual_file}" > "${temp_actual}"
    
    # 使用diff比较处理后的文件
    local -a diff_options=(-u -w \
        --label="预期输出" "${temp_expected}" \
        --label="实际输出" "${temp_actual}")

    if [[ "${DIFF_STRIP_TRAILING_CR_SUPPORTED}" == "true" ]]; then
        diff_options+=(--strip-trailing-cr)
    fi

    if diff "${diff_options[@]}" >/dev/null 2>&1; then
        # 清理临时文件
        rm -f "${temp_expected}" "${temp_actual}"
        return 0
    else
        # 文件不同，输出差异
        echo "文件差异详情（GUID已标准化）："
        diff "${diff_options[@]}" || true
        # 清理临时文件
        rm -f "${temp_expected}" "${temp_actual}"
        return 1
    fi
}

# 函数：生成文件的详细比较报告
# 参数：
#   $1 - 预期文件路径
#   $2 - 实际文件路径
# 输出：详细的差异信息
generate_diff_report() {
    local expected_file="$1"
    local actual_file="$2"
    
    echo "=========================================="
    echo "文件比较报告"
    echo "=========================================="
    echo "预期文件: ${expected_file}"
    echo "实际文件: ${actual_file}"
    echo ""
    
    if compare_files "${expected_file}" "${actual_file}"; then
        echo "✓ 文件内容完全一致"
        return 0
    else
        echo "✗ 文件内容存在差异"
        echo ""
        return 1
    fi
}

# ============================================================================
# 错误处理函数
# ============================================================================

# 函数：处理转换错误
# 参数：
#   $1 - 测试用例名称
#   $2 - 错误消息
handle_converter_error() {
    local test_name="$1"
    local error_msg="$2"
    
    echo "=========================================="
    echo "转换失败: ${test_name}"
    echo "=========================================="
    echo "${error_msg}"
    echo ""
}

# 函数：处理文件不存在错误
# 参数：
#   $1 - 文件路径
#   $2 - 文件类型描述（如"输入文件"、"预期输出"）
handle_file_not_found() {
    local file_path="$1"
    local file_type="${2:-文件}"
    
    echo "错误: ${file_type}不存在: ${file_path}" >&2
}

# ============================================================================
# 测试结果结构
# ============================================================================

# 函数：创建测试结果对象（使用关联数组模拟）
# 参数：
#   $1 - 测试名称
#   $2 - 是否通过（true/false）
#   $3 - 执行时间（秒）
#   $4 - 错误信息（可选）
# 输出：格式化的测试结果字符串
create_test_result() {
    local test_name="$1"
    local passed="$2"
    local duration="$3"
    local error_msg="${4:-}"
    
    # 输出格式: NAME|PASSED|DURATION|ERROR_MSG
    echo "${test_name}|${passed}|${duration}|${error_msg}"
}

# 函数：解析测试结果字符串
# 参数：$1 - 测试结果字符串
# 输出：按行输出各个字段
parse_test_result() {
    local result="$1"
    
    IFS='|' read -r name passed duration error_msg <<< "${result}"
    
    echo "测试名称: ${name}"
    echo "是否通过: ${passed}"
    echo "执行时间: ${duration}秒"
    if [[ -n "${error_msg}" ]]; then
        echo "错误信息: ${error_msg}"
    fi
}

# ============================================================================
# 导出函数供外部使用
# ============================================================================

# 注意：Bash不支持直接导出函数，但可以通过source方式引入
# 调用方式：source lib/test-helpers.sh