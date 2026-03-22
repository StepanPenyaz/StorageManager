import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState } from './app/store';
import { selectCabinet } from './features/storage/storageSlice';
import { useGetCabinetsQuery } from './features/storage/storageApi';
import { Header } from './components/Header/Header';
import { CabinetTabs } from './components/CabinetTabs/CabinetTabs';
import { CabinetView } from './components/CabinetView/CabinetView';
import './App.css';

function App() {
  const dispatch = useDispatch();
  const selectedCabinet = useSelector((state: RootState) => state.storage.selectedCabinet);
  const { data: cabinets, isLoading } = useGetCabinetsQuery();

  useEffect(() => {
    if (cabinets && cabinets.length > 0 && selectedCabinet === null) {
      dispatch(selectCabinet(cabinets[0]));
    }
  }, [cabinets, selectedCabinet, dispatch]);

  return (
    <div className="appLayout">
      <Header />
      {isLoading ? (
        <div className="loadingMessage">Loading storage data…</div>
      ) : (
        <>
          <CabinetTabs cabinets={cabinets ?? []} />
          {selectedCabinet !== null && <CabinetView cabinetNumber={selectedCabinet} />}
        </>
      )}
    </div>
  );
}

export default App;
