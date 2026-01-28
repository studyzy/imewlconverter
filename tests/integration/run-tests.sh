#!/usr/bin/env bash
# 词库转换集成测试运行器
# 用途：执行词库转换集成测试，生成测试报告
# 使用方法：./run-tests.sh [选项]

set -euo pipefail

# ============================================================================
# 脚本目录和路径设置
# ============================================================================

# 获取脚本所在目录的绝对路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# CI环境检测（在加载颜色输出前处理）
CI_MODE=false
if [[ "${CI:-}" == "true" ]] || [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    CI_MODE=true
    if [[ "${FORCE_COLOR:-}" != "1" ]]; then
        export NO_COLOR=1
    fi
fi

# 引入辅助函数库
# shellcheck source=lib/test-helpers.sh
source "${SCRIPT_DIR}/lib/test-helpers.sh"

# shellcheck source=lib/color-output.sh
source "${SCRIPT_DIR}/lib/color-output.sh"

# shellcheck source=lib/report-generator.sh
source "${SCRIPT_DIR}/lib/report-generator.sh"

# shellcheck source=lib/yaml-parser.sh
source "${SCRIPT_DIR}/lib/yaml-parser.sh"

# ============================================================================
# 全局变量
# ============================================================================

# 命令行参数
SUITE_NAME=""           # 指定测试套件名称
TAG_FILTER=""           # 标签过滤器
VERBOSE=false           # 详细输出模式
KEEP_OUTPUT=false       # 保留测试输出文件
GENERATE_XML=false      # 生成JUnit XML报告
RUN_ALL=false           # 运行所有测试套件

# 测试配置
TEST_CASES_DIR="${SCRIPT_DIR}/test-cases"
TEST_OUTPUT_DIR="${SCRIPT_DIR}/test-output"
REPORTS_DIR="${SCRIPT_DIR}/reports"

# ============================================================================
# 帮助信息
# ============================================================================

show_help() {
    cat << EOF
词库转换集成测试运行器 v1.0

用法: $(basename "$0") [选项]

选项:
  -h, --help          显示帮助信息
  -s, --suite <name>  运行指定测试套件（如: sougou-pinyin）
  -t, --tag <tag>     按标签过滤测试（如: basic）
  -v, --verbose       显示详细输出
  --keep-output       保留测试输出文件（用于调试）
  --xml               生成JUnit XML报告
  --all               运行所有测试套件

示例:
  $(basename "$0")                    # 显示帮助信息
  $(basename "$0") --all              # 运行所有测试
  $(basename "$0") -s sougou-pinyin   # 只运行搜狗拼音测试
  $(basename "$0") -t basic           # 只运行basic标签的测试
  $(basename "$0") -v --xml           # 详细模式 + 生成XML报告

EOF
}

# ============================================================================
# 命令行参数解析
# ============================================================================

parse_arguments() {
    # 如果没有参数，显示帮助
    if [[ $# -eq 0 ]]; then
        show_help
        exit 0
    fi
    
    while [[ $# -gt 0 ]]; do
        case "$1" in
            -h|--help)
                show_help
                exit 0
                ;;
            -s|--suite)
                if [[ -n "${2:-}" ]]; then
                    SUITE_NAME="$2"
                    shift 2
                else
                    print_error "选项 -s/--suite 需要参数"
                    exit 1
                fi
                ;;
            -t|--tag)
                if [[ -n "${2:-}" ]]; then
                    TAG_FILTER="$2"
                    shift 2
                else
                    print_error "选项 -t/--tag 需要参数"
                    exit 1
                fi
                ;;
            -v|--verbose)
                VERBOSE=true
                shift
                ;;
            --keep-output)
                KEEP_OUTPUT=true
                shift
                ;;
            --xml)
                GENERATE_XML=true
                shift
                ;;
            --all)
                RUN_ALL=true
                shift
                ;;
            *)
                print_error "未知选项: $1"
                show_help
                exit 1
                ;;
        esac
    done
}

# ============================================================================
# 测试套件发现
# ============================================================================

# 函数：发现所有可用的测试套件
# 返回：测试套件目录列表
discover_test_suites() {
    if [[ ! -d "${TEST_CASES_DIR}" ]]; then
        print_error "测试用例目录不存在: ${TEST_CASES_DIR}"
        return 1
    fi
    
    # 查找包含test-config.yaml的目录
    find "${TEST_CASES_DIR}" -mindepth 1 -maxdepth 1 -type d | while read -r suite_dir; do
        if [[ -f "${suite_dir}/test-config.yaml" ]]; then
            basename "${suite_dir}"
        fi
    done
}

