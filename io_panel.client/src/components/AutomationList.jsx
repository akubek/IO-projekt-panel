import React, { useMemo } from "react";
import AutomationCard from "./AutomationCard";

/*
  AutomationList Component
  This component serves as a grid container for all defined automation rules.
  Its primary responsibility is to prepare efficient data lookup maps(using useMemo)
  so that child cards can instantly resolve device and scene names from their IDs
  without re - scanning arrays on every render.
*/
function AutomationList({ automations, isAdmin, onDelete, onToggleEnabled, devices, scenes }) {
    // Creates a Map for O(1) lookup of device details by their ID
    const deviceById = useMemo(() => {
        const map = new Map();
        (devices ?? []).forEach(d => map.set(d.id, d));
        return map;
    }, [devices]);

    // Creates a Map for O(1) lookup of scene details by their ID
    const sceneById = useMemo(() => {
        const map = new Map();
        (scenes ?? []).forEach(s => map.set(s.id, s));
        return map;
    }, [scenes]);

    // Empty state handling to guide the user
    if (!automations || automations.length === 0) {
        return <p className="px-6 text-slate-500">No automations found. Automations will allow you to create smart home routines.</p>;
    }

    return (
        /* Responsive grid layout for AutomationCards */
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