import { useState } from 'react';
import { QrScanner } from '../../components/qr/QrScanner';
import { qrApi } from '../../api/qr.api';
import type { QrLookupResult } from '../../types/domain.types';
import { getErrorMessage } from '../../utils/errorUtils';

export function QrScanPage() {
  const [scanning, setScanning] = useState(false);
  const [result, setResult] = useState<QrLookupResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleScanResult = async (token: string) => {
    setScanning(false);
    setError(null);
    try {
      const data = await qrApi.lookup(token);
      setResult(data);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  return (
    <div style={{ maxWidth: 480 }}>
      <h2>Scan QR Code</h2>
      {!scanning && (
        <button onClick={() => { setResult(null); setError(null); setScanning(true); }}>
          Start Scanner
        </button>
      )}
      {scanning && (
        <>
          <QrScanner onResult={handleScanResult} onError={(e) => setError(e.message)} />
          <button onClick={() => setScanning(false)} style={{ marginTop: 8 }}>Cancel</button>
        </>
      )}
      {error && <p style={{ color: 'red', marginTop: 12 }}>{error}</p>}
      {result && (
        <div style={{ marginTop: 16, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
          <strong>Type:</strong> {result.subjectType}<br />
          <strong>ID:</strong> <code>{result.subjectId}</code>
        </div>
      )}
    </div>
  );
}
