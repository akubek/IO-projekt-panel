export default function DeviceCard({ device }) {

    const cardColor = device.type === "Sensor" ? "bg-blue-100" : device.type === "Slider" ? "bg-green-100" : "bg-red-100";

    return (
        <div className={`w-80 rounded-2xl shadow-lg p-4 ${cardColor}`}>
            {/*Header*/}
            <div className="space-y-1">
                <h2 className="text-xl font-semibold flex items-center gap-2 text-slate-900 dark:text-slate-50">
                    {device.name}
                </h2>
                <p className="text-sm text-slate-500 dark:text-slate-400">{device.localization} - {device.status}</p>
            </div>

            <div className="mt-4">
                {/*Content*/}
                {device.type == "Switch" && (
                    <div className="grid grid-cols-2 gap-4">
                    <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                        <span className="text-xs text-gray-500">Temperature</span>
                        <span className="text-lg font-semibold">22.4^C</span>
                    </div>
                    <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                        <span className="text-xs text-gray-500">Humidity</span>
                        <span className="text-lg font-semibold">41%</span>
                    </div>
                    </div>)}

                {device.type == "Sensor" && (
                    <div className="grid grid-cols-2 gap-4">
                        <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                            <span className="text-xs text-gray-500">Temperature</span>
                            <span className="text-lg font-semibold">22.4^C</span>
                        </div>
                        <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                            <span className="text-xs text-gray-500">Humidity</span>
                            <span className="text-lg font-semibold">41%</span>
                        </div>
                    </div>)}

                {device.type == "Slider" && (
                    <div className="grid grid-cols-2 gap-4">
                        <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                            <span className="text-xs text-gray-500">Temperature</span>
                            <span className="text-lg font-semibold">22.4^C</span>
                        </div>
                        <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                            <span className="text-xs text-gray-500">Humidity</span>
                            <span className="text-lg font-semibold">41%</span>
                        </div>
                    </div>)}
            </div>
        </div>
    );
}