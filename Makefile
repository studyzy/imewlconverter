# ============================================================================
# IME WL Converter - Professional Makefile
# ============================================================================
# A cross-platform dictionary converter for Input Method Editors
# ============================================================================

# ============================================================================
# Configuration Variables
# ============================================================================

# .NET Configuration
DOTNET := dotnet
DOTNET_CONFIG ?= Debug
DOTNET_RUNTIME_OSX_ARM64 := osx-arm64
DOTNET_RUNTIME_OSX_X64 := osx-x64
DOTNET_FRAMEWORK := net10.0

# Project Paths
SRC_DIR := src
CORE_PROJECT := $(SRC_DIR)/ImeWlConverterCore/ImeWlConverterCore.csproj
CMD_PROJECT := $(SRC_DIR)/ImeWlConverterCmd/ImeWlConverterCmd.csproj
MAC_PROJECT := $(SRC_DIR)/ImeWlConverterMac/ImeWlConverterMac.csproj
TEST_PROJECT := $(SRC_DIR)/ImeWlConverterCoreTest/ImeWlConverterCoreTest.csproj

# Output Directories
BUILD_DIR := build
PUBLISH_DIR := publish
DIST_DIR := dist
MAC_ARM64_DIR := $(PUBLISH_DIR)/mac-arm64
MAC_X64_DIR := $(PUBLISH_DIR)/mac-x64

# Application Info
APP_NAME := 深蓝词库转换
APP_NAME_ARM64 := $(APP_NAME)-arm64
APP_NAME_X64 := $(APP_NAME)-x64
APP_BUNDLE_ARM64 := $(APP_NAME_ARM64).app
APP_BUNDLE_X64 := $(APP_NAME_X64).app
APP_ARCHIVE_ARM64 := $(APP_NAME_ARM64).zip
APP_ARCHIVE_X64 := $(APP_NAME_X64).zip

# Scripts
SCRIPT_DIR := scripts
APP_BUNDLE_SCRIPT := $(SCRIPT_DIR)/create-app-bundle.sh

# Build Options
PUBLISH_OPTS := --configuration $(DOTNET_CONFIG) --self-contained true
BUILD_OPTS := --configuration $(DOTNET_CONFIG)
TEST_OPTS := --configuration $(DOTNET_CONFIG) --logger "console;verbosity=normal" --settings test.runsettings --blame-hang --blame-hang-timeout 2m

