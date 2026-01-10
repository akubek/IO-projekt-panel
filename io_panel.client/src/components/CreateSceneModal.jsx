import React, { useMemo, useState } from "react";

function toNumberOrZero(v) {
    const n = Number(v);
    return Number.isFinite(n) ? n : 0;
}

export default function CreateSceneModal({ open, devices, authToken, onClose, onCreated }) {
    const [name, setName] = useState("");
    const [isPublic, setIsPublic] = useState(true);
    const [selectedDeviceId, setSelectedDeviceId] = useState("");
    const [isBusy, setIsBusy] = useState(false);

    // deviceId -> { type, targetValue, targetUnit }
    const [actions, setActions] = useState({});

    const isAdmin = !!authToken;

    const eligibleDevices = useMemo(() => {
        return (devices ?? []).filter(d => (d.type === "switch" || d.type === "slider"));
    }, [devices]);

    const availableToAdd = useMemo(() => {
        const used = new Set(Object.keys(actions));
        return eligibleDevices.filter(d => !used.has(d.id));
    }, [eligibleDevices, actions]);

    // Hard-block: non-admins cannot create scenes.
    if (!open || !isAdmin) return null;

    function addAction() {
        const id = selectedDeviceId;
        if (!id) return;

        const device = (devices ?? []).find(d => d.id === id);
        if (!device) return;

        if (device.type === "switch") {
            setActions(prev => ({
                ...prev,
                [id]: { deviceId: id, type: "switch", targetValue: 1, targetUnit: device.state?.unit ?? null }
            }));
        } else if (device.type === "slider") {
            const initial = device.state?.value ?? device.config?.min ?? 0;
            setActions(prev => ({
                ...prev,
                [id]: { deviceId: id, type: "slider", targetValue: initial, targetUnit: device.state?.unit ?? null }
            }));
        }

        setSelectedDeviceId("");
    }

    function removeAction(deviceId) {
        setActions(prev => {
            const copy = { ...prev };
            delete copy[deviceId];
            return copy;
        });
    }

    function updateSwitch(deviceId, isOn) {
        setActions(prev => ({
            ...prev,
            [deviceId]: { ...prev[deviceId], targetValue: isOn ? 1 : 0 }
        }));
    }

    function updateSlider(deviceId, value) {
        setActions(prev => ({
            ...prev,
            [deviceId]: { ...prev[deviceId], targetValue: value }
        }));
    }

    async function createScene() {
        const sceneName = name.trim();
        if (!sceneName) return;

        const actionList = Object.values(actions).map(a => ({
            deviceId: a.deviceId,
            targetState: { value: a.targetValue, unit: a.targetUnit }
        }));

        setIsBusy(true);
        try {
            const headers = {
                "Content-Type": "application/json",
                Authorization: `Bearer ${authToken}`
            };

            const res = await fetch("/scene", {
                method: "POST",
                headers,
                body: JSON.stringify({ name: sceneName, isPublic, actions: actionList })
            });

            if (!res.ok) {
                const text = await res.text();
                console.error("Failed to create scene:", res.status, text);
                return;
            }

            const created = await res.json();
            onCreated && onCreated(created);

            setName("");
            setIsPublic(true);
            setSelectedDeviceId("");
            setActions({});

            onClose && onClose();
        } catch (e) {
            console.error("Error creating scene:", e);
        } finally {
            setIsBusy(false);
        }
    }

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div className="bg-white rounded-lg shadow-lg p-6 w-11/12 max-w-3xl">
                <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold">Create Scene</h3>
                    <button
                        className="px-4 py-2 rounded bg-slate-100 text-slate-800 hover:bg-slate-200 disabled:opacity-50"
                        onClick={onClose}
                        disabled={isBusy}
                    >
                        Close
                    </button>
                </div>

                <div className="space-y-4">
                    <label className="block text-sm">
                        Scene name
                        <input
                            className="mt-1 w-full border rounded px-2 py-2"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            disabled={isBusy}
                        />
                    </label>

                    <label className="inline-flex items-center gap-2 text-sm">
                        <input
                            type="checkbox"
                            checked={isPublic}
                            onChange={(e) => setIsPublic(e.target.checked)}
                            disabled={isBusy}
                        />
                        Public (uncheck for private)
                    </label>

                    <div className="flex flex-col sm:flex-row gap-2 items-end">
                        <label className="block text-sm flex-1">
                            Add device
                            <select
                                className="mt-1 w-full border rounded px-2 py-2"
                                value={selectedDeviceId}
                                onChange={(e) => setSelectedDeviceId(e.target.value)}
                                disabled={isBusy}
                            >
                                <option value="">Select a device...</option>
                                {availableToAdd.map(d => (
                                    <option key={d.id} value={d.id}>
                                        {d.displayName} ({d.type})
                                    </option>
                                ))}
                            </select>
                        </label>

                        <button
                            type="button"
                            onClick={addAction}
                            disabled={isBusy || !selectedDeviceId}
                            className="inline-flex items-center justify-center px-4 py-2 rounded-md bg-gradient-to-r from-blue-600 to-cyan-600 text-white shadow hover:from-blue-700 hover:to-cyan-700 disabled:opacity-50"
                        >
                            +
                        </button>
                    </div>

                    <div className="border rounded-md">
                        <div className="px-4 py-2 border-b bg-slate-50 font-semibold text-sm">
                            Actions ({Object.keys(actions).length})
                        </div>

                        {Object.keys(actions).length === 0 ? (
                            <div className="p-4 text-sm text-slate-500">No devices added yet.</div>
                        ) : (
                            <div className="p-4 space-y-3">
                                {Object.values(actions).map(a => {
                                    const device = (devices ?? []).find(d => d.id === a.deviceId);
                                    if (!device) return null;

                                    return (
                                        <div key={a.deviceId} className="flex flex-col md:flex-row md:items-center gap-3 border rounded p-3">
                                            <div className="flex-1">
                                                <div className="font-semibold text-slate-800">{device.displayName}</div>
                                                <div className="text-xs text-slate-500">{device.type} , {device.localization ?? device.location ?? ""}</div>
                                            </div>

                                            {device.type === "switch" && (
                                                <div className="flex items-center gap-2">
                                                    <span className="text-sm text-slate-600">Target</span>
                                                    <button
                                                        type="button"
                                                        disabled={isBusy}
                                                        onClick={() => updateSwitch(a.deviceId, a.targetValue !== 1)}
                                                        className="px-4 py-2 rounded-md bg-slate-100 hover:bg-slate-200 disabled:opacity-50"
                                                    >
                                                        {a.targetValue === 1 ? "ON" : "OFF"}
                                                    </button>
                                                </div>
                                            )}

                                            {device.type === "slider" && (
                                                <div className="flex items-center gap-2">
                                                    <span className="text-sm text-slate-600">Target</span>
                                                    <input
                                                        type="number"
                                                        className="w-32 border rounded px-2 py-2"
                                                        min={device.config?.min ?? 0}
                                                        max={device.config?.max ?? 100}
                                                        step={device.config?.step ?? 1}
                                                        value={a.targetValue}
                                                        disabled={isBusy}
                                                        onChange={(e) => updateSlider(a.deviceId, toNumberOrZero(e.target.value))}
                                                    />
                                                    {device.state?.unit && (
                                                        <span className="text-sm text-slate-500">{device.state.unit}</span>
                                                    )}
                                                </div>
                                            )}

                                            <button
                                                type="button"
                                                onClick={() => removeAction(a.deviceId)}
                                                disabled={isBusy}
                                                className="px-3 py-2 rounded bg-slate-100 hover:bg-slate-200 disabled:opacity-50"
                                            >
                                                Remove
                                            </button>
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>
                </div>

                <div className="mt-6 flex justify-end gap-2">
                    <button
                        className="px-4 py-2 rounded bg-slate-100 text-slate-800 hover:bg-slate-200 disabled:opacity-50"
                        onClick={onClose}
                        disabled={isBusy}
                    >
                        Cancel
                    </button>
                    <button
                        className="inline-flex items-center gap-2 px-4 py-2 rounded-md bg-gradient-to-r from-blue-600 to-cyan-600 text-white shadow hover:from-blue-700 hover:to-cyan-700 disabled:opacity-50"
                        onClick={createScene}
                        disabled={isBusy || !name.trim()}
                    >
                        {isBusy ? "Creating..." : "Create scene"}
                    </button>
                </div>
            </div>
        </div>
    );
}