import 'reflect-metadata';
import { Container } from 'inversify';
import { TYPES } from './types';

// Core Services
import { IAuditor, IImmutableStorage, IJudge, IConscience } from '../conscience/interfaces';
import { AuditorService } from '../conscience/services/AuditorService';
import { JudgeService } from '../conscience/services/JudgeService';
import { ConscienceService } from '../conscience/services/ConscienceService';

// Infrastructure (Node.js)
import { IotaImmutableStorageNode } from '../conscience/infrastructure/IotaImmutableStorageNode';

// Duality (if needed in Main, likely not fully but for consistency)
// We might not need Duality services in Main for now, just Audit.

const containerNode = new Container();

// Bind Auditor
containerNode.bind<IAuditor>(TYPES.Auditor).to(AuditorService).inSingletonScope();

// Bind Storage (Node Implementation)
containerNode.bind<IImmutableStorage>(TYPES.ImmutableStorage).to(IotaImmutableStorageNode).inSingletonScope();

// Bind Judge/Conscience (if needed by Auditor or other flows, Auditor is standalone usually but good to have)
containerNode.bind<IJudge>(TYPES.Judge).to(JudgeService).inSingletonScope();
containerNode.bind<IConscience>(TYPES.Conscience).to(ConscienceService).inSingletonScope();

// Note: IWalletProvider must be bound by the consumer (Electron Main) as the implementation resides there.

export { containerNode };
export { TYPES } from './types';
