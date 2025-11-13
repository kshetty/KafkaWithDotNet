# User Service SPA - React TypeScript Frontend

A modern React TypeScript single-page application for managing users in the User Service system. Built with Vite, Material-UI, and Azure AD B2C authentication.

## Features

- **Azure AD B2C Authentication**: Secure login with Microsoft Identity Platform
- **User Management**: Full CRUD operations (Create, Read, Update, Delete)
- **Material-UI Components**: Modern and responsive UI
- **TypeScript**: Type-safe development
- **MSAL.js Integration**: Microsoft Authentication Library for seamless token management
- **API Integration**: Connects to backend through Azure API Management
- **Pagination**: Server-side pagination for efficient data loading
- **Environment Configuration**: Support for dev, staging, and production environments

## Tech Stack

- **Framework**: React 18
- **Build Tool**: Vite 5
- **Language**: TypeScript 5
- **UI Library**: Material-UI (MUI) v5
- **Authentication**: @azure/msal-react, @azure/msal-browser
- **HTTP Client**: Axios
- **Routing**: React Router v6
- **Data Grid**: MUI X Data Grid

## Prerequisites

- Node.js 18+ and npm
- Azure AD B2C tenant configured
- Azure API Management instance
- APIM subscription key

## Project Structure

```
src/
├── auth/
│   └── authConfig.ts          # MSAL configuration
├── components/
│   ├── ErrorDisplay.tsx       # Error message component
│   ├── LoadingSpinner.tsx     # Loading indicator
│   ├── NavigationBar.tsx      # Top navigation bar
│   └── ProtectedRoute.tsx     # Route guard for authentication
├── config/
│   └── environment.ts         # Environment configuration
├── pages/
│   ├── HomePage.tsx           # Home/dashboard page
│   ├── LoginPage.tsx          # Login page
│   └── UsersPage.tsx          # User management page
├── services/
│   ├── api.service.ts         # Base API service with interceptors
│   └── user.service.ts        # User-specific API calls
├── types/
│   └── user.ts                # TypeScript interfaces
├── App.tsx                    # Main app component with routing
├── main.tsx                   # Application entry point
└── vite-env.d.ts              # Vite environment type definitions
```

## Installation

1. **Clone the repository** (if not already done)

2. **Navigate to the project directory**:

   ```bash
   cd frontend/userservice-spa
   ```

3. **Install dependencies**:

   ```bash
   npm install
   ```

4. **Configure environment variables**:

   ```bash
   cp .env.example .env
   ```

5. **Edit `.env` file** with your configuration:
   ```env
   VITE_ENVIRONMENT=development
   VITE_API_BASE_URL=https://apim-userservice-dev.azure-api.net
   VITE_APIM_SUBSCRIPTION_KEY=your-subscription-key
   VITE_B2C_AUTHORITY=https://yourtenantdev.b2clogin.com/yourtenantdev.onmicrosoft.com/B2C_1_signup_signin
   VITE_B2C_CLIENT_ID=your-client-id
   VITE_B2C_REDIRECT_URI=http://localhost:3000
   VITE_B2C_KNOWN_AUTHORITY=yourtenantdev.b2clogin.com
   VITE_B2C_SCOPE=https://yourtenantdev.onmicrosoft.com/api/User.ReadWrite.All
   ```

## Development

### Start Development Server

```bash
npm run dev
```

The application will start at `http://localhost:3000`

### Build for Production

```bash
npm run build
```

Build output will be in the `dist/` directory.

### Preview Production Build

```bash
npm run preview
```

### Lint Code

```bash
npm run lint
```

## Environment Configuration

The application supports multiple environments:

### Development

- Local development with hot reload
- Points to development APIM and B2C tenant

### Staging

- Pre-production testing
- Points to staging APIM and B2C tenant
- Uses Basic tier APIM

### Production

- Live production environment
- Points to production APIM and B2C tenant
- Uses Standard tier APIM

Create environment-specific `.env` files:

