import React, { useMemo, useState } from "react";
import Card from "@mui/material/Card";
import Badge from "@mui/material/Badge";
import Switch from "@mui/material/Switch";
import Slider from "@mui/material/Slider";
import { Activity, AlertTriangle, Gauge, MapPin, Power, Trash2 } from "lucide-react";
import clsx from "clsx";

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
        color: "from-purple-500 to-purple-600",
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

/*
  DeviceCard
  This component renders a single smart home device card.
  It adapts its visualization and available controls based on the device type
  (switch, slider, or sensor).
*/
export default function DeviceCard({ device, onSelect, isAdmin, onDelete, onToggle, onSetValue, roomNames }) {
    const config = deviceTypeConfig[device.type] || deviceTypeConfig.switch;
    const Icon = config.icon;

    const isControllable = useMemo(() => {
        if (device?.config?.readOnly) return false;
        return device?.type === 'switch' || device?.type === 'slider';
    }, [device?.config?.readOnly, device?.type]);

    // State Logic
    const isOn = (device.state?.value ?? 0) > 0;

    const sliderMin = device.config?.min ?? 0;
    const sliderMax = device.config?.max ?? 100;
    const sliderStep = device.config?.step ?? 1;

    const [isDragging, setIsDragging] = useState(false);
    const [localSliderValue, setLocalSliderValue] = useState(device.state?.value ?? 0);

    const displayedSliderValue = device.type === 'slider' ? (device.state?.value ?? 0) : 0;
    const sliderValueToRender = isDragging ? localSliderValue : displayedSliderValue;

    // Handles device deletion (admin only).
    const handleDelete = (e) => {
        e.stopPropagation();
        if (onDelete) {
            onDelete(device.id);
        }
    };

    // Handles toggle interactions for Switch devices.
    const handleSwitchChange = (e, checked) => {
        e.stopPropagation();
        if (!isControllable || device.type !== 'switch') return;

        if (onToggle) {
            onToggle(device, checked);
        }
    };

    // Updates local state while dragging.
    const handleSliderChange = (e, value) => {
        e.stopPropagation();
        if (!isControllable || device.type !== 'slider') return;

        if (!isDragging) setIsDragging(true);
        setLocalSliderValue(Array.isArray(value) ? value[0] : value);
    };

    // Commits the value when the user releases the slider handle.
    const handleSliderCommit = (e, value) => {
        e.stopPropagation();
        if (!isControllable || device.type !== 'slider') return;

        const v = Array.isArray(value) ? value[0] : value;

        setIsDragging(false);
        setLocalSliderValue(v);

        if (onSetValue) {
            onSetValue(device, v);
        }
    };

    const roomsText = (roomNames ?? []).filter(Boolean).join(", ");
    const hasRooms = !!roomsText;

    const isOffline = String(device?.status ?? "").trim().toLowerCase() === "offline";
    const warningText = isOffline
        ? "Warning: device is offline"
        : (device.malfunctioning ? "Warning: device malfunctioning" : null);

    return (
        <div className="w-full">
            <div className="max-w-sm w-full">
                <Card
                    className={clsx(
                        "group cursor-pointer border-2 transition-all duration-200 overflow-hidden",
                        "hover:shadow-xl",
                        "h-full flex flex-col min-h-56",
                        config.borderColor
                    )}
                    onClick={onSelect}
                >
                    {/* --- Card Header: Icon, Info & Actions --- */}
                    <div className={clsx("p-6 border-b", config.bgLight)}>
                        <div className="flex items-start justify-between mb-4 gap-3">
                            <div className="flex items-start gap-3 min-w-0">
                                <div className={clsx("w-12 h-12 rounded-xl bg-gradient-to-br shadow-lg flex items-center justify-center", config.color)}>
                                    <Icon className="w-6 h-6 text-white" />
                                </div>
                                <div className="min-w-0">
                                    <h3 className="text-lg font-bold text-slate-900 mb-1 text-left truncate">{device.displayName}</h3>

                                    {hasRooms && (
                                        <div className="text-sm text-slate-500 text-left truncate">
                                            Rooms: {roomsText}
                                        </div>
                                    )}

                                    {device.location && (
                                        <div className="text-sm text-slate-500 text-left truncate">
                                            Location: {device.location}
                                        </div>
                                    )}

                                    {device.localization && (
                                        <div className="flex items-center gap-1 text-sm text-slate-500">
                                            <MapPin className="w-3 h-3" />
                                            {device.localization}
                                        </div>
                                    )}
                                </div>
                            </div>

                            <div className="flex items-center gap-2">
                                <Badge variant="secondary" className={clsx(config.bgLight, config.textColor, "font-medium")}>
                                    {device.type.toUpperCase()}
                                </Badge>

                                {isAdmin && (
                                    <button
                                        type="button"
                                        onClick={handleDelete}
                                        className="inline-flex items-center gap-2 px-3 py-2 rounded-md bg-gradient-to-r !text-white from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 shadow-lg"
                                        aria-label={`Delete device ${device.displayName}`}
                                    >
                                        <Trash2 className="w-4 h-4" />
                                    </button>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* --- Card Body: Controls & State --- */}
                    <div className="p-6 flex-1">
                        {device.type === 'switch' && (
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="text-2xl font-bold text-slate-900 text-left">{isOn ? 'ON' : 'OFF'}</p>
                                    <p className="text-sm text-slate-500 mt-1">
                                        {isControllable ? "Click to toggle" : "Read-only"}
                                    </p>
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

                        {device.type === 'slider' && (
                            <div className="space-y-3" onClick={(e) => e.stopPropagation()}>
                                <div className="flex items-end justify-between">
                                    <p className="text-3xl font-bold text-slate-900">{displayedSliderValue}</p>
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

                        {device.type === 'sensor' && (
                            <div>
                                <div className="flex items-end gap-2 mb-2">
                                    <p className="text-3xl font-bold text-slate-900">{device.state?.value ?? 0}</p>
                                    {device.state?.unit && (
                                        <p className="text-xl text-slate-500 mb-1">{device.state.unit}</p>
                                    )}
                                </div>
                                <p className="text-sm text-gray-400 text-left">
                                    Range: {device.config?.min ?? 0} - {device.config?.max ?? 100}
                                </p>
                                <div className="mt-3 h-2 bg-slate-100 rounded-full overflow-hidden">
                                    <div
                                        className={clsx("h-full bg-gradient-to-r", config.color)}
                                        style={{
                                            width: `${((device.state?.value ?? 0) / (device.config?.max ?? 100)) * 100}%`
                                        }}
                                    />
                                </div>
                            </div>
                        )}

                        {device.description && (
                            <p className="text-sm text-grey-400 mt-4 line-clamp-2 text-left">{device.description}</p>
                        )}
                    </div>

                    {warningText && (
                        <div className="px-6 pb-5">
                            <div className="flex items-center gap-2 text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-md px-3 py-2">
                                <AlertTriangle className="w-4 h-4" />
                                <span>{warningText}</span>
                            </div>
                        </div>
                    )}
                </Card>
            </div>
        </div>
    );
}