export interface IService {
  initialize(): Promise<void>;
  getName(): string;
}
