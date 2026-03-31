import { useAuth } from '../../auth/AuthContext';

export function DashboardPage() {
  const { role } = useAuth();

  return (
    <div>
      <h2>Dashboard</h2>
      <p>Welcome! You are signed in as <strong>{role}</strong>.</p>
    </div>
  );
}
