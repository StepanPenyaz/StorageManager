import { useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState } from '../../app/store';
import { toggleTheme } from '../../features/storage/storageSlice';
import type { ProcessingResult } from '../../features/storage/storageSlice';
import { useProcessOrdersMutation, useProcessStorageUpdatesMutation } from '../../features/storage/storageApi';
import styles from './Header.module.css';

interface Props {
  onOpenInit: () => void;
}

export function Header({ onOpenInit }: Props) {
  const dispatch = useDispatch();
  const theme = useSelector((state: RootState) => state.storage.theme);

  const [menuOpen, setMenuOpen] = useState(false);
  const [dirSettingsOpen, setDirSettingsOpen] = useState(false);
  const [incomingOrdersFolder, setIncomingOrdersFolder] = useState('');
  const [processedOrdersFolder, setProcessedOrdersFolder] = useState('');
  const [incomingUpdatesFolder, setIncomingUpdatesFolder] = useState('');
  const [processedUpdatesFolder, setProcessedUpdatesFolder] = useState('');

  const [ordersResult, setOrdersResult] = useState<ProcessingResult | null>(null);
  const [ordersError, setOrdersError] = useState<string | null>(null);
  const [updatesResult, setUpdatesResult] = useState<ProcessingResult | null>(null);
  const [updatesError, setUpdatesError] = useState<string | null>(null);

  const incomingOrdersPickerRef = useRef<HTMLInputElement>(null);
  const processedOrdersPickerRef = useRef<HTMLInputElement>(null);
  const incomingUpdatesPickerRef = useRef<HTMLInputElement>(null);
  const processedUpdatesPickerRef = useRef<HTMLInputElement>(null);

  const [processOrders, { isLoading: isProcessingOrders }] = useProcessOrdersMutation();
  const [processStorageUpdates, { isLoading: isProcessingUpdates }] = useProcessStorageUpdatesMutation();

  const foldersValid =
    incomingOrdersFolder.trim() !== '' &&
    processedOrdersFolder.trim() !== '' &&
    incomingUpdatesFolder.trim() !== '' &&
    processedUpdatesFolder.trim() !== '';

  const handleProcessOrders = async () => {
    setOrdersResult(null);
    setOrdersError(null);
    try {
      const result = await processOrders({
        incomingOrdersFolder: incomingOrdersFolder.trim(),
        processedOrdersFolder: processedOrdersFolder.trim(),
      }).unwrap();
      setOrdersResult(result);
    } catch (err: unknown) {
      setOrdersError(extractErrorMessage(err, 'Order processing failed.'));
    }
  };

  const handleProcessStorageUpdates = async () => {
    setUpdatesResult(null);
    setUpdatesError(null);
    try {
      const result = await processStorageUpdates({
        incomingStorageUpdatesFolder: incomingUpdatesFolder.trim(),
        processedStorageUpdatesFolder: processedUpdatesFolder.trim(),
      }).unwrap();
      setUpdatesResult(result);
    } catch (err: unknown) {
      setUpdatesError(extractErrorMessage(err, 'Storage update processing failed.'));
    }
  };

  const handleFolderPick = (
    ref: React.RefObject<HTMLInputElement | null>,
    setter: (v: string) => void,
  ) => {
    if (!ref.current) return;
    ref.current.onchange = (e) => {
      const input = e.target as HTMLInputElement;
      const file = input.files?.[0];
      if (file) {
        const fullPath = (file as File & { path?: string }).path;
        if (fullPath) {
          const sep = fullPath.includes('\\') ? '\\' : '/';
          setter(fullPath.substring(0, fullPath.lastIndexOf(sep)));
        } else {
          setter(file.name);
        }
      }
      input.value = '';
    };
    ref.current.click();
  };

  return (
    <header className={`${styles.header} ${theme === 'light' ? styles.headerLight : ''}`}>
      <div className={styles.left}>
        <button
          className={styles.menuButton}
          aria-label="Open menu"
          aria-expanded={menuOpen}
          onClick={() => setMenuOpen((prev) => !prev)}
        >
          ☰
        </button>
        <h1 className={styles.title}>Storage Manager</h1>
      </div>

      <div className={styles.right}>
        <button
          className={styles.actionButton}
          disabled={!foldersValid || isProcessingOrders}
          onClick={handleProcessOrders}
          title={foldersValid ? 'Process orders' : 'Configure folder paths in the menu first'}
        >
          {isProcessingOrders ? 'Processing…' : 'Process orders'}
        </button>
        <button
          className={styles.actionButton}
          disabled={!foldersValid || isProcessingUpdates}
          onClick={handleProcessStorageUpdates}
          title={foldersValid ? 'Process storage updates' : 'Configure folder paths in the menu first'}
        >
          {isProcessingUpdates ? 'Processing…' : 'Process storage updates'}
        </button>
      </div>

      {menuOpen && (
        <div className={`${styles.menuPanel} ${theme === 'light' ? styles.menuPanelLight : ''}`}>
          <div className={styles.menuPanelHeader}>
            <span className={styles.menuPanelTitle}>Configuration</span>
            <button className={styles.closeMenuButton} onClick={() => setMenuOpen(false)}>✕</button>
          </div>

          <div className={styles.menuSection}>
            <div className={styles.menuSectionTitle}>Store initialization</div>
            <button
              className={styles.menuActionButton}
              onClick={() => { setMenuOpen(false); onOpenInit(); }}
            >
              Storage Init
            </button>
          </div>

          <div className={styles.menuSection}>
            <div className={styles.menuSectionTitle}>Directory settings</div>
            <button
              className={styles.menuActionButton}
              onClick={() => setDirSettingsOpen((prev) => !prev)}
            >
              {dirSettingsOpen ? '▲ Hide directories' : '▼ Configure directories'}
            </button>
            {dirSettingsOpen && (
              <div className={styles.dirSettings}>
                {/* Hidden file inputs for folder picking */}
                <input ref={incomingOrdersPickerRef} type="file" className={styles.hiddenFileInput} />
                <input ref={processedOrdersPickerRef} type="file" className={styles.hiddenFileInput} />
                <input ref={incomingUpdatesPickerRef} type="file" className={styles.hiddenFileInput} />
                <input ref={processedUpdatesPickerRef} type="file" className={styles.hiddenFileInput} />

                <div className={styles.menuLabel}>
                  Incoming orders folder
                  <div className={styles.filePickerRow}>
                    <button
                      type="button"
                      className={styles.browseButton}
                      onClick={() => handleFolderPick(incomingOrdersPickerRef, setIncomingOrdersFolder)}
                    >
                      Browse…
                    </button>
                    <input
                      className={styles.menuInput}
                      type="text"
                      value={incomingOrdersFolder}
                      placeholder="C:\Storage\Orders"
                      onChange={(e) => setIncomingOrdersFolder(e.target.value)}
                    />
                  </div>
                </div>
                <div className={styles.menuLabel}>
                  Processed orders folder
                  <div className={styles.filePickerRow}>
                    <button
                      type="button"
                      className={styles.browseButton}
                      onClick={() => handleFolderPick(processedOrdersPickerRef, setProcessedOrdersFolder)}
                    >
                      Browse…
                    </button>
                    <input
                      className={styles.menuInput}
                      type="text"
                      value={processedOrdersFolder}
                      placeholder="C:\Storage\Orders\Finished"
                      onChange={(e) => setProcessedOrdersFolder(e.target.value)}
                    />
                  </div>
                </div>
                <div className={styles.menuLabel}>
                  Incoming storage updates folder
                  <div className={styles.filePickerRow}>
                    <button
                      type="button"
                      className={styles.browseButton}
                      onClick={() => handleFolderPick(incomingUpdatesPickerRef, setIncomingUpdatesFolder)}
                    >
                      Browse…
                    </button>
                    <input
                      className={styles.menuInput}
                      type="text"
                      value={incomingUpdatesFolder}
                      placeholder="C:\Storage\Updates"
                      onChange={(e) => setIncomingUpdatesFolder(e.target.value)}
                    />
                  </div>
                </div>
                <div className={styles.menuLabel}>
                  Processed storage updates folder
                  <div className={styles.filePickerRow}>
                    <button
                      type="button"
                      className={styles.browseButton}
                      onClick={() => handleFolderPick(processedUpdatesPickerRef, setProcessedUpdatesFolder)}
                    >
                      Browse…
                    </button>
                    <input
                      className={styles.menuInput}
                      type="text"
                      value={processedUpdatesFolder}
                      placeholder="C:\Storage\Updates\Finished"
                      onChange={(e) => setProcessedUpdatesFolder(e.target.value)}
                    />
                  </div>
                </div>
              </div>
            )}
          </div>

          <div className={styles.menuSection}>
            <div className={styles.menuSectionTitle}>Theme</div>
            <button
              className={styles.themeToggleButton}
              onClick={() => dispatch(toggleTheme())}
            >
              {theme === 'dark' ? '☀ Switch to light mode' : '🌙 Switch to dark mode'}
            </button>
          </div>
        </div>
      )}

      {(ordersResult || ordersError) && (
        <div className={`${styles.feedbackBar} ${ordersError ? styles.feedbackError : styles.feedbackSuccess}`}>
          {ordersError
            ? `Orders: ${ordersError}`
            : `Orders processed — files: ${ordersResult!.filesProcessed}, items: ${ordersResult!.itemsProcessed}` +
              (ordersResult!.warningCount > 0 ? `, warnings: ${ordersResult!.warningCount}` : '') +
              (ordersResult!.errorCount > 0 ? `, errors: ${ordersResult!.errorCount}` : '')}
          <button className={styles.feedbackClose} onClick={() => { setOrdersResult(null); setOrdersError(null); }}>✕</button>
        </div>
      )}

      {(updatesResult || updatesError) && (
        <div className={`${styles.feedbackBar} ${updatesError ? styles.feedbackError : styles.feedbackSuccess}`}>
          {updatesError
            ? `Updates: ${updatesError}`
            : `Storage updates processed — files: ${updatesResult!.filesProcessed}, items: ${updatesResult!.itemsProcessed}` +
              (updatesResult!.warningCount > 0 ? `, warnings: ${updatesResult!.warningCount}` : '') +
              (updatesResult!.errorCount > 0 ? `, errors: ${updatesResult!.errorCount}` : '')}
          <button className={styles.feedbackClose} onClick={() => { setUpdatesResult(null); setUpdatesError(null); }}>✕</button>
        </div>
      )}
    </header>
  );
}

function extractErrorMessage(err: unknown, fallback: string): string {
  if (!err || typeof err !== 'object') return fallback;
  const e = err as Record<string, unknown>;
  if ('data' in e) {
    const data = e.data;
    if (typeof data === 'string') return data;
    if (data && typeof data === 'object') {
      const d = data as Record<string, unknown>;
      if (typeof d.detail === 'string') return d.detail;
      if (typeof d.error === 'string') return d.error;
    }
  }
  return fallback;
}