- `.env` - Development (default)
- `.env.staging` - Staging
- `.env.production` - Production

## Azure AD B2C Setup

1. **Register the SPA application** in Azure AD B2C
2. **Configure redirect URIs**:
   - Dev: `http://localhost:3000`
   - Staging: `https://staging.userservice.com`
   - Prod: `https://app.userservice.com`
3. **Configure API permissions**: Add the User.ReadWrite.All scope
4. **Enable implicit flow** if needed for token acquisition

## Azure API Management Configuration

1. **Get subscription key** from APIM portal
2. **Configure CORS** to allow your frontend domain
3. **Configure JWT validation policy** for B2C tokens
4. **Test API endpoints** with Postman before frontend integration

## Authentication Flow

1. User clicks "Sign In" button
2. Redirected to Azure AD B2C login page
3. After successful authentication, redirected back with authorization code
4. MSAL acquires access token
5. Access token included in all API requests
6. API service automatically refreshes tokens when needed

## API Integration

The application communicates with the backend through Azure API Management:

- **Base URL**: Configured per environment
- **Authentication**: Bearer token from Azure AD B2C
- **Subscription Key**: Required header for APIM
- **Endpoints**:
  - `GET /api/users` - Get paginated users
  - `GET /api/users/{id}` - Get user by ID
  - `POST /api/users` - Create user
  - `PUT /api/users/{id}` - Update user
  - `DELETE /api/users/{id}` - Delete user

## Features Implemented

### User Management Page

- **List Users**: Paginated data grid with sorting
- **Create User**: Form dialog with validation
- **Edit User**: Update user details
- **Delete User**: Confirmation dialog
- **Status Indicator**: Visual active/inactive status
- **Timestamps**: Display creation and update times

### Authentication

- **Protected Routes**: Require authentication
- **Automatic Token Refresh**: Silent token acquisition
- **Logout**: Clear session and redirect

### Error Handling

- **API Errors**: Display user-friendly messages
- **Token Expiry**: Automatic refresh or re-authentication
- **Network Errors**: Graceful degradation

## Troubleshooting

### CORS Errors

- Ensure APIM has CORS policy configured for your domain
- Check browser console for specific CORS messages

### Authentication Fails

- Verify B2C tenant configuration
- Check redirect URIs match exactly
- Ensure client ID is correct

### API Calls Fail

- Verify APIM subscription key is correct
- Check API base URL is correct
- Ensure JWT token is valid

### Build Errors

- Clear node_modules: `rm -rf node_modules package-lock.json && npm install`
- Check Node.js version: `node -v` (should be 18+)
- Verify all environment variables are set

## Deployment

### Azure Static Web Apps (Recommended)

1. Push code to GitHub
2. Create Static Web App in Azure Portal
3. Configure build:
   - App location: `frontend/userservice-spa`
   - Build command: `npm run build`
   - Output location: `dist`
4. Add environment variables in portal

### Azure Blob Storage + CDN

1. Build the application: `npm run build`
2. Upload `dist/` contents to Azure Blob Storage
3. Enable static website hosting
4. Configure Azure CDN (optional)

### Docker

```dockerfile
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## Security Considerations

- **Environment Variables**: Never commit `.env` files
- **Subscription Keys**: Rotate regularly
- **HTTPS**: Always use HTTPS in production
- **Token Storage**: MSAL uses sessionStorage by default
- **CORS**: Configure specific origins, avoid wildcards

## Performance Optimization

- **Code Splitting**: React Router lazy loading
- **Tree Shaking**: Vite automatically removes unused code
- **Compression**: Enable gzip/brotli on server
- **CDN**: Use CDN for static assets
- **Caching**: Configure cache headers

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Contributing

1. Create a feature branch
2. Make your changes
3. Run linting and tests
4. Submit a pull request

## License

Proprietary - All rights reserved

## Support

For issues or questions:

- Create an issue in the repository
- Contact the development team
- Check Azure documentation for B2C/APIM issues
