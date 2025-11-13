import React from 'react';
import { Alert, AlertTitle, Box } from '@mui/material';

interface ErrorDisplayProps {
  title?: string;
  message: string;
}

/**
 * Error display component
 */
export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({ title = 'Error', message }) => {
  return (
    <Box sx={{ width: '100%', my: 2 }}>
      <Alert severity="error">
        <AlertTitle>{title}</AlertTitle>
        {message}
      </Alert>
    </Box>
  );
};
