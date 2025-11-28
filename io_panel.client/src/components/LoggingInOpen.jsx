import Dialog from "@mui/material/Dialog";
import { useState } from "react";

function validateLogin(username, password) {
    // Funkcja waliduj¹ca dane logowania (testowa implementacja, bardzo prosta)
    return username === "admin" && password === "admin";
}
export default function LoggingInOpen({ open, onClose }) {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    function handleLogin() {
        const ok = validateLogin(username, password);
        // na razie tylko wywo³aj onClose przy poprawnym logowaniu
        if (ok) {
            onClose && onClose();
        } else {
            // pokazujemy b³¹d (na razie alert)
            alert("Nieprawidlowe dane");
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
                        <button className="px-4 py-2 bg-gray-200 rounded" onClick={handleLogin}>Log In</button>
                    </div>

                    <div className="flex justify-end">
                        <button className="px-4 py-2 bg-gray-200 rounded" onClick={() => onClose()}>Return</button>
                    </div>
                </div>
            </div>
        </Dialog>
    );
}