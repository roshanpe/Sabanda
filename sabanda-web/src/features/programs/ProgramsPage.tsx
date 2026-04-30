import { useState, type FormEvent } from 'react';
import { programsApi } from '../../api/programs.api';
import type { Program, Enrolment } from '../../types/domain.types';
import type { ProgramFrequency, ProgramDay } from '../../types/enums';
import { getErrorMessage } from '../../utils/errorUtils';

export function ProgramsPage() {
  const [name, setName] = useState('');
  const [capacity, setCapacity] = useState('');
  const [description, setDescription] = useState('');
  const [ageGroup, setAgeGroup] = useState('');
  const [frequency, setFrequency] = useState<ProgramFrequency | ''>('');
  const [venue, setVenue] = useState('');
  const [day, setDay] = useState<ProgramDay | ''>('');
  const [time, setTime] = useState('');
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
      const p = await programsApi.create({
        name,
        capacity: Number(capacity),
        description,
        ageGroup: ageGroup || undefined,
        frequency: frequency || undefined,
        venue: venue || undefined,
        day: day || undefined,
        time: time || undefined,
      });
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
          <input
            placeholder="Age group (optional)"
            value={ageGroup}
            onChange={(e) => setAgeGroup(e.target.value)}
          />
          <select value={frequency} onChange={(e) => setFrequency(e.target.value as ProgramFrequency)}>
            <option value="">Frequency (optional)</option>
            <option value="weekly">Weekly</option>
            <option value="fortnightly">Fortnightly</option>
            <option value="monthly">Monthly</option>
          </select>
          <input
            placeholder="Venue (optional)"
            value={venue}
            onChange={(e) => setVenue(e.target.value)}
          />
          <select value={day} onChange={(e) => setDay(e.target.value as ProgramDay)}>
            <option value="">Day (optional)</option>
            <option value="Monday">Monday</option>
            <option value="Tuesday">Tuesday</option>
            <option value="Wednesday">Wednesday</option>
            <option value="Thursday">Thursday</option>
            <option value="Friday">Friday</option>
            <option value="Saturday">Saturday</option>
            <option value="Sunday">Sunday</option>
          </select>
          <input
            type="time"
            placeholder="Time (optional)"
            value={time}
            onChange={(e) => setTime(e.target.value)}
          />
          {createError && <p style={{ color: 'red' }}>{createError}</p>}
          <button type="submit">Create Program</button>
        </form>
        {createdProgram && (
          <div style={{ marginTop: 12, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            <strong>{createdProgram.name}</strong> — Capacity: {createdProgram.capacity}
            <br />
            {createdProgram.ageGroup && <>Age group: {createdProgram.ageGroup}<br /></>}
            {createdProgram.frequency && <>Frequency: {createdProgram.frequency}<br /></>}
            {createdProgram.venue && <>Venue: {createdProgram.venue}<br /></>}
            {createdProgram.day && <>Day: {createdProgram.day}<br /></>}
            {createdProgram.time && <>Time: {createdProgram.time}<br /></>}
            <code>{createdProgram.id}</code>
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
