import { injectable, inject } from 'inversify';
import { IOperationalMode, MODE_CALM } from '../interfaces';
import { IConscience } from '../../conscience/interfaces';
import { TYPES } from '../../di/types';

@injectable()
export class CalmMode implements IOperationalMode {
  public name = MODE_CALM;
  public isAutonomous = true;

  constructor(
    @inject(TYPES.Conscience) private _conscience: IConscience
  ) {}

  public async execute(): Promise<void> {
    console.log('[CALM MODE] System is executing autonomously...');

    // Conscience Check before action
    const verdict = await this._conscience.judge({ action: 'autonomous_maintenance' });

    if (verdict.approved) {
      console.log('[CALM MODE] Action APPROVED. Executing...');
      // Simulate work
      await this._conscience.record({ action: 'autonomous_maintenance', status: 'success' });
    } else {
      console.warn(`[CALM MODE] Action REJECTED: ${verdict.reason}`);
      await this._conscience.record({ action: 'autonomous_maintenance', status: 'rejected', reason: verdict.reason });
    }
  }

  public async activate(): Promise<void> {
    console.log('--------------------------------------------------');
    console.log('[CALM MODE] ACTIVATED');
    console.log('System is autonomous under Conscience supervision.');
    console.log('--------------------------------------------------');
  }

  public async deactivate(): Promise<void> {
    console.log('[CALM MODE] Deactivating...');
  }
}
