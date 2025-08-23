# Task Completion Checklist

## Essential Checks Before Task Completion

### 1. Build Verification
```bash
# Must pass without errors or warnings
cd EmployeeManagement
dotnet build
```

### 2. Code Quality Standards
- [ ] **XMLコメント**: All public/protected members have XML documentation
- [ ] **インラインコメント**: Complex logic has explanatory comments  
- [ ] **サービス分離**: Business logic properly separated into services
- [ ] **インターフェース実装**: Services implement interfaces for loose coupling
- [ ] **エラーハンドリング**: Proper try-catch blocks and error logging
- [ ] **定数化**: No magic numbers, use Constants classes
- [ ] **null安全性**: Null checks and guard clauses implemented

### 3. Architecture Compliance
- [ ] **単一責任原則**: Each class/method has single responsibility
- [ ] **依存性注入**: Dependencies injected via DI container  
- [ ] **Repository Pattern**: Data access through repository interfaces
- [ ] **Service Layer**: Business logic in dedicated services
- [ ] **ViewModels**: State management in ViewModels when needed

### 4. Performance Considerations
- [ ] **非同期処理**: Async/await used appropriately for I/O operations
- [ ] **メモリキャッシュ**: Caching implemented for frequently accessed data
- [ ] **リソース管理**: Proper disposal of resources (IDisposable)

### 5. Testing and Validation
```bash
# Test application manually
dotnet watch run
# Navigate to https://localhost:7049
# Test with demo accounts: admin/password, user/password
```

- [ ] **機能テスト**: All modified functionality works correctly
- [ ] **認証フロー**: Login/logout works if authentication-related
- [ ] **CRUD操作**: Create, Read, Update, Delete operations function properly
- [ ] **バリデーション**: Form validation works as expected
- [ ] **エラー処理**: Error scenarios display appropriate messages
- [ ] **レスポンシブ**: UI works on different screen sizes

### 6. Code Consistency
- [ ] **命名規約**: C# naming conventions followed
- [ ] **コードスタイル**: Consistent with existing codebase
- [ ] **インデント**: Proper indentation and formatting
- [ ] **Using文**: Appropriate using statements and cleanup

### 7. Documentation Updates
- [ ] **コードコメント**: New functionality properly documented
- [ ] **XML Documentation**: API documentation updated if public interfaces changed
- [ ] **TODO/HACK**: Temporary code marked appropriately

### 8. Integration Checks
- [ ] **依存関係**: New dependencies properly registered in Program.cs
- [ ] **サービス登録**: Services added to DI container correctly
- [ ] **設定ファイル**: Configuration changes documented
- [ ] **静的ファイル**: New assets placed in wwwroot correctly

### 9. Security Considerations
- [ ] **入力検証**: User input properly validated and sanitized
- [ ] **認証**: Authentication requirements respected
- [ ] **セッション管理**: Session state handled correctly
- [ ] **ログ出力**: No sensitive data logged

### 10. Before Git Commit (if applicable)
```bash
# Final build check
dotnet clean && dotnet build

# Optional: Run tests when implemented
dotnet test
```

## Quality Gates
- ✅ **Build Success**: `dotnet build` passes without errors
- ✅ **Functionality**: Manual testing confirms features work
- ✅ **Standards**: Code follows project coding standards
- ✅ **Architecture**: Implementation follows established patterns
- ✅ **Documentation**: Code is properly documented

## Common Issues to Check
- **Port conflicts**: Ensure port 7049 is available
- **Package dependencies**: All NuGet packages properly referenced
- **Session issues**: Clear browser cache if authentication problems
- **Hot reload issues**: Restart `dotnet watch run` if changes not reflected
- **SignalR connections**: Monitor browser console for connection issues

## Post-Completion Verification
1. **Restart application**: `Ctrl+C` and `dotnet watch run` again
2. **Test core functionality**: Login, navigation, main features
3. **Check console logs**: No errors in browser console or application logs
4. **Performance check**: Application responds quickly and smoothly

This checklist ensures code quality, maintainability, and adherence to project standards.