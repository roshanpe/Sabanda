import { useState, type FormEvent } from 'react';
import { eventsApi } from '../../api/events.api';
import type { Event, Registration } from '../../types/domain.types';
import { EventBillingType } from '../../types/enums';
import { getErrorMessage } from '../../utils/errorUtils';
import { formatDateTime } from '../../utils/formatDate';

export function EventsPage() {
  const [name, setName] = useState('');
  const [eventDate, setEventDate] = useState('');
  const [capacity, setCapacity] = useState('');
  const [billingType, setBillingType] = useState<EventBillingType>(EventBillingType.Family);
  const [description, setDescription] = useState('');
  const [createdEvent, setCreatedEvent] = useState<Event | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);

  const [eventId, setEventId] = useState('');
  const [familyId, setFamilyId] = useState('');
  const [memberId, setMemberId] = useState('');
  const [regResult, setRegResult] = useState<Registration | null>(null);
  const [regError, setRegError] = useState<string | null>(null);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    setCreateError(null);
    try {
      const ev = await eventsApi.create({
        name,
        eventDate: new Date(eventDate).toISOString(),
        capacity: Number(capacity),
        billingType,
        description,
      });
      setCreatedEvent(ev);
    } catch (err) {
      setCreateError(getErrorMessage(err));
    }
  };

  const handleRegister = async (e: FormEvent) => {
    e.preventDefault();
    setRegError(null);
    try {
      const reg = await eventsApi.register(eventId, {
        familyId,
        memberId: memberId || undefined,
      });
      setRegResult(reg);
    } catch (err) {
      setRegError(getErrorMessage(err));
    }
  };

  return (
    <div style={{ maxWidth: 520, display: 'flex', flexDirection: 'column', gap: 32 }}>
      <div>
        <h2>Create Event</h2>
        <form onSubmit={handleCreate} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input placeholder="Event name" value={name} onChange={(e) => setName(e.target.value)} required />
          <input
            type="datetime-local"
            value={eventDate}
            onChange={(e) => setEventDate(e.target.value)}
            required
          />
          <input
            type="number"
            placeholder="Capacity"
            value={capacity}
            onChange={(e) => setCapacity(e.target.value)}
            required
          />
          <select value={billingType} onChange={(e) => setBillingType(e.target.value as EventBillingType)}>
            {Object.values(EventBillingType).map((t) => <option key={t}>{t}</option>)}
          </select>
          <textarea
            placeholder="Description (optional)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          {createError && <p style={{ color: 'red' }}>{createError}</p>}
          <button type="submit">Create Event</button>
        </form>
        {createdEvent && (
          <div style={{ marginTop: 12, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            <strong>{createdEvent.name}</strong> — {formatDateTime(createdEvent.eventDate)}
            <br />Capacity: {createdEvent.capacity} — Billing: {createdEvent.billingType}
          </div>
        )}
      </div>

      <div>
        <h2>Register for Event</h2>
        <form onSubmit={handleRegister} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <input placeholder="Event ID" value={eventId} onChange={(e) => setEventId(e.target.value)} required />
          <input placeholder="Family ID" value={familyId} onChange={(e) => setFamilyId(e.target.value)} required />
          <input
            placeholder="Member ID (required for Individual billing)"
            value={memberId}
            onChange={(e) => setMemberId(e.target.value)}
          />
          {regError && <p style={{ color: 'red' }}>{regError}</p>}
          <button type="submit">Register</button>
        </form>
        {regResult && (
          <div style={{ marginTop: 12, padding: 12, background: '#f0fdf4', borderRadius: 6 }}>
            Status: <strong>{regResult.status}</strong>
            {regResult.waitlistPosition != null && <> — Waitlist position: {regResult.waitlistPosition}</>}
          </div>
        )}
      </div>
    </div>
  );
}
