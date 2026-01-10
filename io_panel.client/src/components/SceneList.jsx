import React, { useMemo } from "react";
import Button from "@mui/material/Button";
import { Wand2 } from "lucide-react";

function formatTargetStateForDevice(targetState, deviceType) {
    if (!targetState) return "-";

    const value = targetState.value;
    const unit = targetState.unit ? ` ${targetState.unit}` : "";

    if (deviceType === "switch") {
        return (value ?? 0) > 0 ? "ON" : "OFF";
    }

    // slider/sensor/etc: always show numeric value (0 is valid)
    if (value === null || value === undefined) return `0${unit}`;
    return `${value}${unit}`;
}

function SceneList({ scenes, onActivate, isLoggedIn, devices }) {
    const deviceById = useMemo(() => {
        const map = new Map();
        (devices ?? []).forEach(d => map.set(d.id, d));
        return map;
    }, [devices]);

    if (!scenes || scenes.length === 0) {
        return <p className="px-6 text-slate-500">No scenes found. Create scenes to quickly control groups of devices.</p>;
    }

    return (
        <div className="px-6 space-y-6">
            {scenes.map(scene => {
                const canActivate = scene.isPublic || isLoggedIn;

                return (
                    <div
                        key={scene.id}
                        className="bg-white rounded-2xl shadow-md border border-slate-200 overflow-hidden"
                    >
                        <div className="p-5 flex flex-col md:flex-row md:items-start md:justify-between gap-4">
                            <div className="min-w-0">
                                <h3 className="text-lg font-bold text-slate-800 truncate">{scene.name}</h3>
                                <p className="text-sm text-slate-500 mt-1">
                                    {scene.isPublic ? "Public" : "Private"} : {scene.actions?.length || 0} action(s)
                                </p>
                            </div>

                            <div className="flex flex-col items-stretch md:items-end gap-2 w-full md:w-auto">
                                <Button
                                    onClick={() => onActivate(scene.id)}
                                    disabled={!canActivate}
                                    className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
                                    startIcon={<Wand2 className="w-4 h-4" />}
                                >
                                    Activate
                                </Button>

                                {!canActivate && (
                                    <p className="text-xs text-slate-500">Log in to activate private scenes.</p>
                                )}
                            </div>
                        </div>

                        <div className="border-t border-slate-100 bg-slate-50 p-5">
                            {(scene.actions?.length ?? 0) === 0 ? (
                                <p className="text-sm text-slate-500">No actions in this scene.</p>
                            ) : (
                                <div className="grid gap-2 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
                                    {scene.actions.map(a => {
                                        const d = deviceById.get(a.deviceId);
                                        const displayName = d?.displayName ?? d?.name ?? a.deviceId;
                                        const deviceType = d?.type;

                                        return (
                                            <div
                                                key={a.deviceId}
                                                className="bg-white border border-slate-200 rounded-lg p-3 flex items-center justify-between gap-3"
                                            >
                                                <div className="min-w-0">
                                                    <div className="font-semibold text-slate-800 truncate">
                                                        {displayName}
                                                    </div>
                                                    <div className="text-xs text-slate-500">
                                                        Target:{" "}
                                                        <span className="font-semibold text-slate-700">
                                                            {formatTargetStateForDevice(a.targetState, deviceType)}
                                                        </span>
                                                    </div>
                                                </div>

                                                <div className="text-xs px-2 py-1 rounded-full bg-slate-100 text-slate-600 whitespace-nowrap">
                                                    {(deviceType ?? "device").toUpperCase()}
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

export default SceneList;