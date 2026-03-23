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
  bsxFileName?: string;
  bsxFileContentBase64?: string | null;
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
