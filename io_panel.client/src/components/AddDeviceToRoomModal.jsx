import React, { useMemo, useState } from "react";

/* AddDeviceToRoomModal Component
  This modal allows the Administrator to assign existing virtual devices to a specific room.
  It manages the organizational structure of the Smart Home by linking devices to rooms
  via the Control Panel API.
*/
export default function AddDeviceToRoomModal({ open, room, devices, authToken, onClose, onAdded }) {
    const [isBusy, setIsBusy] = useState(false);

    // Filters out devices that are already assigned to the current room to prevent duplicates
    const availableDevices = useMemo(() => {
        const assigned = new Set((room?.devices ?? []).map(d => d.id));
        return (devices ?? []).filter(d => !assigned.has(d.id));
    }, [devices, room]);

    if (!open || !room) return null;

    //Handles the API call to link a device to the room.
    //Communicates with the Control Panel backend to persist the room's new configuration.
    async function addDevice(deviceId) {
        try {
            setIsBusy(true);

            const headers = {};
            if (authToken) {
                headers.Authorization = `Bearer ${authToken}`;
            }

            const res = await fetch(`/room/${room.id}/devices/${deviceId}`, {
                method: "POST",
                headers
            });

            if (!res.ok) {
                const text = await res.text();
                console.error("Failed to add device to room:", res.status, text);
                return;
            }

            const addedDevice = (devices ?? []).find(d => d.id === deviceId);
            if (addedDevice && onAdded) {
                onAdded(room.id, addedDevice);
            }

            onClose && onClose();
        } catch (e) {
            console.error("Error adding device to room:", e);
        } finally {
            setIsBusy(false);
        }
    }

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div className="bg-white rounded-lg shadow-lg p-6 w-11/12 max-w-3xl">
                <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold">Add device to "{room.name}"</h3>
                    <button
                        className="px-4 py-2 rounded bg-slate-100 text-slate-800 hover:bg-slate-200 disabled:opacity-50"
                        onClick={onClose}
                        disabled={isBusy}
                    >
                        Close
                    </button>
                </div>

                {/* List of devices eligible for assignment */}
                <div className="max-h-72 overflow-auto">
                    {availableDevices.length === 0 ? (
                        <p className="text-sm text-slate-500">No available configured devices.</p>
                    ) : (
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-left">
                                    <th className="pr-4">Name</th>
                                    <th className="pr-4">Type</th>
                                    <th className="pr-4">Location</th>
                                    <th className="pr-4">Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                {availableDevices.map((d) => (
                                    <tr key={d.id} className="border-t">
                                        <td className="py-2 pr-4">{d.displayName ?? d.name ?? d.id}</td>
                                        <td className="py-2 pr-4">{d.type}</td>
                                        <td className="py-2 pr-4">{d.localization ?? d.location}</td>
                                        <td className="py-2 pr-4">
                                            <button
                                                type="button"
                                                disabled={isBusy}
                                                onClick={() => addDevice(d.id)}
                                                className="inline-flex items-center justify-center w-9 h-9 rounded-md bg-gradient-to-r from-blue-600 to-cyan-600 text-white shadow hover:from-blue-700 hover:to-cyan-700 disabled:opacity-50"
                                                aria-label={`Add ${d.displayName ?? d.name ?? d.id} to room`}
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
            </div>
        </div>
    );
}