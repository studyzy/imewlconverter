#!/usr/bin/env bash
# 测试报告生成函数库
# 用途：生成终端测试报告和JUnit XML格式报告

set -euo pipefail

# 防止重复加载
if [[ -n "${REPORT_GENERATOR_LOADED:-}" ]]; then
    return 0
fi
REPORT_GENERATOR_LOADED=true

# 引入彩色输出库（确保只加载一次）
if [[ -z "${COLOR_OUTPUT_LOADED:-}" ]]; then
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    # shellcheck source=lib/color-output.sh
    source "${SCRIPT_DIR}/color-output.sh"
fi

# ============================================================================
# 全局变量
# ============================================================================

# 测试结果数组（全局变量，由测试运行器填充）
declare -a TEST_RESULTS=()

# 测试统计
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
SKIPPED_TESTS=0
TOTAL_DURATION=0

# ============================================================================
# 测试结果收集函数
# ============================================================================

# 函数：添加测试结果
# 参数：
#   $1 - 测试名称
#   $2 - 状态（pass/fail/skip）
#   $3 - 执行时间（秒）
#   $4 - 错误消息（可选）
add_test_result() {
    local test_name="$1"
    local status="$2"
    local duration="$3"
    local error_msg="${4:-}"
    
    # 格式: NAME|STATUS|DURATION|ERROR_MSG
    TEST_RESULTS+=("${test_name}|${status}|${duration}|${error_msg}")
    
    # 更新统计
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    TOTAL_DURATION=$(awk "BEGIN {print ${TOTAL_DURATION} + ${duration}}")
    
    case "${status}" in
        pass)
            PASSED_TESTS=$((PASSED_TESTS + 1))
            ;;
        fail)
            FAILED_TESTS=$((FAILED_TESTS + 1))
            ;;
        skip)
            SKIPPED_TESTS=$((SKIPPED_TESTS + 1))
            ;;
    esac
}

# 函数：重置测试统计
reset_test_stats() {
    TEST_RESULTS=()
    TOTAL_TESTS=0
    PASSED_TESTS=0
    FAILED_TESTS=0
    SKIPPED_TESTS=0
    TOTAL_DURATION=0
}

# ============================================================================
# 终端报告生成函数
# ============================================================================

# 函数：生成终端测试报告头部
# 参数：$1 - 报告标题（可选）
generate_report_header() {
    local title="${1:-词库转换集成测试报告}"
    local timestamp
    timestamp=$(date '+%Y-%m-%d %H:%M:%S')
    
    print_separator '='
    print_bold_blue "${title}"
    print_separator '='
    echo "开始时间: ${timestamp}"
    print_separator '='
    echo ""
}

# 函数：生成测试结果列表
generate_test_results_list() {
    if [[ ${#TEST_RESULTS[@]} -eq 0 ]]; then
        print_warning "没有测试结果"
        return
    fi
    
    local index=1
    for result in "${TEST_RESULTS[@]}"; do
        IFS='|' read -r name status duration error_msg <<< "${result}"
        
        # 格式化序号
        printf "[%d/%d] " "${index}" "${TOTAL_TESTS}"
        
        # 根据状态输出不同颜色
        case "${status}" in
            pass)
                print_test_pass "${name}"
                printf "      耗时: %.2f秒\n" "${duration}"
                printf "      状态: PASS\n"
                ;;
            fail)
                print_test_fail "${name}"
                printf "      耗时: %.2f秒\n" "${duration}"
                printf "      状态: FAIL\n"
                if [[ -n "${error_msg}" ]]; then
                    print_red "      错误: ${error_msg}"
                fi
                ;;
            skip)
                print_test_skip "${name}"
                ;;
        esac
        echo ""
        
        index=$((index + 1))
    done
}

# 函数：生成测试统计摘要
generate_test_summary() {
    print_separator '='
    print_bold_white "测试总结"
    print_separator '='
    
    # 计算通过率
    local pass_rate=0
    if [[ ${TOTAL_TESTS} -gt 0 ]]; then
        pass_rate=$(awk "BEGIN {printf \"%.1f\", (${PASSED_TESTS} / ${TOTAL_TESTS}) * 100}")
    fi
    
    # 输出统计信息
    echo "总计: ${TOTAL_TESTS} 个测试"
    
    if [[ ${PASSED_TESTS} -gt 0 ]]; then
        print_stat "通过" "${PASSED_TESTS} (${pass_rate}%)" "green"
    else
        echo "通过: 0 (0.0%)"
    fi
    
    if [[ ${FAILED_TESTS} -gt 0 ]]; then
        local fail_rate
        fail_rate=$(awk "BEGIN {printf \"%.1f\", (${FAILED_TESTS} / ${TOTAL_TESTS}) * 100}")
        print_stat "失败" "${FAILED_TESTS} (${fail_rate}%)" "red"
    else
        echo "失败: 0 (0.0%)"
    fi
    
    if [[ ${SKIPPED_TESTS} -gt 0 ]]; then
        print_stat "跳过" "${SKIPPED_TESTS}" "yellow"
    fi
    
    printf "总耗时: %.2f秒\n" "${TOTAL_DURATION}"
    
    print_separator '='
    
    # 根据结果输出最终状态
    if [[ ${FAILED_TESTS} -eq 0 ]]; then
        print_bold_green "✓ 所有测试通过！"
    else
        print_bold_red "✗ 有测试失败，请检查上述错误信息"
    fi
    
    echo ""
}

# 函数：生成完整的终端报告
# 参数：$1 - 报告标题（可选）
generate_terminal_report() {
    local title="${1:-}"
    
    generate_report_header "${title}"
    generate_test_results_list
    generate_test_summary
}

# ============================================================================
# JUnit XML报告生成函数
# ============================================================================

