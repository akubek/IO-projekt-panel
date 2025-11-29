import React from 'react';

export default function AddDeviceModal({ open, devices, onClose, onSelect }) {
    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div className="bg-white rounded-lg shadow-lg p-6 w-11/12 max-w-3xl">
                <div className="flex justify-between items-center mb-4">
                    <h3 className="text-lg font-semibold">Available Devices (external)</h3>
                    <button className="text-sm text-slate-500" onClick={onClose}>Close</button>
                </div>

                <div className="max-h-64 overflow-auto">
                    {(!devices || devices.length === 0) ? (
                        <p className="text-sm text-slate-500">No devices found.</p>
                    ) : (
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left">
                                    <th className="pr-4">Name</th>
                                    <th className="pr-4">Type</th>
                                    <th className="pr-4">Location</th>
                                    <th className="pr-4">Value</th>
                                    <th className="pr-4">Unit</th>
                                </tr>
                            </thead>
                            <tbody>
                                {devices.map((d, idx) => (
                                    <tr
                                        key={idx}
                                        className="border-t cursor-pointer hover:bg-slate-50"
                                        onClick={() => { console.log('click row', d); onSelect && onSelect(d); }}
                                        title="Click to configure and add this device"
                                    >
                                        <td className="py-2 pr-4">{d.name}</td>
                                        <td className="py-2 pr-4">{d.type}</td>
                                        <td className="py-2 pr-4">{d.location}</td>
                                        <td className="py-2 pr-4">{d.state?.value}</td>
                                        <td className="py-2 pr-4">{d.state?.unit}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                </div>

                <div className="mt-4 flex justify-end">
                    <button className="px-4 py-2 bg-slate-100 rounded mr-2" onClick={onClose}>Close</button>
                </div>
            </div>
        </div>
    );
}