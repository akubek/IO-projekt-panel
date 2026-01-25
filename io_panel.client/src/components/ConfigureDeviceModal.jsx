import React, { useState, useEffect } from 'react';

/*
  ConfigureDeviceModal Component
  This modal handles the registration of a discovered virtual device into the system.
  It allows the Administrator to assign a custom "Display Name" to a device
  detected from the Simulator, effectively moving it from 'unconfigured' to 'active'.
*/
export default function ConfigureDeviceModal({ open, apiDevice, onClose, onAdd }) {
    // hooks unconditionally at top
    const [displayName, setDisplayName] = useState('');
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (!open || !apiDevice) {
            setDisplayName('');
            return;
        }

        setDisplayName(apiDevice.name ?? '');
    }, [apiDevice, open]);

    if (!open) return null;

    /**
     * Persists the device configuration to the Control Panel backend.
     * Links the simulator's hardware ID with the user's chosen display name.
     */
    async function handleAdd() {
        setSaving(true);

        const payload = {
            apiDeviceId: apiDevice.id,
            displayName: displayName
        };

        try {
            const resp = await fetch('/device', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (resp.ok) {
                const created = await resp.json();
                onAdd && onAdd(created);
                onClose && onClose();
            } else {
                const text = await resp.text();
                alert(`Failed to add device: ${resp.status} ${text}`);
            }
        } catch (err) {
            alert(`Error adding device: ${err}`);
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div className="bg-white rounded-lg shadow-lg p-6 w-11/12 max-w-lg">
                <div className="flex items-center mb-4">
                    <h3 className="text-lg font-semibold">Configure Device</h3>
                </div>

                <div className="space-y-4">
                    <label className="block text-sm">Display Name
                        <input className="mt-1 w-full border rounded px-2 py-1" value={displayName} onChange={e => setDisplayName(e.target.value)} />
                    </label>
                    
                    <div className="bg-slate-50 p-3 rounded-md border border-slate-200 text-sm">
                        <h4 className="font-semibold text-slate-700 mb-2">Device Properties (from API)</h4>
                        <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-slate-600">
                            <div><strong>Type:</strong> {apiDevice.type}</div>
                            <div><strong>Location:</strong> {apiDevice.location}</div>
                            <div><strong>Value:</strong> {apiDevice.state?.value} {apiDevice.state?.unit}</div>
                            <div><strong>Read-only:</strong> {apiDevice.config?.readonly ? 'Yes' : 'No'}</div>
                        </div>
                    </div>
                </div>

                <div className="mt-6 flex justify-end gap-2">
                    <button className="px-4 py-2 bg-slate-100 rounded" onClick={onClose} disabled={saving}>Cancel</button>
                    <button className="px-4 py-2 bg-blue-600 text-white rounded" onClick={handleAdd} disabled={saving}>
                        {saving ? 'Adding...' : 'Add device'}
                    </button>
                </div>
            </div>
        </div>
    );
}
