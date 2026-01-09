import { useEffect, useState } from 'react';
import './App.css';
import DeviceCard from './components/DeviceCard';
import LoggingInOpen from './components/LoggingInOpen';
import AddDeviceModal from './components/AddDeviceModal';
import ConfigureDeviceModal from './components/ConfigureDeviceModal';
import { Plus, Cpu, User, Home, LayoutGrid, Film, Zap } from "lucide-react"; // Import new icons
import { AnimatePresence } from "framer-motion";
import Button from "@mui/material/Button";
import { CircleDot } from "lucide-react";
import { motion } from "framer-motion";
import RoomList from './components/RoomList'; // Import the new component
import SceneList from './components/SceneList';
import AutomationList from './components/AutomationList';
import AddRoomModal from './components/AddRoomModal';
import DeviceDetailsModal from './components/DeviceDetailsModal';


//Próba połączenia frontu z backendem i wypisywania urządzeń z backendu, bazowane na przykładzie z weatherforecast
function App() {
    const [devices, setDevices] = useState([]);
    const [rooms, setRooms] = useState([]); // New state for rooms
    const [scenes, setScenes] = useState([]); // New state for scenes
    const [automations, setAutomations] = useState([]); // New state for automations
    const [activeTab, setActiveTab] = useState('devices'); // 'devices', 'rooms', 'scenes', or 'automations'
    const [selectedDevice, setSelectedDevice] = useState(null);
    const [loggingIn, setLoggingIn] = useState(false);

    // nowy stan: czy użytkownik jest zalogowany
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [authToken, setAuthToken] = useState(null);

    // Add-device modal state
    const [showAddModal, setShowAddModal] = useState(false);
    const [externalDevices, setExternalDevices] = useState([]);

    // Add-room modal state
    const [showAddRoomModal, setShowAddRoomModal] = useState(false);

    // Configure modal
    const [showConfigureModal, setShowConfigureModal] = useState(false);
    const [deviceToConfigure, setDeviceToConfigure] = useState(null);

    useEffect(() => {
        populateAllData();
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
            const res = await fetch('/device/admin/unconfigured', {
                headers: {
                    'Authorization': `Bearer ${authToken}`
                }
            });
            if (res.ok) {
                const data = await res.json();
                setExternalDevices(data);
            } else {
                console.error(`Failed to fetch unconfigured devices: ${res.status}`);
                setExternalDevices([]);
            }
        } catch (err) {
            console.error('Error fetching unconfigured devices:', err);
            setExternalDevices([]);
        }
    }

    function closeAddModal() {
        setShowAddModal(false);
        setExternalDevices([]);
    }

    // Called when user clicks a row in AddDeviceModal
    function handleSelectExternalDevice(apiDevice) {
        console.log('selected', apiDevice);
        setDeviceToConfigure(apiDevice);
        setShowAddModal(false);
        setShowConfigureModal(true);
    }

    // Called when ConfigureDeviceModal confirms Add
    function handleAddDevice(newDevice) {
        setDevices(prev => [...prev, newDevice]);
        setShowConfigureModal(false);
        setDeviceToConfigure(null);
    }

    async function handleAddRoom(newRoom) {
        try {
            // 1. Sends a POST request to the `/room` endpoint
            const response = await fetch('/room', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${authToken}`
                },
                body: JSON.stringify(newRoom),
            });
            if (response.ok) {
                const createdRoom = await response.json(); // Get the new room from the response
                setRooms(prevRooms => [...prevRooms, { ...createdRoom, devices: [] }]); // Add it to the state
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
                    'Authorization': `Bearer ${authToken}`
                }
            });
            if (response.ok) {
                console.log(`Scene ${sceneId} activated`);
                // Optionally, you could show a success notification here
            } else {
                console.error(`Failed to activate scene ${sceneId}`);
            }
        } catch (error) {
            console.error("Error activating scene:", error);
        }
    }

    function handleLogin(token) {
        setAuthToken(token);
        setIsLoggedIn(true);
        setLoggingIn(false);
    }

    function handleLogout() {
        setAuthToken(null);
        setIsLoggedIn(false);
    }

    //Wyświetlanie
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
                                    {isLoggedIn ? (<span className="font-semibold">Admin</span>
                                    ) : (
                                        <span className="font-semibold">User</span>
                                    )}
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
                        <Button onClick={() => setShowAddRoomModal(true)} className="!bg-blue-500 !text-white" startIcon={<Plus />}>
                            Add Room
                        </Button>
                    )}
                </div>
            </div>

            {/* Conditional Content */}
            <div className="mt-10">
                {activeTab === 'devices' && (
                    <motion.div
                        layout
                        className="px-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                        <AnimatePresence mode="popLayout">
                            {devices.map((device) => (
                                <DeviceCard
                                    key={device.id}
                                    device={device}
                                    onSelect={() => setSelectedDevice(device)} />)
                            )}
                        </AnimatePresence>
                    </motion.div>
                )}

                {activeTab === 'rooms' && (
                    <RoomList rooms={rooms} />
                )}

                {activeTab === 'scenes' && (
                    <SceneList scenes={scenes} onActivate={handleActivateScene} />
                )}

                {activeTab === 'automations' && (
                    <AutomationList automations={automations} />
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

            <DeviceDetailsModal
                open={!!selectedDevice}
                device={selectedDevice}
                onClose={() => setSelectedDevice(null)}
            />
        </div>
    );

    //Pobieranie danych z backendu!!!
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

            // For each room, fetch its devices
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