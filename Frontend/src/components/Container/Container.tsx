import type { CSSProperties } from 'react';
import type { ContainerDto } from '../../features/storage/storageSlice';
import styles from './Container.module.css';

interface Props {
  container: ContainerDto;
}

function getContainerStyle(container: ContainerDto): CSSProperties {
  const { type, emptySections, totalSections } = container;

  if (emptySections === totalSections) {
    return { background: '#4caf50' };
  }

  if (type === 'PX12') {
    const greenFraction = emptySections === 1 ? 33.33 : 66.67;
    return {
      background: `linear-gradient(to bottom, #4caf50 ${greenFraction}%, #f44336 ${greenFraction}%)`,
    };
  }

  return { background: '#f44336' };
}

export function Container({ container }: Props) {
  return (
    <div className={styles.container} style={getContainerStyle(container)}>
      <span className={styles.label}>#{container.number}</span>
    </div>
  );
}
