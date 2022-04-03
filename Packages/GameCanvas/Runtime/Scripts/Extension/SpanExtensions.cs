/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace GameCanvas
{
    public static class SpanExtensions
    {
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in NativeArray<T> self) where T : unmanaged
        {
            if (!self.IsCreated || self.Length == 0)
            {
                return ReadOnlySpan<T>.Empty;
            }
            unsafe
            {
                return new ReadOnlySpan<T>(self.GetUnsafePtr(), self.Length);
            }
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in NativeList<T> self) where T : unmanaged
        {
            if (!self.IsCreated || self.Length == 0)
            {
                return ReadOnlySpan<T>.Empty;
            }
            unsafe
            {
                return new ReadOnlySpan<T>(self.GetUnsafePtr(), self.Length);
            }
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> self)
        {
            if (self is null || self.Count == 0)
            {
                return ReadOnlySpan<T>.Empty;
            }
            return new ReadOnlySpan<T>(Unsafe.As<Tuple<T[]>>(self).Item1, 0, self.Count);
        }

        public static ReadOnlySpan<T> AsReadOnlySpan<TBase, T>(this List<TBase> self) where T : TBase where TBase : class
        {
            if (self is null || self.Count == 0)
            {
                return ReadOnlySpan<T>.Empty;
            }
            return new ReadOnlySpan<T>(Unsafe.As<Tuple<T[]>>(self).Item1, 0, self.Count);
        }

        public static Span<T> AsSpan<T>(this in NativeArray<T> self) where T : unmanaged
        {
            if (!self.IsCreated || self.Length == 0)
            {
                return Span<T>.Empty;
            }
            unsafe
            {
                return new Span<T>(self.GetUnsafePtr(), self.Length);
            }
        }

        public static Span<T> AsSpan<T>(this in NativeList<T> self) where T : unmanaged
        {
            if (!self.IsCreated || self.Length == 0)
            {
                return Span<T>.Empty;
            }
            unsafe
            {
                return new Span<T>(self.GetUnsafePtr(), self.Length);
            }
        }

        public static Span<T> AsSpan<T>(this List<T> self)
        {
            if (self is null || self.Count == 0)
            {
                return Span<T>.Empty;
            }
            return new Span<T>(Unsafe.As<Tuple<T[]>>(self).Item1, 0, self.Count);
        }
    }
}
