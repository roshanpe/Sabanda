import { type ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { RoleGuard } from '../../auth/RoleGuard';
import { UserRole } from '../../types/enums';

export function Layout({ children }: { children: ReactNode }) {
  const { logout, role } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div>
      <nav style={{ display: 'flex', gap: 16, padding: '12px 24px', background: '#1e293b', color: '#fff' }}>
        <Link to="/dashboard" style={{ color: '#fff', textDecoration: 'none', fontWeight: 600 }}>
          Sabanda
        </Link>
        <Link to="/families" style={{ color: '#cbd5e1', textDecoration: 'none' }}>Families</Link>
        <Link to="/memberships" style={{ color: '#cbd5e1', textDecoration: 'none' }}>Memberships</Link>
        <Link to="/programs" style={{ color: '#cbd5e1', textDecoration: 'none' }}>Programs</Link>
        <Link to="/events" style={{ color: '#cbd5e1', textDecoration: 'none' }}>Events</Link>
        <RoleGuard roles={[UserRole.Administrator, UserRole.EventCoordinator, UserRole.ProgramCoordinator]}>
          <Link to="/qr/scan" style={{ color: '#cbd5e1', textDecoration: 'none' }}>Scan QR</Link>
        </RoleGuard>
        <div style={{ marginLeft: 'auto', display: 'flex', gap: 12, alignItems: 'center' }}>
          <span style={{ color: '#94a3b8', fontSize: 13 }}>{role}</span>
          <button onClick={handleLogout} style={{ cursor: 'pointer' }}>Logout</button>
        </div>
      </nav>
      <main style={{ padding: '24px' }}>{children}</main>
    </div>
  );
}
