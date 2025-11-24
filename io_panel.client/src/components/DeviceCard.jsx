export default function DeviceCard() {
    return (
        <div className="w-80 rounded-2xl shadow-lg p-4 bg-white/80 backdrop-blur-md border border-gray-200">
            <div className="space-y-1">
                <h2 className="text-xl font-semibold flex items-center gap-2">
                      Test - Sensor Node A12
                </h2>
                <p className="text-sm text-gray-500">Living Room - Online</p>
            </div>


            <div className="mt-4">
                <div className="grid grid-cols-2 gap-4">
                    <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                        <span className="text-xs text-gray-500">Temperature</span>
                        <span className="text-lg font-semibold">22.4^C</span>
                    </div>
                    <div className="flex flex-col bg-gray-100 rounded-xl p-3 items-start">
                        <span className="text-xs text-gray-500">Humidity</span>
                        <span className="text-lg font-semibold">41%</span>
                    </div>
                </div>
            </div>
        </div>
    );
}