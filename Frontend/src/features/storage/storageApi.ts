import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { ContainerDto } from './storageSlice';

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
  }),
});

export const { useGetCabinetsQuery, useGetCabinetContainersQuery } = storageApi;
