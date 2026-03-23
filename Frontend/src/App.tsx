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
  const theme = useSelector((state: RootState) => state.storage.theme);
  const { data: cabinets, isLoading, refetch } = useGetCabinetsQuery();

  useEffect(() => {
    if (cabinets && cabinets.length > 0 && selectedCabinet === null) {
      dispatch(selectCabinet(cabinets[0]));
    }
  }, [cabinets, selectedCabinet, dispatch]);

  useEffect(() => {
    document.body.setAttribute('data-theme', theme);
  }, [theme]);

  const handleInitClose = () => {
    setShowInit(false);
    refetch();
  };

  return (
    <div className={`appLayout ${theme === 'light' ? 'appLayoutLight' : ''}`}>
      <Header onOpenInit={() => setShowInit(true)} />
      {isLoading ? (
        <div className="loadingMessage">Loading storage data…</div>
      ) : (
        <>
          <CabinetTabs cabinets={cabinets ?? []} />
          {selectedCabinet !== null && <CabinetView cabinetNumber={selectedCabinet} />}
        </>
      )}
      {showInit && <StorageInitWizard onClose={handleInitClose} />}
    </div>
  );
}

export default App;
