#!/usr/bin/env bash
# YAML配置文件解析函数库
# 用途：解析test-config.yaml配置文件
# 注意：这是一个简单的YAML解析器，只支持测试配置所需的基本结构

set -euo pipefail

# 防止重复加载
if [[ -n "${YAML_PARSER_LOADED:-}" ]]; then
    return 0
fi
YAML_PARSER_LOADED=true

# ============================================================================
# YAML解析辅助函数
# ============================================================================

# 函数：去除字符串首尾空白
# 参数：$1 - 字符串
trim() {
    local text="$1"
    # 去除前导空白
    text="${text#"${text%%[![:space:]]*}"}"
    # 去除尾随空白
    text="${text%"${text##*[![:space:]]}"}"
    echo "${text}"
}

# 函数：从YAML中提取简单的键值对
# 参数：
#   $1 - YAML文件路径
#   $2 - 键名（支持点号分隔的路径，如 "input.file"）
# 返回：键对应的值
get_yaml_value() {
    local yaml_file="$1"
    local key="$2"
    
    if [[ ! -f "${yaml_file}" ]]; then
        echo "错误: YAML文件不存在: ${yaml_file}" >&2
        return 1
    fi
    
    # 简单的键值提取（适用于单层结构）
    local value
    value=$(grep "^${key}:" "${yaml_file}" | sed "s/^${key}:[[:space:]]*//" | sed 's/^["'\'']//' | sed 's/["'\'']$//')
    
    if [[ -n "${value}" ]]; then
        echo "${value}"
    fi
}

# ============================================================================
# 测试套件配置解析
# ============================================================================

# 函数：解析测试套件基本信息
# 参数：$1 - test-config.yaml文件路径
# 输出：套件名称、描述、维护者等信息（每行一个）
parse_suite_info() {
    local yaml_file="$1"
    
    if [[ ! -f "${yaml_file}" ]]; then
        echo "错误: 配置文件不存在: ${yaml_file}" >&2
        return 1
    fi
    
    # 提取套件信息
    local suite_name
    local description
    local maintainer
    local format_options
    
    suite_name=$(get_yaml_value "${yaml_file}" "suite_name" || echo "未命名测试套件")
    description=$(get_yaml_value "${yaml_file}" "description" || echo "")
    maintainer=$(get_yaml_value "${yaml_file}" "maintainer" || echo "")
    format_options=$(get_yaml_value "${yaml_file}" "format_options" || echo "")
    
    echo "suite_name=${suite_name}"
    echo "description=${description}"
    echo "maintainer=${maintainer}"
    echo "format_options=${format_options}"
}

# ============================================================================
# 测试用例解析（高级功能）
# ============================================================================

# 函数：获取测试用例数量
# 参数：$1 - test-config.yaml文件路径
# 返回：测试用例数量
count_test_cases() {
    local yaml_file="$1"
    
    if [[ ! -f "${yaml_file}" ]]; then
        echo "0"
        return 1
    fi
    
    # 计算包含 "- name:" 的行数（每个测试用例都有name字段）
    grep -c "^  - name:" "${yaml_file}" 2>/dev/null || echo "0"
}

# 函数：提取所有测试用例名称
# 参数：$1 - test-config.yaml文件路径
# 输出：每行一个测试用例名称
extract_test_case_names() {
    local yaml_file="$1"
    
    if [[ ! -f "${yaml_file}" ]]; then
        return 1
    fi
    
    # 提取所有测试用例名称
    grep "^  - name:" "${yaml_file}" | sed 's/^  - name:[[:space:]]*//' | sed 's/^["'\'']//' | sed 's/["'\'']$//'
}

# 函数：提取测试用例的标签
# 参数：
#   $1 - test-config.yaml文件路径
#   $2 - 测试用例名称
# 输出：标签列表（每行一个）
extract_test_case_tags() {
    local yaml_file="$1"
    local test_name="$2"
    
    if [[ ! -f "${yaml_file}" ]]; then
        return 1
    fi
    
    # 这是一个简化版本，实际需要更复杂的解析
    # 暂时返回空，后续可以增强
    return 0
}

# ============================================================================
# 测试用例详细信息提取
# ============================================================================

# 函数：检查测试用例是否启用
# 参数：
#   $1 - test-config.yaml文件路径
#   $2 - 测试用例索引（从0开始）
# 返回：0=启用, 1=禁用
is_test_case_enabled() {
    local yaml_file="$1"
    local test_index="$2"
    
    # 简单实现：假设所有测试都启用
    # 实际需要解析enabled字段
    return 0
}

# ============================================================================
# YAML配置验证
# ============================================================================

