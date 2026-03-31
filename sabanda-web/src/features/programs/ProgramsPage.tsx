import { useState, type FormEvent } from 'react';
import { programsApi } from '../../api/programs.api';
import type { Program, Enrolment } from '../../types/domain.types';
import { getErrorMessage } from '../../utils/errorUtils';

export function ProgramsPage() {
  const [name, setName] = useState('');
  const [capacity, setCapacity] = useState('');
  const [description, setDescription] = useState('');
  const [createdProgram, setCreatedProgram] = useState<Program | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);

  const [programId, setProgramId] = useState('');
  const [memberId, setMemberId] = useState('');
  const [enrolResult, setEnrolResult] = useState<Enrolment | null>(null);
  const [enrolError, setEnrolError] = useState<string | null>(null);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    setCreateError(null);
    try {
      const p = await programsApi.create({ name, capacity: Number(capacity), description });
      setCreatedProgram(p);
    } catch (err) {
      setCreateError(getErrorMessage(err));
    }
  };

  const handleEnrol = async (e: FormEvent) => {
    e.preventDefault();
    setEnrolError(null);
    try {
      const enrolment = await programsApi.enrol(programId, { memberId });
      setEnrolResult(enrolment);
    } catch (err) {
      setEnrolError(getErrorMessage(err));
    }
  };

  return (
    <div style={{ maxWidth: 520, display: 'flex', flexDirection: 'column', gap: 32 }}>
      <div>
        <h2>Create Program</h2>
        <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input placeholder="Program name" value={name} onChange={(e) => setName(e.target.value)} required />
          <input
            type="number"
            placeholder="Capacity"
            value={capacity}
            onChange={(e) => setCapacity(e.target.value)}
            required
          />
          <textarea
            placeholder="Description (optional)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          {createError && <p style={{ color: 'red' }}>{createError}</p>}
          <button type="submit">Create Program</button>
        </form>
        {createdProgram && (
          <div style={{ marginTop: 12, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            <strong>{createdProgram.name}</strong> — Capacity: {createdProgram.capacity}
            <br /><code>{createdProgram.id}</code>
          </div>
        )}
      </div>

      <div>
        <h2>Enrol Member</h2>
        <form onSubmit={handleEnrol} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input placeholder="Program ID" value={programId} onChange={(e) => setProgramId(e.target.value)} required />
          <input placeholder="Member ID" value={memberId} onChange={(e) => setMemberId(e.target.value)} required />
          {enrolError && <p style={{ color: 'red' }}>{enrolError}</p>}
          <button type="submit">Enrol</button>
        </form>
        {enrolResult && (
          <div style={{ marginTop: 12, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            Status: <strong>{enrolResult.status}</strong>
            {enrolResult.waitlistPosition != null && <> — Waitlist position: {enrolResult.waitlistPosition}</>}
          </div>
        )}
      </div>
    </div>
  );
}
