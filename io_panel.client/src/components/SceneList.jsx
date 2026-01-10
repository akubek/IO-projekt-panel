import React, { useMemo } from "react";
import SceneCard from "./SceneCard";

function SceneList({ scenes, onActivate, isLoggedIn, devices, isAdmin, onDelete }) {
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
                    <SceneCard
                        key={scene.id}
                        scene={scene}
                        canActivate={canActivate}
                        onActivate={onActivate}
                        deviceById={deviceById}
                        isAdmin={isAdmin}
                        onDelete={onDelete}
                    />
                );
            })}
        </div>
    );
}

export default SceneList;