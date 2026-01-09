import React from "react";
import { motion } from "framer-motion"
import Card from "@mui/material/Card";
import Badge from "@mui/material/Badge";
import Switch from "@mui/material/Switch";
import Slider from "@mui/material/Slider";
import { Power, Gauge, Activity, MapPin } from "lucide-react";
import clsx from "clsx";

const deviceTypeConfig = {
    switch: {
        icon: Power,
        color: "from-blue-500 to-blue-600",
        bgLight: "bg-blue-50",
        textColor: "text-blue-700",
        borderColor: "border-blue-200"
    },
    slider: {
        icon: Gauge,
        color: "from-purple-500 to-purple-600",
        bgLight: "bg-purple-50",
        textColor: "text-purple-700",
        borderColor: "border-purple-200"
    },
    sensor: {
        icon: Activity,
        color: "from-green-500 to-green-600",
        bgLight: "bg-green-50",
        textColor: "text-green-700",
        borderColor: "border-green-200"
    }
};

export default function DeviceCard({ device, onSelect }) {

    // get config based on device type
    const config = deviceTypeConfig[device.type] || deviceTypeConfig.switch;
    const Icon = config.icon;
    const isOn = device.state?.value > 0;

    //handler for quick toggle on a switch device
    const handleQuickToggle = (e) => {
        e.stopPropagation();
        if (device.type === 'switch') {
            //place onUpdate logic here!!!
            console.log("Handler quick toggle dziala!");
        }
    };

    return (
        <motion.div
            layout
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.9 }}
            whileHover={{ y: -4 }}
            transition={{ duration: 0.2 }}
            className="w-full"
        >
            {/* Wrapper ogranicza szerokoœæ karty, wyrównuje j¹ w kolumnie i zapobiega rozci¹ganiu */}
            <div className="max-w-sm w-full mx-auto">
                <Card
                    className={clsx(
                        "group cursor-pointer border-2 transition-all duration-200 overflow-hidden",
                        "hover:shadow-xl",
                        "h-full flex flex-col min-h-56", // spójny baseline, karta mo¿e rosn¹æ w dó³
                        config.borderColor
                    )}
                    onClick={onSelect}
                >
                    {/* Header */}
                    <div className={clsx("p-6 border-b", config.bgLight)}>
                        <div className="flex items-start justify-between mb-4">
                            <div className={clsx("w-12 h-12 rounded-xl bg-gradient-to-br shadow-lg flex items-center justify-center", config.color)}>
                                <Icon className="w-6 h-6 text-white" />
                            </div>
                            <Badge variant="secondary" className={clsx(config.bgLight, config.textColor, "font-medium")}>
                                {device.type.toUpperCase()}
                            </Badge>
                        </div>
                        <h3 className="text-lg font-bold text-slate-900 mb-1 text-left">{device.displayName}</h3>
                        {device.localization && (
                            <div className="flex items-center gap-1 text-sm text-slate-500">
                                <MapPin className="w-3 h-3" />
                                {device.localization}
                            </div>
                        )}
                    </div>

                    {/* Content */}
                    <div className="p-6 flex-1">
                        {/* Switch Type */}
                        {device.type === 'switch' && (
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="text-2xl font-bold text-slate-900 text-left">{isOn ? 'ON' : 'OFF'}</p>
                                    <p className="text-sm text-slate-500 mt-1">Quick toggle available</p>
                                </div>
                                <div onClick={handleQuickToggle}>
                                    <Switch
                                        checked={isOn}
                                        className="scale-125"
                                    />
                                </div>
                            </div>
                        )}

                        {/* Slider Type */}
                        {device.type === 'slider' && (
                            <div className="space-y-3">
                                <div className="flex items-end justify-between">
                                    <p className="text-3xl font-bold text-slate-900">{device.state?.value || 0}</p>
                                    <p className="text-sm text-gray-400 text-left">
                                        {device.config?.min || 0} - {device.config?.max || 100}
                                    </p>
                                </div>
                                <div className="mt-3 h-2 bg-slate-100 rounded-full overflow-hidden">
                                    <motion.div
                                        className={clsx("h-full bg-gradient-to-r", config.color)}
                                        initial={{ width: 0 }}
                                        animate={{
                                            width: `${((device.state?.value || 0) / (device.config?.max || 100)) * 100}%`
                                        }}
                                        transition={{ duration: 0.5 }}
                                    />
                                </div>
                            </div>
                        )}

                        {/* Sensor Type */}
                        {device.type === 'sensor' && (
                            <div>
                                <div className="flex items-end gap-2 mb-2">
                                    <p className="text-3xl font-bold text-slate-900">{device.state?.value || 0}</p>
                                    {device.state?.unit && (
                                        <p className="text-xl text-slate-500 mb-1">{device.state.unit}</p>
                                    )}
                                </div>
                                <p className="text-sm text-gray-400 text-left">
                                    Range: {device.config?.min || 0} - {device.config?.max || 100}
                                </p>
                                <div className="mt-3 h-2 bg-slate-100 rounded-full overflow-hidden">
                                    <motion.div
                                        className={clsx("h-full bg-gradient-to-r", config.color)}
                                        initial={{ width: 0 }}
                                        animate={{
                                            width: `${((device.state?.value || 0) / (device.config?.max || 100)) * 100}%`
                                        }}
                                        transition={{ duration: 0.5 }}
                                    />
                                </div>
                            </div>
                        )}

                        {device.description && (
                            <p className="text-sm text-grey-400 mt-4 line-clamp-2 text-left">{device.description}</p>
                        )}
                    </div>
                </Card>
            </div>
        </motion.div>
    );
}