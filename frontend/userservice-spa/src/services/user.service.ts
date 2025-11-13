import { apiService } from './api.service';
import { User, CreateUserDto, UpdateUserDto, PaginatedResponse } from '../types/user';

/**
 * User Service for managing user-related API calls
 */
class UserService {
  private readonly baseUrl = '/api/users';

  /**
   * Get all users with pagination
   */
  public async getUsers(pageNumber: number = 1, pageSize: number = 10): Promise<PaginatedResponse<User>> {
    return apiService.get<PaginatedResponse<User>>(
      `${this.baseUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  }

  /**
   * Get user by ID
   */
  public async getUserById(id: string): Promise<User> {
    return apiService.get<User>(`${this.baseUrl}/${id}`);
  }

  /**
   * Create a new user
   */
  public async createUser(userData: CreateUserDto): Promise<User> {
    return apiService.post<User>(this.baseUrl, userData);
  }

  /**
   * Update an existing user
   */
  public async updateUser(id: string, userData: UpdateUserDto): Promise<User> {
    return apiService.put<User>(`${this.baseUrl}/${id}`, userData);
  }

  /**
   * Delete a user
   */
  public async deleteUser(id: string): Promise<void> {
    return apiService.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Search users by email
   */
  public async searchUsers(email: string): Promise<User[]> {
    return apiService.get<User[]>(`${this.baseUrl}/search?email=${encodeURIComponent(email)}`);
  }
}

export const userService = new UserService();
