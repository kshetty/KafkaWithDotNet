import React from 'react';
import { useMsal } from '@azure/msal-react';
import { Navigate } from 'react-router-dom';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

/**
 * Protected Route component that requires authentication
 */
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { accounts } = useMsal();

  if (accounts.length === 0) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};