# Colors for terminal output (ANSI escape codes)
COLOR_RESET := \033[0m
COLOR_BOLD := \033[1m
COLOR_RED := \033[31m
COLOR_GREEN := \033[32m
COLOR_YELLOW := \033[33m
COLOR_BLUE := \033[34m
COLOR_MAGENTA := \033[35m
COLOR_CYAN := \033[36m

# Emojis for better UX
EMOJI_ROCKET := 🚀
EMOJI_CHECK := ✅
EMOJI_CROSS := ❌
EMOJI_BUILD := 🔨
EMOJI_TEST := 🧪
EMOJI_CLEAN := 🧹
EMOJI_PACKAGE := 📦
EMOJI_INFO := ℹ️
EMOJI_WARNING := ⚠️

# ============================================================================
# Phony Targets (not actual files)
# ============================================================================

.PHONY: all help clean clean-all \
        restore build build-all build-cmd build-mac build-release \
        test test-verbose test-coverage \
        run-cmd run-mac \
        publish publish-mac publish-mac-arm64 publish-mac-x64 \
        app-mac app-mac-arm64 app-mac-x64 \
        package package-mac package-all \
        install-mac uninstall-mac \
        version info check-deps \
        format lint

# ============================================================================
# Default Target
# ============================================================================

.DEFAULT_GOAL := help

# ============================================================================
# Help System (Auto-generated from comments)
# ============================================================================

## help: Display this help message
help:
	@echo "$(COLOR_BOLD)$(COLOR_CYAN)════════════════════════════════════════════════════════════════$(COLOR_RESET)"
	@echo "$(COLOR_BOLD)$(COLOR_CYAN)  $(EMOJI_ROCKET) IME WL Converter - Makefile Commands$(COLOR_RESET)"
	@echo "$(COLOR_BOLD)$(COLOR_CYAN)════════════════════════════════════════════════════════════════$(COLOR_RESET)"
	@echo ""
	@echo "$(COLOR_BOLD)Build Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make restore$(COLOR_RESET)              - Restore NuGet packages"
	@echo "  $(COLOR_GREEN)make build$(COLOR_RESET)                - Build all projects (Debug)"
	@echo "  $(COLOR_GREEN)make build-cmd$(COLOR_RESET)            - Build command-line version"
	@echo "  $(COLOR_GREEN)make build-mac$(COLOR_RESET)            - Build macOS GUI version"
	@echo "  $(COLOR_GREEN)make build-release$(COLOR_RESET)        - Build all projects (Release)"
	@echo ""
	@echo "$(COLOR_BOLD)Test Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make test$(COLOR_RESET)                 - Run all unit tests"
	@echo "  $(COLOR_GREEN)make test-verbose$(COLOR_RESET)         - Run tests with detailed output"
	@echo "  $(COLOR_GREEN)make test-coverage$(COLOR_RESET)        - Run tests with coverage report"
	@echo ""
	@echo "$(COLOR_BOLD)Run Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make run-cmd$(COLOR_RESET)              - Run command-line version"
	@echo "  $(COLOR_GREEN)make run-mac$(COLOR_RESET)              - Run macOS GUI version"
	@echo ""
	@echo "$(COLOR_BOLD)Publish Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make publish-mac$(COLOR_RESET)          - Publish macOS versions (ARM64 + x64)"
	@echo "  $(COLOR_GREEN)make publish-mac-arm64$(COLOR_RESET)    - Publish macOS ARM64 version only"
	@echo "  $(COLOR_GREEN)make publish-mac-x64$(COLOR_RESET)      - Publish macOS x64 version only"
	@echo ""
	@echo "$(COLOR_BOLD)Package Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make app-mac$(COLOR_RESET)              - Create macOS .app bundles (both architectures)"
	@echo "  $(COLOR_GREEN)make app-mac-arm64$(COLOR_RESET)        - Create ARM64 .app bundle only"
	@echo "  $(COLOR_GREEN)make app-mac-x64$(COLOR_RESET)          - Create x64 .app bundle only"
	@echo "  $(COLOR_GREEN)make package-mac$(COLOR_RESET)          - Create distributable .zip packages"
	@echo "  $(COLOR_GREEN)make package-all$(COLOR_RESET)          - Package all platforms"
	@echo ""
	@echo "$(COLOR_BOLD)Installation Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make install-mac$(COLOR_RESET)          - Install app to /Applications (requires sudo)"
	@echo "  $(COLOR_GREEN)make uninstall-mac$(COLOR_RESET)        - Uninstall app from /Applications"
	@echo ""
	@echo "$(COLOR_BOLD)Maintenance Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make clean$(COLOR_RESET)                - Clean build artifacts"
	@echo "  $(COLOR_GREEN)make clean-all$(COLOR_RESET)            - Clean everything (including packages)"
	@echo "  $(COLOR_GREEN)make format$(COLOR_RESET)               - Format code (dotnet format)"
	@echo "  $(COLOR_GREEN)make lint$(COLOR_RESET)                 - Lint code (check formatting)"
	@echo ""
	@echo "$(COLOR_BOLD)Info Commands:$(COLOR_RESET)"
	@echo "  $(COLOR_GREEN)make version$(COLOR_RESET)              - Display version information"
	@echo "  $(COLOR_GREEN)make info$(COLOR_RESET)                 - Display project information"
	@echo "  $(COLOR_GREEN)make check-deps$(COLOR_RESET)           - Check required dependencies"
	@echo ""
	@echo "$(COLOR_BOLD)Environment Variables:$(COLOR_RESET)"
	@echo "  $(COLOR_YELLOW)DOTNET_CONFIG$(COLOR_RESET)            - Build configuration (Debug/Release, default: Debug)"
	@echo ""
	@echo "$(COLOR_BOLD)Examples:$(COLOR_RESET)"
	@echo "  make build-mac                    # Build macOS version (Debug)"
	@echo "  DOTNET_CONFIG=Release make package-mac    # Create release package"
	@echo ""
	@echo "$(COLOR_CYAN)════════════════════════════════════════════════════════════════$(COLOR_RESET)"

# ============================================================================
# Dependency Checking
# ============================================================================

## check-deps: Check if required tools are installed
check-deps:
	@echo "$(COLOR_BLUE)$(EMOJI_INFO) Checking dependencies...$(COLOR_RESET)"
	@command -v $(DOTNET) >/dev/null 2>&1 || { echo "$(COLOR_RED)$(EMOJI_CROSS) Error: dotnet CLI not found$(COLOR_RESET)"; exit 1; }
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) .NET SDK: $$($(DOTNET) --version)$(COLOR_RESET)"
	@command -v zip >/dev/null 2>&1 || { echo "$(COLOR_YELLOW)$(EMOJI_WARNING) Warning: zip not found (packaging will fail)$(COLOR_RESET)"; }
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) All required dependencies are installed$(COLOR_RESET)"

# ============================================================================
# Build Targets
# ============================================================================

## restore: Restore NuGet packages for all projects
restore: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Restoring packages...$(COLOR_RESET)"
	@$(DOTNET) restore $(CORE_PROJECT)
	@$(DOTNET) restore $(CMD_PROJECT)
	@$(DOTNET) restore $(MAC_PROJECT)
	@$(DOTNET) restore $(TEST_PROJECT)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Package restore completed$(COLOR_RESET)"

## build: Build all projects (alias for build-all)
build: build-all

## build-all: Build all projects
build-all: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Building all projects ($(DOTNET_CONFIG))...$(COLOR_RESET)"
	@$(DOTNET) build $(BUILD_OPTS) $(CORE_PROJECT)
	@$(DOTNET) build $(BUILD_OPTS) $(CMD_PROJECT)
	@$(DOTNET) build $(BUILD_OPTS) $(MAC_PROJECT)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Build completed successfully$(COLOR_RESET)"

## build-cmd: Build command-line version
build-cmd: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Building command-line version ($(DOTNET_CONFIG))...$(COLOR_RESET)"
	@$(DOTNET) build $(BUILD_OPTS) $(CMD_PROJECT)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Command-line build completed$(COLOR_RESET)"

## build-mac: Build macOS GUI version
build-mac: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Building macOS version ($(DOTNET_CONFIG))...$(COLOR_RESET)"
	@$(DOTNET) build $(BUILD_OPTS) $(MAC_PROJECT)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) macOS build completed$(COLOR_RESET)"

## build-release: Build all projects in Release mode
build-release:
	@$(MAKE) build-all DOTNET_CONFIG=Release

# ============================================================================
# Test Targets
# ============================================================================

## test: Run unit tests
test: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_TEST) Running tests...$(COLOR_RESET)"
	@$(DOTNET) test $(TEST_OPTS) $(TEST_PROJECT) || { echo "$(COLOR_RED)$(EMOJI_CROSS) Tests failed$(COLOR_RESET)"; exit 1; }
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) All tests passed$(COLOR_RESET)"

## test-verbose: Run tests with verbose output
test-verbose: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_TEST) Running tests (verbose)...$(COLOR_RESET)"
	@$(DOTNET) test $(TEST_OPTS) --verbosity detailed $(TEST_PROJECT)

## test-coverage: Run tests with code coverage
test-coverage: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_TEST) Running tests with coverage...$(COLOR_RESET)"
	@$(DOTNET) test $(TEST_OPTS) --collect:"XPlat Code Coverage" $(TEST_PROJECT)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Coverage report generated$(COLOR_RESET)"

# ============================================================================
# Run Targets
# ============================================================================

## run-cmd: Run command-line version
run-cmd: build-cmd
	@echo "$(COLOR_BLUE)$(EMOJI_ROCKET) Running command-line version...$(COLOR_RESET)"
	@cd $(SRC_DIR)/ImeWlConverterCmd && $(DOTNET) run

## run-mac: Run macOS GUI version
run-mac: build-mac
	@echo "$(COLOR_BLUE)$(EMOJI_ROCKET) Running macOS version...$(COLOR_RESET)"
	@cd $(SRC_DIR)/ImeWlConverterMac && $(DOTNET) run

# ============================================================================
# Publish Targets
# ============================================================================

## publish-mac: Publish both ARM64 and x64 versions for macOS
publish-mac: publish-mac-arm64 publish-mac-x64
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Published all macOS architectures$(COLOR_RESET)"

## publish-mac-arm64: Publish ARM64 version for macOS
publish-mac-arm64: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Publishing macOS ARM64 version...$(COLOR_RESET)"
	@$(DOTNET) publish $(MAC_PROJECT) $(PUBLISH_OPTS) --runtime $(DOTNET_RUNTIME_OSX_ARM64) --output $(MAC_ARM64_DIR)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) ARM64 publish completed: $(MAC_ARM64_DIR)$(COLOR_RESET)"

