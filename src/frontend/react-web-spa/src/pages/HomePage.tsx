import React from 'react';
import { Container, Typography, Box } from '@mui/material';
import { NavigationBar } from '../components/NavigationBar';

/**
 * Home page component
 */
export const HomePage: React.FC = () => {
  return (
    <>
      <NavigationBar />
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            Welcome to User Service Admin
          </Typography>
          <Typography variant="body1" paragraph>
            This is the administrative portal for managing users in the User Service application.
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Navigate to the Users section to view and manage user accounts.
          </Typography>
        </Box>
      </Container>
    </>
  );
};
