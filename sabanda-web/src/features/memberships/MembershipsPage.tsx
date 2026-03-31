import { useState, type FormEvent } from 'react';
import { membershipsApi } from '../../api/memberships.api';
import type { Membership } from '../../types/domain.types';
import { MembershipType, PaymentStatus } from '../../types/enums';
import { getErrorMessage } from '../../utils/errorUtils';
import { formatDate } from '../../utils/formatDate';

export function MembershipsPage() {
  const [familyId, setFamilyId] = useState('');
  const [type, setType] = useState<MembershipType>(MembershipType.Program);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [result, setResult] = useState<Membership | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const [updateId, setUpdateId] = useState('');
  const [newStatus, setNewStatus] = useState<PaymentStatus>(PaymentStatus.Pending);
  const [updateResult, setUpdateResult] = useState<Membership | null>(null);
  const [updateError, setUpdateError] = useState<string | null>(null);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const m = await membershipsApi.create({ familyId, type, startDate, endDate });
      setResult(m);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (e: FormEvent) => {
    e.preventDefault();
    setUpdateError(null);
    try {
      const m = await membershipsApi.updatePaymentStatus(updateId, { newStatus });
      setUpdateResult(m);
    } catch (err) {
      setUpdateError(getErrorMessage(err));
    }
  };

  return (
    <div style={{ maxWidth: 520, display: 'flex', flexDirection: 'column', gap: 32 }}>
      <div>
        <h2>Create Membership</h2>
        <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input placeholder="Family ID" value={familyId} onChange={(e) => setFamilyId(e.target.value)} required />
          <select value={type} onChange={(e) => setType(e.target.value as MembershipType)}>
            {Object.values(MembershipType).map((t) => <option key={t}>{t}</option>)}
          </select>
          <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required />
          <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required />
          {error && <p style={{ color: 'red' }}>{error}</p>}
          <button type="submit" disabled={loading}>{loading ? 'Creating...' : 'Create'}</button>
        </form>
        {result && (
          <div style={{ marginTop: 12, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            ID: <code>{result.id}</code> — Status: <strong>{result.paymentStatus}</strong>
            <br />
            {formatDate(result.startDate)} → {formatDate(result.endDate)}
          </div>
        )}
      </div>

      <div>
        <h2>Update Payment Status</h2>
        <form onSubmit={handleUpdateStatus} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input placeholder="Membership ID" value={updateId} onChange={(e) => setUpdateId(e.target.value)} required />
          <select value={newStatus} onChange={(e) => setNewStatus(e.target.value as PaymentStatus)}>
            {Object.values(PaymentStatus).map((s) => <option key={s}>{s}</option>)}
          </select>
          {updateError && <p style={{ color: 'red' }}>{updateError}</p>}
          <button type="submit">Update Status</button>
        </form>
        {updateResult && (
          <div style={{ marginTop: 12, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            Updated: <strong>{updateResult.paymentStatus}</strong>
          </div>
        )}
      </div>
    </div>
  );
}
