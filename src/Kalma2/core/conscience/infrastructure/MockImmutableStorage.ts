import { injectable } from 'inversify';
import { IImmutableStorage } from '../interfaces';

/**
 * Simulates a Distributed Ledger Technology (DLT) storage like IOTA or Blockchain.
 * In a real scenario, this would interact with the Tangle or a Blockchain node.
 */
@injectable()
export class MockImmutableStorage implements IImmutableStorage {
  private _chain: { hash: string; previousHash: string; data: unknown; timestamp: number }[] = [];

  constructor() {
    // Genesis block
    this._chain.push({
      hash: '00000000000000000000000000000000',
      previousHash: '',
      data: 'GENESIS_BLOCK',
      timestamp: Date.now(),
    });
  }

  public async append(data: unknown): Promise<string> {
    const lastBlock = this._chain[this._chain.length - 1];
    const newBlock = {
      previousHash: lastBlock.hash,
      data,
      timestamp: Date.now(),
      hash: '',
    };
    newBlock.hash = this._calculateHash(newBlock);

    this._chain.push(newBlock);
    console.log(`[IOTA SIMULATION] Block Appended: ${newBlock.hash}`);
    return newBlock.hash;
  }

  public async verifyIntegrity(): Promise<boolean> {
    for (let i = 1; i < this._chain.length; i++) {
      const currentBlock = this._chain[i];
      const previousBlock = this._chain[i - 1];

      if (currentBlock.previousHash !== previousBlock.hash) {
        console.error(`[INTEGRITY FAILURE] Block ${i} previous hash mismatch.`);
        return false;
      }

      if (currentBlock.hash !== this._calculateHash(currentBlock)) {
        console.error(`[INTEGRITY FAILURE] Block ${i} hash invalid.`);
        return false;
      }
    }
    return true;
  }

  private _calculateHash(block: { previousHash: string; data: unknown; timestamp: number }): string {
    const str = block.previousHash + JSON.stringify(block.data) + block.timestamp;
    // Simple hash function for simulation (DJB2)
    let hash = 5381;
    for (let i = 0; i < str.length; i++) {
      hash = (hash * 33) ^ str.charCodeAt(i);
    }
    return (hash >>> 0).toString(16).padStart(32, '0');
  }
}
