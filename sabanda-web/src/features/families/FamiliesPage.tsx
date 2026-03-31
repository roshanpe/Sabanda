import { useState, type FormEvent } from 'react';
import { familiesApi } from '../../api/families.api';
import { RoleGuard } from '../../auth/RoleGuard';
import { UserRole } from '../../types/enums';
import type { Family } from '../../types/domain.types';
import { getErrorMessage } from '../../utils/errorUtils';

export function FamiliesPage() {
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [result, setResult] = useState<Family | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const family = await familiesApi.create({
        displayName,
        primaryHolderEmail: email,
        primaryHolderPassword: password,
      });
      setResult(family);
      setDisplayName('');
      setEmail('');
      setPassword('');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 480 }}>
      <h2>Families</h2>
      <RoleGuard roles={[UserRole.Administrator]}>
        <h3>Create Family</h3>
        <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input
            placeholder="Family display name"
            value={displayName}
            onChange={(e) => setDisplayName(e.target.value)}
            required
          />
          <input
            type="email"
            placeholder="Primary holder email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Primary holder password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          {error && <p style={{ color: 'red' }}>{error}</p>}
          <button type="submit" disabled={loading}>{loading ? 'Creating...' : 'Create Family'}</button>
        </form>
        {result && (
          <div style={{ marginTop: 16, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            <strong>Created:</strong> {result.displayName} <code>({result.id})</code>
          </div>
        )}
      </RoleGuard>
    </div>
  );
}