## publish-mac-x64: Publish x64 version for macOS
publish-mac-x64: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Publishing macOS x64 version...$(COLOR_RESET)"
	@$(DOTNET) publish $(MAC_PROJECT) $(PUBLISH_OPTS) --runtime $(DOTNET_RUNTIME_OSX_X64) --output $(MAC_X64_DIR)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) x64 publish completed: $(MAC_X64_DIR)$(COLOR_RESET)"

# ============================================================================
# App Bundle Creation
# ============================================================================

## app-mac: Create .app bundles for both architectures
app-mac: app-mac-arm64 app-mac-x64
	@echo "$(COLOR_GREEN)$(EMOJI_PACKAGE) All macOS app bundles created$(COLOR_RESET)"
	@echo ""
	@echo "$(COLOR_CYAN)$(EMOJI_INFO) Created Applications:$(COLOR_RESET)"
	@echo "  $(COLOR_YELLOW)→$(COLOR_RESET) $(APP_BUNDLE_ARM64) (Apple Silicon)"
	@echo "  $(COLOR_YELLOW)→$(COLOR_RESET) $(APP_BUNDLE_X64) (Intel Mac)"
	@echo ""
	@echo "$(COLOR_CYAN)$(EMOJI_ROCKET) Usage:$(COLOR_RESET)"
	@echo "  Double-click:    open './$(APP_BUNDLE_ARM64)'"
	@echo "  Install:         make install-mac"

