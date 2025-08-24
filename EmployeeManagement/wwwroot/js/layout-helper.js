/**
 * Layout Helper - レスポンシブレイアウトのブラウザーサイズ監視
 * 
 * このスクリプトは、ブラウザーのウィンドウサイズの変更を監視し、
 * Blazorコンポーネントに通知を送信します。
 */

/**
 * ブレークポイント定数
 * MudBlazorのブレークポイントに合わせて設定
 */
const BREAKPOINTS = {
    MD: 768,  // デスクトップとモバイルの境界
    LG: 1024, // ラージスクリーンの境界
    XL: 1280  // エクストララージスクリーンの境界
};

/**
 * 現在のブレークポイントを取得
 * @returns {string} 現在のブレークポイント名 (xs, sm, md, lg, xl)
 */
function getCurrentBreakpoint() {
    const width = window.innerWidth;
    
    if (width < 576) return 'xs';
    if (width < 768) return 'sm';
    if (width < 1024) return 'md';
    if (width < 1280) return 'lg';
    return 'xl';
}

/**
 * モバイルデバイス判定
 * @returns {boolean} モバイルデバイスの場合true
 */
function isMobileSize() {
    return window.innerWidth < BREAKPOINTS.MD;
}

/**
 * デスクトップデバイス判定
 * @returns {boolean} デスクトップデバイスの場合true
 */
function isDesktopSize() {
    return window.innerWidth >= BREAKPOINTS.MD;
}

/**
 * 画面サイズ情報を取得
 * @returns {object} 画面サイズの詳細情報
 */
function getScreenInfo() {
    return {
        width: window.innerWidth,
        height: window.innerHeight,
        breakpoint: getCurrentBreakpoint(),
        isMobile: isMobileSize(),
        isDesktop: isDesktopSize()
    };
}

/**
 * リサイズイベントのコールバック管理
 */
let dotNetCallback = null;
let currentBreakpoint = getCurrentBreakpoint();
let navMenuObserver = null;

/**
 * ウィンドウリサイズイベントハンドラー
 * ブレークポイントが変更された場合のみBlazorに通知
 */
function handleResize() {
    const newBreakpoint = getCurrentBreakpoint();
    
    // ブレークポイントが変更された場合のみ通知
    if (newBreakpoint !== currentBreakpoint && dotNetCallback) {
        currentBreakpoint = newBreakpoint;
        
        try {
            // Blazorコンポーネントに通知
            dotNetCallback.invokeMethodAsync('OnBreakpointChanged', getScreenInfo());
        } catch (error) {
            console.error('Error calling Blazor callback:', error);
        }
    }
}

/**
 * NavMenuのテキスト修正処理
 * MudBlazorのTextプロパティが正しく表示されない問題を解決
 */
function fixNavMenuTexts() {
    try {
        const navGroups = document.querySelectorAll('.mud-nav-group');
        const texts = ['👥 社員管理', '🏢 部門管理'];
        
        let updatedCount = 0;
        navGroups.forEach((group, index) => {
            const textDiv = group.querySelector('.mud-nav-link-text');
            if (textDiv && texts[index] && !textDiv.textContent.trim()) {
                textDiv.textContent = texts[index];
                textDiv.style.color = 'white';
                textDiv.style.fontWeight = '500';
                updatedCount++;
                console.log(`NavMenu: Updated group ${index} with text: ${texts[index]}`);
            }
        });
        
        console.log(`NavMenu: Fixed ${updatedCount} navigation groups`);
        return updatedCount;
    } catch (error) {
        console.error('Error fixing NavMenu texts:', error);
        return 0;
    }
}

/**
 * DOM変更を監視してNavMenuテキストを自動修正
 */
