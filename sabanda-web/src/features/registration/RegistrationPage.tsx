import { useState, type FormEvent } from 'react';
import { familiesApi } from '../../api/families.api';
import { membersApi } from '../../api/members.api';
import { RoleGuard } from '../../auth/RoleGuard';
import { QrDisplay } from '../../components/qr/QrDisplay';
import { UserRole } from '../../types/enums';
import type { Family, Member } from '../../types/domain.types';
import type { CreateMemberRequest } from '../../types/api.types';
import { getErrorMessage } from '../../utils/errorUtils';

const defaultMemberData: CreateMemberRequest = {
  fullName: '',
  dateOfBirth: '',
  gender: '',
  email: '',
  phone: '',
  consentGiven: false,
  consentGivenBy: '',
  consentGivenAt: '',
  occupation: '',
  businessName: '',
};

export function RegistrationPage() {
  const [displayName, setDisplayName] = useState('');
  const [primaryEmail, setPrimaryEmail] = useState('');
  const [primaryPassword, setPrimaryPassword] = useState('');
  const [familyResult, setFamilyResult] = useState<Family | null>(null);
  const [familyError, setFamilyError] = useState<string | null>(null);
  const [familyLoading, setFamilyLoading] = useState(false);
  const [familyQrToken, setFamilyQrToken] = useState<string | null>(null);
  const [familyQrError, setFamilyQrError] = useState<string | null>(null);
  const [familyQrLoading, setFamilyQrLoading] = useState(false);

  const [familyId, setFamilyId] = useState('');
  const [memberData, setMemberData] = useState<CreateMemberRequest>(defaultMemberData);
  const [memberResult, setMemberResult] = useState<Member | null>(null);
  const [memberError, setMemberError] = useState<string | null>(null);
  const [memberLoading, setMemberLoading] = useState(false);
  const [memberQrToken, setMemberQrToken] = useState<string | null>(null);
  const [memberQrError, setMemberQrError] = useState<string | null>(null);
  const [memberQrLoading, setMemberQrLoading] = useState(false);

  const handleCreateFamily = async (event: FormEvent) => {
    event.preventDefault();
    setFamilyError(null);
    setFamilyLoading(true);
    setFamilyQrToken(null);
    setFamilyQrError(null);

    try {
      const family = await familiesApi.create({
        displayName,
        primaryHolderEmail: primaryEmail,
        primaryHolderPassword: primaryPassword,
      });
      setFamilyResult(family);
      setFamilyId(family.id);
      setDisplayName('');
      setPrimaryEmail('');
      setPrimaryPassword('');
    } catch (error) {
      setFamilyError(getErrorMessage(error));
    } finally {
      setFamilyLoading(false);
    }
  };

  const handleGenerateFamilyQr = async (): Promise<string> => {
    if (!familyResult) {
      throw new Error('A family must be created first.');
    }

    setFamilyQrError(null);
    setFamilyQrLoading(true);

    try {
      const result = await familiesApi.regenerateQr(familyResult.id);
      setFamilyQrToken(result.token);
      return result.token;
    } catch (error) {
      const message = getErrorMessage(error);
      setFamilyQrError(message);
      throw error;
    } finally {
      setFamilyQrLoading(false);
    }
  };

  const handleAddMember = async (event: FormEvent) => {
    event.preventDefault();
    setMemberError(null);
    setMemberLoading(true);
    setMemberQrToken(null);
    setMemberQrError(null);

    try {
      const member = await familiesApi.createMember(familyId, {
        fullName: memberData.fullName,
        dateOfBirth: memberData.dateOfBirth,
        gender: memberData.gender || undefined,
        email: memberData.email || undefined,
        phone: memberData.phone || undefined,
        consentGiven: memberData.consentGiven,
        consentGivenBy: memberData.consentGivenBy || undefined,
        consentGivenAt: memberData.consentGivenAt || undefined,
        occupation: memberData.occupation || undefined,
        businessName: memberData.businessName || undefined,
      });
      setMemberResult(member);
      setMemberData(defaultMemberData);
    } catch (error) {
      setMemberError(getErrorMessage(error));
    } finally {
      setMemberLoading(false);
    }
  };

  const handleGenerateMemberQr = async (): Promise<string> => {
    if (!memberResult) {
      throw new Error('A member must be created first.');
    }

    setMemberQrError(null);
    setMemberQrLoading(true);

    try {
      const result = await membersApi.regenerateQr(memberResult.id);
      setMemberQrToken(result.token);
      return result.token;
    } catch (error) {
      const message = getErrorMessage(error);
      setMemberQrError(message);
      throw error;
    } finally {
      setMemberQrLoading(false);
    }
  };

  return (
    <div style={{ display: 'grid', gap: 32, maxWidth: 760 }}>
      <RoleGuard roles={[UserRole.Administrator]}>
        <section style={{ padding: 24, borderRadius: 10, background: '#f8fafc', border: '1px solid #cbd5e1' }}>
          <h2>Register a Family</h2>
          <form onSubmit={handleCreateFamily} style={{ display: 'grid', gap: 12, marginTop: 12 }}>
            <input
              placeholder="Family display name"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              required
            />
            <input
              type="email"
              placeholder="Primary holder email"
              value={primaryEmail}
              onChange={(e) => setPrimaryEmail(e.target.value)}
              required
            />
            <input
              type="password"
              placeholder="Primary holder password"
              value={primaryPassword}
              onChange={(e) => setPrimaryPassword(e.target.value)}
              required
            />
            {familyError && <p style={{ color: 'red' }}>{familyError}</p>}
            <button type="submit" disabled={familyLoading}>
              {familyLoading ? 'Registering...' : 'Register Family'}
            </button>
          </form>

          {familyResult && (
            <div style={{ marginTop: 16, padding: 16, background: '#ecfccb', borderRadius: 8 }}>
              <strong>Family created:</strong>
              <div>{familyResult.displayName}</div>
              <div>ID: <code>{familyResult.id}</code></div>
              <button
                type="button"
                onClick={handleGenerateFamilyQr}
                disabled={familyQrLoading}
                style={{ marginTop: 12 }}
              >
                {familyQrLoading ? 'Generating QR...' : familyQrToken ? 'Regenerate QR' : 'Generate QR'}
              </button>
              {familyQrError && <p style={{ color: 'red' }}>{familyQrError}</p>}
              {familyQrToken && (
                <div style={{ marginTop: 16 }}>
                  <h3>Family QR Code</h3>
                  <QrDisplay token={familyQrToken} onRegenerate={handleGenerateFamilyQr} />
                </div>
              )}
            </div>
          )}
        </section>
      </RoleGuard>

      <section style={{ padding: 24, borderRadius: 10, background: '#f8fafc', border: '1px solid #cbd5e1' }}>
        <h2>Add a Family Member</h2>
        <form onSubmit={handleAddMember} style={{ display: 'grid', gap: 12, marginTop: 12 }}>
          <input
            placeholder="Family ID"
            value={familyId}
            onChange={(e) => setFamilyId(e.target.value)}
            required
          />
          <input
            placeholder="Member full name"
            value={memberData.fullName}
            onChange={(e) => setMemberData((prev) => ({ ...prev, fullName: e.target.value }))}
            required
          />
          <input
            type="date"
            placeholder="Date of birth"
            value={memberData.dateOfBirth}
            onChange={(e) => setMemberData((prev) => ({ ...prev, dateOfBirth: e.target.value }))}
            required
          />
          <input
            placeholder="Gender"
            value={memberData.gender}
            onChange={(e) => setMemberData((prev) => ({ ...prev, gender: e.target.value }))}
          />
          <input
            type="email"
            placeholder="Email (optional)"
            value={memberData.email}
            onChange={(e) => setMemberData((prev) => ({ ...prev, email: e.target.value }))}
          />
          <input
            placeholder="Phone (optional)"
            value={memberData.phone}
            onChange={(e) => setMemberData((prev) => ({ ...prev, phone: e.target.value }))}
          />
          <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <input
              type="checkbox"
              checked={memberData.consentGiven}
              onChange={(e) => setMemberData((prev) => ({ ...prev, consentGiven: e.target.checked }))}
            />
            Consent given
          </label>
          <input
            placeholder="Consent given by"
            value={memberData.consentGivenBy}
            onChange={(e) => setMemberData((prev) => ({ ...prev, consentGivenBy: e.target.value }))}
          />
          <input
            type="datetime-local"
            placeholder="Consent given at"
            value={memberData.consentGivenAt}
            onChange={(e) => setMemberData((prev) => ({ ...prev, consentGivenAt: e.target.value }))}
          />
          <input
            placeholder="Occupation (optional)"
            value={memberData.occupation}
            onChange={(e) => setMemberData((prev) => ({ ...prev, occupation: e.target.value }))}
          />
          <input
            placeholder="Business name (optional)"
            value={memberData.businessName}
            onChange={(e) => setMemberData((prev) => ({ ...prev, businessName: e.target.value }))}
          />
          {memberError && <p style={{ color: 'red' }}>{memberError}</p>}
          <button type="submit" disabled={memberLoading}>
            {memberLoading ? 'Adding member...' : 'Add Member'}
          </button>
        </form>

        {memberResult && (
          <div style={{ marginTop: 16, padding: 16, background: '#ecfccb', borderRadius: 8 }}>
            <strong>Member added:</strong>
            <div>{memberResult.fullName}</div>
            <div>ID: <code>{memberResult.id}</code></div>
            <button
              type="button"
              onClick={handleGenerateMemberQr}
              disabled={memberQrLoading}
              style={{ marginTop: 12 }}
            >
              {memberQrLoading ? 'Generating QR...' : memberQrToken ? 'Regenerate QR' : 'Generate QR'}
            </button>
            {memberQrError && <p style={{ color: 'red' }}>{memberQrError}</p>}
            {memberQrToken && (
              <div style={{ marginTop: 16 }}>
                <h3>Member QR Code</h3>
                <QrDisplay token={memberQrToken} onRegenerate={handleGenerateMemberQr} />
              </div>
            )}
          </div>
        )}
      </section>
    </div>
  );
}
