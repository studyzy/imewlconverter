#!/usr/bin/env bash
# 彩色终端输出函数库
# 用途：为测试报告提供彩色输出，提高可读性

set -euo pipefail

# 防止重复加载
if [[ -n "${COLOR_OUTPUT_LOADED:-}" ]]; then
    return 0
fi
COLOR_OUTPUT_LOADED=true

# ============================================================================
# 颜色代码定义
# ============================================================================

# ANSI颜色代码
readonly COLOR_RESET='\033[0m'
readonly COLOR_RED='\033[0;31m'
readonly COLOR_GREEN='\033[0;32m'
readonly COLOR_YELLOW='\033[0;33m'
readonly COLOR_BLUE='\033[0;34m'
readonly COLOR_MAGENTA='\033[0;35m'
readonly COLOR_CYAN='\033[0;36m'
readonly COLOR_WHITE='\033[0;37m'

# 粗体颜色
readonly COLOR_BOLD_RED='\033[1;31m'
readonly COLOR_BOLD_GREEN='\033[1;32m'
readonly COLOR_BOLD_YELLOW='\033[1;33m'
readonly COLOR_BOLD_BLUE='\033[1;34m'
readonly COLOR_BOLD_WHITE='\033[1;37m'

# ============================================================================
# 符号定义
# ============================================================================

readonly SYMBOL_CHECK='✓'
readonly SYMBOL_CROSS='✗'
readonly SYMBOL_ARROW='→'
readonly SYMBOL_DOT='•'
readonly SYMBOL_STAR='★'

# ============================================================================
# 环境检测
# ============================================================================

# 函数：检测终端是否支持彩色输出
# 返回：0=支持, 1=不支持
is_color_supported() {
    # 检查是否在终端中运行
    if [[ ! -t 1 ]]; then
        return 1
    fi
    
    # 检查TERM环境变量
    if [[ -z "${TERM:-}" ]]; then
        return 1
    fi
    
    # 检查是否明确禁用了颜色
    if [[ "${NO_COLOR:-}" == "1" ]] || [[ "${NO_COLOR:-}" == "true" ]]; then
        return 1
    fi
    
    # 检查CI环境（某些CI系统可能不支持颜色）
    if [[ "${CI:-}" == "true" ]] && [[ "${FORCE_COLOR:-}" != "1" ]]; then
        # GitHub Actions支持颜色
        if [[ "${GITHUB_ACTIONS:-}" != "true" ]]; then
            return 1
        fi
    fi
    
    return 0
}

# 全局变量：是否启用彩色输出
COLOR_ENABLED=false
if is_color_supported; then
    COLOR_ENABLED=true
fi

# ============================================================================
# 基础彩色输出函数
# ============================================================================

# 函数：输出彩色文本
# 参数：
#   $1 - 颜色代码
#   $2 - 要输出的文本
print_colored() {
    local color="$1"
    local text="$2"
    
    if [[ "${COLOR_ENABLED}" == "true" ]]; then
        echo -e "${color}${text}${COLOR_RESET}"
    else
        echo "${text}"
    fi
}

# 函数：输出红色文本（错误）
print_red() {
    print_colored "${COLOR_RED}" "$1"
}

# 函数：输出绿色文本（成功）
print_green() {
    print_colored "${COLOR_GREEN}" "$1"
}

# 函数：输出黄色文本（警告）
print_yellow() {
    print_colored "${COLOR_YELLOW}" "$1"
}

# 函数：输出蓝色文本（信息）
print_blue() {
    print_colored "${COLOR_BLUE}" "$1"
}

# 函数：输出青色文本
print_cyan() {
    print_colored "${COLOR_CYAN}" "$1"
}

# 函数：输出粗体红色文本（重要错误）
print_bold_red() {
    print_colored "${COLOR_BOLD_RED}" "$1"
}

# 函数：输出粗体绿色文本（重要成功）
print_bold_green() {
    print_colored "${COLOR_BOLD_GREEN}" "$1"
}

# 函数：输出粗体黄色文本（重要警告）
print_bold_yellow() {
    print_colored "${COLOR_BOLD_YELLOW}" "$1"
}

# 函数：输出粗体白色文本（重要信息）
print_bold_white() {
    if [[ "${COLOR_ENABLED}" == "true" ]]; then
        echo -e "\033[1;37m${1}${COLOR_RESET}"
    else
        echo "${1}"
    fi
}

# 函数：输出粗体蓝色文本（重要信息）
print_bold_blue() {
    print_colored "${COLOR_BOLD_BLUE}" "$1"
}

# ============================================================================
# 测试状态输出函数
# ============================================================================

