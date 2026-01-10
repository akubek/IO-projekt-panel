import React from 'react';
import RoomCard from './RoomCard';

function RoomList({ rooms, isAdmin, onAddDevice, onDelete, onRemoveDevice, onToggle, onSetValue, pendingCommandsByDeviceId, roomNamesByDeviceId, onSelectDevice }) {
    if (!rooms || rooms.length === 0) {
        return <p className="px-6 text-slate-500">No rooms found. Add a room to get started.</p>;
    }

    return (
        <div className="px-6 space-y-6">
            {rooms.map(room => (
                <RoomCard
                    key={room.id}
                    room={room}
                    isAdmin={isAdmin}
                    onAddDevice={onAddDevice}
                    onDelete={onDelete}
                    onRemoveDevice={onRemoveDevice}
                    onToggle={onToggle}
                    onSetValue={onSetValue}
                    pendingCommandsByDeviceId={pendingCommandsByDeviceId}
                    roomNamesByDeviceId={roomNamesByDeviceId}
                    onSelectDevice={onSelectDevice}
                />
            ))}
        </div>
    );
}

export default RoomList;