# ============================================================================
# 单个测试用例执行
# ============================================================================

# 函数：执行单个测试用例
# 参数：$1 - 测试用例字符串（由yaml-parser生成）
#      $2 - 测试套件目录路径（未使用，保留用于兼容性）
# 返回：0=通过, 1=失败
execute_test_case() {
    local test_case_str="$1"
    # suite_dir参数保留但不使用，因为yaml-parser已经返回绝对路径
    
    # 解析测试用例信息
    IFS='|' read -r test_name description enabled timeout input_file input_format output_format expected_file tags extra_args <<< "${test_case_str}"
    
    # 检查是否启用
    if [[ "${enabled}" == "false" ]]; then
        print_debug "跳过禁用的测试: ${test_name}"
        add_test_result "${test_name}" "skip" "0" ""
        return 0
    fi
    
    # 检查标签过滤
    if [[ -n "${TAG_FILTER}" ]]; then
        if [[ ! "${tags}" =~ ${TAG_FILTER} ]]; then
            print_debug "跳过不匹配标签的测试: ${test_name}"
            return 0
        fi
    fi
    
    # input_file和expected_file已经是完整路径（由yaml-parser处理）
    local full_input_file="${input_file}"
    local full_expected_file="${expected_file}"
    
    # 输出测试开始信息
    if [[ "${VERBOSE}" == "true" ]]; then
        print_test_running "${test_name}"
        print_debug "  输入文件: ${full_input_file}"
        print_debug "  源格式: ${input_format}"
        print_debug "  目标格式: ${output_format}"
        if [[ -n "${full_expected_file}" ]]; then
            print_debug "  预期输出: ${full_expected_file}"
        fi
    fi
    
    # 检查输入文件是否存在
    if [[ ! -f "${full_input_file}" ]]; then
        print_test_fail "${test_name}"
        if [[ "${VERBOSE}" == "true" ]]; then
            print_error "输入文件不存在: ${full_input_file}"
        fi
        add_test_result "${test_name}" "fail" "0" "输入文件不存在"
        return 1
    fi
    
    # 检查预期输出文件是否存在（如果不为空）
    if [[ -n "${expected_file}" ]] && [[ ! -f "${full_expected_file}" ]]; then
        print_test_fail "${test_name}"
        if [[ "${VERBOSE}" == "true" ]]; then
            print_error "预期输出文件不存在: ${full_expected_file}"
        fi
        add_test_result "${test_name}" "fail" "0" "预期输出文件不存在"
        return 1
    fi
    
    # 记录开始时间
    local start_time
    start_time=$(date +%s.%N)
    
    # 生成输出文件路径
    local output_basename
    output_basename=$(basename "${input_file}")
    output_basename="${output_basename%.*}"  # 移除扩展名
    local output_file="${TEST_OUTPUT_DIR}/${output_basename}-to-${output_format}.txt"
    
    # 确保输出目录存在
    ensure_dir "${TEST_OUTPUT_DIR}"
    
    # 执行转换
    local convert_output
    local convert_exitcode=0
    
    # 获取格式选项（从环境变量或默认为空）
    local format_opts="${FORMAT_OPTIONS:-}"
    
    # 获取额外参数（从测试用例配置或默认为空）
    local converter_extra_args="${extra_args:-}"
    
    # 如果 extra_args 包含 -f: 参数，则不使用全局的 format_opts
    if [[ "${converter_extra_args}" == *"-f:"* ]]; then
        format_opts=""
    fi
    
    # 声明额外参数数组（始终存在，可能为空）
    declare -a extra_args_array=()
    # 如果有额外参数，使用竖线分割为独立参数
    if [[ -n "${converter_extra_args}" ]]; then
        IFS='|' read -ra extra_args_array <<< "${converter_extra_args}"
    fi
    
    # 构建命令字符串用于显示（不通过 run_converter，避免被捕获）
    local cmd_display=""
    cmd_display="dotnet ${CONVERTER_CLI} -i:${input_format} ${full_input_file}"
    if [[ ${#extra_args_array[@]} -gt 0 ]]; then
        cmd_display+=" ${extra_args_array[*]}"
    fi
    cmd_display+=" -o:${output_format} ${output_file}"
    if [[ -n "${format_opts}" ]]; then
        cmd_display+=" -f:${format_opts}"
    fi
    echo "[DEBUG] Executing: ${cmd_display}" >&2
    
    # 根据是否有额外参数选择不同的调用方式
    if [[ ${#extra_args_array[@]} -gt 0 ]]; then
        # 有额外参数，传递它们
        if convert_output=$(run_converter "${full_input_file}" "${input_format}" "${output_format}" "${output_file}" "${timeout}" "${format_opts}" "${extra_args_array[@]}" 2>&1); then
            convert_exitcode=0
        else
            convert_exitcode=$?
        fi
    else
        # 没有额外参数，不传递
        if convert_output=$(run_converter "${full_input_file}" "${input_format}" "${output_format}" "${output_file}" "${timeout}" "${format_opts}" 2>&1); then
            convert_exitcode=0
        else
            convert_exitcode=$?
        fi
    fi
    
    # 计算执行时间
    local end_time
    end_time=$(date +%s.%N)
    local duration
    duration=$(awk "BEGIN {printf \"%.2f\", ${end_time} - ${start_time}}")
    
    # 检查转换是否成功
    if [[ ${convert_exitcode} -ne 0 ]]; then
        print_test_fail "${test_name}"
        # 总是显示详细的错误信息，无论是否在verbose模式
        print_error "转换失败，退出码: ${convert_exitcode}"
        echo "执行的命令: ${cmd_display}"
        echo "转换器输出:"
        echo "${convert_output}"
        add_test_result "${test_name}" "fail" "${duration}" "转换失败: ${convert_output}"
        return 1
    fi
    
    # 如果没有预期输出文件，只检查转换是否成功
    if [[ -z "${expected_file}" ]]; then
        print_test_pass "${test_name}"
        add_test_result "${test_name}" "pass" "${duration}" ""
        
        # 清理输出文件（除非指定保留）
        if [[ "${KEEP_OUTPUT}" == "false" ]]; then
            rm -f "${output_file}"
        fi
        
        return 0
    fi
    
    # 比较输出文件（特殊处理微软拼音格式）
    local compare_result=0
    
    # 如果是微软拼音格式，使用特殊比较函数
    if [[ "${output_format}" == "mspy" ]]; then
        if compare_mspy_files "${full_expected_file}" "${output_file}"; then
            compare_result=0
        else
            compare_result=1
        fi
    else
        # 其他格式使用标准比较
        if compare_files "${full_expected_file}" "${output_file}"; then
            compare_result=0
        else
            compare_result=1
        fi
    fi
    
    if [[ ${compare_result} -eq 0 ]]; then
        print_test_pass "${test_name}"
        add_test_result "${test_name}" "pass" "${duration}" ""
        
        # 清理输出文件（除非指定保留）
        if [[ "${KEEP_OUTPUT}" == "false" ]]; then
            rm -f "${output_file}"
        fi
        
        return 0
    else
        print_test_fail "${test_name}"
        if [[ "${VERBOSE}" == "true" ]]; then
            generate_diff_report "${full_expected_file}" "${output_file}"
        fi
        add_test_result "${test_name}" "fail" "${duration}" "输出文件不匹配"
        return 1
    fi
}

# ============================================================================
# 测试套件执行
# ============================================================================

# 函数：执行测试套件
# 参数：$1 - 测试套件名称（目录名）
# 返回：0=所有测试通过, 1=有测试失败
execute_test_suite() {
    local suite_name="$1"
    local suite_dir="${TEST_CASES_DIR}/${suite_name}"
    local config_file="${suite_dir}/test-config.yaml"
    
    # 检查配置文件是否存在
    if [[ ! -f "${config_file}" ]]; then
        print_error "测试配置文件不存在: ${config_file}"
        return 1
    fi
    
    # 验证配置文件格式
    if ! validate_yaml_config "${config_file}"; then
        print_error "配置文件格式无效: ${config_file}"
        return 1
    fi
    
    # 解析套件信息
    print_debug "正在加载测试套件: ${suite_name}"
    
    # 读取全局格式选项（如果有）
    local format_options
    format_options=$(get_yaml_value "${config_file}" "format_options" || echo "")
    
    # 提取测试用例
    local test_cases
    test_cases=$(extract_test_cases_simple "${config_file}")
    
    if [[ -z "${test_cases}" ]]; then
        print_warning "测试套件中没有测试用例: ${suite_name}"
        return 0
    fi
    
    # 执行每个测试用例
    local test_case_count=0
    local failed_count=0
    
    while IFS= read -r test_case; do
        test_case_count=$((test_case_count + 1))
        
        # 将format_options作为环境变量传递
        FORMAT_OPTIONS="${format_options}" execute_test_case "${test_case}" "${suite_dir}"
        
        if [[ $? -ne 0 ]]; then
            failed_count=$((failed_count + 1))
        fi
    done <<< "${test_cases}"
    
    # 返回执行结果
    if [[ ${failed_count} -eq 0 ]]; then
        return 0
    else
        return 1
    fi
}

# ============================================================================
# 主执行流程
# ============================================================================

# 函数：GitHub Actions日志分组
# 参数：$1 - 分组标题
start_github_group() {
    local title="$1"
    if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
        echo "::group::${title}"
    fi
}

# 函数：结束GitHub Actions日志分组
end_github_group() {
    if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
        echo "::endgroup::"
    fi
}

main() {
    # 解析命令行参数
    parse_arguments "$@"

    # CI环境默认参数
    if [[ "${CI_MODE}" == "true" ]]; then
        VERBOSE=true
        GENERATE_XML=true
    fi
    
    # 重置测试统计
    reset_test_stats
    
    # 生成报告标题
    local report_title="词库转换集成测试报告"
    
    # 确定要运行的测试套件
    local suites_to_run=()
    
    if [[ "${RUN_ALL}" == "true" ]]; then
        # 运行所有测试套件
        print_info "正在发现所有测试套件..."
        
        # 使用兼容的方式替代mapfile（支持旧版本Bash）
        while IFS= read -r suite; do
            suites_to_run+=("${suite}")
        done < <(discover_test_suites)
        
        if [[ ${#suites_to_run[@]} -eq 0 ]]; then
            print_error "没有找到测试套件"
            exit 1
        fi
        
        print_info "找到 ${#suites_to_run[@]} 个测试套件"
    elif [[ -n "${SUITE_NAME}" ]]; then
        # 运行指定的测试套件
        suites_to_run=("${SUITE_NAME}")
        report_title="测试套件: ${SUITE_NAME}"
    else
        print_error "请指定测试套件（-s）或使用 --all 运行所有测试"
        show_help
        exit 1
    fi
    
    # 生成报告头部
    generate_report_header "${report_title}"
    
    # 执行所有测试套件
    local suite_failed_count=0
    
    for suite in "${suites_to_run[@]}"; do
        if [[ ${#suites_to_run[@]} -gt 1 ]]; then
            print_subheader "测试套件: ${suite}"
        fi

        start_github_group "测试套件: ${suite}"
        if ! execute_test_suite "${suite}"; then
            suite_failed_count=$((suite_failed_count + 1))
        fi
        end_github_group
    done
    
    # 生成测试摘要
    generate_test_summary
    
    # 生成失败详情（如果有失败）
    if [[ ${FAILED_TESTS} -gt 0 ]] && [[ "${VERBOSE}" == "true" ]]; then
        generate_failure_details
    fi
    
    # 生成JUnit XML报告（如果需要）
    if [[ "${GENERATE_XML}" == "true" ]]; then
        local xml_file="${REPORTS_DIR}/test-results.xml"
        generate_junit_xml_report "${xml_file}" "${report_title}"

        if command -v xmllint >/dev/null 2>&1; then
            if ! xmllint --noout "${xml_file}"; then
                print_error "JUnit XML格式验证失败: ${xml_file}"
                exit 1
            fi
        else
            print_warning "未找到xmllint，跳过XML格式验证"
        fi
    fi
    
    # 清理临时输出目录（如果不保留）
    if [[ "${KEEP_OUTPUT}" == "false" ]] && [[ -d "${TEST_OUTPUT_DIR}" ]]; then
        rm -rf "${TEST_OUTPUT_DIR}"
    fi
    
    # 根据测试结果返回退出码
    if [[ ${FAILED_TESTS} -eq 0 ]]; then
        exit 0
    else
        exit 1
    fi
}

# ============================================================================
# 脚本入口
# ============================================================================

# 只有直接运行此脚本时才执行main函数
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi