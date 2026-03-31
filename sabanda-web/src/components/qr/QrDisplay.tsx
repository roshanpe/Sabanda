import { useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { useAuth } from '../../auth/AuthContext';
import { UserRole } from '../../types/enums';

interface QrDisplayProps {
  token: string;
  size?: number;
  onRegenerate: () => Promise<string>;
}

export function QrDisplay({ token, size = 200, onRegenerate }: QrDisplayProps) {
  const { role } = useAuth();
  const [currentToken, setCurrentToken] = useState(token);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const canRegenerate =
    role === UserRole.Administrator || role === UserRole.PrimaryAccountHolder;

  const handleRegenerate = async () => {
    setLoading(true);
    setError(null);
    try {
      const newToken = await onRegenerate();
      setCurrentToken(newToken);
    } catch {
      setError('Failed to regenerate QR code.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ display: 'inline-flex', flexDirection: 'column', alignItems: 'center', gap: 12 }}>
      <QRCodeSVG value={currentToken} size={size} />
      {canRegenerate && (
        <button onClick={handleRegenerate} disabled={loading}>
          {loading ? 'Regenerating...' : 'Regenerate QR'}
        </button>
      )}
      {error && <p style={{ color: 'red' }}>{error}</p>}
    </div>
  );
}
