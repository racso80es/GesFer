import { injectable } from 'inversify';
import { IJudge, IVerdict } from '../interfaces';

@injectable()
export class JudgeService implements IJudge {
  public async evaluate(context: unknown): Promise<IVerdict> {
    console.log('[JUDGE] Evaluating context:', context);

    // In a real implementation, this would contain complex logic, rules engine, or AI.
    // For now, we simulate a basic check: reject if context contains "error".

    const contextStr = JSON.stringify(context);
    if (contextStr.toLowerCase().includes('error') || contextStr.toLowerCase().includes('fail')) {
      return {
        approved: false,
        reason: 'Context indicates failure or error.',
        context,
        timestamp: new Date(),
      };
    }

    return {
      approved: true,
      reason: 'Context validated successfully.',
      context,
      timestamp: new Date(),
    };
  }
}