# 函数：转义XML特殊字符
# 参数：$1 - 要转义的文本
escape_xml() {
    local text="$1"
    text="${text//&/&amp;}"
    text="${text//</&lt;}"
    text="${text//>/&gt;}"
    text="${text//\"/&quot;}"
    text="${text//\'/&apos;}"
    echo "${text}"
}

# 函数：生成JUnit XML报告
# 参数：
#   $1 - 输出文件路径
#   $2 - 测试套件名称（可选）
generate_junit_xml_report() {
    local output_file="$1"
    local suite_name="${2:-词库转换集成测试}"
    local timestamp
    timestamp=$(date -u '+%Y-%m-%dT%H:%M:%S')
    
    # 创建输出目录
    local output_dir
    output_dir="$(dirname "${output_file}")"
    mkdir -p "${output_dir}"
    
    # 开始写入XML
    {
        echo '<?xml version="1.0" encoding="UTF-8"?>'
        echo "<testsuites>"
        echo "  <testsuite name=\"$(escape_xml "${suite_name}")\" "
        echo "             tests=\"${TOTAL_TESTS}\" "
        echo "             failures=\"${FAILED_TESTS}\" "
        echo "             errors=\"0\" "
        echo "             skipped=\"${SKIPPED_TESTS}\" "
        echo "             time=\"${TOTAL_DURATION}\" "
        echo "             timestamp=\"${timestamp}\">"
        
        # 输出每个测试用例
        for result in "${TEST_RESULTS[@]}"; do
            IFS='|' read -r name status duration error_msg <<< "${result}"
            
            echo "    <testcase name=\"$(escape_xml "${name}")\" "
            echo "              classname=\"IntegrationTest\" "
            echo "              time=\"${duration}\">"
            
            case "${status}" in
                fail)
                    echo "      <failure type=\"AssertionError\" message=\"$(escape_xml "${error_msg}")\">"
                    echo "        $(escape_xml "${error_msg}")"
                    echo "      </failure>"
                    ;;
                skip)
                    echo "      <skipped/>"
                    ;;
            esac
            
            echo "    </testcase>"
        done
        
        echo "  </testsuite>"
        echo "</testsuites>"
    } > "${output_file}"
    
    print_success "JUnit XML报告已生成: ${output_file}"
}

# 函数：生成JUnit XML报告（别名，兼容任务命名）
# 参数：
#   $1 - 输出文件路径
#   $2 - 测试套件名称（可选）
generate_junit_xml() {
    generate_junit_xml_report "$@"
}

# ============================================================================
# 失败测试详情报告
# ============================================================================

# 函数：生成失败测试详情报告
generate_failure_details() {
    if [[ ${FAILED_TESTS} -eq 0 ]]; then
        return
    fi
    
    print_subheader "失败测试详情"
    
    for result in "${TEST_RESULTS[@]}"; do
        IFS='|' read -r name status duration error_msg <<< "${result}"
        
        if [[ "${status}" == "fail" ]]; then
            print_bold_red "✗ ${name}"
            echo "  错误信息: ${error_msg}"
            echo ""
        fi
    done
}

# ============================================================================
# 性能统计报告
# ============================================================================

# 函数：生成性能统计（最慢的测试）
# 参数：$1 - 显示数量（默认5）
generate_performance_stats() {
    local show_count="${1:-5}"
    
    if [[ ${#TEST_RESULTS[@]} -eq 0 ]]; then
        return
    fi
    
    print_subheader "性能统计（最慢的 ${show_count} 个测试）"
    
    # 按执行时间排序（使用临时数组）
    local -a sorted_results=()
    for result in "${TEST_RESULTS[@]}"; do
        sorted_results+=("${result}")
    done
    
    # 简单排序（冒泡排序，适用于小数据集）
    local n=${#sorted_results[@]}
    for ((i=0; i<n; i++)); do
        for ((j=0; j<n-i-1; j++)); do
            IFS='|' read -r _ _ duration1 _ <<< "${sorted_results[j]}"
            IFS='|' read -r _ _ duration2 _ <<< "${sorted_results[j+1]}"
            
            if (( $(awk "BEGIN {print (${duration1} < ${duration2})}") )); then
                # 交换
                local temp="${sorted_results[j]}"
                sorted_results[j]="${sorted_results[j+1]}"
                sorted_results[j+1]="${temp}"
            fi
        done
    done
    
    # 显示前N个最慢的测试
    local count=0
    for result in "${sorted_results[@]}"; do
        if [[ ${count} -ge ${show_count} ]]; then
            break
        fi
        
        IFS='|' read -r name status duration _ <<< "${result}"
        printf "%d. %s: %.2f秒\n" "$((count + 1))" "${name}" "${duration}"
        
        count=$((count + 1))
    done
    echo ""
}

# ============================================================================
# 示例用法（测试时取消注释）
# ============================================================================

# 如果直接运行此脚本，显示示例
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    echo "报告生成库示例："
    echo ""
    
    # 模拟添加测试结果
    add_test_result "搜狗拼音到QQ拼音 - 基本词条" "pass" "2.3" ""
    add_test_result "搜狗拼音到QQ拼音 - 特殊字符" "fail" "1.8" "输出文件内容不匹配"
    add_test_result "搜狗拼音到QQ拼音 - 大文件" "skip" "0" ""
    add_test_result "QQ拼音到搜狗拼音 - 基本词条" "pass" "3.1" ""
    
    # 生成终端报告
    generate_terminal_report "示例测试报告"
    
    # 生成失败详情
    generate_failure_details
    
    # 生成性能统计
    generate_performance_stats 3
    
    # 生成JUnit XML（可选）
    # generate_junit_xml_report "/tmp/test-results.xml" "示例测试套件"
fi