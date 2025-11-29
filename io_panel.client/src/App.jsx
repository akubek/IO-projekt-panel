import { useEffect, useState } from 'react';
import './App.css';
import DeviceCard from './components/DeviceCard';
import LoggingInOpen from './components/LoggingInOpen';
import AddDeviceModal from './components/AddDeviceModal';
import ConfigureDeviceModal from './components/ConfigureDeviceModal';
import { Plus, Cpu, User } from "lucide-react";
import { AnimatePresence } from "framer-motion";
import Button from "@mui/material/Button";
import { CircleDot } from "lucide-react";


//Próba połączenia frontu z backendem i wypisywania urządzeń z backendu, bazowane na przykładzie z weatherforecast
function App() {
    const [devices, setDevices] = useState([]);
    const [_selectedDevice, setSelectedDevice] = useState(null);
    const [loggingIn, setLoggingIn] = useState(false);

    // nowy stan: czy użytkownik jest zalogowany
    const [isLoggedIn, setIsLoggedIn] = useState(false);

    // Add-device modal state
    const [showAddModal, setShowAddModal] = useState(false);
    const [externalDevices, setExternalDevices] = useState([]);

    // Configure modal
    const [showConfigureModal, setShowConfigureModal] = useState(false);
    const [deviceToConfigure, setDeviceToConfigure] = useState(null);

    useEffect(() => {
        populateDeviceData();
    }, []);

    async function openAddModal() {
        setShowAddModal(true);
        try {
            const res = await fetch('/device/external');
            if (res.ok) {
                const data = await res.json();
                setExternalDevices(data);
            } else {
                setExternalDevices([]);
            }
        } catch {
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

    function closeConfigureModal() {
        setShowConfigureModal(false);
        setDeviceToConfigure(null);
    }

    const contents = devices === undefined
        ? <p><em>Loading...</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Status</th>
                    <th>Last seen</th>
                    <th>Localization</th>
                </tr>
            </thead>
            <tbody>
                {devices.map(device =>
                    <tr key={device.id}>
                        <td>{device.id}</td>
                        <td>{device.name}</td>
                        <td>{device.type}</td>
                        <td>{device.status}</td>
                        <td>{new Date(device.lastSeen).toLocaleString()}</td>
                        <td>{device.localization}</td>
                    </tr>
                )}
            </tbody>
        </table>;

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

                            <Button onClick={openAddModal} className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg">
                                <Plus className="w-4 h-4 mr-2" />
                                Add Device
                            </Button>

                            {!isLoggedIn ? (
                                <Button className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg" onClick={() => setLoggingIn(true)}>
                                    <User className="w-4 h-4 mr-2" />
                                    Log in
                                </Button>
                            ) : (
                                <Button className="bg-gradient-to-r !text-white from-gray-600 to-gray-700 hover:from-gray-700 hover:to-gray-800 shadow-lg" onClick={() => setIsLoggedIn(false)}>
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
            {/* Wyświetlanie danych z tej "tabeli", najpierw normalnie potem w postaci kart */}
            <div>
                <h1 id="tableLabel"> Urzadzenia IoT</h1>
                <p>Testowa lista urzadzen do porownania z kartami urzadzen</p>
                {contents}
                <p>Testowa lista urzadzen w postaci kart</p>
                <div className="mt-6 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                    {devices.map((device) => (
                        <DeviceCard
                            key={device.id}
                            device={device}
                            onSelect={() => setSelectedDevice(device)} />
                    ))}
                </div>
            </div>

            <LoggingInOpen
                open={loggingIn}
                onClose={() => setLoggingIn(false)}
                onLogin={() => { setIsLoggedIn(true); setLoggingIn(false); }}
            />

            <AddDeviceModal
                open={showAddModal}
                devices={externalDevices}
                onClose={closeAddModal}
                onSelect={handleSelectExternalDevice}
            />

            <ConfigureDeviceModal
                open={showConfigureModal}
                apiDevice={deviceToConfigure}
                onClose={closeConfigureModal}
                onAdd={handleAddDevice}
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
}

export default App;