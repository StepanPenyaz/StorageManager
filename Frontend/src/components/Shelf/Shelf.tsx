import type { ContainerDto } from '../../features/storage/storageSlice';
import { Container } from '../Container/Container';
import styles from './Shelf.module.css';

interface Props {
  shelfNumber: number;
  containers: ContainerDto[];
}

function groupIntoRows(containers: ContainerDto[]): ContainerDto[][] {
  const rowMap = new Map<string, ContainerDto[]>();

  for (const container of containers) {
    const key = `${container.groupRow}-${container.positionRow}`;
    const row = rowMap.get(key) ?? [];
    row.push(container);
    rowMap.set(key, row);
  }

  const sortedKeys = [...rowMap.keys()].sort((a, b) => {
    const [aGroupRow, aPosRow] = a.split('-').map(Number);
    const [bGroupRow, bPosRow] = b.split('-').map(Number);
    return aGroupRow !== bGroupRow ? aGroupRow - bGroupRow : aPosRow - bPosRow;
  });

  return sortedKeys.map((key) => {
    const row = rowMap.get(key)!;
    return row.sort((a, b) =>
      a.groupColumn !== b.groupColumn
        ? a.groupColumn - b.groupColumn
        : a.positionColumn - b.positionColumn,
    );
  });
}

export function Shelf({ shelfNumber, containers }: Props) {
  const rows = groupIntoRows(containers);

  return (
    <div className={styles.shelf}>
      <span className={styles.shelfLabel}>Shelf {shelfNumber}</span>
      <div className={styles.rows}>
        {rows.map((row, index) => (
          <div key={index} className={styles.row}>
            {row.map((container) => (
              <Container key={container.number} container={container} />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
