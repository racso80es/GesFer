import { injectable, inject } from 'inversify';
import { IAuditor, IImmutableStorage } from '../interfaces';
import { TYPES } from '../../di/types';

@injectable()
export class AuditorService implements IAuditor {
  private _storage: IImmutableStorage;

  constructor(
    @inject(TYPES.ImmutableStorage) storage: IImmutableStorage
  ) {
    this._storage = storage;
  }

  public async verify(record: unknown): Promise<boolean> {
    console.log('[AUDITOR] Verifying integrity of immutable records...');

    // In a real implementation, this would verify specific records against the blockchain hash.
    // For now, verify the integrity of the entire chain.
    const result = await this._storage.verifyIntegrity();

    if (result) {
      console.log('[AUDITOR] Integrity verified.');
    } else {
      console.error('[AUDITOR] Integrity verification FAILED.');
    }

    return result;
  }
}
