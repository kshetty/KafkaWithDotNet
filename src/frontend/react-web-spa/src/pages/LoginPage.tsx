import React from 'react';
import { useMsal } from '@azure/msal-react';
import { loginRequest } from '../auth/authConfig';
import { Box, Button, Container, Paper, Typography } from '@mui/material';
import LoginIcon from '@mui/icons-material/Login';

/**
 * Login page component
 */
export const LoginPage: React.FC = () => {
  const { instance } = useMsal();

  const handleLogin = () => {
    instance.loginRedirect(loginRequest);
  };

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Paper elevation={3} sx={{ p: 4, width: '100%' }}>
          <Typography component="h1" variant="h4" align="center" gutterBottom>
            User Service Admin
          </Typography>
          <Typography variant="body1" align="center" color="text.secondary" paragraph>
            Sign in with your Azure AD B2C account to access the admin portal
          </Typography>
          <Box sx={{ mt: 3 }}>
            <Button
              fullWidth
              variant="contained"
              size="large"
              startIcon={<LoginIcon />}
              onClick={handleLogin}
            >
              Sign In
            </Button>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};
