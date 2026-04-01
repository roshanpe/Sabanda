import { useState } from 'react';
import { QrScanner } from '../../components/qr/QrScanner';
import { qrApi } from '../../api/qr.api';
import type { QrLookupResult } from '../../types/domain.types';
import { getErrorMessage } from '../../utils/errorUtils';

export function QrScanPage() {
  const [scanning, setScanning] = useState(false);
  const [token, setToken] = useState('');
  const [result, setResult] = useState<QrLookupResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleLookup = async (lookupToken: string) => {
    setScanning(false);
    setError(null);
    try {
      const data = await qrApi.lookup(lookupToken);
      setResult(data);
      setToken(lookupToken);
    } catch (err) {
      setResult(null);
      setError(getErrorMessage(err));
    }
  };

  const handleScanResult = async (scannedToken: string) => {
    await handleLookup(scannedToken);
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    await handleLookup(token);
  };

  return (
    <div style={{ maxWidth: 520 }}>
      <h2>Scan QR Code</h2>
      <p style={{ marginBottom: 16 }}>
        Use this page to scan a QR code generated during registration. The scanner reads the QR token and looks it up in the database.
      </p>

      {!scanning && (
        <button onClick={() => { setResult(null); setError(null); setScanning(true); }}>
          Start Scanner
        </button>
      )}

      {scanning && (
        <>
          <QrScanner onResult={handleScanResult} onError={(e) => setError(e.message)} />
          <button onClick={() => setScanning(false)} style={{ marginTop: 8 }}>
            Cancel
          </button>
        </>
      )}

      <section style={{ marginTop: 24, padding: 16, border: '1px solid #cbd5e1', borderRadius: 8, background: '#f8fafc' }}>
        <h3>Manual QR Token</h3>
        <form onSubmit={handleSubmit} style={{ display: 'grid', gap: 10 }}>
          <input
            placeholder="Paste QR token here"
            value={token}
            onChange={(e) => setToken(e.target.value)}
            style={{ width: '100%' }}
          />
          <button type="submit" disabled={!token.trim()}>
            Lookup Token
          </button>
        </form>
      </section>

      {error && <p style={{ color: 'red', marginTop: 16 }}>{error}</p>}

      {result && (
        <div style={{ marginTop: 16, padding: 16, background: '#f0fdf4', borderRadius: 6 }}>
          <strong>Scanned QR token:</strong>
          <div style={{ wordBreak: 'break-all', marginBottom: 12 }}>{token}</div>
          <strong>Type:</strong> {result.subjectType}<br />
          <strong>ID:</strong> <code>{result.subjectId}</code>
        </div>
      )}
    </div>
  );
}
