import { useEffect, useMemo, useState } from 'react';
import {
  type CabinetConfig,
  type ContainerTypeOption,
  type StorageInitializationRequest,
} from '../../features/storage/storageSlice';
import { useInitializeStorageMutation } from '../../features/storage/storageApi';
import styles from './StorageInitWizard.module.css';

type WizardShelf = { shelfIndex: number; rowTypes: (ContainerTypeOption | null)[] };
type WizardCabinet = { cabinetIndex: number; shelves: WizardShelf[] };

type GroupPreview = {
  groupRow: number;
  groupColumn: number;
  type: ContainerTypeOption;
  start: number;
  end: number;
};

type ShelfPreview = { shelfIndex: number; groups: GroupPreview[] };
type CabinetPreview = { cabinetIndex: number; shelves: ShelfPreview[] };

const typeOptions: ContainerTypeOption[] = ['PX12', 'PX6', 'PX4', 'PX2'];

const layoutMap: Record<ContainerTypeOption, { rows: number; columns: number; sections: number }> = {
  PX12: { rows: 4, columns: 3, sections: 3 },
  PX6: { rows: 2, columns: 3, sections: 1 },
  PX4: { rows: 2, columns: 2, sections: 1 },
  PX2: { rows: 1, columns: 2, sections: 1 },
};

const createDefaultShelves = (): WizardShelf[] =>
  Array.from({ length: 4 }, (_, index) => ({
    shelfIndex: index + 1,
    rowTypes: [null, null, null],
  }));

const createCabinet = (index: number): WizardCabinet => ({
  cabinetIndex: index,
  shelves: createDefaultShelves(),
});

interface Props {
  onClose: () => void;
}

