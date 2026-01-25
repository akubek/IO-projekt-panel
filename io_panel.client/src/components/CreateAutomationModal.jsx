import React, { useEffect, useMemo, useState } from "react";

const operatorOptions = [
    { value: "GreaterThan", label: ">" },
    { value: "GreaterThanOrEqual", label: ">=" },
    { value: "LessThan", label: "<" },
    { value: "LessThanOrEqual", label: "<=" },
    { value: "Equal", label: "=" }
];

const operatorValueToEnumNumber = {
    Equal: 0,
    GreaterThan: 1,
    GreaterThanOrEqual: 2,
    LessThan: 3,
    LessThanOrEqual: 4
};

function normalizeUnit(value) {
    if (value == null) return "";
    const s = String(value).trim();
    if (!s) return "";
    const lowered = s.toLowerCase();
    if (lowered === "null" || lowered === "undefined" || lowered === "none") return "";
    return s;
}

function toNumber(value, fallback) {
    const n = typeof value === "number" ? value : Number(value);
    return Number.isFinite(n) ? n : fallback;
}

function clamp(value, min, max) {
    if (!Number.isFinite(value)) return min;
    if (value < min) return min;
    if (value > max) return max;
    return value;
}

function roundToStep(value, min, step) {
    const s = toNumber(step, 1);
    if (!Number.isFinite(s) || s <= 0) return value;
    const base = Number.isFinite(min) ? min : 0;
    return base + Math.round((value - base) / s) * s;
}

function isBoolUnit(unit) {
    return normalizeUnit(unit).toLowerCase() === "bool";
}

function getDeviceUnit(device) {
    if (!device) return "";
    return normalizeUnit(device.state?.unit) || (device.type === "switch" ? "bool" : "");
}

function getDeviceNumericLimits(device) {
    const min = toNumber(device?.config?.min, 0);
    const maxRaw = toNumber(device?.config?.max, 100);
    const max = maxRaw === 0 ? 100 : maxRaw;
    const step = toNumber(device?.config?.step, 1);
    return { min, max, step };
}

function normalizeTimeToHhMmSs(value) {
    const s = String(value ?? "").trim();
    if (!s) return "00:00:00";
    if (/^\d{2}:\d{2}$/.test(s)) return `${s}:00`;
    if (/^\d{2}:\d{2}:\d{2}$/.test(s)) return s;
    return s;
}

