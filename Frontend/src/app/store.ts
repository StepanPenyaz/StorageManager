import { configureStore } from '@reduxjs/toolkit';
import { storageApi } from '../features/storage/storageApi';
import storageReducer from '../features/storage/storageSlice';

export const store = configureStore({
  reducer: {
    [storageApi.reducerPath]: storageApi.reducer,
    storage: storageReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(storageApi.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
