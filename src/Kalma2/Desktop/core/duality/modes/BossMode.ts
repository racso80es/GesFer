import { injectable } from 'inversify';
import { IOperationalMode, MODE_BOSS } from '../interfaces';

@injectable()
export class BossMode implements IOperationalMode {
  public name = MODE_BOSS;
  public isAutonomous = false;

  public async execute(): Promise<void> {
    console.log('[BOSS MODE] Waiting for user commands...');
    // In a real implementation, this would enable UI controls and disable automated loops.
  }

  public async activate(): Promise<void> {
    console.log('--------------------------------------------------');
    console.log('[BOSS MODE] ACTIVATED');
    console.log('System is under manual control.');
    console.log('--------------------------------------------------');
  }

  public async deactivate(): Promise<void> {
    console.log('[BOSS MODE] Deactivating...');
  }
}
