/**
 * Environment configuration for different deployment environments
 */

export interface Environment {
  name: string;
  apiBaseUrl: string;
  apimSubscriptionKey: string;
  b2c: {
    authority: string;
    clientId: string;
    redirectUri: string;
    knownAuthorities: string[];
    scopes: string[];
  };
}

const environments: Record<string, Environment> = {
  development: {
    name: 'Development',
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'https://apim-userservice-dev.azure-api.net',
    apimSubscriptionKey: import.meta.env.VITE_APIM_SUBSCRIPTION_KEY || '',
    b2c: {
      authority: import.meta.env.VITE_B2C_AUTHORITY || 'https://userservicedev.b2clogin.com/userservicedev.onmicrosoft.com/B2C_1_signup_signin',
      clientId: import.meta.env.VITE_B2C_CLIENT_ID || '',
      redirectUri: import.meta.env.VITE_B2C_REDIRECT_URI || 'http://localhost:3000',
      knownAuthorities: [import.meta.env.VITE_B2C_KNOWN_AUTHORITY || 'userservicedev.b2clogin.com'],
      scopes: [import.meta.env.VITE_B2C_SCOPE || 'https://userservicedev.onmicrosoft.com/api/User.ReadWrite.All']
    }
  },
  staging: {
    name: 'Staging',
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'https://apim-userservice-staging.azure-api.net',
    apimSubscriptionKey: import.meta.env.VITE_APIM_SUBSCRIPTION_KEY || '',
    b2c: {
      authority: import.meta.env.VITE_B2C_AUTHORITY || 'https://userservice-staging.b2clogin.com/userservice-staging.onmicrosoft.com/B2C_1_signup_signin',
      clientId: import.meta.env.VITE_B2C_CLIENT_ID || '',
      redirectUri: import.meta.env.VITE_B2C_REDIRECT_URI || 'https://staging.userservice.com',
      knownAuthorities: [import.meta.env.VITE_B2C_KNOWN_AUTHORITY || 'userservice-staging.b2clogin.com'],
      scopes: [import.meta.env.VITE_B2C_SCOPE || 'https://userservice-staging.onmicrosoft.com/api/User.ReadWrite.All']
    }
  },
  production: {
    name: 'Production',
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL || 'https://apim-userservice-prod.azure-api.net',
    apimSubscriptionKey: import.meta.env.VITE_APIM_SUBSCRIPTION_KEY || '',
    b2c: {
      authority: import.meta.env.VITE_B2C_AUTHORITY || 'https://userservice-prod.b2clogin.com/userservice-prod.onmicrosoft.com/B2C_1_signup_signin',
      clientId: import.meta.env.VITE_B2C_CLIENT_ID || '',
      redirectUri: import.meta.env.VITE_B2C_REDIRECT_URI || 'https://app.userservice.com',
      knownAuthorities: [import.meta.env.VITE_B2C_KNOWN_AUTHORITY || 'userservice-prod.b2clogin.com'],
      scopes: [import.meta.env.VITE_B2C_SCOPE || 'https://userservice-prod.onmicrosoft.com/api/User.ReadWrite.All']
    }
  }
};

const currentEnvironment = import.meta.env.VITE_ENVIRONMENT || 'development';

export const environment: Environment = environments[currentEnvironment];