function observeNavMenuChanges() {
    try {
        const targetNode = document.querySelector('.mud-navmenu, .mud-drawer');
        if (!targetNode) {
            console.log('NavMenu: Target node not found for observation');
            return;
        }

        const observer = new MutationObserver((mutations) => {
            let hasNavChanges = false;
            mutations.forEach((mutation) => {
                if (mutation.type === 'childList' || mutation.type === 'subtree') {
                    // NavGroupの変更を検出
                    const addedNodes = Array.from(mutation.addedNodes);
                    const hasNavGroup = addedNodes.some(node => 
                        node.nodeType === Node.ELEMENT_NODE && 
                        (node.classList?.contains('mud-nav-group') || 
                         node.querySelector?.('.mud-nav-group'))
                    );
                    
                    if (hasNavGroup) {
                        hasNavChanges = true;
                    }
                }
            });
            
            if (hasNavChanges) {
                console.log('NavMenu: Detected navigation changes, fixing texts...');
                setTimeout(fixNavMenuTexts, 100); // 少し遅延させて確実に実行
            }
        });

        observer.observe(targetNode, {
            childList: true,
            subtree: true
        });
        
        console.log('NavMenu: Started observing navigation changes');
        return observer;
    } catch (error) {
        console.error('Error setting up NavMenu observer:', error);
        return null;
    }
}

/**
 * レイアウトヘルパーの初期化
 * @param {object} dotNetRef - Blazorコンポーネントの参照
 */
window.layoutHelper = {
    /**
     * 初期化処理
     * @param {object} dotNetRef - Blazorコンポーネントの参照
     */
    initialize: function (dotNetRef) {
        console.log('Layout helper initializing...');
        
        dotNetCallback = dotNetRef;
        currentBreakpoint = getCurrentBreakpoint();
        
        // リサイズイベントリスナーを追加
        window.addEventListener('resize', handleResize);
        
        // NavMenuテキストの修正を初期化時に実行
        setTimeout(() => {
            fixNavMenuTexts();
            navMenuObserver = observeNavMenuChanges();
        }, 500); // Blazorのレンダリング完了を待つ
        
        console.log('Layout helper initialized. Current breakpoint:', currentBreakpoint);
        
        // 初期状態を返す
        return getScreenInfo();
    },

    /**
     * クリーンアップ処理
     */
    dispose: function () {
        console.log('Layout helper disposing...');
        
        // イベントリスナーを削除
        window.removeEventListener('resize', handleResize);
        
        // NavMenu観察者をクリーンアップ
        if (navMenuObserver) {
            navMenuObserver.disconnect();
            navMenuObserver = null;
            console.log('NavMenu observer disposed.');
        }
        
        dotNetCallback = null;
        
        console.log('Layout helper disposed.');
    },

    /**
     * 現在の画面情報を取得
     * @returns {object} 現在の画面サイズ情報
     */
    getScreenInfo: function () {
        return getScreenInfo();
    },

    /**
     * ブレークポイント変更チェック
     * @returns {boolean} ブレークポイントが変更された場合true
     */
    checkBreakpointChange: function () {
        const newBreakpoint = getCurrentBreakpoint();
        const changed = newBreakpoint !== currentBreakpoint;
        
        if (changed) {
            currentBreakpoint = newBreakpoint;
        }
        
        return changed;
    },

    /**
     * NavMenuテキストを手動で修正
     * @returns {number} 修正されたナビゲーショングループの数
     */
    fixNavMenuTexts: function () {
        return fixNavMenuTexts();
    }
};

/**
 * ファイルダウンロード機能
 * @param {string} fileName - ダウンロードするファイル名
 * @param {string} mimeType - MIMEタイプ
 * @param {string} content - ファイルの内容
 */
window.downloadFile = function(fileName, mimeType, content) {
    try {
        const blob = new Blob([content], { type: mimeType });
        const url = window.URL.createObjectURL(blob);
        
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        
        // リンクを一時的にDOMに追加してクリック
        document.body.appendChild(link);
        link.click();
        
        // クリーンアップ
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        
        console.log(`File downloaded: ${fileName}`);
    } catch (error) {
        console.error('Error downloading file:', error);
        throw error;
    }
};

// ページロード完了時の初期化ログ
document.addEventListener('DOMContentLoaded', function () {
    console.log('Layout helper script loaded. Initial breakpoint:', getCurrentBreakpoint());
});