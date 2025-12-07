import React from 'react';
import RoomCard from './RoomCard';

function RoomList({ rooms }) {
    if (!rooms || rooms.length === 0) {
        return <p className="px-6 text-slate-500">No rooms found. Add a room to get started.</p>;
    }

    return (
        <div className="px-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {rooms.map(room => (
                <RoomCard key={room.id} room={room} />
            ))}
        </div>
    );
}

export default RoomList;