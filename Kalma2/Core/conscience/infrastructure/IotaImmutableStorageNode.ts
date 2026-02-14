import { injectable, inject } from 'inversify';
import { Client, SecretManager, utf8ToHex } from '@iota/sdk';
import { IImmutableStorage, IWalletProvider } from '../interfaces';
import { TYPES } from '../../di/types';

@injectable()
export class IotaImmutableStorageNode implements IImmutableStorage {
  private _client: Client | null = null;
  private _nodeUrl = 'https://api.testnet.iota.cafe'; // IOTA Rebased Testnet
  private _explorerUrl = 'https://explorer.iota.org/testnet';
  private _walletProvider: IWalletProvider;
  private _initialized = false;

  constructor(
    @inject(TYPES.WalletProvider) walletProvider: IWalletProvider
  ) {
    this._walletProvider = walletProvider;
    this._initializeClient();
  }

  private async _initializeClient() {
    try {
      this._client = new Client({
        nodes: [this._nodeUrl],
      });
      console.log('[IOTA STORAGE NODE] Client initialized successfully.');
      this._initialized = true;
    } catch (e) {
      console.warn('[IOTA STORAGE NODE] Failed to initialize client:', e);
      this._client = null;
    }
  }

  public async append(data: unknown): Promise<string> {
    if (!this._initialized) {
        await this._initializeClient();
    }

    const payloadString = JSON.stringify(data);
    const hash = this._calculateHash(payloadString);

    if (this._client) {
      try {
        console.log(`[IOTA STORAGE NODE] Attempting to register hash: ${hash}`);

        // Retrieve SecretManager from WalletProvider
        const secretManager = await this._walletProvider.getSecretManager() as SecretManager;

        if (!secretManager) {
            throw new Error('WalletProvider returned no SecretManager.');
        }

        const options = {
            tag: utf8ToHex('KALMA2_AUDIT'),
            data: utf8ToHex(payloadString),
        };

        const [blockId, block] = await this._client.buildAndPostBlock(secretManager as any, options);
        console.log(`[IOTA STORAGE NODE] Success! Block ID: ${blockId}`);
        return `iota:${blockId}`;
      } catch (error) {
        console.error('[IOTA STORAGE NODE] Failed to register on IOTA. Falling back to local simulation.', error);
      }
    } else {
        console.warn('[IOTA STORAGE NODE] Client not initialized. Falling back to local simulation.');
    }

    // Fallback Simulation
    return `sim:${hash}-${Date.now()}`;
  }

  public async verifyIntegrity(): Promise<boolean> {
    // Basic connectivity check
    if (!this._client) return false;
    try {
        const info = await this._client.getInfo();
        return !!info;
    } catch (e) {
        return false;
    }
  }

  private _calculateHash(str: string): string {
    let hash = 5381;
    for (let i = 0; i < str.length; i++) {
      hash = (hash * 33) ^ str.charCodeAt(i);
    }
    return (hash >>> 0).toString(16).padStart(32, '0');
  }
}
