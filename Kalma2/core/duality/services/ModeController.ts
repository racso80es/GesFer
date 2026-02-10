import { injectable, inject } from 'inversify';
import { IModeController, IOperationalMode } from '../interfaces';
import { TYPES } from '../../di/types';

@injectable()
export class ModeController implements IModeController {
  private _modes: Map<string, IOperationalMode> = new Map();
  private _currentMode: IOperationalMode | null = null;

  constructor(
    @inject(TYPES.BossMode) bossMode: IOperationalMode,
    @inject(TYPES.CalmMode) calmMode: IOperationalMode
  ) {
    this.registerMode(bossMode);
    this.registerMode(calmMode);
    // Initial mode should be set explicitly, maybe by default BOSS?
  }

  public registerMode(mode: IOperationalMode): void {
    this._modes.set(mode.name, mode);
  }

  public async switchMode(modeName: string): Promise<void> {
    const newMode = this._modes.get(modeName);
    if (!newMode) {
      throw new Error(`Mode ${modeName} not registered.`);
    }

    if (this._currentMode) {
      await this._currentMode.deactivate();
    }

    this._currentMode = newMode;
    await this._currentMode.activate();
    // Execute immediately upon switch? Or wait for loop?
    // For now, execute once to signal start.
    await this._currentMode.execute();
  }

  public get currentMode(): IOperationalMode {
    if (!this._currentMode) throw new Error('No mode active');
    return this._currentMode;
  }
}
