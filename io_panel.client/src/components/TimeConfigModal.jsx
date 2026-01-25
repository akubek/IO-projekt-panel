import React, { useEffect, useMemo, useState } from "react";

function pad2(n) {
    return String(n).padStart(2, "0");
}

function toDateTimeLocalValue(date) {
    const d = date instanceof Date ? date : new Date(date);
    return `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}T${pad2(d.getHours())}:${pad2(d.getMinutes())}`;
}

function dateTimeLocalToUtcIso(dateTimeLocal) {
    // Interpret as LOCAL time, convert to UTC ISO
    const m = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})$/.exec(dateTimeLocal);
    if (!m) return null;

    const year = Number(m[1]);
    const month = Number(m[2]) - 1;
    const day = Number(m[3]);
    const hour = Number(m[4]);
    const minute = Number(m[5]);

    const local = new Date(year, month, day, hour, minute, 0, 0);
    if (Number.isNaN(local.getTime())) return null;

    return local.toISOString();
}

/* 
  TimeConfigModal
  This component provides an administrative interface to manually override the 
  server's system time (Virtual Time).
*/
export default function TimeConfigModal({ open, authToken, onClose, onSaved }) {
    const [saving, setSaving] = useState(false);
    const [virtualNowLocal, setVirtualNowLocal] = useState(toDateTimeLocalValue(new Date()));

    const isAdmin = !!authToken;

    const localTimeZone = useMemo(
        () => Intl.DateTimeFormat().resolvedOptions().timeZone || "Local",
        []
    );

    useEffect(() => {
        if (!open) return;

        void (async () => {
            try {
                const res = await fetch("/time");
                if (!res.ok) return;

                const data = await res.json();

                // Use server's computed local clock for the input.
                setVirtualNowLocal(toDateTimeLocalValue(data.nowLocal ?? data.nowUtc ?? new Date()));
            } catch {
                /* empty */
            }
        })();
    }, [open]);

    if (!open || !isAdmin) return null;

    async function handleSave() {
        if (!authToken) return;

        if (!virtualNowLocal || !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(virtualNowLocal)) {
            alert("Invalid date/time format.");
            return;
        }

        setSaving(true);
        try {
            const res = await fetch("/time", {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${authToken}`
                },
                body: JSON.stringify({ timeZoneId: localTimeZone, virtualNowLocal })
            });

            if (!res.ok) {
                const text = await res.text();
                alert(`Failed to save time config: ${res.status} ${text}`);
                return;
            }

            const saved = await res.json();
            onSaved && onSaved(saved);
            onClose && onClose();
        } catch (e) {
            alert(`Failed to save time config: ${e}`);
        } finally {
            setSaving(false);
        }
    }

    async function handleReset() {
        if (!authToken) return;

        const ok = window.confirm("Reset virtual time to system time?");
        if (!ok) return;

        setSaving(true);
        try {
            const res = await fetch("/time", {
                method: "DELETE",
                headers: {
                    Authorization: `Bearer ${authToken}`
                }
            });

            if (!res.ok) {
                const text = await res.text();
                alert(`Failed to reset time config: ${res.status} ${text}`);
                return;
            }

            const saved = await res.json();
            onSaved && onSaved(saved);
            onClose && onClose();
        } catch (e) {
            alert(`Failed to reset time config: ${e}`);
        } finally {
            setSaving(false);
        }
    }

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm px-4">
            <div className="w-full max-w-xl rounded-2xl border border-slate-200 bg-white shadow-2xl">
                <div className="flex items-start justify-between gap-4 border-b border-slate-100 px-6 py-5">
                    <div>
                        <div className="flex items-center gap-2">
                            <h3 className="text-lg font-semibold text-slate-900">Time & Date</h3>
                            <span className="inline-flex items-center rounded-full bg-slate-100 px-2 py-0.5 text-xs font-semibold text-slate-700 border border-slate-200">
                                {localTimeZone}
                            </span>
                        </div>
                    </div>

                    <button
                        className="rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                        onClick={onClose}
                        disabled={saving}
                    >
                        Close
                    </button>
                </div>

                <div className="px-6 py-5 space-y-4">
                    <label className="block text-sm font-semibold text-slate-700">
                        Virtual time (local)
                        <input
                            className="mt-2 w-full rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-slate-900 shadow-sm outline-none focus:border-blue-300 focus:ring-4 focus:ring-blue-100 disabled:opacity-60"
                            type="datetime-local"
                            value={virtualNowLocal}
                            onChange={(e) => setVirtualNowLocal(e.target.value)}
                            disabled={saving}
                        />
                    </label>
                </div>

                <div className="flex flex-col-reverse gap-3 border-t border-slate-100 px-6 py-5 sm:flex-row sm:items-center sm:justify-between">
                    <button
                        className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                        onClick={handleReset}
                        disabled={saving}
                    >
                        Reset to system time
                    </button>

                    <div className="flex gap-2 sm:justify-end">
                        <button
                            className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:opacity-60"
                            onClick={onClose}
                            disabled={saving}
                        >
                            Cancel
                        </button>
                        <button
                            className="rounded-xl bg-gradient-to-r from-blue-600 to-cyan-600 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-cyan-700 disabled:opacity-60"
                            onClick={handleSave}
                            disabled={saving}
                        >
                            {saving ? "Saving..." : "Save"}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}