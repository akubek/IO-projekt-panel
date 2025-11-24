import { useEffect, useState } from 'react';
import './App.css';
import DeviceCard from './components/DeviceCard'; //importowanie komponentu DeviceCard

//Próba po³¹czenia frontu z backendem i wypisywania urz¹dzeñ z backendu, bazowane na przyk³adzie z weatherforecast
function App() {
    const [devices, setDevices] = useState();

    //Use effect nie rozumiem do czego to jest jeszcze, ale jest potrzebne do pobierania danych z backendu
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
                        <td>{device.status}</td>
                        <td>{new Date(device.lastSeen).toLocaleString()}</td>
                        <td>{device.localization}</td>
                    </tr>
                )}
            </tbody>
        </table>;

    //Wyœwietlanie danych z tej "tabeli"
    return (
        <div>
            <h1 id="tableLabel"> Urzadzenia IoT</h1>
            <p>Lista urzadzen</p>
            {contents}
            <DeviceCard></DeviceCard>
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