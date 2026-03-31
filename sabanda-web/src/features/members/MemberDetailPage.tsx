import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { membersApi } from '../../api/members.api';
import type { Member } from '../../types/domain.types';
import { QrDisplay } from '../../components/qr/QrDisplay';
import { getErrorMessage } from '../../utils/errorUtils';
import { formatDate } from '../../utils/formatDate';

export function MemberDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [member, setMember] = useState<Member | null>(null);
  const [qrToken, setQrToken] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    membersApi.getById(id).then(setMember).catch((err) => setError(getErrorMessage(err)));
  }, [id]);

  if (error) return <p style={{ color: 'red' }}>{error}</p>;
  if (!member) return <p>Loading...</p>;

  const handleRegenerateQr = async (): Promise<string> => {
    const result = await membersApi.regenerateQr(member.id);
    setQrToken(result.token);
    return result.token;
  };

  return (
    <div style={{ maxWidth: 480 }}>
      <h2>{member.fullName}</h2>
      <p>Date of birth: {formatDate(member.dateOfBirth)} — {member.isAdult ? 'Adult' : 'Minor'}</p>
      {member.email && <p>Email: {member.email}</p>}
      {member.phone && <p>Phone: {member.phone}</p>}

      <h3>QR Code</h3>
      {qrToken ? (
        <QrDisplay token={qrToken} onRegenerate={handleRegenerateQr} />
      ) : (
        <button onClick={async () => {
          const result = await membersApi.regenerateQr(member.id);
          setQrToken(result.token);
        }}>
          Generate QR
        </button>
      )}
    </div>
  );
}
