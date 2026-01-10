import React, { useEffect, useMemo, useState } from "react";
import clsx from "clsx";

function formatTime(value) {
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "";
    return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

function toNumberOr(value, fallback) {
    const n = typeof value === "number" ? value : Number(value);
    return Number.isFinite(n) ? n : fallback;
}

export default function DeviceHistoryChart({ deviceId, unit, className, height = 160, refreshToken = 0 }) {
    const [points, setPoints] = useState([]);
    const [isLoading, setIsLoading] = useState(false); // first load only
    const [isRefreshing, setIsRefreshing] = useState(false); // subsequent loads
    const [errorText, setErrorText] = useState(null);

    useEffect(() => {
        if (!deviceId) return;

        const controller = new AbortController();

        async function load() {
            const hasDataAlready = points.length > 0;

            if (hasDataAlready) {
                setIsRefreshing(true);
            } else {
                setIsLoading(true);
            }

            setErrorText(null);

            try {
                const to = new Date();
                const from = new Date(to.getTime() - 60 * 60 * 1000);

                const url =
                    `/device/${encodeURIComponent(deviceId)}/history` +
                    `?from=${encodeURIComponent(from.toISOString())}` +
                    `&to=${encodeURIComponent(to.toISOString())}` +
                    `&limit=240`;

                const res = await fetch(url, { signal: controller.signal });
                if (!res.ok) {
                    const text = await res.text();
                    throw new Error(`${res.status} ${text}`);
                }

                const data = await res.json();
                setPoints(Array.isArray(data) ? data : []);
            } catch (err) {
                if (err?.name === "AbortError") return;

                setErrorText(String(err?.message ?? err));

                // IMPORTANT: don't clear points on refresh -> avoids flashing/empty chart
                if (!points.length) {
                    setPoints([]);
                }
            } finally {
                setIsLoading(false);
                setIsRefreshing(false);
            }
        }

        void load();

        return () => controller.abort();
        // points intentionally included so we can detect "hasDataAlready"
    }, [deviceId, refreshToken, points.length]);

    const chart = useMemo(() => {
        if (!points || points.length < 2) return null;

        const w = 1200;
        const h = Math.max(320, toNumberOr(height, 160));

        const padLeft = 72;
        const padRight = 12;
        const padTop = 6;
        const padBottom = 22;

        const xs = points
            .map((p) => new Date(p.recordedAt).getTime())
            .filter((t) => Number.isFinite(t));

        const ys = points.map((p) => toNumberOr(p.value, 0));

        if (xs.length < 2) return null;

        const minX = Math.min(...xs);
        const maxX = Math.max(...xs);

        const rawMinY = Math.min(...ys);
        const rawMaxY = Math.max(...ys);
        const rawRange = rawMaxY - rawMinY;

        const pad = Math.max(1e-6, rawRange * 0.10);
        const minY = rawRange < 1e-6 ? rawMinY - 1 : rawMinY - pad;
        const maxY = rawRange < 1e-6 ? rawMaxY + 1 : rawMaxY + pad;

        const dx = Math.max(1, maxX - minX);
        const dy = Math.max(1e-9, maxY - minY);

        const plotW = w - padLeft - padRight;
        const plotH = h - padTop - padBottom;

        const toX = (t) => padLeft + ((t - minX) / dx) * plotW;
        const toY = (v) => padTop + (1 - (v - minY) / dy) * plotH;

        const d = points
            .map((p, i) => {
                const t = new Date(p.recordedAt).getTime();
                const x = toX(t);
                const y = toY(toNumberOr(p.value, 0));
                return `${i === 0 ? "M" : "L"} ${x.toFixed(2)} ${y.toFixed(2)}`;
            })
            .join(" ");

        const first = points[0];
        const last = points[points.length - 1];

        const yLabel = (v) => {
            const rounded = Math.abs(v) >= 1000 ? v.toFixed(0) : v.toFixed(2);
            return unit ? `${rounded} ${unit}` : rounded;
        };

        const gridYCount = 4;
        const gridXCount = 6;

        const gridYs = Array.from({ length: gridYCount + 1 }, (_, i) => padTop + (i / gridYCount) * plotH);
        const gridXs = Array.from({ length: gridXCount + 1 }, (_, i) => padLeft + (i / gridXCount) * plotW);

        return {
            w,
            h,
            d,
            minY,
            maxY,
            first,
            last,
            padLeft,
            padRight,
            padTop,
            padBottom,
            gridYs,
            gridXs,
            yLabel
        };
    }, [points, unit, height]);

    // Only show blocking loading UI when we truly have no chart yet.
    if (isLoading && (!points || points.length < 2)) {
        return <div className={clsx("text-sm text-slate-500", className)}>Loading history…</div>;
    }

    // If we have no chart and an error, show error.
    if (!chart && errorText) {
        return (
            <div className={clsx("text-sm text-rose-700 bg-rose-50 border border-rose-200 rounded-md p-3", className)}>
                Failed to load history: {errorText}
            </div>
        );
    }

    if (!chart) {
        return <div className={clsx("text-sm text-slate-500", className)}>No history to display.</div>;
    }

    return (
        <div className={clsx("w-full", className)}>
            <div className="flex items-baseline justify-between mb-2">
                <div className="text-xs text-slate-500">
                    {formatTime(chart.first.recordedAt)} → {formatTime(chart.last.recordedAt)}
                </div>
                <div className="text-xs text-slate-400 min-h-[1rem]">
                    {isRefreshing ? "Refreshing…" : ""}
                </div>
            </div>

            <div className="rounded-md border border-slate-200 bg-white p-2">
                <svg viewBox={`0 0 ${chart.w} ${chart.h}`} className="w-full" style={{ height: `${chart.h}px` }}>
                    {chart.gridYs.map((y, i) => (
                        <line key={`gy-${i}`} x1={chart.padLeft} x2={chart.w - chart.padRight} y1={y} y2={y} stroke="rgb(226 232 240)" strokeWidth="1" />
                    ))}
                    {chart.gridXs.map((x, i) => (
                        <line key={`gx-${i}`} x1={x} x2={x} y1={chart.padTop} y2={chart.h - chart.padBottom} stroke="rgb(226 232 240)" strokeWidth="1" />
                    ))}

                    <line x1={chart.padLeft} x2={chart.padLeft} y1={chart.padTop} y2={chart.h - chart.padBottom} stroke="rgb(148 163 184)" strokeWidth="1" />
                    <line x1={chart.padLeft} x2={chart.w - chart.padRight} y1={chart.h - chart.padBottom} y2={chart.h - chart.padBottom} stroke="rgb(148 163 184)" strokeWidth="1" />

                    <text x={chart.padLeft - 8} y={chart.padTop + 4} textAnchor="end" fontSize="11" fill="rgb(100 116 139)">
                        {chart.yLabel(chart.maxY)}
                    </text>
                    <text x={chart.padLeft - 8} y={chart.h - chart.padBottom} textAnchor="end" fontSize="11" fill="rgb(100 116 139)" dominantBaseline="ideographic">
                        {chart.yLabel(chart.minY)}
                    </text>

                    <text x={chart.padLeft} y={chart.h - 8} textAnchor="start" fontSize="11" fill="rgb(100 116 139)">
                        {formatTime(chart.first.recordedAt)}
                    </text>
                    <text x={chart.w - chart.padRight} y={chart.h - 8} textAnchor="end" fontSize="11" fill="rgb(100 116 139)">
                        {formatTime(chart.last.recordedAt)}
                    </text>

                    <path
                        d={chart.d}
                        fill="none"
                        stroke="rgb(59 130 246)"
                        strokeWidth="2.5"
                        opacity={isRefreshing ? 0.85 : 1}
                    />
                </svg>
            </div>
        </div>
    );
}