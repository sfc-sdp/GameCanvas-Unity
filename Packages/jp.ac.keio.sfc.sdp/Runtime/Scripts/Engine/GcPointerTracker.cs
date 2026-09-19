#nullable enable
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace GameCanvas.Engine
{
    // OSの接触番号をアプリ内のIDに変換し、イベントと状態を同時に作る。
    internal sealed class GcPointerTracker
    {
        struct Entry
        {
            public int Device, Contact, Id;
            public GcPointerType Kind;
            public bool Down, Held, Up, Cancelled, Present, Inside;
            public GcPoint Position, Start, Previous;
            public float2 Screen;
            public double Began, Ended;
        }
        readonly List<Entry> entries = new(16);
        readonly List<GcPointer> pointers = new(16);
        readonly List<GcPointerEvent> events = new(64);
        int nextId, primaryId, frame;
        double time;
        public GcReadOnlyList<GcPointer> Pointers => new(pointers);
        public GcReadOnlyList<GcPointerEvent> Events => new(events);
        public GcPointer Pointer { get; private set; }

        internal void BeginFrame(int frame, double time)
        {
            this.frame = frame; this.time = time;
            events.Clear(); pointers.Clear(); Pointer = default;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var e = entries[i];
                if (e.Up || e.Cancelled)
                {
                    if (primaryId == e.Id) primaryId = 0;
                    bool replaced = false;
                    for (int j = i + 1; j < entries.Count; j++)
                        if (entries[j].Device == e.Device && entries[j].Contact == e.Contact) { replaced = true; break; }
                    if (e.Kind == GcPointerType.Touch || e.Cancelled || !e.Present || replaced)
                    { entries.RemoveAt(i); continue; }
                }
                if (!e.Present && !e.Held) { entries.RemoveAt(i); continue; }
                e.Down = e.Up = e.Cancelled = false; e.Previous = e.Position;
                entries[i] = e;
            }
        }

        internal void Accept(in GcPointerRecord record, GcPoint position, bool inside)
        {
            if (record.CancelDevice)
            {
                for (int j = 0; j < entries.Count; j++)
                {
                    var cancelled = entries[j];
                    if (record.Device != -1 && cancelled.Device != record.Device) continue;
                    if (cancelled.Held)
                    {
                        cancelled.Held = false; cancelled.Cancelled = true;
                        cancelled.Ended = record.Time;
                        AddEvent(cancelled, GcPointerEventPhase.Cancelled, record.Time);
                    }
                    cancelled.Present = false;
                    entries[j] = cancelled;
                }
                return;
            }
            int index = -1;
            for (int j = 0; j < entries.Count; j++)
            {
                var candidate = entries[j];
                if (candidate.Device == record.Device && candidate.Contact == record.Contact &&
                    !candidate.Up && !candidate.Cancelled) { index = j; break; }
            }
            Entry e = index < 0 ? default : entries[index];
            if (record.Phase == GcPointerEventPhase.Begin)
            {
                // 不正な重複Beginも元の捕捉を残さず中断する。
                if (index >= 0 && e.Held)
                {
                    e.Held = false; e.Cancelled = true; e.Ended = record.Time;
                    entries[index] = e; AddEvent(e, GcPointerEventPhase.Cancelled, record.Time);
                    index = -1;
                }
                e = new Entry { Device = record.Device, Contact = record.Contact,
                    Id = checked(++nextId), Kind = record.Kind, Position = position,
                    Previous = position, Start = position, Began = record.Time, Down = true, Held = true };
                if (primaryId == 0) primaryId = e.Id;
            }
            else if (index < 0)
            {
                // 途中から届いたTouchの移動・解放を新しい押下にしない。
                if (record.Phase != GcPointerEventPhase.Hover) return;
                e = new Entry { Device = record.Device, Contact = record.Contact,
                    Id = checked(++nextId), Kind = record.Kind, Previous = position, Start = position };
            }
            e.Position = position; e.Screen = record.Screen; e.Present = record.Present; e.Inside = inside;
            if (record.Phase == GcPointerEventPhase.End || record.Phase == GcPointerEventPhase.Cancelled)
            {
                if (!e.Held) return;
                e.Held = false; e.Up = record.Phase == GcPointerEventPhase.End;
                e.Cancelled = record.Phase == GcPointerEventPhase.Cancelled; e.Ended = record.Time;
            }
            if (index < 0) entries.Add(e); else entries[index] = e;
            AddEvent(e, record.Phase, record.Time);
        }

        internal void EndFrame()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                var duration = e.Held ? time - e.Began : e.Up || e.Cancelled ? e.Ended - e.Began : 0;
                var p = new GcPointer(e.Id, e.Kind, e.Present, e.Down, e.Held, e.Up,
                    e.Cancelled, e.Inside, e.Position, e.Start, e.Position - e.Previous, Math.Max(0, duration));
                pointers.Add(p);
                if (e.Id == primaryId) Pointer = p;
            }
            // 残った指へ捕捉を引き継がない。非押下のマウス・ペンだけがホバーの代表になる。
            if (primaryId == 0)
                for (int i = 0; i < pointers.Count; i++)
                    if (pointers[i].Present && !pointers[i].Held && !pointers[i].Up && !pointers[i].Cancelled &&
                        pointers[i].Kind != GcPointerType.Touch) { Pointer = pointers[i]; break; }
        }

        void AddEvent(in Entry e, GcPointerEventPhase phase, double eventTime) =>
            events.Add(new GcPointerEvent(frame, (float)eventTime, e.Id, phase,
                new float2(e.Position.X, e.Position.Y), e.Screen, e.Kind));
    }
}
