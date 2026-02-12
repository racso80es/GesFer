export interface IVerdict {
  approved: boolean;
  reason: string;
  context: unknown;
  timestamp: Date;
}

export interface IJudge {
  evaluate(context: unknown): Promise<IVerdict>;
}

export interface IAuditor {
  verify(record: unknown): Promise<boolean>;
  /**
   * Registers a hash of a system process on the IOTA/Shimmer network.
   * @param processId Unique identifier of the process (e.g., 'SPEC-001').
   * @param data The data to hash and store.
   * @returns The Block ID or Transaction Hash.
   */
  registerProcess(processId: string, data: unknown): Promise<string>;
}

export interface IImmutableStorage {
  /**
   * Appends data to the immutable ledger.
   * @param data The data to store.
   * @returns The transaction hash or ID.
   */
  append(data: unknown): Promise<string>;

  /**
   * Verifies the integrity of the storage chain.
   * @returns True if integrity is preserved.
   */
  verifyIntegrity(): Promise<boolean>;
}

export interface IConscience {
  /**
   * Evaluates a decision context using the Judge.
   */
  judge(context: unknown): Promise<IVerdict>;

  /**
   * Audits the historical records using the Auditor.
   */
  audit(): Promise<boolean>;

  /**
   * Records an event in the immutable storage.
   */
  record(event: unknown): Promise<string>;
}
