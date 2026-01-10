import { useEffect, useState, useMemo } from 'react';
import './App.css';
import DeviceCard from './components/DeviceCard';
import LoggingInOpen from './components/LoggingInOpen';
import AddDeviceModal from './components/AddDeviceModal';
import ConfigureDeviceModal from './components/ConfigureDeviceModal';
import { Plus, Cpu, User, Home, LayoutGrid, Film, Zap } from "lucide-react";
import { AnimatePresence } from "framer-motion";
import Button from "@mui/material/Button";
import { CircleDot } from "lucide-react";
import { motion } from "framer-motion";
import RoomList from './components/RoomList';
import SceneList from './components/SceneList';
import AutomationList from './components/AutomationList';
import AddRoomModal from './components/AddRoomModal';
import DeviceDetailsModal from './components/DeviceDetailsModal';
import AddDeviceToRoomModal from './components/AddDeviceToRoomModal';
import CreateSceneModal from './components/CreateSceneModal';
import * as signalR from "@microsoft/signalr";

function App() {
    const [devices, setDevices] = useState([]);
    const [rooms, setRooms] = useState([]);
    const [scenes, setScenes] = useState([]);
    const [automations, setAutomations] = useState([]);
    const [activeTab, setActiveTab] = useState('devices');
    const [selectedDevice, setSelectedDevice] = useState(null);
    const [loggingIn, setLoggingIn] = useState(false);

    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [authToken, setAuthToken] = useState(null);

    const [showAddModal, setShowAddModal] = useState(false);
    const [externalDevices, setExternalDevices] = useState([]);

    const [showAddRoomModal, setShowAddRoomModal] = useState(false);

    const [showConfigureModal, setShowConfigureModal] = useState(false);
    const [deviceToConfigure, setDeviceToConfigure] = useState(null);

    const [showAddDeviceToRoomModal, setShowAddDeviceToRoomModal] = useState(false);
    const [roomToAddDevice, setRoomToAddDevice] = useState(null);

    const [showCreateSceneModal, setShowCreateSceneModal] = useState(false);

    // deviceId -> { targetValue, unit, sentAtMs }
    const [pendingCommandsByDeviceId, setPendingCommandsByDeviceId] = useState({});

    const roomNamesByDeviceId = useMemo(() => {
        const map = {};

        for (const room of rooms ?? []) {
            for (const device of room.devices ?? []) {
                const current = map[device.id] ?? [];
                map[device.id] = current.includes(room.name) ? current : [...current, room.name];
            }
        }

        return map;
    }, [rooms]);

    useEffect(() => {
        populateAllData();
    }, []);

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/device-updates")
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        connection.on("deviceUpdated", (update) => {
            setDevices((prev) =>
                prev.map((d) => {
                    if (d.id !== update.deviceId) {
                        return d;
                    }

                    return {
                        ...d,
                        state: {
                            ...d.state,
                            value: update.value ?? d.state?.value ?? 0,
                            unit: update.unit ?? d.state?.unit
                        },
                        malfunctioning: update.malfunctioning ?? d.malfunctioning
                    };
                })
            );

            setRooms((prevRooms) =>
                prevRooms.map((room) => {
                    if (!room.devices || room.devices.length === 0) {
                        return room;
                    }

                    const updatedDevices = room.devices.map((d) => {
                        if (d.id !== update.deviceId) {
                            return d;
                        }

                        return {
                            ...d,
                            state: {
                                ...d.state,
                                value: update.value ?? d.state?.value ?? 0,
                                unit: update.unit ?? d.state?.unit
                            },
                            malfunctioning: update.malfunctioning ?? d.malfunctioning
                        };
                    });

                    return { ...room, devices: updatedDevices };
                })
            );

            setSelectedDevice((prev) => {
                if (!prev || prev.id !== update.deviceId) {
                    return prev;
                }

                return {
                    ...prev,
                    state: {
                        ...prev.state,
                        value: update.value ?? prev.state?.value ?? 0,
                        unit: update.unit ?? prev.state?.unit
                    },
                    malfunctioning: update.malfunctioning ?? prev.malfunctioning
                };
            });

            // Clear "pending" when we reach the target value.
            setPendingCommandsByDeviceId((prev) => {
                const pending = prev[update.deviceId];
                if (!pending) {
                    return prev;
                }

                const updateValue = update.value;
                if (typeof updateValue !== "number") {
                    return prev;
                }

                const eps = 1e-6;
                if (Math.abs(updateValue - pending.targetValue) > eps) {
                    return prev;
                }

                const { [update.deviceId]: _, ...rest } = prev;
                return rest;
            });
        });

        connection.start().catch((err) => console.error("SignalR start failed:", err));

        return () => {
            connection.stop().catch(() => { });
        };
    }, []);

    function populateAllData() {
        populateDeviceData();
        populateRoomData();
        populateSceneData();
        populateAutomationData();
    }

    async function openAddModal() {
        setShowAddModal(true);

        try {
            const headers = {};
            if (authToken) {
                headers.Authorization = `Bearer ${authToken}`;
            }

            const res = await fetch('/device/admin/unconfigured', { headers });

            if (res.status === 401 || res.status === 403) {
                console.warn("Not authorized to view unconfigured devices.");
                setExternalDevices([]);
                return;
            }

            if (!res.ok) {
                const text = await res.text();
                console.error(`Failed to fetch unconfigured devices: ${res.status} ${text}`);
                setExternalDevices([]);
                return;
            }

            const data = await res.json();
            setExternalDevices(Array.isArray(data) ? data : (data.devices ?? []));
        } catch (err) {
            console.error('Error fetching unconfigured devices:', err);
            setExternalDevices([]);
        }
    }

    function closeAddModal() {
        setShowAddModal(false);
        setExternalDevices([]);
    }

    function handleSelectExternalDevice(apiDevice) {
        console.log('selected', apiDevice);
        setDeviceToConfigure(apiDevice);
        setShowAddModal(false);
        setShowConfigureModal(true);
    }

    function handleAddDevice(newDevice) {
        setDevices(prev => [...prev, newDevice]);
        setShowConfigureModal(false);
        setDeviceToConfigure(null);
    }

    async function handleAddRoom(newRoom) {
        try {
            const response = await fetch('/room', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(authToken ? { Authorization: `Bearer ${authToken}` } : {})
                },
                body: JSON.stringify(newRoom),
            });

            if (response.ok) {
                const createdRoom = await response.json();
                setRooms(prevRooms => [...prevRooms, { ...createdRoom, devices: [] }]);
            } else {
                console.error("Failed to add room");
            }
        } catch (error) {
            console.error("Error adding room:", error);
        }
    }

    function closeConfigureModal() {
        setShowConfigureModal(false);
        setDeviceToConfigure(null);
    }

    async function handleActivateScene(sceneId) {
        try {
            const response = await fetch(`/scene/${sceneId}/activate`, {
                method: 'POST',
                headers: {
                    ...(authToken ? { Authorization: `Bearer ${authToken}` } : {})
                }
            });

            if (response.ok) {
                console.log(`Scene ${sceneId} activated`);
            } else {
                console.error(`Failed to activate scene ${sceneId}`);
            }
        } catch (error) {
            console.error("Error activating scene:", error);
        }
    }

    async function handleDeleteRoom(roomId) {
        if (!authToken) return;

        const ok = window.confirm("Delete this room?");
        if (!ok) return;

        try {
            const res = await fetch(`/room/${roomId}`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${authToken}` }
            });

            if (!res.ok) {
                const text = await res.text();
                console.error(`Failed to delete room: ${res.status} ${text}`);
                return;
            }

            setRooms(prev => prev.filter(r => r.id !== roomId));
        } catch (err) {
            console.error("Error deleting room:", err);
        }
    }

    async function handleDeleteScene(sceneId) {
        if (!authToken) return;

        const ok = window.confirm("Delete this scene?");
        if (!ok) return;

        try {
            const res = await fetch(`/scene/${sceneId}`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${authToken}` }
            });

            if (!res.ok) {
                const text = await res.text();
                console.error(`Failed to delete scene: ${res.status} ${text}`);
                return;
            }

            setScenes(prev => prev.filter(s => s.id !== sceneId));
        } catch (err) {
            console.error("Error deleting scene:", err);
        }
    }

    async function handleDeleteAutomation(automationId) {
        if (!authToken) return;

        const ok = window.confirm("Delete this automation?");
        if (!ok) return;

        try {
            const res = await fetch(`/automation/${automationId}`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${authToken}` }
            });

            if (!res.ok) {
                const text = await res.text();
                console.error(`Failed to delete automation: ${res.status} ${text}`);
                return;
            }

            setAutomations(prev => prev.filter(a => a.id !== automationId));
        } catch (err) {
            console.error("Error deleting automation:", err);
        }
    }

    async function handleDeleteDevice(deviceId) {
        if (!authToken) return;

        const ok = window.confirm("Delete this device? It will be removed from all rooms.");
        if (!ok) return;

        try {
            const res = await fetch(`/device/admin/${deviceId}`, {
                method: 'DELETE',
                headers: { Authorization: `Bearer ${authToken}` }
            });

            if (!res.ok) {
                const text = await res.text();
                console.error(`Failed to delete device: ${res.status} ${text}`);
                return;
            }

            setDevices(prev => prev.filter(d => d.id !== deviceId));
            setRooms(prev =>
                prev.map(r => ({ ...r, devices: (r.devices ?? []).filter(d => d.id !== deviceId) }))
            );
            setSelectedDevice(prev => (prev?.id === deviceId ? null : prev));
        } catch (err) {
            console.error("Error deleting device:", err);
        }
    }

    function handleLogin(token) {
        setAuthToken(token);
        setIsLoggedIn(!!token);
        setLoggingIn(false);
    }

    function handleLogout() {
        setAuthToken(null);
        setIsLoggedIn(false);
    }

    function openAddDeviceToRoom(room) {
        setRoomToAddDevice(room);
        setShowAddDeviceToRoomModal(true);
    }

    function closeAddDeviceToRoom() {
        setShowAddDeviceToRoomModal(false);
        setRoomToAddDevice(null);
    }

    function handleDeviceAddedToRoom(roomId, device) {
        setRooms(prev =>
            prev.map(r => {
                if (r.id !== roomId) {
                    return r;
                }

                const existing = r.devices ?? [];
                if (existing.some(d => d.id === device.id)) {
                    return r;
                }

                return { ...r, devices: [...existing, device] };
            })
        );
    }

    function handleSceneCreated(createdScene) {
        setScenes(prev => [...prev, createdScene]);
    }

    async function sendDeviceState(deviceId, state) {
        try {
            const headers = {
                'Content-Type': 'application/json',
                ...(authToken ? { Authorization: `Bearer ${authToken}` } : {})
            };

            const res = await fetch(`/device/${deviceId}/state`, {
                method: 'POST',
                headers,
                body: JSON.stringify(state)
            });

            if (!res.ok) {
                const text = await res.text();
                console.error(`Failed to send device state: ${res.status} ${text}`);
            }
        } catch (err) {
            console.error("Error sending device state:", err);
        }
    }

    function markDevicePending(deviceId, targetValue, unit) {
        setPendingCommandsByDeviceId((prev) => ({
            ...prev,
            [deviceId]: {
                targetValue,
                unit,
                sentAtMs: Date.now()
            }
        }));
    }

    function handleToggleDevice(device, nextIsOn) {
        if (!device) return;
        if (device.config?.readOnly) return;

        const value = nextIsOn ? 1 : 0;

        const unit =
            device.state?.unit ??
            (device.type === "switch" ? "bool" : null);

        markDevicePending(device.id, value, unit);

        void sendDeviceState(device.id, { value, unit });
    }

    function handleSetSliderDeviceValue(device, value) {
        if (!device) return;
        if (device.config?.readOnly) return;

        const unit = device.state?.unit ?? null;

        markDevicePending(device.id, value, unit);

        void sendDeviceState(device.id, { value, unit });
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-slate-100">
            {/* Header */}
            <div className="border-b border-slate-200 bg-white/80 backdrop-blur-sm sticky top-0 z-10">
                <div className="w-full px-6 py-6">
                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-cyan-500 rounded-2xl flex items-center justify-center shadow-lg">
                                <Cpu className="w-6 h-6 text-white" />
                            </div>
                            <div>
                                <h1 className="text-2xl font-bold text-slate-900">IoT Device Controller</h1>
                                <p className="text-sm text-slate-500">Simulate and control virtual devices</p>
                            </div>
                        </div>
                        <div className="flex items-center gap-3">
                            <div className="hidden sm:flex items-center gap-2">
                                <div
                                    className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-medium shadow-sm border ${isLoggedIn
                                        ? "bg-blue-50 border-blue-200 text-blue-700"
                                        : "bg-slate-100 border-slate-300 text-slate-600"}`}>

                                    <span className="font-semibold">{isLoggedIn ? "Admin" : "User"}</span>
                                    <span className="flex items-center gap-1 text-xs opacity-80">
                                        <CircleDot className="w-3 h-3" />
                                        active
                                    </span>
                                </div>
                            </div>

                            {!isLoggedIn ? (
                                <Button disabled className="bg-gradient-to-r !text-white from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 shadow-lg">
                                    <Plus className="w-4 h-4 mr-2" />
                                    Add Device
                                </Button>
                            ) : (
                                <Button onClick={openAddModal} className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg">
                                    <Plus className="w-4 h-4 mr-2" />
                                    Add Device
                                </Button>
                            )}

                            {!isLoggedIn ? (
                                <Button className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg" onClick={() => setLoggingIn(true)}>
                                    <User className="w-4 h-4 mr-2" />
                                    Log in
                                </Button>
                            ) : (
                                <Button className="bg-gradient-to-r !text-white from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 shadow-lg" onClick={handleLogout}>
                                    <User className="w-4 h-4 mr-2" />
                                    Log out
                                </Button>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Tekst widoczny tylko po zalogowaniu (Admin) */}
            {isLoggedIn && (
                <div className="w-full px-6 py-4 bg-red-100 border-l-4 border-red-500">
                    <p className="text-red-700 font-bold">⚠️ Uwaga: Ten tekst widzą tylko zalogowani użytkownicy (ADMIN)! ⚠️</p>
                </div>
            )}

            {/* Tabs Navigation */}
            <div className="px-6 mt-6 border-b border-slate-200">
                <div className="flex justify-between items-center">
                    <div className="flex gap-4">
                        <button
                            onClick={() => setActiveTab('devices')}
                            className={`flex items-center gap-2 pb-3 border-b-2 ${activeTab === 'devices' ? 'border-blue-500 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-800'}`}>
                            <LayoutGrid className="w-5 h-5" />
                            <span className="font-semibold">All Devices</span>
                        </button>
                        <button
                            onClick={() => setActiveTab('rooms')}
                            className={`flex items-center gap-2 pb-3 border-b-2 ${activeTab === 'rooms' ? 'border-blue-500 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-800'}`}>
                            <Home className="w-5 h-5" />
                            <span className="font-semibold">Rooms</span>
                        </button>
                        <button
                            onClick={() => setActiveTab('scenes')}
                            className={`flex items-center gap-2 pb-3 border-b-2 ${activeTab === 'scenes' ? 'border-blue-500 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-800'}`}>
                            <Film className="w-5 h-5" />
                            <span className="font-semibold">Scenes</span>
                        </button>
                        <button
                            onClick={() => setActiveTab('automations')}
                            className={`flex items-center gap-2 pb-3 border-b-2 ${activeTab === 'automations' ? 'border-blue-500 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-800'}`}>
                            <Zap className="w-5 h-5" />
                            <span className="font-semibold">Automations</span>
                        </button>
                    </div>
                    {isLoggedIn && activeTab === 'rooms' && (
                        <Button
                            onClick={() => setShowAddRoomModal(true)}
                            className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg">
                            <Plus className="w-4 h-4 mr-2" />
                            Add Room
                        </Button>
                    )}

                    {isLoggedIn && activeTab === 'scenes' && (
                        <Button
                            onClick={() => setShowCreateSceneModal(true)}
                            className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg">
                            <Plus className="w-4 h-4 mr-2" />
                            Add Scene
                        </Button>
                    )}
                </div>
            </div>

            {/* Conditional Content */}
            <div className="mt-10">
                {activeTab === 'devices' && (
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
                                    isAdmin={isLoggedIn}
                                    onDelete={() => handleDeleteDevice(device.id)}
                                    onSelect={() => setSelectedDevice(device)}
                                    onToggle={handleToggleDevice}
                                    onSetValue={handleSetSliderDeviceValue}
                                    pendingCommand={pendingCommandsByDeviceId[device.id] ?? null}
                                />)
                            )}
                        </AnimatePresence>
                    </motion.div>
                )}

                {activeTab === 'rooms' && (
                    <RoomList
                        rooms={rooms}
                        isAdmin={isLoggedIn}
                        onAddDevice={openAddDeviceToRoom}
                        onDelete={handleDeleteRoom}
                        onToggle={handleToggleDevice}
                        onSetValue={handleSetSliderDeviceValue}
                        pendingCommandsByDeviceId={pendingCommandsByDeviceId}
                        roomNamesByDeviceId={roomNamesByDeviceId}
                    />
                )}

                {activeTab === 'scenes' && (
                    <SceneList
                        scenes={scenes}
                        onActivate={handleActivateScene}
                        isLoggedIn={isLoggedIn}
                        devices={devices}
                        isAdmin={isLoggedIn}
                        onDelete={handleDeleteScene}
                    />
                )}

                {activeTab === 'automations' && (
                    <AutomationList
                        automations={automations}
                        isAdmin={isLoggedIn}
                        onDelete={handleDeleteAutomation}
                    />
                )}
            </div>

            <LoggingInOpen
                open={loggingIn}
                onClose={() => setLoggingIn(false)}
                onLogin={handleLogin}
            />

            <AddDeviceModal
                open={showAddModal}
                devices={externalDevices}
                onClose={closeAddModal}
                onSelect={handleSelectExternalDevice}
            />

            <AddRoomModal
                open={showAddRoomModal}
                onClose={() => setShowAddRoomModal(false)}
                onAdd={handleAddRoom}
            />

            <ConfigureDeviceModal
                open={showConfigureModal}
                apiDevice={deviceToConfigure}
                onClose={closeConfigureModal}
                onAdd={handleAddDevice}
            />

            <AddDeviceToRoomModal
                open={showAddDeviceToRoomModal}
                room={roomToAddDevice}
                devices={devices}
                authToken={authToken}
                onClose={closeAddDeviceToRoom}
                onAdded={handleDeviceAddedToRoom}
            />

            <DeviceDetailsModal
                open={!!selectedDevice}
                device={selectedDevice}
                onClose={() => setSelectedDevice(null)}
            />

            <CreateSceneModal
                open={showCreateSceneModal}
                devices={devices}
                authToken={authToken}
                onClose={() => setShowCreateSceneModal(false)}
                onCreated={handleSceneCreated}
            />
        </div>
    );

    async function populateDeviceData() {
        const response = await fetch('/device');
        if (response.ok) {
            const data = await response.json();
            setDevices(data);
        }
    }

    async function populateRoomData() {
        try {
            const roomsResponse = await fetch('/room');
            if (!roomsResponse.ok) return;

            const roomsData = await roomsResponse.json();

            const roomsWithDevices = await Promise.all(
                roomsData.map(async (room) => {
                    const devicesResponse = await fetch(`/room/${room.id}/devices`);
                    const devicesData = devicesResponse.ok ? await devicesResponse.json() : [];
                    return { ...room, devices: devicesData };
                })
            );

            setRooms(roomsWithDevices);
        } catch (error) {
            console.error("Failed to fetch room data:", error);
            setRooms([]);
        }
    }

    async function populateSceneData() {
        try {
            const response = await fetch('/scene');
            if (response.ok) {
                const data = await response.json();
                setScenes(data);
            }
        } catch (error) {
            console.error("Failed to fetch scene data:", error);
            setScenes([]);
        }
    }

    async function populateAutomationData() {
        try {
            const response = await fetch('/automation');
            if (response.ok) {
                const data = await response.json();
                setAutomations(data);
            }
        } catch (error) {
            console.error("Failed to fetch automation data:", error);
            setAutomations([]);
        }
    }
}

export default App;