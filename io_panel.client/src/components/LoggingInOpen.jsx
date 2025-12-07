import Dialog from "@mui/material/Dialog";
import Button from "@mui/material/Button";
import { useState } from "react";


export default function LoggingInOpen({ open, onClose, onLogin }) {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    async function handleLogin() {
        try {
            const response = await fetch('/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ Username: username, Password: password }),
            });

            if (response.ok) {
                const data = await response.json(); // Instead use JWT token in the future

                if (onLogin) {
                    onLogin();
                }
            } else if (response.status === 401) {
                alert("Invalid username or password.");
            } else {
                alert("Server error during login.");
            }
        } catch (error) {
            console.error('Login API error:', error);
            alert("Server connection error.");
        }
    }

    return (
        <Dialog open={open}>
            <div className="fixed inset-0 flex items-center justify-center z-50">
                <div className="bg-white rounded-lg shadow-lg w-96 p-6">
                    <h3 className="text-lg font-semibold mb-4">Please provide username and password!</h3>

                    <input
                        type="text"
                        placeholder="Username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        className="w-full mb-3 px-3 py-2 border border-gray-200 rounded"
                    />

                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full mb-4 px-3 py-2 border border-gray-200 rounded"
                    />
                    <div className="flex justify-end">
                        <Button className="px-4 py-2 bg-gray-200 rounded" onClick={handleLogin}>Log In</Button>
                    </div>

                    <div className="flex justify-end">
                        <Button className="px-4 py-2 bg-gray-200 rounded" onClick={() => onClose()}>Return</Button>
                    </div>
                </div>
            </div>
        </Dialog>
    );
}