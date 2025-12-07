import React from 'react';
import Button from "@mui/material/Button";
import { Wand2 } from 'lucide-react';
import { motion } from 'framer-motion';
import { Home, Tv } from 'lucide-react';

const RoomCard = ({ room }) => {
    const deviceCount = room.devices?.length || 0;

    return (
        <motion.div
            layout
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.8 }}
            transition={{ type: 'spring', stiffness: 300, damping: 25 }}
            className="bg-white rounded-2xl shadow-md border border-slate-200 overflow-hidden"
        >
            <div className="p-5">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-slate-100 rounded-lg flex items-center justify-center">
                            <Home className="w-5 h-5 text-slate-600" />
                        </div>
                        <h2 className="text-lg font-bold text-slate-800">{room.name}</h2>
                    </div>
                </div>
                <div className="mt-4 text-sm text-slate-500">
                    {deviceCount} {deviceCount === 1 ? 'device' : 'devices'}
                </div>
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

function SceneList({ scenes, onActivate }) {
    if (!scenes || scenes.length === 0) {
        return <p className="px-6 text-slate-500">No scenes found. Create scenes to quickly control groups of devices.</p>;
    }

    return (
        <div className="px-6 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {scenes.map(scene => (
                <div key={scene.id} className="bg-white p-4 rounded-lg shadow-sm border border-slate-200 flex flex-col justify-between">
                    <h3 className="text-lg font-bold text-slate-800">{scene.name}</h3>
                    <p className="text-sm text-slate-500 mt-1 mb-4">
                        Activates {scene.actions?.length || 0} device action(s).
                    </p>
                    <Button
                        onClick={() => onActivate(scene.id)}
                        className="!bg-blue-500 !text-white hover:!bg-blue-600 w-full"
                        startIcon={<Wand2 className="w-4 h-4" />}
                    >
                        Activate
                    </Button>
                </div>
            ))}
        </div>
    );
}

export default SceneList;