## app-mac-arm64: Create ARM64 .app bundle
app-mac-arm64: publish-mac-arm64
	@echo "$(COLOR_BLUE)$(EMOJI_PACKAGE) Creating ARM64 .app bundle...$(COLOR_RESET)"
	@chmod +x $(APP_BUNDLE_SCRIPT)
	@$(APP_BUNDLE_SCRIPT) $(MAC_ARM64_DIR) "$(APP_NAME_ARM64)"
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) ARM64 .app bundle: $(APP_BUNDLE_ARM64)$(COLOR_RESET)"

## app-mac-x64: Create x64 .app bundle
app-mac-x64: publish-mac-x64
	@echo "$(COLOR_BLUE)$(EMOJI_PACKAGE) Creating x64 .app bundle...$(COLOR_RESET)"
	@chmod +x $(APP_BUNDLE_SCRIPT)
	@$(APP_BUNDLE_SCRIPT) $(MAC_X64_DIR) "$(APP_NAME_X64)"
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) x64 .app bundle: $(APP_BUNDLE_X64)$(COLOR_RESET)"

# ============================================================================
# Packaging Targets
# ============================================================================

## package-mac: Create distributable packages for macOS
package-mac: app-mac
	@echo "$(COLOR_BLUE)$(EMOJI_PACKAGE) Creating distribution packages...$(COLOR_RESET)"
	@mkdir -p $(DIST_DIR)
	@cd $(DIST_DIR) && zip -r $(APP_ARCHIVE_ARM64) ../$(APP_BUNDLE_ARM64) > /dev/null 2>&1
	@cd $(DIST_DIR) && zip -r $(APP_ARCHIVE_X64) ../$(APP_BUNDLE_X64) > /dev/null 2>&1
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Packaging completed$(COLOR_RESET)"
	@echo ""
	@echo "$(COLOR_CYAN)$(EMOJI_INFO) Distribution Packages:$(COLOR_RESET)"
	@echo "  $(COLOR_YELLOW)→$(COLOR_RESET) $(DIST_DIR)/$(APP_ARCHIVE_ARM64) ($$(du -h $(DIST_DIR)/$(APP_ARCHIVE_ARM64) | cut -f1))"
	@echo "  $(COLOR_YELLOW)→$(COLOR_RESET) $(DIST_DIR)/$(APP_ARCHIVE_X64) ($$(du -h $(DIST_DIR)/$(APP_ARCHIVE_X64) | cut -f1))"

