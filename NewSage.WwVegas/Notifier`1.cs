// -----------------------------------------------------------------------
// <copyright file="Notifier`1.cs" company="NewSage">
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

public class Notifier<TEvent> : IDisposable
    where TEvent : EventArgs
{
    private readonly System.Collections.Generic.List<Observer<TEvent>> _observers = [];
    private bool _disposed;

    public event EventHandler<TEvent>? OnEvent;

    public bool HasObservers => _observers.Count > 0;

    public virtual void NotifyObservers(TEvent e) => OnEvent?.Invoke(this, e);

    public void AddObserver(Observer<TEvent> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        if (_observers.Contains(observer))
        {
            return;
        }

        _observers.Add(observer);
        OnEvent += observer.HandleNotification;
        observer.AddNotifier(this);
    }

    public void RemoveObserver(Observer<TEvent> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        if (!_observers.Remove(observer))
        {
            return;
        }

        OnEvent -= observer.HandleNotification;
        observer.NotificationEnded(this);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            for (var i = _observers.Count - 1; i >= 0; i--)
            {
                _observers[i].NotificationEnded(this);
            }

            _observers.Clear();
            OnEvent = null;
        }

        _disposed = true;
    }
}
