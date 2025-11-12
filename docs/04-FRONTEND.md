# React Frontend Application

## Overview

The React frontend is a single-page application (SPA) built with TypeScript, using Material-UI for components and MSAL.js for Microsoft Entra External ID authentication.

## Technology Stack

- **React 18** - UI library
- **TypeScript 5.x** - Type safety
- **Vite** - Build tool and dev server
- **@azure/msal-browser** - Entra External ID authentication
- **@azure/msal-react** - React integration for MSAL
- **React Router v6** - Client-side routing
- **Material-UI (MUI) v5** - Component library
- **Axios** - HTTP client
- **React Query (TanStack Query)** - Server state management
- **Zustand** - Client state management
- **React Hook Form** - Form handling
- **Zod** - Schema validation

## Project Structure

```
src/frontend/order-app/
├── public/
│   ├── index.html
│   └── favicon.ico
├── src/
│   ├── components/
│   │   ├── auth/
│   │   │   ├── SignupForm.tsx
│   │   │   ├── SigninForm.tsx
│   │   │   ├── ProtectedRoute.tsx
│   │   │   └── AuthGuard.tsx
│   │   ├── layout/
│   │   │   ├── Header.tsx
│   │   │   ├── Navigation.tsx
│   │   │   ├── Footer.tsx
│   │   │   └── Layout.tsx
│   │   ├── user/
│   │   │   ├── UserProfile.tsx
│   │   │   └── UserProfileEdit.tsx
│   │   └── common/
│   │       ├── LoadingSpinner.tsx
│   │       ├── ErrorBoundary.tsx
│   │       └── Toast.tsx
│   ├── contexts/
│   │   └── AuthContext.tsx
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   ├── useUser.ts
│   │   └── useApi.ts
│   ├── services/
│   │   ├── apiClient.ts
│   │   ├── authService.ts
│   │   └── userService.ts
│   ├── config/
│   │   ├── msalConfig.ts
│   │   └── apiConfig.ts
│   ├── pages/
│   │   ├── Home.tsx
│   │   ├── Signup.tsx
│   │   ├── Signin.tsx
│   │   ├── Dashboard.tsx
│   │   └── NotFound.tsx
│   ├── types/
│   │   ├── user.ts
│   │   └── auth.ts
│   ├── utils/
│   │   ├── constants.ts
│   │   └── helpers.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── vite-env.d.ts
├── .env.example
├── .env.development
├── .env.production
├── package.json
├── tsconfig.json
├── vite.config.ts
└── Dockerfile
```

## Authentication Setup

### MSAL Configuration (`config/msalConfig.ts`)

```typescript
import { Configuration, PublicClientApplication } from '@azure/msal-browser';

const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_ENTRA_CLIENT_ID,
    authority: import.meta.env.VITE_ENTRA_AUTHORITY,
    redirectUri: import.meta.env.VITE_REDIRECT_URI || window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);

export const loginRequest = {
  scopes: ['openid', 'profile', 'email', 'User.Read'],
};

export const tokenRequest = {
  scopes: [`api://${import.meta.env.VITE_API_CLIENT_ID}/User.ReadWrite`],
};
```

### AuthContext (`contexts/AuthContext.tsx`)

```typescript
import React, { createContext, useContext, useEffect, useState } from 'react';
import { useMsal } from '@azure/msal-react';
import { AuthenticationResult } from '@azure/msal-browser';
import { userService } from '../services/userService';
import { User, AuthTokens } from '../types/user';

