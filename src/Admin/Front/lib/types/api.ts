// Tipos para Auth
export interface LoginRequest {
  empresa: string;
  usuario: string;
  contraseña: string;
}

export interface LoginResponse {
  userId: string;
  username: string;
  firstName: string;
  lastName: string;
  companyId: string; // Puede estar vacío para admin global, o null
  companyName: string;
  email?: string; // Admin suele tener email
  role?: string; // Admin role
  permissions: string[];
  token: string; // JWT Token
  cursorId: string; // Cursor ID del usuario
}

// Tipos genéricos para respuestas de API
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}

export interface ApiResponse<T> {
  data?: T;
  error?: ApiError;
}
