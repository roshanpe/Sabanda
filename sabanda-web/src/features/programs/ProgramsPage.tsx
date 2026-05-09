import { useState, useEffect, type FormEvent } from 'react';
import { programsApi } from '../../api/programs.api';
import { usersApi } from '../../api/users.api';
import { useAuthStore } from '../../store/authStore';
import { useTenantStore } from '../../store/tenantStore';
import type { Program, Enrolment, User } from '../../types/domain.types';
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
  const [users, setUsers] = useState<User[]>([]);
  const [usersLoading, setUsersLoading] = useState(true);
  const [usersError, setUsersError] = useState<string | null>(null);
  const [createdProgram, setCreatedProgram] = useState<Program | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);

  const [programs, setPrograms] = useState<Program[]>([]);
  const [programsLoading, setProgramsLoading] = useState(true);
  const [programsError, setProgramsError] = useState<string | null>(null);

  const [programId, setProgramId] = useState('');
  const [memberId, setMemberId] = useState('');
  const [enrolResult, setEnrolResult] = useState<Enrolment | null>(null);
  const [enrolError, setEnrolError] = useState<string | null>(null);

  useEffect(() => {
    const loadUsers = async () => {
      try {
        setUsersLoading(true);
        setUsersError(null);
        console.log('Loading users...');
        console.log('Auth token:', useAuthStore.getState().token);
        console.log('Tenant slug:', useTenantStore.getState().tenantSlug);
        const allUsers = await usersApi.getAll();
        console.log('Users loaded:', allUsers);
        setUsers(allUsers);
      } catch (err) {
        const errorMsg = getErrorMessage(err);
        console.error('Failed to load users:', errorMsg);
        setUsersError(errorMsg);
      } finally {
        setUsersLoading(false);
      }
    };
    loadUsers();
  }, []);

  useEffect(() => {
    const loadPrograms = async () => {
      try {
        setProgramsLoading(true);
        setProgramsError(null);
        console.log('Loading programs...');
        console.log('Auth token:', useAuthStore.getState().token);
        console.log('Tenant slug:', useTenantStore.getState().tenantSlug);
        const allPrograms = await programsApi.getAll();
        console.log('Programs loaded:', allPrograms);
        setPrograms(allPrograms);
      } catch (err) {
        const errorMsg = getErrorMessage(err);
        console.error('Failed to load programs:', errorMsg);
        setProgramsError(errorMsg);
      } finally {
        setProgramsLoading(false);
      }
    };
    loadPrograms();
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
      // Refresh the programs list
      const allPrograms = await programsApi.getAll();
      setPrograms(allPrograms);
      // Clear the form
      setName('');
      setCapacity('');
      setDescription('');
      setAgeGroup('');
      setFrequency('');
      setVenue('');
      setDay('');
      setTime('');
      setCoordinatorUserId('');
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
          <select value={coordinatorUserId} onChange={(e) => setCoordinatorUserId(e.target.value)} disabled={usersLoading}>
            <option value="">{usersLoading ? 'Loading coordinators...' : 'Coordinator (optional)'}</option>
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.email} ({user.role})
              </option>
            ))}
          </select>
          {usersError && <p style={{ color: 'red', fontSize: '0.9em' }}>Error loading coordinators: {usersError}</p>}
          {!usersLoading && users.length === 0 && (
            <p style={{ color: '#666', fontSize: '0.9em' }}>
              No coordinators are available yet. Create a user first or assign an existing user as a coordinator.
            </p>
          )}
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
        <h2>All Programs</h2>
        {programsLoading && <p>Loading programs...</p>}
        {programsError && <p style={{ color: 'red' }}>Error loading programs: {programsError}</p>}
        {!programsLoading && !programsError && programs.length === 0 && <p>No programs found.</p>}
        {!programsLoading && !programsError && programs.length > 0 && (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: 16 }}>
            {programs.map((program) => (
              <div key={program.id} style={{ border: '1px solid #ddd', borderRadius: 8, padding: 16, background: '#fafafa' }}>
                <h3 style={{ margin: '0 0 8px 0' }}>{program.name}</h3>
                <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                  <strong>Capacity:</strong> {program.capacity}
                </p>
                {program.description && (
                  <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                    <strong>Description:</strong> {program.description}
                  </p>
                )}
                {program.ageGroup && (
                  <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                    <strong>Age Group:</strong> {program.ageGroup}
                  </p>
                )}
                {program.frequency && (
                  <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                    <strong>Frequency:</strong> {program.frequency}
                  </p>
                )}
                {program.venue && (
                  <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                    <strong>Venue:</strong> {program.venue}
                  </p>
                )}
                {program.day && (
                  <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                    <strong>Day:</strong> {program.day}
                  </p>
                )}
                {program.time && (
                  <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                    <strong>Time:</strong> {program.time}
                  </p>
                )}
                {program.coordinatorUserId && (
                  <p style={{ margin: '4px 0', fontSize: '0.9em' }}>
                    <strong>Coordinator ID:</strong> {program.coordinatorUserId}
                  </p>
                )}
                <p style={{ margin: '8px 0 0 0', fontSize: '0.8em', color: '#666' }}>
                  <code>{program.id}</code>
                </p>
              </div>
            ))}
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
