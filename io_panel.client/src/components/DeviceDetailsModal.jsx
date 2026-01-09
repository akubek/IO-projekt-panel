import React from 'react';

export default function DeviceDetailsModal({ open, device, onClose }) {
    if (!open || !device) return null;

    const detailItem = (label, value) => (
        <div className="py-2">
            <p className="text-xs font-medium text-slate-500">{label}</p>
            <p className="text-sm text-slate-800">{value ?? 'N/A'}</p>
        </div>
    );

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60" onClick={onClose}>
            <div className="bg-white rounded-xl shadow-2xl p-6 w-11/12 max-w-md" onClick={e => e.stopPropagation()}>
                <div className="flex items-start justify-between mb-4">
                    <div>
                        <h3 className="text-xl font-bold text-slate-900">{device.displayName}</h3>
                        <p className="text-sm text-slate-500 font-mono">{device.id}</p>
                    </div>
                    <button className="text-slate-400 hover:text-slate-600" onClick={onClose}>&times;</button>
                </div>

                <div className="space-y-3 max-h-[70vh] overflow-y-auto pr-2">
                    <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        {detailItem("Device Name (API)", device.deviceName)}
                        {detailItem("Type", device.type)}
                        {detailItem("Location", device.location)}
                        {detailItem("Status", device.status)}
                    </div>
                    <div className="border-t pt-3">
                        {detailItem("Description", device.description)}
                    </div>
                    <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        <h4 className="col-span-2 text-md font-semibold text-slate-700 mb-1">State</h4>
                        {detailItem("Value", `${device.state?.value} ${device.state?.unit || ''}`.trim())}
                    </div>
                    <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        <h4 className="col-span-2 text-md font-semibold text-slate-700 mb-1">Configuration</h4>
                        {detailItem("Read-Only", device.config?.readOnly ? 'Yes' : 'No')}
                        {detailItem("Min", device.config?.min)}
                        {detailItem("Max", device.config?.max)}
                        {detailItem("Step", device.config?.step)}
                    </div>
                     <div className="grid grid-cols-2 gap-x-4 border-t pt-3">
                        <h4 className="col-span-2 text-md font-semibold text-slate-700 mb-1">Metadata</h4>
                        {detailItem("Last Seen", new Date(device.lastSeen).toLocaleString())}
                        {detailItem("Created At (API)", device.createdAt ? new Date(device.createdAt).toLocaleString() : 'N/A')}
                    </div>
                </div>

                <div className="mt-6 flex justify-end">
                    <button className="px-4 py-2 bg-slate-100 rounded-md text-sm font-semibold" onClick={onClose}>Close</button>
                </div>
            </div>
        </div>
    );
}