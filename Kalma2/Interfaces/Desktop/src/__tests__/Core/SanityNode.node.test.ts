import { describe, it, expect } from 'vitest';
import crypto from 'node:crypto';
import fs from 'node:fs';

describe('Node Environment Sanity Check', () => {
    it('should have access to Node.js crypto module', () => {
        const id = crypto.randomUUID();
        expect(id).toBeDefined();
        expect(typeof id).toBe('string');
    });

    it('should have access to Node.js fs module', () => {
        expect(fs).toBeDefined();
        expect(typeof fs.readFileSync).toBe('function');
    });

    it('should be running in node environment', () => {
        expect(typeof process).toBe('object');
        expect(process.versions.node).toBeDefined();
        // @ts-ignore
        expect(typeof window).toBe('undefined'); // verify we are NOT in jsdom
    });
});
