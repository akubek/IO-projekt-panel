import React, { useEffect, useMemo, useState } from "react";
import Card from "@mui/material/Card";
import Badge from "@mui/material/Badge";
import Switch from "@mui/material/Switch";
import Slider from "@mui/material/Slider";
import { Activity, Gauge, MapPin, Power, X } from "lucide-react";
import clsx from "clsx";
import DeviceHistoryChart from "./DeviceHistoryChart";

const deviceTypeConfig = {
    switch: {
        icon: Power,
        color: "from-blue-500 to-blue-600",
        bgLight: "bg-blue-50",
        textColor: "text-blue-700",
        borderColor: "border-blue-200"
    },
    slider: {
        icon: Gauge,
        color: "from-purple-500 to-blue-600",
        bgLight: "bg-purple-50",
        textColor: "text-purple-700",
        borderColor: "border-purple-200"
    },
    sensor: {
        icon: Activity,
        color: "from-green-500 to-green-600",
        bgLight: "bg-green-50",
        textColor: "text-green-700",
        borderColor: "border-green-200"
    }
};

function toNumberOr(value, fallback) {
    const n = typeof value === "number" ? value : Number(value);
    return Number.isFinite(n) ? n : fallback;
}

function clamp(value, min, max) {
    if (!Number.isFinite(value)) return min;
    if (value < min) return min;
    if (value > max) return max;
    return value;
}

function normalizeUnit(value) {
    if (value == null) return "";
    const s = String(value).trim();
    if (!s) return "";
    const lowered = s.toLowerCase();
    if (lowered === "null" || lowered === "undefined" || lowered === "none") return "";
    return s;
}

