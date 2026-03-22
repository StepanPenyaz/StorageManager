import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState } from './app/store';
import { selectCabinet } from './features/storage/storageSlice';
import { useGetCabinetsQuery } from './features/storage/storageApi';
import { Header } from './components/Header/Header';
import { CabinetTabs } from './components/CabinetTabs/CabinetTabs';
import { CabinetView } from './components/CabinetView/CabinetView';
import { StorageInitWizard } from './components/StorageInitWizard/StorageInitWizard';
import './App.css';

function App() {
  const dispatch = useDispatch();
  const [showInit, setShowInit] = useState(false);
  const selectedCabinet = useSelector((state: RootState) => state.storage.selectedCabinet);
  const { data: cabinets, isLoading } = useGetCabinetsQuery();

  useEffect(() => {
    if (cabinets && cabinets.length > 0 && selectedCabinet === null) {
      dispatch(selectCabinet(cabinets[0]));
    }
  }, [cabinets, selectedCabinet, dispatch]);

  return (
    <div className="appLayout">
      <Header onOpenInit={() => setShowInit(true)} />
      {isLoading ? (
        <div className="loadingMessage">Loading storage data…</div>
      ) : (
        <>
          <CabinetTabs cabinets={cabinets ?? []} />
          {selectedCabinet !== null && <CabinetView cabinetNumber={selectedCabinet} />}
        </>
      )}
      {showInit && <StorageInitWizard onClose={() => setShowInit(false)} />}
    </div>
  );
}

export default App;
