// -----------------------------------------------------------------------
// <copyright file="ProfileNode.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwDebug;

public sealed class ProfileNode(string name, ProfileNode? parent) : Node<ProfileNode>
{
    public string Name { get; } = name;

    public int TotalCalls { get; set; }

    public TimeSpan TotalTime { get; set; }

    public TimeSpan StartTime { get; set; }

    public int RecursionCounter { get; set; }

    public ProfileNode? Parent { get; } = parent;

    public List<ProfileNode> Children { get; } = new();

    public ProfileNode GetSubNode(string name)
    {
        foreach (ProfileNode child in Children)
        {
            if (child.Name == name)
            {
                return child;
            }
        }

        var newNode = new ProfileNode(name, this);
        Children.AddTail(newNode);
        return newNode;
    }

    public void Call()
    {
        TotalCalls++;
        if (RecursionCounter++ == 0)
        {
            StartTime = TimeSpan.FromTicks(Mpu.CpuClock);
        }
    }

    public bool Return()
    {
        if (--RecursionCounter != 0)
        {
            return RecursionCounter == 0;
        }

        var elapsed = Mpu.CpuClock - StartTime.Ticks;
        TotalTime += TimeSpan.FromSeconds((double)elapsed / Mpu.CpuRate);

        return RecursionCounter == 0;
    }

    public void Reset()
    {
        TotalCalls = 0;
        TotalTime = TimeSpan.Zero;
        RecursionCounter = 0;
        foreach (ProfileNode child in Children)
        {
            child.Reset();
        }
    }
}