export function StorageInitWizard({ onClose }: Props) {
  const [step, setStep] = useState(1);
  const [startIndex, setStartIndex] = useState(1000);
  const [cabinets, setCabinets] = useState<WizardCabinet[]>([createCabinet(1)]);
  const [bsxFile, setBsxFile] = useState<File | null>(null);
  const [bsxContent, setBsxContent] = useState<string | null>(null);
  const [initialize, { data, error, isLoading, isSuccess }] = useInitializeStorageMutation();

  const cabinetIsValid = useMemo(
    () =>
      startIndex > 0 &&
      cabinets.length > 0 &&
      cabinets.every(
        (cabinet) =>
          cabinet.shelves.length === 4 &&
          cabinet.shelves.every((shelf) => shelf.rowTypes.every((type) => type !== null)),
      ),
    [cabinets, startIndex],
  );

  const preview = useMemo(() => {
    if (!cabinetIsValid) return null;

    let current = startIndex;
    const containersByType: Record<ContainerTypeOption, number> = {
      PX12: 0,
      PX6: 0,
      PX4: 0,
      PX2: 0,
    };
    const cabinetPreviews: CabinetPreview[] = [];
    let totalSections = 0;

    for (const cabinet of [...cabinets].sort((a, b) => a.cabinetIndex - b.cabinetIndex)) {
      const shelfPreviews: ShelfPreview[] = [];

      for (const shelf of [...cabinet.shelves].sort((a, b) => a.shelfIndex - b.shelfIndex)) {
        const groups: GroupPreview[] = [];

        for (let groupRow = 1; groupRow <= 3; groupRow++) {
          for (let groupColumn = 1; groupColumn <= 3; groupColumn++) {
            const type = shelf.rowTypes[groupRow - 1]!;
            const layout = layoutMap[type];
            const count = layout.rows * layout.columns;
            const start = current;
            const end = current + count - 1;

            groups.push({ groupRow, groupColumn, type, start, end });

            containersByType[type] += count;
            totalSections += count * layout.sections;
            current += count;
          }
        }

        shelfPreviews.push({ shelfIndex: shelf.shelfIndex, groups });
      }

      cabinetPreviews.push({ cabinetIndex: cabinet.cabinetIndex, shelves: shelfPreviews });
    }

    return {
      cabinets: cabinetPreviews,
      totalContainers: current - startIndex,
      totalSections,
      containersByType,
    };
  }, [cabinetIsValid, cabinets, startIndex]);

  useEffect(() => {
    if (!bsxFile) {
      setBsxContent(null);
      return;
    }

    const reader = new FileReader();
    reader.onload = () => setBsxContent(typeof reader.result === 'string' ? reader.result : null);
    reader.readAsDataURL(bsxFile);
  }, [bsxFile]);

  const updateType = (cabinetIndex: number, shelfIndex: number, rowIndex: number, value: string) => {
    setCabinets((prev) =>
      prev.map((cabinet) =>
        cabinet.cabinetIndex !== cabinetIndex
          ? cabinet
          : {
              ...cabinet,
              shelves: cabinet.shelves.map((shelf) =>
                shelf.shelfIndex !== shelfIndex
                  ? shelf
                  : {
                      ...shelf,
                      rowTypes: shelf.rowTypes.map((type, idx) =>
                        idx === rowIndex ? (value ? (value as ContainerTypeOption) : null) : type,
                      ),
                    },
              ),
            },
      ),
    );
  };

  const addCabinet = () => {
    const nextIndex = Math.max(...cabinets.map((c) => c.cabinetIndex)) + 1;
    setCabinets([...cabinets, createCabinet(nextIndex)]);
  };

  const removeCabinet = (cabinetIndex: number) => {
    if (cabinets.length === 1) return;
    setCabinets(cabinets.filter((c) => c.cabinetIndex !== cabinetIndex));
  };

  const toRequest = (): StorageInitializationRequest => ({
    startIndex,
    cabinets: cabinets
      .map<CabinetConfig>((cabinet) => ({
        cabinetIndex: cabinet.cabinetIndex,
        shelves: cabinet.shelves.map((shelf) => ({
          shelfIndex: shelf.shelfIndex,
          rowTypes: shelf.rowTypes.filter(Boolean) as ContainerTypeOption[],
        })),
      }))
      .sort((a, b) => a.cabinetIndex - b.cabinetIndex),
    bsxFileName: bsxFile?.name,
    bsxFileContentBase64: bsxContent,
  });

  const handleInitialize = async () => {
    if (!preview) return;
    await initialize(toRequest());
  };

  const renderGroupGrid = (groups: GroupPreview[]) => {
    const sorted = [...groups].sort((a, b) =>
      a.groupRow !== b.groupRow ? a.groupRow - b.groupRow : a.groupColumn - b.groupColumn,
    );

    return (
      <div className={styles.groupGrid}>
        {sorted.map((group) => (
          <div key={`${group.groupRow}-${group.groupColumn}`} className={styles.groupCell}>
            <div className={styles.groupHeader}>
              Row {group.groupRow} · Col {group.groupColumn}
            </div>
            <div className={styles.groupType}>{group.type}</div>
            <div className={styles.groupRange}>
              #{group.start} – #{group.end}
            </div>
          </div>
        ))}
      </div>
    );
  };

  return (
    <div className={styles.overlay}>
      <div className={styles.wizard}>
        <div className={styles.wizardHeader}>
          <h2>Storage Master</h2>
          <button className={styles.closeButton} onClick={onClose}>
            ✕
          </button>
        </div>

        <div className={styles.steps}>Step {step} of 3</div>

        {step === 1 && (
          <div className={styles.stepContent}>
            <label className={styles.fieldLabel}>
              Container number starting index
              <input
                type="number"
                className={styles.input}
                value={startIndex}
                min={1}
                onChange={(e) => setStartIndex(parseInt(e.target.value, 10) || 0)}
              />
            </label>
            <p className={styles.helper}>Enter a positive integer to begin numbering.</p>
          </div>
        )}

        {step === 2 && (
          <div className={styles.stepContent}>
            <div className={styles.cabinetActions}>
              <h3>Cabinets</h3>
              <button className={styles.secondaryButton} onClick={addCabinet}>
                Add cabinet
              </button>
            </div>
            {cabinets.map((cabinet) => (
              <div key={cabinet.cabinetIndex} className={styles.cabinetCard}>
                <div className={styles.cabinetHeader}>
                  <span>Cabinet {cabinet.cabinetIndex}</span>
                  {cabinets.length > 1 && (
                    <button
                      className={styles.removeButton}
                      onClick={() => removeCabinet(cabinet.cabinetIndex)}
                    >
                      Remove
                    </button>
                  )}
                </div>
                {cabinet.shelves.map((shelf) => (
                  <div key={shelf.shelfIndex} className={styles.shelfBlock}>
                    <div className={styles.shelfTitle}>Shelf {shelf.shelfIndex}</div>
                    <div className={styles.rowSelectors}>
                      {shelf.rowTypes.map((value, rowIdx) => (
                        <label key={rowIdx} className={styles.rowLabel}>
                          Group row {rowIdx + 1}
                          <select
                            className={styles.select}
                            value={value ?? ''}
                            onChange={(e) =>
                              updateType(
                                cabinet.cabinetIndex,
                                shelf.shelfIndex,
                                rowIdx,
                                e.target.value,
                              )
                            }
                          >
                            <option value="">Select type</option>
                            {typeOptions.map((option) => (
                              <option key={option} value={option}>
                                {option}
                              </option>
                            ))}
                          </select>
                        </label>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            ))}
          </div>
        )}

        {step === 3 && preview && (
          <div className={styles.stepContent}>
            <div className={styles.summary}>
              <div>
                <div className={styles.summaryLabel}>Total containers</div>
                <div className={styles.summaryValue}>{preview.totalContainers}</div>
              </div>
              <div>
                <div className={styles.summaryLabel}>Total sections</div>
                <div className={styles.summaryValue}>{preview.totalSections}</div>
              </div>
              <div>
                <div className={styles.summaryLabel}>By type</div>
                <div className={styles.typeBadges}>
                  {typeOptions.map((type) => (
                    <span key={type} className={styles.badge}>
                      {type}: {preview.containersByType[type]}
                    </span>
                  ))}
                </div>
              </div>
            </div>

            <div className={styles.fileInput}>
              <label className={styles.fieldLabel}>
                Optional .bsx file
                <input
                  type="file"
                  accept=".bsx,application/xml"
                  onChange={(e) => setBsxFile(e.target.files?.[0] ?? null)}
                />
              </label>
              {bsxFile && <div className={styles.helper}>{bsxFile.name}</div>}
            </div>

            {preview.cabinets.map((cabinet) => (
              <div key={cabinet.cabinetIndex} className={styles.previewCard}>
                <div className={styles.previewHeader}>Cabinet {cabinet.cabinetIndex}</div>
                {cabinet.shelves.map((shelf) => (
                  <div key={shelf.shelfIndex} className={styles.shelfPreview}>
                    <div className={styles.shelfTitle}>Shelf {shelf.shelfIndex}</div>
                    {renderGroupGrid(shelf.groups)}
                  </div>
                ))}
              </div>
            ))}

            {isSuccess && (
              <div className={styles.successMessage}>
                Initialization completed. Containers created: {data?.containersCreated}. Sections
                created: {data?.sectionsCreated}.
              </div>
            )}

            {error && (
              <div className={styles.errorMessage}>
                {(error as { data?: string })?.data ?? 'Initialization failed.'}
              </div>
            )}
          </div>
        )}

        <div className={styles.footer}>
          <div className={styles.footerLeft}>
            {step > 1 && (
              <button className={styles.secondaryButton} onClick={() => setStep(step - 1)}>
                Back
              </button>
            )}
          </div>
          <div className={styles.footerRight}>
            {step < 3 && (
              <button
                className={styles.primaryButton}
                disabled={(step === 1 && startIndex <= 0) || (step === 2 && !cabinetIsValid)}
                onClick={() => setStep(step + 1)}
              >
                Next
              </button>
            )}
            {step === 3 && (
              <button
                className={styles.primaryButton}
                disabled={!cabinetIsValid || isLoading}
                onClick={handleInitialize}
              >
                {isLoading ? 'Initializing…' : 'Initialize store'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
