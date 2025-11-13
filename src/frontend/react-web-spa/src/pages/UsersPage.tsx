import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  IconButton,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import { DataGrid, GridColDef, GridPaginationModel } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { NavigationBar } from '../components/NavigationBar';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { ErrorDisplay } from '../components/ErrorDisplay';
import { userService } from '../services/user.service';
import { User, CreateUserDto, UpdateUserDto } from '../types/user';

interface UserFormData {
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
}

/**
 * Users page with full CRUD operations
 */
export const UsersPage: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({
    pageSize: 10,
    page: 0,
  });
  const [totalCount, setTotalCount] = useState(0);

  // Dialog states
  const [openCreateDialog, setOpenCreateDialog] = useState(false);
  const [openEditDialog, setOpenEditDialog] = useState(false);
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<UserFormData>({
    email: '',
    firstName: '',
    lastName: '',
    isActive: true,
  });
  const [formErrors, setFormErrors] = useState<Partial<UserFormData>>({});

  // Load users
  useEffect(() => {
    loadUsers();
  }, [paginationModel]);

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await userService.getUsers(
        paginationModel.page + 1, // API is 1-indexed
        paginationModel.pageSize
      );
      setUsers(response.data);
      setTotalCount(response.totalCount);
    } catch (err) {
      setError('Failed to load users. Please try again.');
      console.error('Error loading users:', err);
    } finally {
      setLoading(false);
    }
  };

  // Handle create user
  const handleCreateClick = () => {
    setFormData({
      email: '',
      firstName: '',
      lastName: '',
      isActive: true,
    });
    setFormErrors({});
    setOpenCreateDialog(true);
  };

  const handleCreateSubmit = async () => {
    if (!validateForm()) return;

    try {
      const newUser: CreateUserDto = {
        email: formData.email,
        firstName: formData.firstName,
        lastName: formData.lastName,
        isActive: formData.isActive,
      };
      await userService.createUser(newUser);
      setOpenCreateDialog(false);
      await loadUsers();
    } catch (err) {
      setError('Failed to create user. Please try again.');
      console.error('Error creating user:', err);
    }
  };

  // Handle edit user
  const handleEditClick = (user: User) => {
    setSelectedUser(user);
    setFormData({
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      isActive: user.isActive,
    });
    setFormErrors({});
    setOpenEditDialog(true);
  };

  const handleEditSubmit = async () => {
    if (!validateForm() || !selectedUser) return;

    try {
      const updateData: UpdateUserDto = {
        email: formData.email,
        firstName: formData.firstName,
        lastName: formData.lastName,
        isActive: formData.isActive,
      };
      await userService.updateUser(selectedUser.id, updateData);
      setOpenEditDialog(false);
      await loadUsers();
    } catch (err) {
      setError('Failed to update user. Please try again.');
      console.error('Error updating user:', err);
    }
  };

  // Handle delete user
  const handleDeleteClick = (user: User) => {
    setSelectedUser(user);
    setOpenDeleteDialog(true);
  };

  const handleDeleteConfirm = async () => {
    if (!selectedUser) return;

    try {
      await userService.deleteUser(selectedUser.id);
      setOpenDeleteDialog(false);
      await loadUsers();
    } catch (err) {
      setError('Failed to delete user. Please try again.');
      console.error('Error deleting user:', err);
    }
  };

  // Form validation
  const validateForm = (): boolean => {
    const errors: Partial<UserFormData> = {};

    if (!formData.email.trim()) {
      errors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      errors.email = 'Invalid email format';
    }

    if (!formData.firstName.trim()) {
      errors.firstName = 'First name is required';
    }

    if (!formData.lastName.trim()) {
      errors.lastName = 'Last name is required';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // Data grid columns
  const columns: GridColDef[] = [
    { field: 'email', headerName: 'Email', flex: 1, minWidth: 200 },
    { field: 'firstName', headerName: 'First Name', flex: 1, minWidth: 150 },
    { field: 'lastName', headerName: 'Last Name', flex: 1, minWidth: 150 },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 100,
      renderCell: (params) => (
        <Box
          sx={{
            px: 2,
            py: 0.5,
            borderRadius: 1,
            bgcolor: params.value ? 'success.light' : 'error.light',
            color: params.value ? 'success.dark' : 'error.dark',
          }}
        >
          {params.value ? 'Active' : 'Inactive'}
        </Box>
      ),
    },
    {
      field: 'createdAt',
      headerName: 'Created',
      width: 180,
      renderCell: (params) => new Date(params.value).toLocaleString(),
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 120,
      sortable: false,
      renderCell: (params) => (
        <>
          <IconButton
            color="primary"
            onClick={() => handleEditClick(params.row as User)}
            size="small"
          >
            <EditIcon />
          </IconButton>
          <IconButton
            color="error"
            onClick={() => handleDeleteClick(params.row as User)}
            size="small"
          >
            <DeleteIcon />
          </IconButton>
        </>
      ),
    },
  ];

  return (
    <>
      <NavigationBar />
      <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
          <Typography variant="h4">Users</Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateClick}
          >
            Add User
          </Button>
        </Box>

        {error && <ErrorDisplay message={error} />}

        {loading ? (
          <LoadingSpinner message="Loading users..." />
        ) : (
          <Paper>
            <DataGrid
              rows={users}
              columns={columns}
              paginationModel={paginationModel}
              onPaginationModelChange={setPaginationModel}
              pageSizeOptions={[5, 10, 25, 50]}
              rowCount={totalCount}
              paginationMode="server"
              loading={loading}
              autoHeight
              disableRowSelectionOnClick
            />
          </Paper>
        )}

        {/* Create User Dialog */}
        <Dialog open={openCreateDialog} onClose={() => setOpenCreateDialog(false)} maxWidth="sm" fullWidth>
          <DialogTitle>Create New User</DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
              <TextField
                label="Email"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                error={!!formErrors.email}
                helperText={formErrors.email}
                fullWidth
                required
              />
              <TextField
                label="First Name"
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                error={!!formErrors.firstName}
                helperText={formErrors.firstName}
                fullWidth
                required
              />
              <TextField
                label="Last Name"
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                error={!!formErrors.lastName}
                helperText={formErrors.lastName}
                fullWidth
                required
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenCreateDialog(false)}>Cancel</Button>
            <Button onClick={handleCreateSubmit} variant="contained">
              Create
            </Button>
          </DialogActions>
        </Dialog>

        {/* Edit User Dialog */}
        <Dialog open={openEditDialog} onClose={() => setOpenEditDialog(false)} maxWidth="sm" fullWidth>
          <DialogTitle>Edit User</DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
              <TextField
                label="Email"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                error={!!formErrors.email}
                helperText={formErrors.email}
                fullWidth
                required
              />
              <TextField
                label="First Name"
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                error={!!formErrors.firstName}
                helperText={formErrors.firstName}
                fullWidth
                required
              />
              <TextField
                label="Last Name"
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                error={!!formErrors.lastName}
                helperText={formErrors.lastName}
                fullWidth
                required
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenEditDialog(false)}>Cancel</Button>
            <Button onClick={handleEditSubmit} variant="contained">
              Save
            </Button>
          </DialogActions>
        </Dialog>

        {/* Delete Confirmation Dialog */}
        <Dialog open={openDeleteDialog} onClose={() => setOpenDeleteDialog(false)}>
          <DialogTitle>Confirm Delete</DialogTitle>
          <DialogContent>
            <DialogContentText>
              Are you sure you want to delete user "{selectedUser?.email}"? This action cannot be undone.
            </DialogContentText>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenDeleteDialog(false)}>Cancel</Button>
            <Button onClick={handleDeleteConfirm} color="error" variant="contained">
              Delete
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </>
  );
};