function CreateAutomationModal({ open, onClose, onCreated, authToken, devices, scenes }) {
    const [saving, setSaving] = useState(false);

    const [name, setName] = useState("");
    const [isEnabled, setIsEnabled] = useState(true);

    const [useTimeWindow, setUseTimeWindow] = useState(false);
    const [from, setFrom] = useState("18:00");
    const [to, setTo] = useState("23:59");

    const [conditionDeviceId, setConditionDeviceId] = useState("");
    const [conditionOp, setConditionOp] = useState("GreaterThanOrEqual");
    const [conditionValue, setConditionValue] = useState("1");

    const [actionKind, setActionKind] = useState("SetDeviceState");
    const [actionDeviceId, setActionDeviceId] = useState("");
    const [actionTargetValue, setActionTargetValue] = useState("1");
    const [actionSceneId, setActionSceneId] = useState("");

    const deviceOptions = useMemo(() => (devices ?? []), [devices]);
    const writableDeviceOptions = useMemo(() => deviceOptions.filter(d => !d?.config?.readOnly), [deviceOptions]);

    const sceneOptions = useMemo(() => (scenes ?? []), [scenes]);

    const deviceById = useMemo(() => {
        const map = new Map();
        deviceOptions.forEach(d => map.set(d.id, d));
        return map;
    }, [deviceOptions]);

    const selectedConditionDevice = deviceById.get(conditionDeviceId);
    const selectedActionDevice = deviceById.get(actionDeviceId);

    const conditionUnit = getDeviceUnit(selectedConditionDevice);
    const actionUnit = getDeviceUnit(selectedActionDevice);

    const conditionLimits = useMemo(
        () => getDeviceNumericLimits(selectedConditionDevice),
        [selectedConditionDevice]
    );

    const actionLimits = useMemo(
        () => getDeviceNumericLimits(selectedActionDevice),
        [selectedActionDevice]
    );

    useEffect(() => {
        if (!open) return;

        const defaultDeviceId = deviceOptions[0]?.id ?? "";
        const defaultWritableDeviceId = writableDeviceOptions[0]?.id ?? "";

        const defaultSceneId = sceneOptions[0]?.id ?? "";

        setName("");
        setIsEnabled(true);

        setUseTimeWindow(false);
        setFrom("18:00");
        setTo("23:59");

        setConditionDeviceId(defaultDeviceId);
        setConditionOp("GreaterThanOrEqual");
        setConditionValue("1");

        setActionKind("SetDeviceState");
        setActionDeviceId(defaultWritableDeviceId);
        setActionTargetValue("1");
        setActionSceneId(defaultSceneId);
    }, [open]); // intentionally only on open

    useEffect(() => {
        if (!open) return;

        if (!conditionDeviceId && deviceOptions.length > 0) {
            setConditionDeviceId(deviceOptions[0].id);
        }

        if (actionKind === "SetDeviceState") {
            const selected = deviceById.get(actionDeviceId);
            const selectedIsReadOnly = !!selected?.config?.readOnly;

            if ((!actionDeviceId || selectedIsReadOnly) && writableDeviceOptions.length > 0) {
                setActionDeviceId(writableDeviceOptions[0].id);
            }
        }

        if (!actionSceneId && sceneOptions.length > 0) {
            setActionSceneId(sceneOptions[0].id);
        }
    }, [
        open,
        actionKind,
        conditionDeviceId,
        actionDeviceId,
        actionSceneId,
        deviceOptions,
        writableDeviceOptions,
        sceneOptions,
        deviceById
    ]);

    useEffect(() => {
        if (!open) return;
        if (!selectedConditionDevice) return;
        const { min, max, step } = conditionLimits;

        const current = toNumber(conditionValue, min);
        const clamped = clamp(current, min, max);
        const stepped = roundToStep(clamped, min, step);
        setConditionValue(String(stepped));
    }, [open, conditionDeviceId]); // keep existing behavior

    useEffect(() => {
        if (!open) return;
        if (!selectedActionDevice) return;
        const { min, max, step } = actionLimits;

        const current = toNumber(actionTargetValue, min);
        const clamped = clamp(current, min, max);
        const stepped = roundToStep(clamped, min, step);
        setActionTargetValue(String(stepped));
    }, [open, actionDeviceId]); // keep existing behavior

    if (!open) return null;

    async function handleCreate() {
        if (!authToken) return;

        if (!name.trim()) {
            alert("Name is required.");
            return;
        }

        if (!conditionDeviceId) {
            alert("Select a device for the trigger condition.");
            return;
        }

        if (actionKind === "SetDeviceState") {
            if (!actionDeviceId) {
                alert("Select a device for the action.");
                return;
            }

            const device = deviceById.get(actionDeviceId);
            if (device?.config?.readOnly) {
                alert("Selected device is read-only and cannot be controlled.");
                return;
            }
        }

        if (actionKind === "RunScene" && !actionSceneId) {
            alert("Select a scene for the action.");
            return;
        }

        const safeConditionValue = (() => {
            if (isBoolUnit(conditionUnit)) {
                return toNumber(conditionValue, 0) > 0 ? 1 : 0;
            }

            const { min, max, step } = conditionLimits;
            const n = toNumber(conditionValue, min);
            return roundToStep(clamp(n, min, max), min, step);
        })();

        const safeActionTargetValue = (() => {
            if (isBoolUnit(actionUnit)) {
                return toNumber(actionTargetValue, 0) > 0 ? 1 : 0;
            }

            const { min, max, step } = actionLimits;
            const n = toNumber(actionTargetValue, min);
            return roundToStep(clamp(n, min, max), min, step);
        })();

        const trigger = {
            conditions: [
                {
                    deviceId: conditionDeviceId,
                    operator: operatorValueToEnumNumber[conditionOp],
                    value: safeConditionValue,
                    unit: conditionUnit || null
                }
            ],
            timeWindow: useTimeWindow
                ? {
                    from: normalizeTimeToHhMmSs(from),
                    to: normalizeTimeToHhMmSs(to)
                }
                : null
        };

        const action =
            actionKind === "SetDeviceState"
                ? {
                    kind: 0,
                    deviceId: actionDeviceId,
                    targetValue: safeActionTargetValue,
                    targetUnit: actionUnit || null,
                    sceneId: null
                }
                : {
                    kind: 1,
                    deviceId: null,
                    targetValue: null,
                    targetUnit: null,
                    sceneId: actionSceneId
                };

        const payload = {
            name: name.trim(),
            isEnabled: !!isEnabled,
            trigger,
            action
        };

        setSaving(true);

        try {
            const res = await fetch("/automation", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${authToken}`
                },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                const text = await res.text();
                alert(`Failed to create automation: ${res.status} ${text}`);
                return;
            }

            const created = await res.json();
            onCreated && onCreated(created);
            onClose && onClose();
        } catch (err) {
            alert(`Failed to create automation: ${err}`);
        } finally {
            setSaving(false);
        }
    }

    const timeInputsDisabled = saving || !useTimeWindow;

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

    const conditionUnitLabel = (!isBoolUnit(conditionUnit) && conditionUnit) ? conditionUnit : "";
    const actionUnitLabel = (!isBoolUnit(actionUnit) && actionUnit) ? actionUnit : "";

    const triggerValueInput = isBoolUnit(conditionUnit) ? (
        <select
            className={selectClassName}
            value={toNumber(conditionValue, 0) > 0 ? "1" : "0"}
            onChange={(e) => setConditionValue(e.target.value)}
            disabled={saving}
        >
            <option value="0">Off</option>
            <option value="1">On</option>
        </select>
    ) : (
        <div className="mt-2 flex items-center gap-2">
            <input
                className={inputClassName}
                type="number"
                value={conditionValue}
                min={conditionLimits.min}
                max={conditionLimits.max}
                step={conditionLimits.step}
                onChange={(e) => setConditionValue(e.target.value)}
                disabled={saving}
            />
            {conditionUnitLabel ? <span className="text-sm text-slate-500 whitespace-nowrap">{conditionUnitLabel}</span> : null}
        </div>
    );

    const actionValueInput =
        actionKind !== "SetDeviceState"
            ? null
            : isBoolUnit(actionUnit)
                ? (
                    <select
                        className={selectClassName}
                        value={toNumber(actionTargetValue, 0) > 0 ? "1" : "0"}
                        onChange={(e) => setActionTargetValue(e.target.value)}
                        disabled={saving}
                    >
                        <option value="0">Off</option>
                        <option value="1">On</option>
                    </select>
                )
                : (
                    <div className="mt-2 flex items-center gap-2">
                        <input
                            className={inputClassName}
                            type="number"
                            value={actionTargetValue}
                            min={actionLimits.min}
                            max={actionLimits.max}
                            step={actionLimits.step}
                            onChange={(e) => setActionTargetValue(e.target.value)}
                            disabled={saving}
                        />
                        {actionUnitLabel ? <span className="text-sm text-slate-500 whitespace-nowrap">{actionUnitLabel}</span> : null}
                    </div>
                );

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm px-4">
            <div className="w-full max-w-2xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
                <div className="flex items-start justify-between gap-4 border-b border-slate-100 px-6 py-5">
                    <div>
                        <h3 className="text-lg font-semibold text-slate-900">Create Automation</h3>
                        <div className="mt-0.5 text-sm text-slate-500">Configure trigger and action.</div>
                    </div>

                    <button
                        className="rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                        onClick={onClose}
                        disabled={saving}
                    >
                        Close
                    </button>
                </div>

                <div className="px-6 py-5 space-y-5">
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <label className="block text-sm font-semibold text-slate-700">
                            Name
                            <input
                                className={inputClassName}
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                                disabled={saving}
                            />
                        </label>

                        <label className="inline-flex items-center gap-2 text-sm mt-6 sm:mt-0 select-none cursor-pointer">
                            <input
                                type="checkbox"
                                className={checkboxInputClassName}
                                checked={isEnabled}
                                onChange={(e) => setIsEnabled(e.target.checked)}
                                disabled={saving}
                            />
                            <span className={checkboxBoxClassName} aria-hidden="true">
                                <span className={checkboxMarkClassName} />
                            </span>
                            <span className="font-semibold text-slate-800">Enabled</span>
                        </label>
                    </div>

                    <div className="rounded-2xl border border-slate-200 bg-white">
                        <div className="border-b border-slate-100 px-4 py-3">
                            <div className="text-sm font-semibold text-slate-900">Trigger</div>
                        </div>

                        <div className="px-4 py-4 space-y-4">
                            <label className="inline-flex items-center gap-2 text-sm select-none cursor-pointer">
                                <input
                                    type="checkbox"
                                    className={checkboxInputClassName}
                                    checked={useTimeWindow}
                                    onChange={(e) => setUseTimeWindow(e.target.checked)}
                                    disabled={saving}
                                />
                                <span className={checkboxBoxClassName} aria-hidden="true">
                                    <span className={checkboxMarkClassName} />
                                </span>
                                <span className="font-semibold text-slate-800">Use time window</span>
                            </label>

                            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                                <label className={`block text-sm font-semibold text-slate-700 ${timeInputsDisabled ? "opacity-60" : ""}`}>
                                    From
                                    <input
                                        className={inputClassName}
                                        type="time"
                                        value={from}
                                        onChange={(e) => setFrom(e.target.value)}
                                        disabled={timeInputsDisabled}
                                    />
                                </label>

                                <label className={`block text-sm font-semibold text-slate-700 ${timeInputsDisabled ? "opacity-60" : ""}`}>
                                    To
                                    <input
                                        className={inputClassName}
                                        type="time"
                                        value={to}
                                        onChange={(e) => setTo(e.target.value)}
                                        disabled={timeInputsDisabled}
                                    />
                                </label>
                            </div>

                            <div className="grid grid-cols-1 sm:grid-cols-4 gap-3">
                                <label className="block text-sm font-semibold text-slate-700 sm:col-span-2">
                                    Device
                                    <select
                                        className={selectClassName}
                                        value={conditionDeviceId}
                                        onChange={(e) => setConditionDeviceId(e.target.value)}
                                        disabled={saving}
                                    >
                                        {deviceOptions.map(d => (
                                            <option key={d.id} value={d.id}>
                                                {d.displayName ?? d.id}
                                            </option>
                                        ))}
                                    </select>
                                </label>

                                <label className="block text-sm font-semibold text-slate-700">
                                    Operator
                                    <select
                                        className={selectClassName}
                                        value={conditionOp}
                                        onChange={(e) => setConditionOp(e.target.value)}
                                        disabled={saving}
                                    >
                                        {operatorOptions.map(o => (
                                            <option key={o.value} value={o.value}>
                                                {o.label}
                                            </option>
                                        ))}
                                    </select>
                                </label>

                                <label className="block text-sm font-semibold text-slate-700">
                                    Value
                                    {triggerValueInput}
                                </label>
                            </div>
                        </div>
                    </div>

                    <div className="rounded-2xl border border-slate-200 bg-white">
                        <div className="border-b border-slate-100 px-4 py-3">
                            <div className="text-sm font-semibold text-slate-900">Action</div>
                        </div>

                        <div className="px-4 py-4">
                            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                                <label className="block text-sm font-semibold text-slate-700">
                                    Kind
                                    <select
                                        className={selectClassName}
                                        value={actionKind}
                                        onChange={(e) => setActionKind(e.target.value)}
                                        disabled={saving}
                                    >
                                        <option value="SetDeviceState">Set device state</option>
                                        <option value="RunScene">Run scene</option>
                                    </select>
                                </label>

                                {actionKind === "SetDeviceState" ? (
                                    <>
                                        <label className="block text-sm font-semibold text-slate-700">
                                            Device
                                            <select
                                                className={selectClassName}
                                                value={actionDeviceId}
                                                onChange={(e) => setActionDeviceId(e.target.value)}
                                                disabled={saving}
                                            >
                                                {writableDeviceOptions.map(d => (
                                                    <option key={d.id} value={d.id}>
                                                        {d.displayName ?? d.id}
                                                    </option>
                                                ))}
                                            </select>
                                        </label>

                                        <label className="block text-sm font-semibold text-slate-700">
                                            Value
                                            {actionValueInput}
                                        </label>
                                    </>
                                ) : (
                                    <label className="block text-sm font-semibold text-slate-700 sm:col-span-2">
                                        Scene
                                        <select
                                            className={selectClassName}
                                            value={actionSceneId}
                                            onChange={(e) => setActionSceneId(e.target.value)}
                                            disabled={saving}
                                        >
                                            {sceneOptions.map(s => (
                                                <option key={s.id} value={s.id}>
                                                    {s.name ?? s.id}
                                                </option>
                                            ))}
                                        </select>
                                    </label>
                                )}
                            </div>
                        </div>
                    </div>
                </div>

                <div className="flex flex-col-reverse gap-3 border-t border-slate-100 px-6 py-5 sm:flex-row sm:items-center sm:justify-end">
                    <button
                        className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                        onClick={onClose}
                        disabled={saving}
                    >
                        Cancel
                    </button>
                    <button
                        className="rounded-xl bg-gradient-to-r from-blue-600 to-cyan-600 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-cyan-700 disabled:opacity-60"
                        onClick={handleCreate}
                        disabled={saving}
                    >
                        {saving ? "Creating..." : "Create"}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default CreateAutomationModal;