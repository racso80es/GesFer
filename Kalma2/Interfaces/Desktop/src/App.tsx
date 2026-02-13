import { useState, useEffect } from 'react'
import { container, TYPES } from '../../../Core/di/container'
import { IGreetingService } from './services/GreetingService'
import { IAuditor } from '../../../Core/conscience/interfaces'

// Import Project Configuration (MCP)
import gesferConfig from '../../../Projects/GesFer/initial.json'
import gesferServices from '../../../Projects/GesFer/services.json'

interface ActionButtonProps {
  onClick: () => void
  label: string
  icon: string
  variant?: 'default' | 'danger'
  disabled?: boolean
}

function App() {
  const [greeting, setGreeting] = useState<string>('')
  // Map of service status: serviceName -> boolean (true = UP, false = DOWN)
  const [serviceStatus, setServiceStatus] = useState<Record<string, boolean>>({})

  // MCP State
  const [auditStatus, setAuditStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle')
  const [auditHash, setAuditHash] = useState<string>('')
  const [iotaLink, setIotaLink] = useState<string | null>(null)

  useEffect(() => {
    // DI Resolution
    const greetingService = container.get<IGreetingService>(TYPES.GreetingService)
    setGreeting(greetingService.getGreeting())

    // Initial check
    checkAllServices();

    // Poll every 5 seconds
    const interval = setInterval(checkAllServices, 5000);
    return () => clearInterval(interval);
  }, [])

  const checkAllServices = async () => {
    const newStatus: Record<string, boolean> = {};
    for (const service of gesferServices) {
        if (service.verifyStatusUrl) {
            const isUp = await window.calmaAPI.checkStatus(service.verifyStatusUrl);
            newStatus[service.name] = isUp;
        }
    }
    setServiceStatus(prev => ({ ...prev, ...newStatus }));
  }

  // Group services by family
  const servicesByFamily = gesferServices.reduce((acc, service) => {
    const family = service.family || 'Other';
    if (!acc[family]) acc[family] = [];
    acc[family].push(service);
    return acc;
  }, {} as Record<string, typeof gesferServices>);

  const handleStartProduct = () => window.calmaAPI.startSequence(1)
  const handleStopAll = () => window.calmaAPI.stopAll()

  const runAudit = () => window.calmaAPI.runAudit()
  const clearCache = () => window.calmaAPI.clearCache()
  const syncSpec = () => window.calmaAPI.syncSpec()

  // Golden Action: Auditor AP Registration
  const handleIotaAudit = async () => {
    setAuditStatus('loading')
    setIotaLink(null)
    try {
        const auditor = container.get<IAuditor>(TYPES.Auditor)
        // Hash the current project config as the "Process" data
        const result = await auditor.registerProcess(`MCP-${gesferConfig.id.toUpperCase()}`, {
             config: gesferConfig,
             services: gesferServices,
             timestamp: Date.now()
        })

        setAuditHash(result)

        if (result.startsWith('iota:')) {
            const blockId = result.split(':')[1];
            // Shimmer Testnet Explorer
            setIotaLink(`https://explorer.shimmer.network/testnet/block/${blockId}`);
        }

        setAuditStatus('success')
    } catch (e) {
        console.error(e)
        setAuditStatus('error')
    }
  }

  const copyLink = () => {
      if (iotaLink) {
          navigator.clipboard.writeText(iotaLink);
          // Optional: Add toast notification here
      }
  }

  return (
    <div className="flex h-screen flex-col bg-background p-6">
      <header className="mb-8 flex items-center justify-between">
        <div>
           <h1 className="text-2xl font-bold tracking-tight text-white">Calma Desktop</h1>
           <div className="flex items-center gap-2 mt-1">
             <p className="text-sm text-green-400">{greeting}</p>
             <span className="text-gray-600">|</span>
             <span className="text-xs text-gray-400">Project: {gesferConfig.name} ({gesferConfig.version})</span>
           </div>
        </div>
        <div className="flex gap-2 items-center">
          <span className="h-2 w-2 rounded-full bg-gray-500"></span>
          <span className="text-xs text-gray-500">System Idle</span>
        </div>
      </header>

      {/* MCP Project View */}
      <section className="mb-6 p-4 rounded-xl border border-border bg-surface/50">
        <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-gray-300 uppercase tracking-wider">Active Project (MCP)</h3>
            <div className="flex items-center gap-2">
                {auditStatus === 'success' && (
                    <div className="flex items-center gap-2 text-green-400 bg-green-900/20 px-3 py-1 rounded-full border border-green-900/50">
                        <span className="text-lg">✅</span>
                        <span className="text-xs font-mono truncate max-w-[150px]" title={auditHash}>{auditHash}</span>
                        {iotaLink && (
                            <button onClick={copyLink} className="text-xs font-bold underline hover:text-green-300 ml-2" title="Copy Explorer Link">
                                🔗 LINK
                            </button>
                        )}
                        <span className="text-xs font-bold">VERIFIED</span>
                    </div>
                )}
                <button
                    onClick={handleIotaAudit}
                    disabled={auditStatus === 'loading'}
                    className={`px-3 py-1 rounded text-xs font-medium transition flex items-center gap-2
                        ${auditStatus === 'loading' ? 'bg-yellow-500/20 text-yellow-500 cursor-wait' : 'bg-blue-500/20 text-blue-400 hover:bg-blue-500/30'}
                    `}
                >
                    {auditStatus === 'loading' ? 'Registering on IOTA...' : 'Audit Process (IOTA)'}
                </button>
            </div>
        </div>
      </section>

      <main className="grid grid-cols-2 gap-6">
        {Object.entries(servicesByFamily).map(([family, services]) => (
             <div key={family} className="rounded-xl border border-border bg-surface p-6">
                <div className="flex items-center justify-between mb-4">
                    <h2 className="text-lg font-semibold text-primary">{family} Domain</h2>
                    {family === 'Product' && (
                        <button
                          onClick={handleStartProduct}
                          className="px-4 py-2 bg-primary/20 text-primary rounded-lg hover:bg-primary/30 transition text-sm font-medium"
                        >
                          Start Sequence
                        </button>
                    )}
                </div>
                <div className="space-y-3">
                    {services.map(service => {
                        const isUp = serviceStatus[service.name];
                        return (
                            <div key={service.name} className="flex justify-between items-center text-sm text-gray-400 p-2 rounded bg-background/50">
                                <div className="flex flex-col">
                                    <span className="font-medium text-gray-300">{service.name}</span>
                                    <span className="text-[10px] text-gray-500">{service.verifyStatusUrl}</span>
                                </div>
                                <div className="flex items-center gap-3">
                                     <span className={`text-xs font-mono font-bold ${isUp ? 'text-green-400' : 'text-red-400'}`}>
                                        {isUp ? 'ONLINE' : 'OFFLINE'}
                                     </span>
                                     {service.actions?.map(action => (
                                         <a
                                            key={action.label}
                                            href={action.url}
                                            target="_blank"
                                            rel="noreferrer"
                                            className={`px-2 py-1 rounded text-[10px] uppercase font-bold tracking-wider transition
                                                ${isUp
                                                    ? 'bg-blue-500/20 text-blue-400 hover:bg-blue-500/30 border border-blue-500/30'
                                                    : 'bg-gray-700/50 text-gray-600 cursor-not-allowed'}
                                            `}
                                            onClick={(e) => !isUp && e.preventDefault()}
                                         >
                                            {action.label}
                                         </a>
                                     ))}
                                </div>
                            </div>
                        )
                    })}
                </div>
             </div>
        ))}
      </main>

      <div className="mt-8">
        <h3 className="mb-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Quick Actions</h3>
        <div className="grid grid-cols-4 gap-4">
          <ActionButton onClick={runAudit} label="Run Full Audit" icon="🚀" />
          <ActionButton onClick={clearCache} label="Clear Docker Cache" icon="🧹" />
          <ActionButton onClick={syncSpec} label="Sync Spec" icon="🔄" />
          <ActionButton onClick={handleStopAll} label="Stop All Services" icon="🛑" variant="danger" />
        </div>
      </div>
    </div>
  )
}

function ActionButton({ onClick, label, icon, variant = 'default', disabled }: ActionButtonProps) {
  const base = "flex flex-col items-center gap-2 px-4 py-4 rounded-xl font-medium transition-all text-sm w-full justify-center border group"
  const styles = variant === 'danger'
    ? "bg-red-500/5 border-red-500/20 text-red-500 hover:bg-red-500/10 hover:border-red-500/30"
    : "bg-surface border-border hover:bg-zinc-800 text-gray-400 hover:text-white hover:border-zinc-700"

  return (
    <button onClick={onClick} disabled={disabled} className={`${base} ${styles} ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}>
      <span className="text-2xl mb-1 opacity-80 group-hover:opacity-100 transition-opacity">{icon}</span>
      <span className="text-xs">{label}</span>
    </button>
  )
}

export default App
