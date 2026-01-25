import React, { useMemo } from "react";
import { motion } from "framer-motion";
import Button from "@mui/material/Button";
import { Film, Wand2, Trash2 } from "lucide-react";

/**
 * Transforms target state values into readable strings based on device hardware type.
 */
function formatTargetStateForDevice(targetState, deviceType) {
    if (!targetState) return "-";

    const value = targetState.value;
    const unit = targetState.unit ? ` ${targetState.unit}` : "";

    if (deviceType === "switch") {
        return (value ?? 0) > 0 ? "ON" : "OFF";
    }

    if (value === null || value === undefined) return `0${unit}`;
    return `${value}${unit}`;
}

/*
  SceneCard
  A specialized automation component that displays a Scene
  It allows users to execute multiple commands
  simultaneously across different hardware.
*/
export default function SceneCard({ scene, canActivate, onActivate, deviceById, isAdmin, onDelete }) {
    const actionCount = scene.actions?.length ?? 0;

    const actions = useMemo(() => scene.actions ?? [], [scene.actions]);

    const handleDelete = () => {
        if (onDelete) {
            onDelete(scene.id);
        }
    };

    return (
        <motion.div
            layout
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.95 }}
            whileHover={{ y: -2 }}
            transition={{ duration: 0.2 }}
            className="bg-white rounded-2xl shadow-md border border-slate-200 overflow-hidden"
        >
            <div className="p-5 flex flex-col md:flex-row md:items-start md:justify-between gap-4">
                <div className="min-w-0">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-slate-100 rounded-lg flex items-center justify-center">
                            <Film className="w-5 h-5 text-slate-600" />
                        </div>
                        <h3 className="text-lg font-bold text-slate-800 truncate">{scene.name}</h3>
                    </div>

                    <p className="text-sm text-slate-500 mt-2">
                        {scene.isPublic ? "Public" : "Private"} - {actionCount} action(s)
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

                    {isAdmin && (
                        <Button
                            onClick={handleDelete}
                            className="bg-gradient-to-r !text-white from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 shadow-lg"
                            startIcon={<Trash2 className="w-4 h-4" />}
                        >
                            Delete
                        </Button>
                    )}

                    {!canActivate && (
                        <p className="text-xs text-slate-500">Log in to activate private scenes.</p>
                    )}
                </div>
            </div>

            {/* Content Section */}
            <div className="border-t border-slate-100 bg-slate-50 p-5">
                {actionCount === 0 ? (
                    <p className="text-sm text-slate-500">No actions in this scene.</p>
                ) : (
                    <div className="grid gap-2 [grid-template-columns:repeat(auto-fit,minmax(240px,1fr))]">
                        {actions.map(a => {
                            const d = deviceById.get(a.deviceId);
                            const displayName = d?.displayName ?? d?.name ?? a.deviceId;
                            const deviceType = d?.type;

                            return (
                                <div
                                    key={a.deviceId}
                                    className="bg-white border border-slate-200 rounded-lg p-3 flex items-center justify-between gap-3"
                                >
                                    <div className="min-w-0">
                                        <div className="font-semibold text-slate-800 truncate">{displayName}</div>
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
        </motion.div>
    );
}