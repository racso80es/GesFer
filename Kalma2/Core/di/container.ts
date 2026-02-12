import 'reflect-metadata';
import { Container } from 'inversify';
import { IGreetingService, GreetingService } from '../../Interfaces/Desktop/src/services/GreetingService';

// Conscience Imports
import { IJudge, IAuditor, IImmutableStorage, IConscience } from '../conscience/interfaces';
import { JudgeService } from '../conscience/services/JudgeService';
import { AuditorService } from '../conscience/services/AuditorService';
import { ConscienceService } from '../conscience/services/ConscienceService';
// import { MockImmutableStorage } from '../conscience/infrastructure/MockImmutableStorage';
import { IotaImmutableStorage } from '../conscience/infrastructure/IotaImmutableStorage';

// Duality Imports
import { IOperationalMode, IModeController } from '../duality/interfaces';
import { ModeController } from '../duality/services/ModeController';
import { BossMode } from '../duality/modes/BossMode';
import { CalmMode } from '../duality/modes/CalmMode';

import { TYPES } from './types';

const container = new Container();

// Existing
container.bind<IGreetingService>(TYPES.GreetingService).to(GreetingService);

// Conscience Bindings
container.bind<IJudge>(TYPES.Judge).to(JudgeService).inSingletonScope();
container.bind<IAuditor>(TYPES.Auditor).to(AuditorService).inSingletonScope();
// Using IotaImmutableStorage which includes fallback to simulation
container.bind<IImmutableStorage>(TYPES.ImmutableStorage).to(IotaImmutableStorage).inSingletonScope();
container.bind<IConscience>(TYPES.Conscience).to(ConscienceService).inSingletonScope();

// Duality Bindings
container.bind<IOperationalMode>(TYPES.BossMode).to(BossMode).inSingletonScope();
container.bind<IOperationalMode>(TYPES.CalmMode).to(CalmMode).inSingletonScope();
container.bind<IModeController>(TYPES.ModeController).to(ModeController).inSingletonScope();

export { container };
export { TYPES } from './types';
