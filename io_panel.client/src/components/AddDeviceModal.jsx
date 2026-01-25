import React from 'react';

/* AddDeviceModal Component
 This modal allows the Administrator to discover and select unconfigured virtual devices
 fetched from the Device Simulator.It serves as the primary interface for expanding
 the smart home system by linking "hardware" from the simulator to the Control Panel.
*/
export default function AddDeviceModal({ open, devices, onClose, onSelect }) {
    // Render nothing if the modal is not active
    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div className="bg-white rounded-lg shadow-lg p-6 w-11/12 max-w-3xl">
                <div className="flex items-center mb-4">
                    <h3 className="text-lg font-semibold">Available Unconfigured Devices</h3>
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
                                    <th className="pr-4">Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {devices.map((d, idx) => (
                                    <tr key={idx} className="border-t">
                                        <td className="py-2 pr-4">{d.name}</td>
                                        <td className="py-2 pr-4">{d.type}</td>
                                        <td className="py-2 pr-4">{d.location}</td>
                                        <td className="py-2 pr-4">{d.state?.value}</td>
                                        <td className="py-2 pr-4">{d.state?.unit}</td>
                                        <td className="py-2 pr-4">
                                            {/*Action to trigger the configuration process for the chosen device*/}
                                            <button
                                                type="button"
                                                onClick={(e) => { e.stopPropagation(); onSelect && onSelect(d); }}
                                                className="inline-flex items-center justify-center w-9 h-9 rounded-md bg-gradient-to-r from-blue-600 to-cyan-600 text-white shadow hover:from-blue-700 hover:to-cyan-700"
                                                aria-label={`Configure ${d.name}`}
                                                title="Add"
                                            >
                                                +
                                            </button>
                                        </td>
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