# 函数：验证YAML文件格式
# 参数：$1 - YAML文件路径
# 返回：0=有效, 1=无效
validate_yaml_config() {
    local yaml_file="$1"
    
    if [[ ! -f "${yaml_file}" ]]; then
        echo "错误: 配置文件不存在: ${yaml_file}" >&2
        return 1
    fi
    
    # 基本验证：检查必需字段
    local suite_name
    suite_name=$(get_yaml_value "${yaml_file}" "suite_name")
    
    if [[ -z "${suite_name}" ]]; then
        echo "错误: 缺少必需字段 'suite_name'" >&2
        return 1
    fi
    
    # 检查是否有test_cases部分
    if ! grep -q "^test_cases:" "${yaml_file}"; then
        echo "错误: 缺少 'test_cases' 部分" >&2
        return 1
    fi
    
    return 0
}

# ============================================================================
# 简化的测试用例数据结构
# ============================================================================

# 由于Bash不支持复杂的数据结构，我们使用简化的方法：
# 每个测试用例信息存储为一个字符串，格式如下：
# NAME|DESCRIPTION|ENABLED|TIMEOUT|INPUT_FILE|INPUT_FORMAT|OUTPUT_FORMAT|EXPECTED_FILE|TAGS|EXTRA_ARGS

# 函数：从YAML文件中提取测试用例列表（简化版）
# 参数：$1 - test-config.yaml文件路径
# 输出：每行一个测试用例信息字符串
# 注意：这是一个简化实现，适用于标准格式的YAML配置
extract_test_cases_simple() {
    local yaml_file="$1"
    local config_dir
    config_dir="$(cd "$(dirname "${yaml_file}")" && pwd)"
    
    if [[ ! -f "${yaml_file}" ]]; then
        echo "错误: 配置文件不存在: ${yaml_file}" >&2
        return 1
    fi
    
    # 使用awk进行简单的YAML解析
    # 这个实现假设YAML格式规范，适用于我们的test-config.yaml
    awk -v config_dir="${config_dir}" '
    BEGIN {
        in_test_case = 0
        in_extra_args = 0
        name = ""
        description = ""
        enabled = "true"
        timeout = "30"
        input_file = ""
        input_format = ""
        output_format = ""
        expected_file = ""
        tags = ""
        extra_args = ""
    }
    
    /^- name:/ || /^  - name:/ {
        # 如果有前一个测试用例，先输出
        if (name != "") {
            print name "|" description "|" enabled "|" timeout "|" input_file "|" input_format "|" output_format "|" expected_file "|" tags "|" extra_args
        }
        # 开始新的测试用例
        in_test_case = 1
        in_extra_args = 0
        name = $0
        sub(/^[- ]*name:[[:space:]]*/, "", name)
        gsub(/^["'\''"]/, "", name)
        gsub(/["'\''"]$/, "", name)
        description = ""
        enabled = "true"
        timeout = "30"
        input_file = ""
        input_format = ""
        output_format = ""
        expected_file = ""
        tags = ""
        extra_args = ""
        next
    }
    
    in_test_case && /^  description:/ {
        description = $0
        sub(/^  description:[[:space:]]*/, "", description)
        gsub(/^["'\''"]/, "", description)
        gsub(/["'\''"]$/, "", description)
        next
    }

    in_test_case && /^  enabled:/ {
        enabled = $0
        sub(/^  enabled:[[:space:]]*/, "", enabled)
        # 移除行尾注释
        sub(/[[:space:]]*#.*$/, "", enabled)
        # 移除尾部空格
        sub(/[[:space:]]*$/, "", enabled)
        next
    }

    in_test_case && /^  timeout:/ {
        timeout = $0
        sub(/^  timeout:[[:space:]]*/, "", timeout)
        # 移除行尾注释
        sub(/[[:space:]]*#.*$/, "", timeout)
        # 移除尾部空格
        sub(/[[:space:]]*$/, "", timeout)
        next
    }

    in_test_case && /^    file:/ {
        input_file = $0
        sub(/^    file:[[:space:]]*/, "", input_file)
        gsub(/^["'\''"]/, "", input_file)
        gsub(/["'\''"]$/, "", input_file)
        # 保留原始相对路径，在后面用shell处理
        next
    }

    in_test_case && /^    format:/ && input_format == "" {
        input_format = $0
        sub(/^    format:[[:space:]]*/, "", input_format)
        # 移除行尾注释（# 开头的部分）
        sub(/[[:space:]]*#.*$/, "", input_format)
        # 移除尾部空格
        sub(/[[:space:]]*$/, "", input_format)
        # 移除引号
        gsub(/^["'\'']+/, "", input_format)
        gsub(/["'\'']+$/, "", input_format)
        next
    }

    in_test_case && /^    format:/ && input_format != "" {
        output_format = $0
        sub(/^    format:[[:space:]]*/, "", output_format)
        # 移除行尾注释（# 开头的部分）
        sub(/[[:space:]]*#.*$/, "", output_format)
        # 移除尾部空格
        sub(/[[:space:]]*$/, "", output_format)
        # 移除引号
        gsub(/^["'\'']+/, "", output_format)
        gsub(/["'\'']+$/, "", output_format)
        next
    }

    in_test_case && /^    expected:/ {
        expected_file = $0
        sub(/^    expected:[[:space:]]*/, "", expected_file)
        gsub(/^["'\''"]/, "", expected_file)
        gsub(/["'\''"]$/, "", expected_file)
        # 保留原始相对路径
        next
    }
    
    in_test_case && /^  tags:/ {
        # 标签在下一行
        getline
        while ($0 ~ /^    -/) {
            tag = $0
            sub(/^    - /, "", tag)
            gsub(/^["'\''"]/, "", tag)
            gsub(/["'\''"]$/, "", tag)
            if (tags == "") {
                tags = tag
            } else {
                tags = tags "," tag
            }
            getline
        }
    }

    in_test_case && /^  extra_args:/ {
        in_extra_args = 1
        next
    }

    in_extra_args && /^    - / || in_extra_args && /^  - / {
        arg = $0
        sub(/^    - /, "", arg)
        sub(/^  - /, "", arg)
        # 注意：不要移除引号，因为参数中的空格需要保留
        # gsub(/^["'\''"]/, "", arg)
        # gsub(/["'\''"]$/, "", arg)
        if (extra_args == "") {
            extra_args = arg
        } else {
            # 使用竖线 | 作为分隔符，而不是空格
            extra_args = extra_args "|" arg
        }
        next
    }
    
    # 如果遇到新的根级别配置项，退出extra_args解析
    in_extra_args && /^    [a-z]/ && !/^      / {
        in_extra_args = 0
    }
    
    END {
        # 输出最后一个测试用例
        if (name != "") {
            print name "|" description "|" enabled "|" timeout "|" input_file "|" input_format "|" output_format "|" expected_file "|" tags "|" extra_args
        }
    }
    ' "${yaml_file}" | while IFS='|' read -r name description enabled timeout input_file input_format output_format expected_file tags extra_args; do
        # 规范化路径（从config_dir开始解析相对路径）
        if [[ -n "${input_file}" ]] && [[ "${input_file}" != /* ]]; then
            # 相对路径，拼接后规范化
            input_file=$(python3 -c "import os; print(os.path.normpath('${config_dir}/${input_file}'))" 2>/dev/null || echo "${config_dir}/${input_file}")
        fi
        if [[ -n "${expected_file}" ]] && [[ "${expected_file}" != /* ]]; then
            expected_file=$(python3 -c "import os; print(os.path.normpath('${config_dir}/${expected_file}'))" 2>/dev/null || echo "${config_dir}/${expected_file}")
        fi
        
        # 对 extra_args 中的相对路径进行规范化（针对 -c: 参数）
        if [[ -n "${extra_args}" ]]; then
            # 注意：-c: 和 -mc: 参数的路径应该保持原样，不进行路径转换
            # 因为这些路径是相对于项目根目录的，不是相对于配置文件目录的
            :  # 空命令，保留 extra_args 不变
        fi
        
        echo "${name}|${description}|${enabled}|${timeout}|${input_file}|${input_format}|${output_format}|${expected_file}|${tags}|${extra_args}"
    done
}

# ============================================================================
# 测试用例数据解析
# ============================================================================

# 函数：解析测试用例字符串
# 参数：$1 - 测试用例字符串（由extract_test_cases_simple生成）
# 输出：各个字段的值（每行一个字段）
parse_test_case_string() {
    local test_case_str="$1"
    
    IFS='|' read -r name description enabled timeout input_file input_format output_format expected_file tags <<< "${test_case_str}"
    
    echo "name=${name}"
    echo "description=${description}"
    echo "enabled=${enabled}"
    echo "timeout=${timeout}"
    echo "input_file=${input_file}"
    echo "input_format=${input_format}"
    echo "output_format=${output_format}"
    echo "expected_file=${expected_file}"
    echo "tags=${tags}"
}

# ============================================================================
# 示例用法（测试时取消注释）
# ============================================================================

# 如果直接运行此脚本，显示示例
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    echo "YAML解析库示例："
    echo ""
    
    # 假设有一个示例配置文件
    # echo "请提供test-config.yaml文件路径进行测试"
fi
