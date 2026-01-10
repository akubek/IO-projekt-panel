import React from 'react';

export default function DeviceDetailsModal({ open, device, onClose }) {
    if (!open || !device) return null;

    const formatDate = (value) => {
        if (!value) return 'N/A';

        const d = new Date(value);
        return Number.isNaN(d.getTime()) ? String(value) : d.toLocaleString();
    };

    const formatBool = (value) => {
        if (value === true) return 'Yes';
        if (value === false) return 'No';
        return 'N/A';
    };

    const detailItem = (label, value) => (
        <div className="py-2">
            <p className="text-xs font-medium text-slate-500">{label}</p>
            <p className="text-sm text-slate-800 break-words">{value ?? 'N/A'}</p>
        </div>
    );

    const jsonBlock = (value) => (
        <pre className="text-xs bg-slate-50 border border-slate-200 rounded-md p-3 overflow-auto">
            {JSON.stringify(value ?? null, null, 2)}
        </pre>
    );

    const stateValueText = (() => {
        const v = device.state?.value;
        const u = device.state?.unit;
        if (v === null || v === undefined) return 'N/A';
        if (!u) return String(v);
        return `${v} ${u}`.trim();
    })();

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={onClose}>
            <div className="bg-white rounded-xl shadow-2xl p-6 w-11/12 max-w-md" onClick={e => e.stopPropagation()}>
                <div className="flex items-start justify-between mb-4">
                    <div>
                        <h3 className="text-xl font-bold text-slate-900">{device.displayName ?? 'N/A'}</h3>
                        <p className="text-sm text-slate-500 font-mono">{device.id ?? 'N/A'}</p>
                    </div>
                    <button className="text-slate-400 hover:text-slate-600" onClick={onClose}>&times;</button>
                </div>

                <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
                    {/* Core fields */}
                    <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        {detailItem("Display Name", device.displayName)}
                        {detailItem("Device ID", device.id)}
                        {detailItem("Device Name (API)", device.deviceName)}
                        {detailItem("Type", device.type)}
                        {detailItem("Location (API)", device.location)}
                        {detailItem("Localization (UI)", device.localization)}
                        {detailItem("Status", device.status)}
                        {detailItem("Malfunctioning", formatBool(device.malfunctioning))}
                    </div>

                    {/* Description */}
                    <div className="border-t pt-3">
                        {detailItem("Description", device.description)}
                    </div>

                    {/* State */}
                    <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        <h4 className="col-span-2 text-md font-semibold text-slate-700 mb-1">State</h4>
                        {detailItem("Value", stateValueText)}
                        {detailItem("Unit", device.state?.unit)}
                        <div className="col-span-2">
                            {jsonBlock(device.state)}
                        </div>
                    </div>

                    {/* Configuration */}
                    <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        <h4 className="col-span-2 text-md font-semibold text-slate-700 mb-1">Configuration</h4>
                        {detailItem("Read-Only", formatBool(device.config?.readOnly))}
                        {detailItem("Min", device.config?.min)}
                        {detailItem("Max", device.config?.max)}
                        {detailItem("Step", device.config?.step)}
                        <div className="col-span-2">
                            {jsonBlock(device.config)}
                        </div>
                    </div>

                    {/* Metadata */}
                    <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        <h4 className="col-span-2 text-md font-semibold text-slate-700 mb-1">Metadata</h4>
                        {detailItem("Last Seen", formatDate(device.lastSeen))}
                        {detailItem("Configured At", formatDate(device.configuredAt))}
                        {detailItem("Created At (API)", formatDate(device.createdAt))}
                    </div>

                    {/* Raw device payload (helps verify you truly see everything) */}
                    <div className="border-t pt-3">
                        <h4 className="text-md font-semibold text-slate-700 mb-2">Raw Device JSON</h4>
                        {jsonBlock(device)}
                    </div>
                </div>

                <div className="mt-6 flex justify-end">
                    <button className="px-4 py-2 bg-slate-100 rounded-md text-sm font-semibold" onClick={onClose}>Close</button>
                </div>
            </div>
        </div>
    );
}