/**
 * MudBlazorダイアログ管理ヘルパー
 * セキュアなダイアログ操作を提供します
 */
window.MudBlazorDialogHelper = {
    
    /**
     * 最後に開いたダイアログを安全に閉じます
     * @returns {boolean} 閉じ処理の成功可否
     */
    closeLastDialog: function() {
        try {
            // MudBlazor内部APIの安全な使用
            if (window.MudBlazor && 
                window.MudBlazor.dialogs && 
                Array.isArray(window.MudBlazor.dialogs) &&
                window.MudBlazor.dialogs.length > 0) {
                
                const lastDialog = window.MudBlazor.dialogs[window.MudBlazor.dialogs.length - 1];
                if (lastDialog && typeof lastDialog.close === 'function') {
                    lastDialog.close();
                    return true;
                }
            }
            return false;
        } catch (error) {
            console.warn('MudBlazor dialog close failed:', error);
            return false;
        }
    },

    /**
     * DOM操作による強制ダイアログクローズ
     * @returns {boolean} 操作の成功可否
     */
    forceCloseDialog: function() {
        try {
            const closeButton = document.querySelector('.mud-dialog .mud-dialog-close');
            if (closeButton && typeof closeButton.click === 'function') {
                closeButton.click();
                return true;
            }
            
            // フォールバック: オーバーレイクリック
            const overlay = document.querySelector('.mud-overlay-scrim');
            if (overlay && typeof overlay.click === 'function') {
                overlay.click();
                return true;
            }
            
            return false;
        } catch (error) {
            console.warn('Force dialog close failed:', error);
            return false;
        }
    },

    /**
     * ダイアログ存在チェック
     * @returns {boolean} ダイアログが表示されているか
     */
    hasOpenDialog: function() {
        try {
            const dialogElement = document.querySelector('.mud-dialog');
            return dialogElement !== null;
        } catch (error) {
            console.warn('Dialog existence check failed:', error);
            return false;
        }
    },

    /**
     * リソースクリーンアップ
     */
    cleanup: function() {
        // 現在は特に処理なし（将来の拡張のため）
        console.debug('MudBlazorDialogHelper cleanup completed');
    }
};