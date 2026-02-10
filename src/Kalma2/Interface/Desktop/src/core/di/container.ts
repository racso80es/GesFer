import 'reflect-metadata';
import { Container } from 'inversify';
import { IGreetingService, GreetingService } from '../../services/GreetingService';

export const TYPES = {
  GreetingService: Symbol.for('GreetingService'),
};

const container = new Container();
container.bind<IGreetingService>(TYPES.GreetingService).to(GreetingService);

export { container };
