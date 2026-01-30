// Tipos compartidos para entidades geográficas

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
