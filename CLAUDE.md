# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Japanese Employee Information Management Mock Application (`社員情報管理モックアプリ`) designed as a customer proposal and demonstration system. The project is **fully implemented** with .NET 8 Blazor Server and MudBlazor UI framework.

**Implementation Status**: ✅ **COMPLETE** - All core features have been successfully implemented and tested.

**📋 Detailed Specifications**: For comprehensive project specifications, features, and technical details, see [`docs/specifications/社員情報管理モックアプリ仕様書.md`](docs/specifications/社員情報管理モックアプリ仕様書.md).

## Architecture & Technology

**🏗️ System Architecture**: See [`docs/architecture/system-architecture.md`](docs/architecture/system-architecture.md) for detailed layered architecture with DDD principles.

**🔧 Technology Stack**: See [`docs/architecture/technology-stack.md`](docs/architecture/technology-stack.md) for comprehensive technology decisions and implementation details.

**🎯 Technical Achievements**: See [`docs/architecture/technical-achievements.md`](docs/architecture/technical-achievements.md) for implementation successes and technical accomplishments.

## Features & Specifications

**✅ Implementation Status**: All core features are fully implemented and tested.

**📋 Feature Details**: See [`docs/specifications/feature-specifications.md`](docs/specifications/feature-specifications.md) for comprehensive feature specifications and UI/UX implementation details.

**🗂️ Demo Data**: See [`docs/specifications/demo-data-specifications.md`](docs/specifications/demo-data-specifications.md) for demo data structure, validation rules, and sample content.

## Development Workflows

**🚀 Development**: See [`docs/workflows/development-workflow.md`](docs/workflows/development-workflow.md) for setup, daily commands, and development procedures.

**🧪 Testing**: See [`docs/workflows/testing-workflow.md`](docs/workflows/testing-workflow.md) for testing procedures, demo accounts, and validation workflows.

**📋 Code Review**: See [`docs/workflows/code-review-workflow.md`](docs/workflows/code-review-workflow.md) for automated and manual code review processes.

**🚀 Deployment**: See [`docs/workflows/deployment-workflow.md`](docs/workflows/deployment-workflow.md) for deployment procedures and environment setup.


## Available Tools

### Claude Code Tools
- **MCP**: Model Context Protocol tools are available for extended functionality
   - Context7
   - PlayWrite
- **Agents**: Specialized agents are available for specific tasks:
  - `code-reviewer`: Expert code review for quality, security, and maintainability
  - `debug-specialist`: Systematic investigation and resolution of technical issues
  - `software-architect`: Architectural guidance and system design decisions

### Project Status
- **Current Status**: ✅ **PRODUCTION READY**
- **Last Updated**: 2025-08-03
- **Implementation**: All core features completed and tested
- **Quick Summary**: Login system, dashboard, employee/department management with full CRUD operations


## Development Guidelines

**📋 開発プロセス**: 詳細なガイドラインは [`docs/development/process-guidelines.md`](docs/development/process-guidelines.md) を参照してください。

**📁 Issues Management**: タスク管理については [`docs/development/issues-management.md`](docs/development/issues-management.md) を参照してください。

**💻 コーディング標準**: コーディングルールとベストプラクティスは [`docs/development/coding-standards.md`](docs/development/coding-standards.md) を参照してください。

## Coding Standards & Rules

**📋 コーディング標準**: 詳細な規約とベストプラクティスは [`docs/development/coding-standards.md`](docs/development/coding-standards.md) を参照してください。

### 🎯 **Core Principles**

1. **シンプルで理解しやすいコード** - 複雑な実装より保守しやすいコードを優先
2. **豊富なコメント** - XMLコメント + インラインコメントで意図を明確化
3. **役割毎の細かい分割と疎結合** - サービス指向アーキテクチャによる責務分離

### 📋 **必須実装項目**

- XMLコメント（全public/protectedメンバー）
- サービス分離（単一責任原則）
- インターフェース実装（疎結合）
- 適切なエラーハンドリングとログ出力
- 定数化（マジックナンバー撲滅）
- null安全性の考慮

## Documentation Structure

- **📋 Specifications**: [`docs/specifications/`](docs/specifications/) - プロジェクト仕様書、機能詳細
- **📋 Development Guidelines**: [`docs/development/`](docs/development/) - 開発プロセス、品質管理、Issues管理、コーディング標準
- **🏗️ Architecture**: [`docs/architecture/`](docs/architecture/) - システム設計、技術的意思決定
- **🔄 Workflows**: [`docs/workflows/`](docs/workflows/) - 開発、テスト、レビュー、デプロイメントの詳細手順

## Future Extensions

- Database integration (Entity Framework Core ready via repository pattern)
- External authentication (Azure AD, LDAP)
- File upload functionality
- API endpoints for external system integration
- Audit logging and operation history
- Performance monitoring and metrics collection
- Unit and integration testing
- Code search functionality (planned)