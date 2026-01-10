import React from "react";
import { AnimatePresence } from "framer-motion";
import { motion } from "framer-motion";
import DeviceCard from "./DeviceCard";

function DeviceList({
    devices,
    roomNamesByDeviceId,
    isAdmin,
    onDelete,
    onSelect,
    onToggle,
    onSetValue,
    pendingCommandsByDeviceId
}) {
    if (!devices || devices.length === 0) {
        return <p className="px-6 text-slate-500">No devices found.</p>;
    }

    return (
        <motion.div
            layout
            className="px-6 grid gap-2 justify-start [grid-template-columns:repeat(auto-fit,minmax(320px,384px))] items-start"
        >
            <AnimatePresence mode="popLayout">
                {devices.map((device) => (
                    <DeviceCard
                        key={device.id}
                        device={device}
                        roomNames={roomNamesByDeviceId?.[device.id] ?? []}
                        isAdmin={isAdmin}
                        onDelete={() => onDelete && onDelete(device.id)}
                        onSelect={() => onSelect && onSelect(device)}
                        onToggle={onToggle}
                        onSetValue={onSetValue}
                        pendingCommand={pendingCommandsByDeviceId?.[device.id] ?? null}
                    />
                ))}
            </AnimatePresence>
        </motion.div>
    );
}

export default DeviceList;