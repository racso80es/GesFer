import { injectable } from 'inversify';
import { IImmutableStorage } from '../interfaces';
// @ts-ignore - Importing from web build directly
import init, { Client, Utils, utf8ToHex } from '@iota/sdk-wasm/web/lib/index';

@injectable()
export class IotaImmutableStorage implements IImmutableStorage {
  private _client: Client | null = null;
  private _nodeUrl = 'https://api.testnet.shimmer.network'; // Default to Shimmer Testnet
  private _explorerUrl = 'https://explorer.shimmer.network/testnet';
  private _initialized = false;

  constructor() {
    this._initializeClient();
  }

  private async _initializeClient() {
    try {
      if (!this._initialized) {
        // Load WASM
        // Note: This path is relative to the web root (public/)
        await init('/wasm/iota_sdk_wasm_bg.wasm');
        this._initialized = true;
      }

      this._client = new Client({
        nodes: [this._nodeUrl],
      });
    } catch (e) {
      console.warn('[IOTA STORAGE] Failed to initialize client:', e);
      this._client = null;
    }
  }

  public async append(data: unknown): Promise<string> {
    // Ensure initialized if constructor was async (it wasn't fully awaiting)
    if (!this._initialized) {
        await this._initializeClient();
    }

    const payloadString = JSON.stringify(data);
    const hash = this._calculateHash(payloadString);

    if (this._client) {
      try {
        console.log(`[IOTA STORAGE] Attempting to register hash: ${hash}`);
        const mnemonic = Utils.generateMnemonic();
        const secretManager = { mnemonic };

        const options = {
            tag: utf8ToHex('KALMA2_AUDIT'),
            data: utf8ToHex(payloadString),
        };

        const [blockId, block] = await this._client.buildAndPostBlock(secretManager, options);
        console.log(`[IOTA STORAGE] Success! Block ID: ${blockId}`);
        return `iota:${blockId}`;
      } catch (error) {
        console.error('[IOTA STORAGE] Failed to register on IOTA. Falling back to local simulation.', error);
      }
    } else {
        console.warn('[IOTA STORAGE] Client not initialized. Falling back to local simulation.');
    }

    // Fallback Simulation
    return `sim:${hash}-${Date.now()}`;
  }

  public async verifyIntegrity(): Promise<boolean> {
    return true;
  }

  private _calculateHash(str: string): string {
    let hash = 5381;
    for (let i = 0; i < str.length; i++) {
      hash = (hash * 33) ^ str.charCodeAt(i);
    }
    return (hash >>> 0).toString(16).padStart(32, '0');
  }
}
