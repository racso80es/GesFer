import { injectable } from 'inversify';

export interface IGreetingService {
  getGreeting(): string;
}

@injectable()
export class GreetingService implements IGreetingService {
  getGreeting(): string {
    return 'Hello World from Kalma2 Desktop Service!';
  }
}