# 函数：输出测试通过标记
# 参数：$1 - 测试名称
print_test_pass() {
    local test_name="$1"
    print_green "${SYMBOL_CHECK} ${test_name}"
}

# 函数：输出测试失败标记
# 参数：$1 - 测试名称
print_test_fail() {
    local test_name="$1"
    print_red "${SYMBOL_CROSS} ${test_name}"
}

# 函数：输出测试跳过标记
# 参数：$1 - 测试名称
print_test_skip() {
    local test_name="$1"
    print_yellow "- ${test_name} (跳过)"
}

# 函数：输出测试运行中标记
# 参数：$1 - 测试名称
print_test_running() {
    local test_name="$1"
    print_blue "${SYMBOL_ARROW} ${test_name}"
}

# ============================================================================
# 分隔线和标题输出
# ============================================================================

# 函数：输出分隔线
# 参数：$1 - 字符（默认'='）
#       $2 - 长度（默认60）
print_separator() {
    local char="${1:-=}"
    local length="${2:-60}"
    
    printf '%*s\n' "${length}" '' | tr ' ' "${char}"
}

# 函数：输出标题
# 参数：$1 - 标题文本
print_header() {
    local title="$1"
    echo ""
    print_separator '='
    print_bold_blue "${title}"
    print_separator '='
    echo ""
}

# 函数：输出子标题
# 参数：$1 - 子标题文本
print_subheader() {
    local subtitle="$1"
    echo ""
    print_bold_white "${subtitle}"
    print_separator '-' 40
}

# ============================================================================
# 进度和统计信息输出
# ============================================================================

# 函数：输出进度信息
# 参数：
#   $1 - 当前进度
#   $2 - 总数
#   $3 - 描述文本
print_progress() {
    local current="$1"
    local total="$2"
    local description="$3"
    
    print_cyan "[${current}/${total}] ${description}"
}

# 函数：输出统计信息
# 参数：
#   $1 - 标签
#   $2 - 数值
#   $3 - 颜色函数名（可选，如 "green" "red"）
print_stat() {
    local label="$1"
    local value="$2"
    local color_func="${3:-}"
    
    if [[ -n "${color_func}" ]]; then
        case "${color_func}" in
            green)
                print_green "${label}: ${value}"
                ;;
            red)
                print_red "${label}: ${value}"
                ;;
            yellow)
                print_yellow "${label}: ${value}"
                ;;
            *)
                echo "${label}: ${value}"
                ;;
        esac
    else
        echo "${label}: ${value}"
    fi
}

# ============================================================================
# 表格输出辅助函数
# ============================================================================

# 函数：输出表格行
# 参数：可变参数，每个参数是一列的值
print_table_row() {
    local columns=("$@")
    local col_width=20
    
    for col in "${columns[@]}"; do
        printf "%-${col_width}s" "${col}"
    done
    echo ""
}

# 函数：输出表格分隔线
# 参数：$1 - 列数
print_table_separator() {
    local num_cols="${1:-3}"
    local col_width=20
    local total_width=$((num_cols * col_width))
    
    printf '%*s\n' "${total_width}" '' | tr ' ' '-'
}

# ============================================================================
# 错误和警告信息输出
# ============================================================================

# 函数：输出错误消息
# 参数：$1 - 错误消息
print_error() {
    print_bold_red "错误: $1" >&2
}

# 函数：输出警告消息
# 参数：$1 - 警告消息
print_warning() {
    print_bold_yellow "警告: $1"
}

# 函数：输出信息消息
# 参数：$1 - 信息消息
print_info() {
    print_blue "信息: $1"
}

# 函数：输出成功消息
# 参数：$1 - 成功消息
print_success() {
    print_bold_green "成功: $1"
}

# ============================================================================
# 调试输出
# ============================================================================

# 函数：输出调试信息（仅在verbose模式下）
# 参数：$1 - 调试消息
print_debug() {
    if [[ "${VERBOSE:-false}" == "true" ]]; then
        print_cyan "[DEBUG] $1"
    fi
}

# ============================================================================
# 示例用法（测试时取消注释）
# ============================================================================

# 如果直接运行此脚本，显示示例
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    echo "彩色输出库示例："
    echo ""
    
    print_header "测试报告"
    
    print_test_pass "搜狗拼音到QQ拼音 - 基本词条"
    print_test_fail "搜狗拼音到QQ拼音 - 特殊字符"
    print_test_skip "搜狗拼音到QQ拼音 - 大文件"
    
    print_subheader "统计信息"
    print_stat "总计" "10 个测试"
    print_stat "通过" "8 个" "green"
    print_stat "失败" "2 个" "red"
    
    echo ""
    print_success "示例完成！"
fi
