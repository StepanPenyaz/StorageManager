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
