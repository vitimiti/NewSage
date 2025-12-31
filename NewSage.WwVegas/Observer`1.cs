// -----------------------------------------------------------------------
// <copyright file="Observer`1.cs" company="NewSage">
// A transliteration and update of the CnC Generals (Zero Hour) engine and games with mod-first support.
// Copyright (C) 2025 NewSage Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>
// -----------------------------------------------------------------------

namespace NewSage.WwVegas;

public abstract class Observer<TEvent> : IDisposable
    where TEvent : EventArgs
{
    private readonly System.Collections.Generic.List<Notifier<TEvent>> _notifiers = [];
    private bool _disposed;

    public abstract void HandleNotification(object? sender, TEvent e);

    public void NotifyMe(Notifier<TEvent> notifier)
    {
        ArgumentNullException.ThrowIfNull(notifier);
        notifier.AddObserver(this);
    }

    public void StopObserving()
    {
        for (var i = _notifiers.Count - 1; i >= 0; i--)
        {
            _notifiers[i].RemoveObserver(this);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal void AddNotifier(Notifier<TEvent> notifier)
    {
        if (!_notifiers.Contains(notifier))
        {
            _notifiers.Add(notifier);
        }
    }

    internal void NotificationEnded(Notifier<TEvent> notifier) => _notifiers.Remove(notifier);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            StopObserving();
        }

        _disposed = true;
    }
}
