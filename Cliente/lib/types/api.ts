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
  companyId: string;
  companyName: string;
   userLanguageId?: string;
   companyLanguageId?: string;
   countryLanguageId?: string;
   effectiveLanguageId?: string;
  permissions: string[];
  token?: string; // Opcional porque la API actual no devuelve token
}

// Tipos para User
export interface User {
  id: string;
  companyId: string;
  companyName: string;
  username: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  address?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  languageId?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateUser {
  companyId: string;
  username: string;
  password: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  address?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  languageId?: string;
}

export interface UpdateUser {
  username: string;
  password?: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  address?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  languageId?: string;
  isActive: boolean;
}

// Tipos para Customer
export interface Customer {
  id: string;
  companyId: string;
  name: string;
  taxId?: string;
  address?: string;
  phone?: string;
  email?: string;
  sellTariffId?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCustomer {
  companyId: string;
  name: string;
  taxId?: string;
  address?: string;
  phone?: string;
  email?: string;
  sellTariffId?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
}

export interface UpdateCustomer {
  name: string;
  taxId?: string;
  address?: string;
  phone?: string;
  email?: string;
  sellTariffId?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  isActive: boolean;
}

// Tipos para State
export interface State {
  id: string;
  countryId: string;
  name: string;
  code?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateState {
  countryId: string;
  name: string;
  code?: string;
}

export interface UpdateState {
  name: string;
  code?: string;
  isActive: boolean;
}

// Tipos para Country
export interface Country {
  id: string;
  name: string;
  code?: string;
  languageId: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

// Tipos para City
export interface City {
  id: string;
  stateId: string;
  name: string;
  postalCode?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

// Tipos para Company
export interface Company {
  id: string;
  name: string;
  taxId?: string;
  address: string;
  phone?: string;
  email?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  languageId?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCompany {
  name: string;
  taxId?: string;
  address: string;
  phone?: string;
  email?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  languageId?: string;
}

export interface UpdateCompany {
  name: string;
  taxId?: string;
  address: string;
  phone?: string;
  email?: string;
  postalCodeId?: string;
  cityId?: string;
  stateId?: string;
  countryId?: string;
  languageId?: string;
  isActive: boolean;
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