## package-all: Package for all platforms
package-all: package-mac
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) All packages created$(COLOR_RESET)"

# ============================================================================
# Installation Targets
# ============================================================================

## install-mac: Install macOS app to /Applications
install-mac: app-mac-arm64
	@echo "$(COLOR_BLUE)$(EMOJI_ROCKET) Installing to /Applications...$(COLOR_RESET)"
	@if [ -d "/Applications/$(APP_BUNDLE_ARM64)" ]; then \
		echo "$(COLOR_YELLOW)$(EMOJI_WARNING) Removing existing installation...$(COLOR_RESET)"; \
		rm -rf "/Applications/$(APP_BUNDLE_ARM64)"; \
	fi
	@cp -R $(APP_BUNDLE_ARM64) /Applications/
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Installed to /Applications/$(APP_BUNDLE_ARM64)$(COLOR_RESET)"
	@echo "$(COLOR_CYAN)$(EMOJI_INFO) You can now launch the app from Launchpad or Spotlight$(COLOR_RESET)"

## uninstall-mac: Uninstall macOS app from /Applications
uninstall-mac:
	@echo "$(COLOR_BLUE)$(EMOJI_CLEAN) Uninstalling from /Applications...$(COLOR_RESET)"
	@if [ -d "/Applications/$(APP_BUNDLE_ARM64)" ]; then \
		rm -rf "/Applications/$(APP_BUNDLE_ARM64)"; \
		echo "$(COLOR_GREEN)$(EMOJI_CHECK) Uninstalled $(APP_BUNDLE_ARM64)$(COLOR_RESET)"; \
	fi
	@if [ -d "/Applications/$(APP_BUNDLE_X64)" ]; then \
		rm -rf "/Applications/$(APP_BUNDLE_X64)"; \
		echo "$(COLOR_GREEN)$(EMOJI_CHECK) Uninstalled $(APP_BUNDLE_X64)$(COLOR_RESET)"; \
	fi
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Uninstall completed$(COLOR_RESET)"

# ============================================================================
# Clean Targets
# ============================================================================

## clean: Clean build artifacts
clean:
	@echo "$(COLOR_BLUE)$(EMOJI_CLEAN) Cleaning build artifacts...$(COLOR_RESET)"
	@$(DOTNET) clean $(CORE_PROJECT) > /dev/null 2>&1 || true
	@$(DOTNET) clean $(CMD_PROJECT) > /dev/null 2>&1 || true
	@$(DOTNET) clean $(MAC_PROJECT) > /dev/null 2>&1 || true
	@$(DOTNET) clean $(TEST_PROJECT) > /dev/null 2>&1 || true
	@rm -rf $(BUILD_DIR)
	@rm -rf $(PUBLISH_DIR)
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Build artifacts cleaned$(COLOR_RESET)"

## clean-all: Clean everything including packages
clean-all: clean
	@echo "$(COLOR_BLUE)$(EMOJI_CLEAN) Cleaning all generated files...$(COLOR_RESET)"
	@rm -rf $(DIST_DIR)
	@rm -rf $(APP_BUNDLE_ARM64) $(APP_BUNDLE_X64)
	@rm -f $(APP_ARCHIVE_ARM64) $(APP_ARCHIVE_X64)
	@find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
	@find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) All generated files cleaned$(COLOR_RESET)"

# ============================================================================
# Code Quality Targets
# ============================================================================

## format: Format code using dotnet format
format: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Formatting code...$(COLOR_RESET)"
	@$(DOTNET) format $(CORE_PROJECT) || true
	@$(DOTNET) format $(CMD_PROJECT) || true
	@$(DOTNET) format $(MAC_PROJECT) || true
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Code formatting completed$(COLOR_RESET)"

