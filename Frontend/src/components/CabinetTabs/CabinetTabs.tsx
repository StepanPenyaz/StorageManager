import { useDispatch, useSelector } from 'react-redux';
import type { RootState } from '../../app/store';
import { selectCabinet } from '../../features/storage/storageSlice';
import styles from './CabinetTabs.module.css';

interface Props {
  cabinets: number[];
}

export function CabinetTabs({ cabinets }: Props) {
  const dispatch = useDispatch();
  const selected = useSelector((state: RootState) => state.storage.selectedCabinet);

  return (
    <div className={styles.tabBar}>
      {cabinets.map((cabinet) => (
        <button
          key={cabinet}
          className={`${styles.tab} ${selected === cabinet ? styles.active : ''}`}
          onClick={() => dispatch(selectCabinet(cabinet))}
        >
          Cabinet {cabinet}
        </button>
      ))}
    </div>
  );
}
