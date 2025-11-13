import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import { PublicClientApplication } from '@azure/msal-browser';
import { environment } from '../config/environment';
import { loginRequest } from '../auth/authConfig';

/**
 * API Service for making authenticated requests to the backend API through Azure APIM
 */
class ApiService {
  private axiosInstance: AxiosInstance;
  private msalInstance: PublicClientApplication | null = null;

  constructor() {
    this.axiosInstance = axios.create({
      baseURL: environment.apiBaseUrl,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
        'Ocp-Apim-Subscription-Key': environment.apimSubscriptionKey
      }
    });

    this.setupInterceptors();
  }

  /**
   * Initialize MSAL instance for token acquisition
   */
  public initializeMsal(msalInstance: PublicClientApplication): void {
    this.msalInstance = msalInstance;
  }

  /**
   * Setup request and response interceptors
   */
  private setupInterceptors(): void {
    // Request interceptor
    this.axiosInstance.interceptors.request.use(
      async (config: InternalAxiosRequestConfig) => {
        // Get access token from MSAL
        if (this.msalInstance) {
          try {
            const account = this.msalInstance.getAllAccounts()[0];
            if (account) {
              const response = await this.msalInstance.acquireTokenSilent({
                ...loginRequest,
                account: account
              });
              
              // Add Bearer token to request
              if (response.accessToken) {
                config.headers.Authorization = `Bearer ${response.accessToken}`;
              }
            }
          } catch (error) {
            console.error('Failed to acquire token silently', error);
            // If silent token acquisition fails, try interactive
            try {
              const response = await this.msalInstance.acquireTokenPopup(loginRequest);
              if (response.accessToken) {
                config.headers.Authorization = `Bearer ${response.accessToken}`;
              }
            } catch (interactiveError) {
              console.error('Failed to acquire token interactively', interactiveError);
            }
          }
        }

        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.axiosInstance.interceptors.response.use(
      (response: AxiosResponse) => {
        return response;
      },
      async (error) => {
        const originalRequest = error.config;

        // If 401 and we haven't retried yet, try to refresh token
        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;

          if (this.msalInstance) {
            try {
              const account = this.msalInstance.getAllAccounts()[0];
              if (account) {
                const response = await this.msalInstance.acquireTokenPopup(loginRequest);
                if (response.accessToken) {
                  originalRequest.headers.Authorization = `Bearer ${response.accessToken}`;
                  return this.axiosInstance(originalRequest);
                }
              }
            } catch (refreshError) {
              console.error('Token refresh failed', refreshError);
              // Redirect to login
              if (this.msalInstance) {
                await this.msalInstance.loginRedirect(loginRequest);
              }
            }
          }
        }

        return Promise.reject(error);
      }
    );
  }

  /**
   * Generic GET request
   */
  public async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.get<T>(url, config);
    return response.data;
  }

  /**
   * Generic POST request
   */
  public async post<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.post<T>(url, data, config);
    return response.data;
  }

  /**
   * Generic PUT request
   */
  public async put<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.put<T>(url, data, config);
    return response.data;
  }

  /**
   * Generic DELETE request
   */
  public async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.delete<T>(url, config);
    return response.data;
  }
}

export const apiService = new ApiService();