## lint: Check code formatting
lint: check-deps
	@echo "$(COLOR_BLUE)$(EMOJI_BUILD) Checking code format...$(COLOR_RESET)"
	@$(DOTNET) format $(CORE_PROJECT) --verify-no-changes || { echo "$(COLOR_RED)$(EMOJI_CROSS) Format check failed$(COLOR_RESET)"; exit 1; }
	@$(DOTNET) format $(CMD_PROJECT) --verify-no-changes || { echo "$(COLOR_RED)$(EMOJI_CROSS) Format check failed$(COLOR_RESET)"; exit 1; }
	@$(DOTNET) format $(MAC_PROJECT) --verify-no-changes || { echo "$(COLOR_RED)$(EMOJI_CROSS) Format check failed$(COLOR_RESET)"; exit 1; }
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) Code format is correct$(COLOR_RESET)"

# ============================================================================
# Information Targets
# ============================================================================

## version: Display version information
version: check-deps
	@echo "$(COLOR_CYAN)$(EMOJI_INFO) Version Information:$(COLOR_RESET)"
	@echo "$(COLOR_YELLOW)Project Version:$(COLOR_RESET)"
	@cd $(SRC_DIR)/ImeWlConverterCore && $(DOTNET) msbuild -getProperty:Version -nologo 2>/dev/null || echo "  Unknown (MinVer requires Git tags)"
	@echo ""
	@echo "$(COLOR_YELLOW).NET SDK:$(COLOR_RESET) $$($(DOTNET) --version)"
	@echo "$(COLOR_YELLOW)Target Framework:$(COLOR_RESET) $(DOTNET_FRAMEWORK)"

## info: Display project information
info:
	@echo "$(COLOR_CYAN)════════════════════════════════════════════════════════════════$(COLOR_RESET)"
	@echo "$(COLOR_BOLD)$(COLOR_CYAN)  $(EMOJI_INFO) IME WL Converter - Project Information$(COLOR_RESET)"
	@echo "$(COLOR_CYAN)════════════════════════════════════════════════════════════════$(COLOR_RESET)"
	@echo ""
	@echo "$(COLOR_YELLOW)Project Structure:$(COLOR_RESET)"
	@echo "  Core Library:      $(CORE_PROJECT)"
	@echo "  Command-line Tool: $(CMD_PROJECT)"
	@echo "  macOS GUI App:     $(MAC_PROJECT)"
	@echo "  Test Project:      $(TEST_PROJECT)"
	@echo ""
	@echo "$(COLOR_YELLOW)Build Configuration:$(COLOR_RESET)"
	@echo "  Configuration:     $(DOTNET_CONFIG)"
	@echo "  Target Framework:  $(DOTNET_FRAMEWORK)"
	@echo "  macOS Runtimes:    $(DOTNET_RUNTIME_OSX_ARM64), $(DOTNET_RUNTIME_OSX_X64)"
	@echo ""
	@echo "$(COLOR_YELLOW)Output Directories:$(COLOR_RESET)"
	@echo "  Publish:           $(PUBLISH_DIR)/"
	@echo "  Distribution:      $(DIST_DIR)/"
	@echo ""
	@$(MAKE) version --no-print-directory
	@echo ""
	@echo "$(COLOR_CYAN)════════════════════════════════════════════════════════════════$(COLOR_RESET)"

# ============================================================================
# Maintenance Targets
# ============================================================================

# Legacy compatibility targets (keep old commands working)
cmd: build-cmd
release: build-release
release-mac:
	@$(MAKE) build-mac DOTNET_CONFIG=Release
run: run-cmd
clean-mac: clean
clean-packages: clean-all

# ============================================================================
# CI/CD Targets
# ============================================================================

## ci-build: Build for CI/CD (with tests)
ci-build: check-deps restore build-all test
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) CI build completed successfully$(COLOR_RESET)"

## ci-package: Package for CI/CD release
ci-package: check-deps
	@$(MAKE) package-all DOTNET_CONFIG=Release
	@echo "$(COLOR_GREEN)$(EMOJI_CHECK) CI packaging completed$(COLOR_RESET)"
