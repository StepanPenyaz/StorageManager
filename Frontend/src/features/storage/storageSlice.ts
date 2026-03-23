import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

export interface ContainerDto {
  number: number;
  type: string;
  shelf: number;
  groupRow: number;
  groupColumn: number;
  positionRow: number;
  positionColumn: number;
  totalSections: number;
  emptySections: number;
}

interface StorageState {
  selectedCabinet: number | null;
}

export type ContainerTypeOption = 'PX12' | 'PX6' | 'PX4' | 'PX2';

export interface ShelfConfig {
  shelfIndex: number;
  rowTypes: ContainerTypeOption[];
}

export interface CabinetConfig {
  cabinetIndex: number;
  groupColumnsCount: number;
  shelves: ShelfConfig[];
}

export interface StorageInitializationRequest {
  startIndex: number;
  cabinets: CabinetConfig[];
}

export interface BsxFilePathRequest {
  filePath: string;
}

export interface BsxFileMetadata {
  fileName: string;
  filePath: string;
  fileSizeBytes: number;
}

export interface BsxFileProcessingRequest {
  filePath: string;
  batchSize?: number;
}

export interface BsxFileProcessingResult {
  status: string;
  processedItemCount: number;
  createdLotCount: number;
  updatedLotCount: number;
  warningCount: number;
  errorCount: number;
  elapsedMilliseconds: number;
  summaryMessage: string;
  warnings: string[];
}

const initialState: StorageState = {
  selectedCabinet: null,
};

const storageSlice = createSlice({
  name: 'storage',
  initialState,
  reducers: {
    selectCabinet(state, action: PayloadAction<number>) {
      state.selectedCabinet = action.payload;
    },
  },
});

export const { selectCabinet } = storageSlice.actions;
export default storageSlice.reducer;
