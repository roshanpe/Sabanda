import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './auth/AuthProvider';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { Layout } from './components/layout/Layout';
import { LoginPage } from './features/auth/LoginPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { FamiliesPage } from './features/families/FamiliesPage';
import { MemberDetailPage } from './features/members/MemberDetailPage';
import { MembershipsPage } from './features/memberships/MembershipsPage';
import { ProgramsPage } from './features/programs/ProgramsPage';
import { EventsPage } from './features/events/EventsPage';
import { QrScanPage } from './features/members/QrScanPage';
import { RegistrationPage } from './features/registration/RegistrationPage';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<Layout><DashboardPage /></Layout>} />
            <Route path="/registration" element={<Layout><RegistrationPage /></Layout>} />
            <Route path="/families" element={<Layout><FamiliesPage /></Layout>} />
            <Route path="/members/:id" element={<Layout><MemberDetailPage /></Layout>} />
            <Route path="/memberships" element={<Layout><MembershipsPage /></Layout>} />
            <Route path="/programs" element={<Layout><ProgramsPage /></Layout>} />
            <Route path="/events" element={<Layout><EventsPage /></Layout>} />
            <Route path="/qr/scan" element={<Layout><QrScanPage /></Layout>} />
          </Route>
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
