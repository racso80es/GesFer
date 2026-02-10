export const MODE_BOSS = 'BOSS';
export const MODE_CALM = 'CALM';

export interface IOperationalMode {
  name: string;
  isAutonomous: boolean;

  /**
   * Executed when the mode is active.
   */
  execute(): Promise<void>;

  /**
   * Called when switching TO this mode.
   */
  activate(): Promise<void>;

  /**
   * Called when switching FROM this mode.
   */
  deactivate(): Promise<void>;
}

export interface IModeController {
  currentMode: IOperationalMode;
  switchMode(modeName: string): Promise<void>;
  registerMode(mode: IOperationalMode): void;
}
