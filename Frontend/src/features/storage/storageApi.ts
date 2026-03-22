import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { ContainerDto, StorageInitializationRequest } from './storageSlice';

export const storageApi = createApi({
  reducerPath: 'storageApi',
  baseQuery: fetchBaseQuery({ baseUrl: '/api/storage' }),
  endpoints: (builder) => ({
    getCabinets: builder.query<number[], void>({
      query: () => '/cabinets',
    }),
    getCabinetContainers: builder.query<ContainerDto[], number>({
      query: (cabinetNumber) => `/cabinets/${cabinetNumber}/containers`,
    }),
    initializeStorage: builder.mutation<
      {
        containersCreated: number;
        sectionsCreated: number;
        containersByType: Record<string, number>;
      },
      StorageInitializationRequest
    >({
      query: (body) => ({
        url: '/init',
        method: 'POST',
        body,
      }),
    }),
  }),
});

export const {
  useGetCabinetsQuery,
  useGetCabinetContainersQuery,
  useInitializeStorageMutation,
} = storageApi;
