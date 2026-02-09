import { injectable, inject } from 'inversify';
import { IConscience, IJudge, IAuditor, IImmutableStorage, IVerdict } from '../interfaces';
import { TYPES } from '../../di/types';

@injectable()
export class ConscienceService implements IConscience {
  private _judge: IJudge;
  private _auditor: IAuditor;
  private _storage: IImmutableStorage;

  constructor(
    @inject(TYPES.Judge) judge: IJudge,
    @inject(TYPES.Auditor) auditor: IAuditor,
    @inject(TYPES.ImmutableStorage) storage: IImmutableStorage
  ) {
    this._judge = judge;
    this._auditor = auditor;
    this._storage = storage;
  }

  public async judge(context: unknown): Promise<IVerdict> {
    console.log('[CONSCIENCE] Seeking judgement...');
    const verdict = await this._judge.evaluate(context);
    console.log(`[CONSCIENCE] Verdict: ${verdict.approved ? 'APPROVED' : 'REJECTED'}`);
    return verdict;
  }

  public async audit(): Promise<boolean> {
    console.log('[CONSCIENCE] Initiating audit...');
    return this._auditor.verify({});
  }

  public async record(event: unknown): Promise<string> {
    console.log('[CONSCIENCE] Recording event...');
    const hash = await this._storage.append(event);
    console.log(`[CONSCIENCE] Event recorded: ${hash}`);
    return hash;
  }
}
