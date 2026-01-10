import React, { useMemo } from "react";
import AutomationCard from "./AutomationCard";

function AutomationList({ automations, isAdmin, onDelete, onToggleEnabled, devices, scenes }) {
    const deviceById = useMemo(() => {
        const map = new Map();
        (devices ?? []).forEach(d => map.set(d.id, d));
        return map;
    }, [devices]);

    const sceneById = useMemo(() => {
        const map = new Map();
        (scenes ?? []).forEach(s => map.set(s.id, s));
        return map;
    }, [scenes]);

    if (!automations || automations.length === 0) {
        return <p className="px-6 text-slate-500">No automations found. Automations will allow you to create smart home routines.</p>;
    }

    return (
        <div className="px-6 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {automations.map(automation => (
                <AutomationCard
                    key={automation.id}
                    automation={automation}
                    isAdmin={isAdmin}
                    onDelete={onDelete}
                    onToggleEnabled={onToggleEnabled}
                    deviceById={deviceById}
                    sceneById={sceneById}
                />
            ))}
        </div>
    );
}

export default AutomationList;