import React from 'react';
import { Zap } from 'lucide-react';

function AutomationList({ automations }) {
    if (!automations || automations.length === 0) {
        return <p className="px-6 text-slate-500">No automations found. Automations will allow you to create smart home routines.</p>;
    }

    return (
        <div className="px-6 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {automations.map(automation => (
                <div key={automation.id} className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                    <div className="flex items-center gap-3">
                        <Zap className="w-5 h-5 text-yellow-500" />
                        <h3 className="text-lg font-bold text-slate-800">{automation.name}</h3>
                    </div>
                    <p className="text-sm text-slate-500 mt-2">
                        {automation.isEnabled ? 'Enabled' : 'Disabled'}
                    </p>
                    <p className="text-xs text-slate-400 mt-4 font-mono break-all">
                        Logic: {automation.logicDefinition || 'Not defined'}
                    </p>
                </div>
            ))}
        </div>
    );
}

export default AutomationList;