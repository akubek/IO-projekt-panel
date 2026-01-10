import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Home, Tv, Plus, Trash2 } from 'lucide-react';
import DeviceCard from './DeviceCard';

const RoomCard = ({ room, isAdmin, onAddDevice, onDelete, onToggle, onSetValue, pendingCommandsByDeviceId, roomNamesByDeviceId, onSelectDevice }) => {
    const deviceCount = room.devices?.length || 0;

    const handleDelete = (e) => {
        e.stopPropagation();
        if (onDelete) {
            onDelete(room.id);
        }
    };

    return (
        <motion.div
            layout
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.8 }}
            transition={{ type: 'spring', stiffness: 300, damping: 25 }}
            className="bg-white rounded-2xl shadow-md border border-slate-200 overflow-hidden"
        >
            <div className="p-4">
                <div className="flex items-center justify-between gap-3">
                    <div className="flex items-center gap-3 min-w-0">
                        <div className="w-10 h-10 bg-slate-100 rounded-lg flex items-center justify-center">
                            <Home className="w-5 h-5 text-slate-600" />
                        </div>
                        <h2 className="text-lg font-bold text-slate-800 truncate">{room.name}</h2>
                    </div>

                    {isAdmin && (
                        <div className="flex items-center gap-2">
                            <button
                                type="button"
                                onClick={() => onAddDevice && onAddDevice(room)}
                                className="inline-flex items-center gap-2 px-4 py-2 rounded-md bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg"
                                aria-label={`Add device to ${room.name}`}
                            >
                                <Plus className="w-4 h-4 mr-2" />
                                Add device to room
                            </button>

                            <button
                                type="button"
                                onClick={handleDelete}
                                className="inline-flex items-center gap-2 px-3 py-2 rounded-md bg-gradient-to-r !text-white from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 shadow-lg"
                                aria-label={`Delete room ${room.name}`}
                            >
                                <Trash2 className="w-4 h-4" />
                                Delete
                            </button>
                        </div>
                    )}
                </div>

                <div className="mt-4 text-sm text-slate-500">
                    {deviceCount} {deviceCount === 1 ? 'device' : 'devices'}
                </div>

                {deviceCount > 0 && (
                    <div className="mt-4 grid gap-2 justify-start [grid-template-columns:repeat(auto-fit,minmax(320px,384px))] items-start">
                        <AnimatePresence mode="popLayout">
                            {room.devices.map((device) => (
                                <DeviceCard
                                    key={device.id}
                                    device={device}
                                    onSelect={() => onSelectDevice && onSelectDevice(device)}
                                    onToggle={onToggle}
                                    onSetValue={onSetValue}
                                    pendingCommand={pendingCommandsByDeviceId?.[device.id] ?? null}
                                    roomNames={roomNamesByDeviceId?.[device.id] ?? []}
                                />
                            ))}
                        </AnimatePresence>
                    </div>
                )}

                {deviceCount === 0 && (
                    <p className="mt-4 text-sm text-slate-500">No devices in this room.</p>
                )}
            </div>

            <div className="bg-slate-50 px-5 py-3 border-t border-slate-100">
                <div className="flex items-center gap-2 text-xs text-slate-500">
                    <Tv className="w-4 h-4" />
                    <span>View devices</span>
                </div>
            </div>
        </motion.div>
    );
};

export default RoomCard;