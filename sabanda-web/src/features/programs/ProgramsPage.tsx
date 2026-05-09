import { useState, useEffect, type FormEvent } from 'react';
import { programsApi } from '../../api/programs.api';
import { membersApi } from '../../api/members.api';
import { useAuthStore } from '../../store/authStore';
import { useTenantStore } from '../../store/tenantStore';
import type { Program, Enrolment, Member } from '../../types/domain.types';
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
  const [coordinatorUserId, setCoordinatorUserId] = useState('');
  const [members, setMembers] = useState<Member[]>([]);
  const [membersLoading, setMembersLoading] = useState(true);
  const [membersError, setMembersError] = useState<string | null>(null);
  const [createdProgram, setCreatedProgram] = useState<Program | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);

  const [programId, setProgramId] = useState('');
  const [memberId, setMemberId] = useState('');
  const [enrolResult, setEnrolResult] = useState<Enrolment | null>(null);
  const [enrolError, setEnrolError] = useState<string | null>(null);

  useEffect(() => {
    const loadMembers = async () => {
      try {
        setMembersLoading(true);
        setMembersError(null);
        console.log('Loading members...');
        console.log('Auth token:', useAuthStore.getState().token);
        console.log('Tenant slug:', useTenantStore.getState().tenantSlug);
        const allMembers = await membersApi.getAll();
        console.log('Members loaded:', allMembers);
        setMembers(allMembers);
      } catch (err) {
        const errorMsg = getErrorMessage(err);
        console.error('Failed to load members:', errorMsg);
        setMembersError(errorMsg);
      } finally {
        setMembersLoading(false);
      }
    };
    loadMembers();
  }, []);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    setCreateError(null);
    try {
      const programData = {
        name,
        capacity: Number(capacity),
        description,
        ageGroup: ageGroup || undefined,
        frequency: frequency || undefined,
        venue: venue || undefined,
        day: day || undefined,
        time: time || undefined,
        // coordinatorUserId: coordinatorUserId || undefined,
      };
      console.log('Creating program with data:', programData);
      console.log('Auth token:', useAuthStore.getState().token);
      console.log('Tenant slug:', useTenantStore.getState().tenantSlug);
      const p = await programsApi.create(programData);
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
            <option value="Weekly">Weekly</option>
            <option value="Fortnightly">Fortnightly</option>
            <option value="Monthly">Monthly</option>
          </select>
          <input
            placeholder="Venue (optional)"
            value={venue}
            onChange={(e) => setVenue(e.target.value)}
          />
          <select value={coordinatorUserId} onChange={(e) => setCoordinatorUserId(e.target.value)} disabled={membersLoading}>
            <option value="">{membersLoading ? 'Loading coordinators...' : 'Coordinator (optional)'}</option>
            {members.map((member) => (
              <option key={member.id} value={member.id}>
                {member.fullName}
              </option>
            ))}
          </select>
          {membersError && <p style={{ color: 'red', fontSize: '0.9em' }}>Error loading members: {membersError}</p>}
          <p style={{ color: 'orange', fontSize: '0.9em' }}>Note: Coordinator selection is currently using members. This may need to be changed to users.</p>
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
            {createdProgram.coordinatorUserId && <>Coordinator ID: {createdProgram.coordinatorUserId}<br /></>}
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
