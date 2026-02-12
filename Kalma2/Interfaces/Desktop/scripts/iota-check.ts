import { Client, utf8ToHex, Utils } from '@iota/sdk';

async function run() {
    console.log('Connecting to IOTA Testnet...');

    // IOTA Testnet (Stardust)
    const NODE_URL = 'https://api.testnet.iota.cafe';
    const EXPLORER_URL = 'https://explorer.iota.org/testnet';

    const client = new Client({
        nodes: [NODE_URL],
    });

    try {
        console.log('Connected to node.');

        // Generate a random mnemonic for signing the block (even if 0 value)
        const mnemonic = Utils.generateMnemonic();
        const secretManager = { mnemonic };

        const options = {
            tag: utf8ToHex('KALMA2_AUDIT'),
            data: utf8ToHex('Hello from Kalma2 Auditor! Timestamp: ' + new Date().toISOString()),
        };

        // Create block with tagged payload
        console.log('Building and posting block...');
        const [blockId, block] = await client.buildAndPostBlock(
            secretManager,
            options,
        );

        console.log('Block ID:', blockId);
        console.log(`Explore: ${EXPLORER_URL}/block/${blockId}`);

        console.log('SUCCESS: Auditor AP can register on IOTA Testnet.');

    } catch (error) {
        console.error('Error:', error);
        if (process.env.SIMULATE_IOTA === 'true') {
             console.log('[SIMULATION] IOTA Network Unreachable. Using Simulation.');
        }
    }
}

run().then(() => process.exit());
