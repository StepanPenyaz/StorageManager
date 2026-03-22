import styles from './Header.module.css';

interface Props {
  onOpenInit: () => void;
}

export function Header({ onOpenInit }: Props) {
  const handleOpen = () => onOpenInit();

  return (
    <header className={styles.header}>
      <div className={styles.left}>
        <button className={styles.menuButton} aria-label="Open menu" onClick={handleOpen}>
          ☰
        </button>
        <h1 className={styles.title}>Storage Manager</h1>
      </div>
      <div className={styles.menu}>
        <button className={styles.menuItem} onClick={handleOpen}>
          Storage Init
        </button>
      </div>
    </header>
  );
}
