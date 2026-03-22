import { useGetCabinetContainersQuery } from '../../features/storage/storageApi';
import type { ContainerDto } from '../../features/storage/storageSlice';
import { Shelf } from '../Shelf/Shelf';
import styles from './CabinetView.module.css';

interface Props {
  cabinetNumber: number;
}

function groupByShelves(containers: ContainerDto[]): Map<number, ContainerDto[]> {
  const map = new Map<number, ContainerDto[]>();
  for (const container of containers) {
    const shelf = map.get(container.shelf) ?? [];
    shelf.push(container);
    map.set(container.shelf, shelf);
  }
  return map;
}

export function CabinetView({ cabinetNumber }: Props) {
  const { data, isLoading, isError } = useGetCabinetContainersQuery(cabinetNumber);

  if (isLoading) {
    return <div className={styles.message}>Loading cabinet {cabinetNumber}…</div>;
  }

  if (isError || !data) {
    return (
      <div className={styles.message}>No containers with empty sections found.</div>
    );
  }

  const shelvesByNumber = groupByShelves(data);
  const sortedShelfNumbers = [...shelvesByNumber.keys()].sort((a, b) => a - b);

  return (
    <div className={styles.cabinet}>
      {sortedShelfNumbers.map((shelfNumber) => (
        <Shelf
          key={shelfNumber}
          shelfNumber={shelfNumber}
          containers={shelvesByNumber.get(shelfNumber)!}
        />
      ))}
    </div>
  );
}
