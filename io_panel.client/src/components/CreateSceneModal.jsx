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

    const writableEligibleDevices = useMemo(() => {
        return eligibleDevices.filter(d => !d?.config?.readOnly);
    }, [eligibleDevices]);

    const availableToAdd = useMemo(() => {
        const used = new Set(Object.keys(actions));
        return writableEligibleDevices.filter(d => !used.has(d.id));
    }, [writableEligibleDevices, actions]);

    // Hard-block: non-admins cannot create scenes.
    if (!open || !isAdmin) return null;

    const checkboxInputClassName = "peer sr-only";
    const checkboxBoxClassName =
        "inline-flex h-5 w-5 items-center justify-center rounded border border-slate-500 bg-white shadow-sm " +
        "peer-checked:bg-blue-600 peer-checked:border-blue-600 " +
        "peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-blue-300 " +
        "peer-disabled:opacity-50 peer-disabled:cursor-not-allowed";

    const checkboxMarkClassName =
        "h-2.5 w-1.5 -mt-0.5 rotate-45 " +
        "border-b-2 border-r-2 border-white rounded-[1px] " +
        "opacity-0 peer-checked:opacity-100";

    const inputClassName =
        "mt-2 w-full rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-slate-900 shadow-sm outline-none " +
        "focus:border-blue-300 focus:ring-4 focus:ring-blue-100 disabled:opacity-60";

    const selectClassName = inputClassName;

    function addAction() {
        const id = selectedDeviceId;
        if (!id) return;

        const device = (devices ?? []).find(d => d.id === id);
        if (!device) return;

        if (device.config?.readOnly) {
            return;
        }

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

        const hasReadOnly = Object.values(actions).some(a => {
            const device = (devices ?? []).find(d => d.id === a.deviceId);
            return !!device?.config?.readOnly;
        });

        if (hasReadOnly) {
            console.error("Cannot create a scene with read-only devices.");
            return;
        }

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
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm px-4">
            <div className="w-full max-w-3xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
                <div className="flex items-start justify-between gap-4 border-b border-slate-100 px-6 py-5">
                    <div>
                        <h3 className="text-lg font-semibold text-slate-900">Create Scene</h3>
                        <div className="mt-0.5 text-sm text-slate-500">Build a reusable set of device states.</div>
                    </div>

                    <button
                        className="rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                        onClick={onClose}
                        disabled={isBusy}
                    >
                        Close
                    </button>
                </div>

                <div className="px-6 py-5 space-y-5">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 items-start">
                        <label className="block text-sm font-semibold text-slate-700">
                            Scene name
                            <input
                                className={inputClassName}
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                                disabled={isBusy}
                            />
                        </label>

                        <label className="inline-flex items-center gap-2 text-sm mt-6 sm:mt-0 select-none cursor-pointer">
                            <input
                                type="checkbox"
                                className={checkboxInputClassName}
                                checked={isPublic}
                                onChange={(e) => setIsPublic(e.target.checked)}
                                disabled={isBusy}
                            />
                            <span className={checkboxBoxClassName} aria-hidden="true">
                                <span className={checkboxMarkClassName} />
                            </span>
                            <span className="font-semibold text-slate-800">Public</span>
                            <span className="text-slate-500 font-normal">(uncheck for private)</span>
                        </label>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-[1fr_auto] gap-3 items-end">
                        <label className="block text-sm font-semibold text-slate-700">
                            Add device
                            <select
                                className={selectClassName}
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
                            className="rounded-xl bg-gradient-to-r from-blue-600 to-cyan-600 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-cyan-700 disabled:opacity-60"
                        >
                            Add
                        </button>
                    </div>

                    <div className="rounded-2xl border border-slate-200 bg-white overflow-hidden">
                        <div className="border-b border-slate-100 bg-slate-50 px-4 py-3 text-sm font-semibold text-slate-900">
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
                                        <div
                                            key={a.deviceId}
                                            className="flex flex-col md:flex-row md:items-center gap-3 rounded-xl border border-slate-200 bg-white p-4"
                                        >
                                            <div className="flex-1 min-w-0">
                                                <div className="font-semibold text-slate-900 truncate">{device.displayName}</div>
                                                <div className="text-xs text-slate-500">
                                                    {device.type}{device.localization || device.location ? ` • ${device.localization ?? device.location}` : ""}
                                                </div>
                                            </div>

                                            {device.type === "switch" && (
                                                <div className="flex items-center gap-2">
                                                    <span className="text-sm font-semibold text-slate-700">Target</span>
                                                    <button
                                                        type="button"
                                                        disabled={isBusy}
                                                        onClick={() => updateSwitch(a.deviceId, a.targetValue !== 1)}
                                                        className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                                                    >
                                                        {a.targetValue === 1 ? "ON" : "OFF"}
                                                    </button>
                                                </div>
                                            )}

                                            {device.type === "slider" && (
                                                <div className="flex items-center gap-2">
                                                    <span className="text-sm font-semibold text-slate-700">Target</span>
                                                    <input
                                                        type="number"
                                                        className="w-36 rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-slate-900 shadow-sm outline-none focus:border-blue-300 focus:ring-4 focus:ring-blue-100 disabled:opacity-60"
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
                                                className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
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

                <div className="flex flex-col-reverse gap-3 border-t border-slate-100 px-6 py-5 sm:flex-row sm:items-center sm:justify-end">
                    <button
                        className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                        onClick={onClose}
                        disabled={isBusy}
                    >
                        Cancel
                    </button>
                    <button
                        className="rounded-xl bg-gradient-to-r from-blue-600 to-cyan-600 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-cyan-700 disabled:opacity-60"
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