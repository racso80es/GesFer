import { describe, it, expect } from 'vitest';

describe('Sanity Check', () => {
    it('should pass basic math', () => {
        expect(1 + 1).toBe(2);
    });

    it('should support async', async () => {
        const result = await Promise.resolve(true);
        expect(result).toBe(true);
    });
});
