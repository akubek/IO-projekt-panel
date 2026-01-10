import React from 'react';
import { Trash2, Zap } from 'lucide-react';

function AutomationList({ automations, isAdmin, onDelete }) {
    if (!automations || automations.length === 0) {
        return <p className="px-6 text-slate-500">No automations found. Automations will allow you to create smart home routines.</p>;
    }

    return (
        <div className="px-6 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {automations.map(automation => (
                <div key={automation.id} className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
                    <div className="flex items-start justify-between gap-3">
                        <div className="flex items-center gap-3 min-w-0">
                            <Zap className="w-5 h-5 text-yellow-500 shrink-0" />
                            <h3 className="text-lg font-bold text-slate-800 truncate">{automation.name}</h3>
                        </div>

                        {isAdmin && (
                            <button
                                type="button"
                                onClick={() => onDelete && onDelete(automation.id)}
                                className="inline-flex items-center gap-2 px-3 py-2 rounded-md bg-gradient-to-r !text-white from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 shadow-lg"
                                aria-label={`Delete automation ${automation.name}`}
                            >
                                <Trash2 className="w-4 h-4" />
                                Delete
                            </button>
                        )}
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