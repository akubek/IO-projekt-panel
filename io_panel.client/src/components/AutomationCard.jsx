import React, { useMemo } from "react";
import { Trash2, Zap } from "lucide-react";

function normalizeUnit(value) {
    if (value == null) return "";
    const s = String(value).trim();
    if (!s) return "";
    const lowered = s.toLowerCase();
    if (lowered === "null" || lowered === "undefined" || lowered === "none") return "";
    return s;
}

function isBoolUnit(unit) {
    return normalizeUnit(unit).toLowerCase() === "bool";
}

function toOnOff(value) {
    const n = typeof value === "number" ? value : Number(value);
    return Number.isFinite(n) && n > 0 ? "On" : "Off";
}

function opToText(op) {
    // server may return enum as number (0..4) or as string
    if (typeof op === "number") {
        switch (op) {
            case 0: return "=";
            case 1: return ">";
            case 2: return ">=";
            case 3: return "<";
            case 4: return "<=";
            default: return String(op);
        }
    }

    switch (op) {
        case "Equal": return "=";
        case "GreaterThan": return ">";
        case "GreaterThanOrEqual": return ">=";
        case "LessThan": return "<";
        case "LessThanOrEqual": return "<=";
        default: return String(op || "?");
    }
}

function formatTimeWindow(window) {
    if (!window) return null;

    const from = window.from ?? window.From;
    const to = window.to ?? window.To;

    if (!from || !to) return null;

    const fromText = String(from).slice(0, 5);
    const toText = String(to).slice(0, 5);

    return `${fromText} - ${toText}`;
}

function AutomationCard({ automation, isAdmin, onDelete, onToggleEnabled, deviceById, sceneById }) {
    const enabledText = automation.isEnabled ? "Enabled" : "Disabled";

    const trigger = automation.trigger ?? {};
    const conditions = trigger.conditions ?? [];
    const timeWindowText = formatTimeWindow(trigger.timeWindow);

    const action = automation.action ?? {};
    const actionKind = action.kind;

    const actionText = useMemo(() => {
        if (actionKind === "SetDeviceState" || actionKind === 0) {
            const deviceId = action.deviceId;
            const deviceName = deviceById?.get(deviceId)?.displayName ?? deviceId ?? "(no device)";

            const unit = normalizeUnit(action.targetUnit);
            const valueText = isBoolUnit(unit) ? toOnOff(action.targetValue) : (action.targetValue ?? "(no value)");
            const unitText = unit && !isBoolUnit(unit) ? ` ${unit}` : "";

            return `Set ${deviceName} to ${valueText}${unitText}`;
        }

        if (actionKind === "RunScene" || actionKind === 1) {
            const sceneId = action.sceneId;
            const sceneName = sceneById?.get(sceneId)?.name ?? sceneId ?? "(no scene)";
            return `Run scene: ${sceneName}`;
        }

        return "No action";
    }, [actionKind, action, deviceById, sceneById]);

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

    return (
        <div className="bg-white p-4 rounded-lg shadow-sm border border-slate-200">
            <div className="flex items-start justify-between gap-3">
                <div className="flex items-center gap-3 min-w-0">
                    <Zap className="w-5 h-5 text-yellow-500 shrink-0" />
                    <h3 className="text-lg font-bold text-slate-800 truncate">
                        {automation.name}
                    </h3>
                </div>

                <div className="flex items-center gap-2">
                    {isAdmin && (
                        <label className="inline-flex items-center gap-2 text-sm select-none cursor-pointer">
                            <input
                                type="checkbox"
                                className={checkboxInputClassName}
                                checked={!!automation.isEnabled}
                                onChange={(e) => onToggleEnabled && onToggleEnabled(automation, e.target.checked)}
                            />
                            <span className={checkboxBoxClassName} aria-hidden="true">
                                <span className={checkboxMarkClassName} />
                            </span>
                            <span className="font-medium text-slate-800">
                                Enabled
                            </span>
                        </label>
                    )}

                    {isAdmin && (
                        <button
                            type="button"
                            onClick={() => onDelete && onDelete(automation.id)}
                            className="inline-flex items-center gap-2 px-3 py-2 rounded-md bg-gradient-to-r !text-white from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 shadow-lg"
                            aria-label={`Delete automation ${automation.name}`}
                        >
                            <Trash2 className="w-4 h-4" />
                            Delete
                        </button>
                    )}
                </div>
            </div>

            <p className="text-sm text-slate-500 mt-2">{enabledText}</p>

            <div className="mt-4 space-y-2">
                <div>
                    <div className="text-xs font-semibold text-slate-700 uppercase tracking-wide">Trigger</div>
                    {timeWindowText ? (
                        <div className="text-sm text-slate-600">
                            Time window: <span className="font-mono">{timeWindowText}</span>
                        </div>
                    ) : (
                        <div className="text-sm text-slate-400">Time window: none</div>
                    )}

                    {conditions.length === 0 ? (
                        <div className="text-sm text-slate-400">Conditions: none</div>
                    ) : (
                        <ul className="mt-1 text-sm text-slate-600 space-y-1">
                            {conditions.map((c, idx) => {
                                const deviceName = deviceById?.get(c.deviceId)?.displayName ?? c.deviceId ?? "(no device)";
                                const unit = normalizeUnit(c.unit);
                                const valueText = isBoolUnit(unit) ? toOnOff(c.value) : c.value;
                                const unitText = unit && !isBoolUnit(unit) ? ` ${unit}` : "";

                                return (
                                    <li key={idx} className="font-mono break-all">
                                        {deviceName} {opToText(c.operator)} {valueText}{unitText}
                                    </li>
                                );
                            })}
                        </ul>
                    )}
                </div>

                <div>
                    <div className="text-xs font-semibold text-slate-700 uppercase tracking-wide">Action</div>
                    <div className="text-sm text-slate-600">{actionText}</div>
                </div>
            </div>
        </div>
    );
}

export default AutomationCard;