import { Configuration, PopupRequest } from '@azure/msal-browser';
import { environment } from '../config/environment';

/**
 * MSAL configuration for Azure AD B2C authentication
 */
export const msalConfig: Configuration = {
  auth: {
    clientId: environment.b2c.clientId,
    authority: environment.b2c.authority,
    knownAuthorities: environment.b2c.knownAuthorities,
    redirectUri: environment.b2c.redirectUri,
    postLogoutRedirectUri: environment.b2c.redirectUri,
    navigateToLoginRequestUrl: true
  },
  cache: {
    cacheLocation: 'sessionStorage', // Use sessionStorage for better security
    storeAuthStateInCookie: false
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case 0: // Error
            console.error(message);
            break;
          case 1: // Warning
            console.warn(message);
            break;
          case 2: // Info
            console.info(message);
            break;
          case 3: // Verbose
            console.debug(message);
            break;
        }
      }
    },
    allowNativeBroker: false // Disables WAM Broker
  }
};

/**
 * Scopes for acquiring access tokens
 */
export const loginRequest: PopupRequest = {
  scopes: environment.b2c.scopes
};

/**
 * Protected resource map for the API
 */
export const protectedResources = {
  apiUsers: {
    endpoint: `${environment.apiBaseUrl}/api/users`,
    scopes: environment.b2c.scopes
  }
};
