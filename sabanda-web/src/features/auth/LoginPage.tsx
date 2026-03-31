import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { authApi } from '../../api/auth.api';
import { useTenantStore } from '../../store/tenantStore';
import { UserRole } from '../../types/enums';
import { getErrorMessage } from '../../utils/errorUtils';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const setTenantSlug = useTenantStore((s) => s.setTenantSlug);

  const [tenantSlug, setTenantSlugLocal] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      setTenantSlug(tenantSlug);
      const resp = await authApi.login({ email, password });
      login(resp.token, resp.userId, resp.role as UserRole, resp.expiresAt, resp.familyId);
      navigate('/dashboard');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 360, margin: '80px auto', display: 'flex', flexDirection: 'column', gap: 16 }}>
      <h1 style={{ textAlign: 'center' }}>Sabanda</h1>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
        <input
          placeholder="Organisation (tenant slug)"
          value={tenantSlug}
          onChange={(e) => setTenantSlugLocal(e.target.value)}
          required
        />
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        {error && <p style={{ color: 'red', margin: 0 }}>{error}</p>}
        <button type="submit" disabled={loading}>
          {loading ? 'Signing in...' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}
