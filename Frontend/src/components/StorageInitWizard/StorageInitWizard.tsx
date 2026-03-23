import { useEffect, useMemo, useState } from 'react';
import {
  type CabinetConfig,
  type ContainerTypeOption,
  type StorageInitializationRequest,
} from '../../features/storage/storageSlice';
import { useInitializeStorageMutation } from '../../features/storage/storageApi';
import styles from './StorageInitWizard.module.css';

type WizardShelf = { shelfIndex: number; rowTypes: (ContainerTypeOption | null)[] };
type WizardCabinet = {
  cabinetIndex: number;
  shelfCount: number;
  groupColumnsCount: number;
  shelves: WizardShelf[];
};

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
  PX12: { rows: 3, columns: 4, sections: 3 },
  PX6: { rows: 3, columns: 2, sections: 1 },
  PX4: { rows: 2, columns: 2, sections: 1 },
  PX2: { rows: 1, columns: 2, sections: 1 },
};

const createShelf = (index: number): WizardShelf => ({
  shelfIndex: index,
  rowTypes: [null],
});

const createCabinet = (index: number): WizardCabinet => ({
  cabinetIndex: index,
  shelfCount: 1,
  groupColumnsCount: 3,
  shelves: [createShelf(1)],
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
          cabinet.groupColumnsCount >= 1 &&
          cabinet.shelves.length >= 1 &&
          cabinet.shelves.every(
            (shelf) => shelf.rowTypes.length >= 1 && shelf.rowTypes.every((type) => type !== null),
          ),
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

        for (let groupRow = 1; groupRow <= shelf.rowTypes.length; groupRow++) {
          const type = shelf.rowTypes[groupRow - 1]!;
          const layout = layoutMap[type];
          const countPerGroup = layout.rows * layout.columns;

          for (let groupColumn = 1; groupColumn <= cabinet.groupColumnsCount; groupColumn++) {
            const start = current;
            const end = current + countPerGroup - 1;

            groups.push({ groupRow, groupColumn, type, start, end });

            containersByType[type] += countPerGroup;
            totalSections += countPerGroup * layout.sections;
            current += countPerGroup;
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

  const addGroupRow = (cabinetIndex: number, shelfIndex: number) => {
    setCabinets((prev) =>
      prev.map((cabinet) =>
        cabinet.cabinetIndex !== cabinetIndex
          ? cabinet
          : {
              ...cabinet,
              shelves: cabinet.shelves.map((shelf) =>
                shelf.shelfIndex !== shelfIndex
                  ? shelf
                  : { ...shelf, rowTypes: [...shelf.rowTypes, null] },
              ),
            },
      ),
    );
  };

  const updateShelfCount = (cabinetIndex: number, newCount: number) => {
    const count = Math.max(1, newCount);
    setCabinets((prev) =>
      prev.map((cabinet) => {
        if (cabinet.cabinetIndex !== cabinetIndex) return cabinet;

        const current = cabinet.shelves;
        const updated =
          count > current.length
            ? [
                ...current,
                ...Array.from({ length: count - current.length }, (_, i) =>
                  createShelf(current.length + i + 1),
                ),
              ]
            : current.slice(0, count);

        return { ...cabinet, shelfCount: count, shelves: updated };
      }),
    );
  };

  const updateGroupColumnsCount = (cabinetIndex: number, newCount: number) => {
    const count = Math.max(1, newCount);
    setCabinets((prev) =>
      prev.map((cabinet) =>
        cabinet.cabinetIndex !== cabinetIndex
          ? cabinet
          : { ...cabinet, groupColumnsCount: count },
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
        groupColumnsCount: cabinet.groupColumnsCount,
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

  const renderGroupTable = (groups: GroupPreview[]) => {
    const sorted = [...groups].sort((a, b) =>
      a.groupRow !== b.groupRow ? a.groupRow - b.groupRow : a.groupColumn - b.groupColumn,
    );

    const maxContainers = Math.max(...groups.map((g) => g.end - g.start + 1));

    return (
      <table className={styles.shelfTable}>
        <tbody>
          {sorted.map((group) => {
            const showType = (group.groupColumn - 1) % layoutMap[group.type].sections === 0;
            const count = group.end - group.start + 1;
            const numbers = Array.from({ length: count }, (_, i) => group.start + i);
            const padCount = maxContainers - count;

            return (
              <tr key={`${group.groupRow}-${group.groupColumn}`}>
                <td className={styles.typeCell}>{showType ? group.type : ''}</td>
                {numbers.map((num) => (
                  <td key={num} className={styles.numberCell}>
                    {num}
                  </td>
                ))}
                {Array.from({ length: padCount }, (_, i) => (
                  <td key={`pad-${i}`} className={styles.emptyCell} />
                ))}
              </tr>
            );
          })}
        </tbody>
      </table>
    );
  };

  return (
    <div className={styles.overlay}>
      <div className={styles.wizard}>
        <div className={styles.wizardHeader}>
          <h2>Storage Initialization</h2>
          <button className={styles.closeButton} onClick={onClose}>
            ✕
          </button>
        </div>

        <div className={styles.steps}>Step {step} of 3</div>

        {error && (
          <div className={styles.errorBar}>
            {(error as { data?: string })?.data ?? 'Initialization failed.'}
          </div>
        )}

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
                <div className={styles.cabinetConfig}>
                  <label className={styles.configLabel}>
                    Shelf count
                    <input
                      type="number"
                      className={styles.configInput}
                      value={cabinet.shelfCount}
                      min={1}
                      onChange={(e) =>
                        updateShelfCount(cabinet.cabinetIndex, parseInt(e.target.value, 10) || 1)
                      }
                    />
                  </label>
                  <label className={styles.configLabel}>
                    Container groups column count
                    <input
                      type="number"
                      className={styles.configInput}
                      value={cabinet.groupColumnsCount}
                      min={1}
                      onChange={(e) =>
                        updateGroupColumnsCount(
                          cabinet.cabinetIndex,
                          parseInt(e.target.value, 10) || 1,
                        )
                      }
                    />
                  </label>
                </div>
                {cabinet.shelves.map((shelf) => (
                  <div key={shelf.shelfIndex} className={styles.shelfBlock}>
                    <div className={styles.shelfTitle}>Shelf {shelf.shelfIndex}</div>
                    <div className={styles.shelfRowsArea}>
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
                      <button
                        className={styles.addRowButton}
                        onClick={() => addGroupRow(cabinet.cabinetIndex, shelf.shelfIndex)}
                        title="Add group row"
                      >
                        +
                      </button>
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
                    {renderGroupTable(shelf.groups)}
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
                {isLoading ? 'Initializing…' : 'Initialize Storage'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
