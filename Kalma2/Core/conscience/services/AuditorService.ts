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

  public async registerProcess(processId: string, data: unknown): Promise<string> {
    console.log(`[AUDITOR] Registering process '${processId}' on IOTA/Shimmer...`);

    // Create a structured payload
    const payload = {
        processId,
        data, // This could be a hash of the document or the raw data if small
        timestamp: new Date().toISOString(),
    };

    try {
        const result = await this._storage.append(payload);
        console.log(`[AUDITOR] Process '${processId}' registered. Reference: ${result}`);
        return result;
    } catch (error) {
        console.error(`[AUDITOR] Failed to register process '${processId}'.`, error);
        throw error; // Propagate or handle?
    }
  }
}
