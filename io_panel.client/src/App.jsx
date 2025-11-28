import { useEffect, useState } from 'react';
import './App.css';
import DeviceCard from './components/DeviceCard'; //importowanie komponentu DeviceCard
import LoggingInOpen from './components/LoggingInOpen'; //importowanie komponentu LoggingInOpen

import { Plus, Cpu, User } from "lucide-react";
import { AnimatePresence } from "framer-motion";
import Button from "@mui/material/Button";


//Próba po³¹czenia frontu z backendem i wypisywania urz¹dzeñ z backendu, bazowane na przyk³adzie z weatherforecast
function App() {
    const [devices, setDevices] = useState([]);
    const [selectedDevice, setSelectedDevice] = useState(null);
    const [loggingIn, setLoggingIn] = useState(false);

    //Use effect, potrzebne do pobierania danych z backendu
    useEffect(() => {
        populateDeviceData();
    }, []);

    //Pobieranie danych z backendu i zapisywanie ich w "tabeli"
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

    //Wyœwietlanie
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
                            <Button className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg">
                                <Plus className="w-4 h-4 mr-2" />
                                Add Device
                            </Button>
                            <Button className="bg-gradient-to-r !text-white from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 shadow-lg" onClick={() => setLoggingIn(true)}>
                                <User className="w-4 h-4 mr-2" />
                                Log in
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
            {/* Wyœwietlanie danych z tej "tabeli", najpierw normalnie potem w postaci kart */}
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
                            onSelect={() => setSelectedDevice(device)}/>
                    ))}
                </div>
            </div>
            <LoggingInOpen open={loggingIn} onClose={() => setLoggingIn(false)} />
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