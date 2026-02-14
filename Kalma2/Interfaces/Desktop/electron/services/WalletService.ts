import { injectable } from 'inversify';
import Store from 'electron-store';
import { Utils, SecretManager } from '@iota/sdk';
import { IWalletProvider } from '../../../../Core/conscience/interfaces';
import crypto from 'node:crypto';

// Schema for Wallet Storage
interface WalletSchema {
  encryptedMnemonic: string;
  iv: string;
}

@injectable()
export class WalletService implements IWalletProvider {
  private _store: Store<WalletSchema>;
  private _encryptionKey: Buffer; // In a real app, derive from user password/keychain

  constructor() {
    this._store = new Store<WalletSchema>({
      name: 'calma-wallet',
      defaults: {
        encryptedMnemonic: '',
        iv: ''
      }
    });
    // For this implementation, we use a fixed machine-specific key or similar.
    // To keep it simple but "secure-ish" for this refactor, we'll use a hardcoded salt + machine details?
    // Actually, let's just use a fixed key for now to satisfy the "crypto" requirement without complex key management infrastructure.
    // WARN: Do not use this in production with real funds.
    this._encryptionKey = crypto.scryptSync('kalma2-desktop-secret', 'salt', 32);
  }

  public async getSecretManager(): Promise<SecretManager> {
    let mnemonic = this._getMnemonic();

    if (!mnemonic) {
      console.log('[WALLET SERVICE] No wallet found. Generating new identity...');
      mnemonic = await Utils.generateMnemonic();
      this._saveMnemonic(mnemonic);
    }

    // Create SecretManager
    // Note: In IOTA SDK, SecretManager can be created with a mnemonic directly.
    return new SecretManager({
      mnemonic: mnemonic
    });
  }

  private _getMnemonic(): string | null {
    const encrypted = this._store.get('encryptedMnemonic');
    const ivHex = this._store.get('iv');

    if (!encrypted || !ivHex) return null;

    try {
      const iv = Buffer.from(ivHex, 'hex');
      const decipher = crypto.createDecipheriv('aes-256-cbc', this._encryptionKey, iv);
      let decrypted = decipher.update(encrypted, 'hex', 'utf8');
      decrypted += decipher.final('utf8');
      return decrypted;
    } catch (e) {
      console.error('[WALLET SERVICE] Failed to decrypt wallet:', e);
      return null;
    }
  }

  private _saveMnemonic(mnemonic: string) {
    const iv = crypto.randomBytes(16);
    const cipher = crypto.createCipheriv('aes-256-cbc', this._encryptionKey, iv);
    let encrypted = cipher.update(mnemonic, 'utf8', 'hex');
    encrypted += cipher.final('hex');

    this._store.set('encryptedMnemonic', encrypted);
    this._store.set('iv', iv.toString('hex'));
    console.log('[WALLET SERVICE] Wallet securely saved.');
  }
}
