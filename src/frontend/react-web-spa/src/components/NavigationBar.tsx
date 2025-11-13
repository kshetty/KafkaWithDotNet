import React from 'react';
import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { useMsal } from '@azure/msal-react';
import { useNavigate } from 'react-router-dom';
import LogoutIcon from '@mui/icons-material/Logout';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';

/**
 * Navigation bar component with user info and logout
 */
export const NavigationBar: React.FC = () => {
  const { instance, accounts } = useMsal();
  const navigate = useNavigate();

  const handleLogout = () => {
    instance.logoutRedirect({
      postLogoutRedirectUri: '/'
    });
  };

  const account = accounts[0];
  const userName = account?.name || account?.username || 'User';

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          User Service Admin
        </Typography>
        
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Button color="inherit" onClick={() => navigate('/users')}>
            Users
          </Button>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <AccountCircleIcon />
            <Typography variant="body2">{userName}</Typography>
          </Box>
          
          <Button
            color="inherit"
            onClick={handleLogout}
            startIcon={<LogoutIcon />}
          >
            Logout
          </Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
};
