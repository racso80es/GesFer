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