export default function DeviceDetailsModal({ open, device, onClose, onToggle, onSetValue, refreshToken }) {
    const safeDevice = device ?? {
        id: "",
        type: "switch",
        displayName: "",
        location: "",
        localization: null,
        description: "",
        state: { value: 0, unit: null },
        config: { readOnly: true, min: 0, max: 100, step: 1 },
        createdAt: null,
        lastSeen: null,
        configuredAt: null
    };

    const config = deviceTypeConfig[safeDevice.type] || deviceTypeConfig.switch;
    const Icon = config.icon;

    const isControllable = useMemo(() => {
        if (safeDevice?.config?.readOnly) return false;
        return safeDevice?.type === "switch" || safeDevice?.type === "slider";
    }, [safeDevice?.config?.readOnly, safeDevice?.type]);

    const isOn = toNumberOr(safeDevice.state?.value, 0) > 0;

    const sliderMin = toNumberOr(safeDevice.config?.min, 0);
    const sliderMax = toNumberOr(safeDevice.config?.max, 100);
    const sliderStep = toNumberOr(safeDevice.config?.step, 1);

    const sliderUnit = normalizeUnit(safeDevice.state?.unit);
    const sensorUnit = normalizeUnit(safeDevice.state?.unit);
    const chartUnit = normalizeUnit(safeDevice.state?.unit) || null;

    const [isDragging, setIsDragging] = useState(false);
    const [localSliderValue, setLocalSliderValue] = useState(() => toNumberOr(safeDevice.state?.value, 0));

    useEffect(() => {
        if (!open) return;

        setIsDragging(false);
        setLocalSliderValue(toNumberOr(safeDevice.state?.value, 0));
    }, [open, safeDevice.id, safeDevice.state?.value]);

    const displayedSliderValue = safeDevice.type === "slider" ? toNumberOr(safeDevice.state?.value, 0) : 0;
    const sliderValueToRender = isDragging ? localSliderValue : displayedSliderValue;

    const handleSwitchChange = (e, checked) => {
        e.stopPropagation();
        if (!isControllable || safeDevice.type !== "switch") return;

        if (onToggle) {
            onToggle(safeDevice, checked);
        }
    };

    const handleSliderChange = (e, value) => {
        e.stopPropagation();
        if (!isControllable || safeDevice.type !== "slider") return;

        if (!isDragging) setIsDragging(true);
        setLocalSliderValue(toNumberOr(Array.isArray(value) ? value[0] : value, 0));
    };

    const handleSliderCommit = (e, value) => {
        e.stopPropagation();
        if (!isControllable || safeDevice.type !== "slider") return;

        const v = toNumberOr(Array.isArray(value) ? value[0] : value, 0);
        setIsDragging(false);
        setLocalSliderValue(v);

        if (onSetValue) {
            onSetValue(safeDevice, v);
        }
    };

    const formatDate = (value) => {
        if (!value) return "N/A";

        const d = new Date(value);
        return Number.isNaN(d.getTime()) ? String(value) : d.toLocaleString();
    };

    const metaItem = (label, value) => (
        <div className="py-2">
            <p className="text-xs font-medium text-slate-500">{label}</p>
            <p className="text-sm text-slate-800 break-words">{value ?? "N/A"}</p>
        </div>
    );

    if (!open || !device) {
        return null;
    }

    const sensorMin = toNumberOr(safeDevice.config?.min, 0);
    const sensorMaxRaw = toNumberOr(safeDevice.config?.max, 100);
    const sensorMax = sensorMaxRaw === 0 ? 100 : sensorMaxRaw;
    const sensorValue = toNumberOr(safeDevice.state?.value, 0);
    const sensorValueClamped = clamp(sensorValue, sensorMin, sensorMax);
    const sensorRange = Math.max(1e-9, sensorMax - sensorMin);
    const sensorPercent = ((sensorValueClamped - sensorMin) / sensorRange) * 100;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4" onClick={onClose}>
            <div className="w-full max-w-6xl" onClick={(e) => e.stopPropagation()}>
                <Card className={clsx("group border-2 transition-all duration-200 overflow-hidden", "h-full", config.borderColor)}>
                    <div className={clsx("p-6 border-b", config.bgLight)}>
                        <div className="flex items-start justify-between gap-3">
                            <div className="flex items-start gap-3 min-w-0">
                                <div className={clsx("w-12 h-12 rounded-xl bg-gradient-to-br shadow-lg flex items-center justify-center", config.color)}>
                                    <Icon className="w-6 h-6 text-white" />
                                </div>

                                <div className="min-w-0">
                                    <h3 className="text-lg font-bold text-slate-900 mb-1 text-left truncate">
                                        {safeDevice.displayName}
                                    </h3>

                                    {safeDevice.location && (
                                        <div className="text-sm text-slate-500 text-left truncate">Location: {safeDevice.location}</div>
                                    )}

                                    {safeDevice.localization && (
                                        <div className="flex items-center gap-1 text-sm text-slate-500">
                                            <MapPin className="w-3 h-3" />
                                            {safeDevice.localization}
                                        </div>
                                    )}
                                </div>
                            </div>

                            <div className="flex items-center gap-2">
                                <Badge variant="secondary" className={clsx(config.bgLight, config.textColor, "font-medium")}>
                                    {safeDevice.type.toUpperCase()}
                                </Badge>

                                <button
                                    type="button"
                                    onClick={onClose}
                                    className="inline-flex items-center justify-center w-10 h-10 rounded-md bg-white/70 hover:bg-white shadow-sm border border-slate-200 text-slate-700"
                                    aria-label="Close"
                                >
                                    <X className="w-4 h-4" />
                                </button>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 lg:grid-cols-[360px_1fr] gap-6 p-6">
                        <div>
                            {safeDevice.type === "switch" && (
                                <div className="flex items-center justify-between">
                                    <div>
                                        <p className="text-2xl font-bold text-slate-900 text-left">{isOn ? "ON" : "OFF"}</p>
                                        <p className="text-sm text-slate-500 mt-1">{isControllable ? "Click to toggle" : "Read-only"}</p>
                                    </div>

                                    <Switch
                                        checked={isOn}
                                        className="scale-125"
                                        disabled={!isControllable}
                                        onClick={(e) => e.stopPropagation()}
                                        onChange={handleSwitchChange}
                                    />
                                </div>
                            )}

                            {safeDevice.type === "slider" && (
                                <div className="space-y-3" onClick={(e) => e.stopPropagation()}>
                                    <div className="flex items-end justify-between">
                                        <p className="text-3xl font-bold text-slate-900">
                                            {displayedSliderValue}
                                            {sliderUnit ? <span className="ml-2 text-xl text-slate-500">{sliderUnit}</span> : null}
                                        </p>
                                        <p className="text-sm text-gray-400 text-left">
                                            {sliderMin} - {sliderMax}
                                        </p>
                                    </div>

                                    <Slider
                                        value={sliderValueToRender}
                                        min={sliderMin}
                                        max={sliderMax}
                                        step={sliderStep}
                                        onChange={handleSliderChange}
                                        onChangeCommitted={handleSliderCommit}
                                        onMouseDown={(e) => e.stopPropagation()}
                                        onTouchStart={(e) => e.stopPropagation()}
                                        disabled={!isControllable}
                                    />
                                </div>
                            )}

                            {safeDevice.type === "sensor" && (
                                <div>
                                    <div className="flex items-end gap-2 mb-2">
                                        <p className="text-3xl font-bold text-slate-900">{sensorValue}</p>
                                        {sensorUnit ? <p className="text-xl text-slate-500 mb-1">{sensorUnit}</p> : null}
                                    </div>
                                    <p className="text-sm text-gray-400 text-left">
                                        Range: {sensorMin} - {sensorMax}
                                    </p>
                                    <div className="mt-3 h-2 bg-slate-100 rounded-full overflow-hidden">
                                        <div className={clsx("h-full bg-gradient-to-r", config.color)} style={{ width: `${sensorPercent}%` }} />
                                    </div>
                                </div>
                            )}

                            {safeDevice.description && <p className="text-sm text-grey-400 mt-4 line-clamp-2 text-left">{safeDevice.description}</p>}

                            <div className="mt-6 border-t pt-4 grid grid-cols-2 gap-x-4">
                                <h4 className="col-span-2 text-md font-semibold text-slate-700 mb-1">Additional info</h4>
                                {metaItem("Created At", formatDate(safeDevice.createdAt))}
                                {metaItem("Last Seen", formatDate(safeDevice.lastSeen))}
                                {metaItem("Configured At", formatDate(safeDevice.configuredAt))}
                            </div>
                        </div>

                        <div className="min-w-0">
                            <DeviceHistoryChart
                                deviceId={safeDevice.id}
                                unit={chartUnit}
                                className="w-full"
                                height={520}
                                refreshToken={refreshToken}
                            />
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    );
}