interface AuthContextType {
  user: User | null;
  tokens: AuthTokens | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  signin: (email: string, password: string) => Promise<void>;
  signup: (data: SignupData) => Promise<void>;
  signout: () => Promise<void>;
  refreshToken: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const { instance, accounts } = useMsal();
  const [user, setUser] = useState<User | null>(null);
  const [tokens, setTokens] = useState<AuthTokens | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    initializeAuth();
  }, [accounts]);

  const initializeAuth = async () => {
    try {
      // Check if user is already authenticated
      if (accounts.length > 0) {
        const tokenResponse = await acquireTokenSilent();
        if (tokenResponse) {
          await loadUserProfile(tokenResponse.accessToken);
        }
      }
    } catch (error) {
      console.error('Auth initialization error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const signin = async (email: string, password: string) => {
    try {
      // First, authenticate with Entra External ID
      const msalResponse = await instance.loginPopup({
        scopes: ['openid', 'profile', 'email'],
        loginHint: email,
      });

      // Then, call our backend to create session
      const response = await userService.signin({
        email,
        password,
        entraIdToken: msalResponse.idToken,
      });

      setUser(response.user);
      setTokens({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
      });

      // Store tokens
      localStorage.setItem('refreshToken', response.refreshToken);
    } catch (error) {
      console.error('Signin error:', error);
      throw error;
    }
  };

  const signup = async (data: SignupData) => {
    try {
      // First, authenticate with Entra External ID
      const msalResponse = await instance.loginPopup({
        scopes: ['openid', 'profile', 'email'],
        loginHint: data.email,
      });

      // Then, call our backend to create user
      const response = await userService.signup({
        ...data,
        entraExternalId: msalResponse.account?.localAccountId,
        entraIdToken: msalResponse.idToken,
      });

      setUser(response.user);
      setTokens({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
      });

      localStorage.setItem('refreshToken', response.refreshToken);
    } catch (error) {
      console.error('Signup error:', error);
      throw error;
    }
  };

  const signout = async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        await userService.signout(refreshToken);
      }

      await instance.logoutPopup();

      setUser(null);
      setTokens(null);
      localStorage.removeItem('refreshToken');
    } catch (error) {
      console.error('Signout error:', error);
    }
  };

  const refreshToken = async () => {
    try {
      const storedRefreshToken = localStorage.getItem('refreshToken');
      if (!storedRefreshToken) throw new Error('No refresh token');

      const response = await userService.refreshToken(storedRefreshToken);

      setTokens({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
      });

      localStorage.setItem('refreshToken', response.refreshToken);
    } catch (error) {
      console.error('Token refresh error:', error);
      await signout();
      throw error;
    }
  };

  const acquireTokenSilent = async (): Promise<AuthenticationResult | null> => {
    try {
      const account = accounts[0];
      const response = await instance.acquireTokenSilent({
        scopes: tokenRequest.scopes,
        account,
      });
      return response;
    } catch (error) {
      console.error('Silent token acquisition failed:', error);
      return null;
    }
  };

  const loadUserProfile = async (accessToken: string) => {
    try {
      const profile = await userService.getProfile(accessToken);
      setUser(profile);
    } catch (error) {
      console.error('Failed to load user profile:', error);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        tokens,
        isAuthenticated: !!user,
        isLoading,
        signin,
        signup,
        signout,
        refreshToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

## API Client with Interceptors

### API Client (`services/apiClient.ts`)

```typescript
import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import { useAuth } from '../contexts/AuthContext';

const apiClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

let refreshTokenPromise: Promise<void> | null = null;

// Request interceptor - Add access token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const tokens = JSON.parse(localStorage.getItem('tokens') || '{}');
    if (tokens.accessToken) {
      config.headers.Authorization = `Bearer ${tokens.accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - Handle token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 and not already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Prevent multiple simultaneous refresh requests
        if (!refreshTokenPromise) {
          refreshTokenPromise = refreshAccessToken();
        }

        await refreshTokenPromise;
        refreshTokenPromise = null;

        // Retry original request with new token
        const tokens = JSON.parse(localStorage.getItem('tokens') || '{}');
        originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed, redirect to login
        window.location.href = '/signin';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

async function refreshAccessToken(): Promise<void> {
  const refreshToken = localStorage.getItem('refreshToken');
  if (!refreshToken) {
    throw new Error('No refresh token available');
  }

  const response = await axios.post(
    `${import.meta.env.VITE_API_URL}/api/users/refresh`,
    { refreshToken }
  );

  const { accessToken, refreshToken: newRefreshToken } = response.data;

  localStorage.setItem('tokens', JSON.stringify({ accessToken }));
  localStorage.setItem('refreshToken', newRefreshToken);
}

export default apiClient;
```

## Protected Routes

### ProtectedRoute Component

```typescript
import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { CircularProgress, Box } from '@mui/material';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <CircularProgress />
      </Box>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/signin" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};
```

## Key Components

### SignupForm

```typescript
import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button, TextField, Box, Typography, Alert } from '@mui/material';
import { useAuth } from '../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

const signupSchema = z
  .object({
    email: z.string().email('Invalid email address'),
    password: z.string().min(8, 'Password must be at least 8 characters'),
    firstName: z.string().min(1, 'First name is required'),
    lastName: z.string().min(1, 'Last name is required'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords don't match",
    path: ['confirmPassword'],
  });

type SignupFormData = z.infer<typeof signupSchema>;

export const SignupForm: React.FC = () => {
  const { signup } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = React.useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<SignupFormData>({
    resolver: zodResolver(signupSchema),
  });

  const onSubmit = async (data: SignupFormData) => {
    try {
      setError(null);
      await signup(data);
      navigate('/dashboard');
    } catch (err: any) {
      setError(
        err.response?.data?.message || 'Signup failed. Please try again.'
      );
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ mt: 3 }}>
      <Typography variant="h4" gutterBottom>
        Create Account
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <TextField
        {...register('email')}
        label="Email"
        type="email"
        fullWidth
        margin="normal"
        error={!!errors.email}
        helperText={errors.email?.message}
      />

      <TextField
        {...register('firstName')}
        label="First Name"
        fullWidth
        margin="normal"
        error={!!errors.firstName}
        helperText={errors.firstName?.message}
      />

      <TextField
        {...register('lastName')}
        label="Last Name"
        fullWidth
        margin="normal"
        error={!!errors.lastName}
        helperText={errors.lastName?.message}
      />

      <TextField
        {...register('password')}
        label="Password"
        type="password"
        fullWidth
        margin="normal"
        error={!!errors.password}
        helperText={errors.password?.message}
      />

      <TextField
        {...register('confirmPassword')}
        label="Confirm Password"
        type="password"
        fullWidth
        margin="normal"
        error={!!errors.confirmPassword}
        helperText={errors.confirmPassword?.message}
      />

      <Button
        type="submit"
        variant="contained"
        fullWidth
        disabled={isSubmitting}
        sx={{ mt: 3, mb: 2 }}
      >
        {isSubmitting ? 'Creating Account...' : 'Sign Up'}
      </Button>
    </Box>
  );
};
```

## Environment Configuration

### .env.example

```bash
# API Configuration
VITE_API_URL=http://localhost:5001

# Entra External ID Configuration
VITE_ENTRA_CLIENT_ID=your-spa-client-id
VITE_ENTRA_AUTHORITY=https://login.microsoftonline.com/your-tenant-id/v2.0
VITE_ENTRA_TENANT_ID=your-tenant-id
VITE_API_CLIENT_ID=your-api-client-id
VITE_REDIRECT_URI=http://localhost:3000

# Application Insights (optional)
VITE_APPINSIGHTS_CONNECTION_STRING=
```

## Build and Deployment

### Development

```bash
npm install
npm run dev
```

### Production Build

```bash
npm run build
npm run preview  # Preview production build
```

### Docker Build

```dockerfile
# Dockerfile
FROM node:18-alpine AS builder

WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

**Next**: See [07-API-REFERENCE.md](07-API-REFERENCE.md) for complete API documentation.
