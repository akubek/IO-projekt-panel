import React, { useState, useEffect } from 'react';


export default function ConfigureDeviceModal({ open, apiDevice, onClose, onAdd }) {
    const [name, setName] = useState('');
    const [type, setType] = useState('');
    const [location, setLocation] = useState('');
    const [value, setValue] = useState(0);
    const [unit, setUnit] = useState('');
    const [readOnly, setReadOnly] = useState(false);
    const [min, setMin] = useState(0);
    const [max, setMax] = useState(0);
    const [step, setStep] = useState(0);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (!open || !apiDevice) {
            setName('');
            setType('');
            setLocation('');
            setValue(0);
            setUnit('');
            setReadOnly(false);
            setMin(0);
            setMax(0);
            setStep(0);
            return;
        }

        setName(apiDevice.name ?? '');
        setType(apiDevice.type ?? '');
        setLocation(apiDevice.location ?? '');
        setValue(apiDevice.state?.value ?? 0);
        setUnit(apiDevice.state?.unit ?? '');
        setReadOnly(apiDevice.config?.readonly ?? false);
        setMin(apiDevice.config?.min ?? 0);
        setMax(apiDevice.config?.max ?? 0);
        setStep(apiDevice.config?.step ?? 0);
    }, [apiDevice, open]);

    if (!open) return null;

    async function handleAdd() {
        setSaving(true);
        const devicePayload = {
            name,
            type,
            location,
            description: apiDevice?.description ?? '',
            state: { value: Number(value), unit },
            config: { readonly: !!readOnly, min: Number(min), max: Number(max), step: Number(step) },
            lastSeen: new Date().toISOString(),
            status: 'Unknown',
            localization: location
        };

        try {
            const resp = await fetch('/device', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(devicePayload)
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
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-semibold">Configure Device</h3>
                    <button className="text-sm text-slate-500" onClick={onClose}>Close</button>
                </div>

                <div className="space-y-2">
                    <label className="block text-sm">Name
                        <input className="mt-1 w-full border rounded px-2 py-1" value={name} onChange={e => setName(e.target.value)} />
                    </label>
                    <label className="block text-sm">Type
                        <input className="mt-1 w-full border rounded px-2 py-1" value={type} onChange={e => setType(e.target.value)} />
                    </label>
                    <label className="block text-sm">Location
                        <input className="mt-1 w-full border rounded px-2 py-1" value={location} onChange={e => setLocation(e.target.value)} />
                    </label>
                    <label className="block text-sm">Initial Value
                        <input type="number" className="mt-1 w-full border rounded px-2 py-1" value={value} onChange={e => setValue(e.target.value)} />
                    </label>
                    <label className="block text-sm">Unit
                        <input className="mt-1 w-full border rounded px-2 py-1" value={unit} onChange={e => setUnit(e.target.value)} />
                    </label>
                    <label className="block text-sm">
                        <input type="checkbox" checked={readOnly} onChange={e => setReadOnly(e.target.checked)} />
                        <span className="ml-2">Read only</span>
                    </label>
                    <div className="grid grid-cols-3 gap-2">
                        <label className="block text-sm">Min
                            <input type="number" className="mt-1 w-full border rounded px-2 py-1" value={min} onChange={e => setMin(e.target.value)} />
                        </label>
                        <label className="block text-sm">Max
                            <input type="number" className="mt-1 w-full border rounded px-2 py-1" value={max} onChange={e => setMax(e.target.value)} />
                        </label>
                        <label className="block text-sm">Step
                            <input type="number" className="mt-1 w-full border rounded px-2 py-1" value={step} onChange={e => setStep(e.target.value)} />
                        </label>
                    </div>
                </div>

                <div className="mt-4 flex justify-end gap-2">
                    <button className="px-4 py-2 bg-slate-100 rounded" onClick={onClose} disabled={saving}>Cancel</button>
                    <button className="px-4 py-2 bg-blue-600 text-white rounded" onClick={handleAdd} disabled={saving}>
                        {saving ? 'Adding...' : 'Add device'}
                    </button>
                </div>
            </div>
        </div>
    );
}
