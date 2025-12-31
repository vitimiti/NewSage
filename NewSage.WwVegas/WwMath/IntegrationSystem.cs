// -----------------------------------------------------------------------
// <copyright file="IntegrationSystem.cs" company="NewSage">
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

namespace NewSage.WwVegas.WwMath;

public static class IntegrationSystem
{
    private static readonly System.Collections.Generic.List<float> Y0 = [];
    private static readonly System.Collections.Generic.List<float> Y1 = [];
    private static readonly System.Collections.Generic.List<float> WorkList0 = [];
    private static readonly System.Collections.Generic.List<float> WorkList1 = [];
    private static readonly System.Collections.Generic.List<float> WorkList2 = [];
    private static readonly System.Collections.Generic.List<float> WorkList3 = [];
    private static readonly System.Collections.Generic.List<float> WorkList4 = [];
    private static readonly System.Collections.Generic.List<float> WorkList5 = [];
    private static readonly System.Collections.Generic.List<float> WorkList6 = [];

    public static void EulerIntegrate(OrdinaryDifferentialEquationsSystem system, float dt)
    {
        ArgumentNullException.ThrowIfNull(system);

        Y0.Clear();
        Y0.AddRange(system.State);
        var count = Y0.Count;

        PrepareList(WorkList0, count);

        _ = system.ComputeDerivatives(0, null, WorkList0);

        PrepareList(Y1, count);
        for (var i = 0; i < Y0.Count; i++)
        {
            Y1[i] = Y0[i] + (WorkList0[i] * dt);
        }

        _ = system.SetState(Y1);
    }

    public static void MidpointIntegrate(OrdinaryDifferentialEquationsSystem system, float dt)
    {
        ArgumentNullException.ThrowIfNull(system);

        Y0.Clear();
        Y0.AddRange(system.State);
        var count = Y0.Count;

        PrepareList(WorkList0, count);
        PrepareList(WorkList1, count);

        System.Collections.Generic.List<float> dyDt = WorkList0;
        System.Collections.Generic.List<float> yMid = WorkList1;

        _ = system.ComputeDerivatives(0.0f, null, dyDt);

        for (var i = 0; i < count; i++)
        {
            yMid[i] = Y0[i] + (dt * dyDt[i] / 2.0f);
        }

        _ = system.ComputeDerivatives(dt / 2.0f, yMid, dyDt);

        PrepareList(Y1, count);
        for (var i = 0; i < count; i++)
        {
            Y1[i] = Y0[i] + (dt * dyDt[i]);
        }

        _ = system.SetState(Y1);
    }

    public static void RungeKuttaIntegrate(OrdinaryDifferentialEquationsSystem system, float dt)
    {
        ArgumentNullException.ThrowIfNull(system);

        var dt2 = dt / 2.0f;
        var dt6 = dt / 6.0f;

        Y0.Clear();
        Y0.AddRange(system.State);
        var count = Y0.Count;

        PrepareList(WorkList0, count);
        PrepareList(WorkList1, count);
        PrepareList(WorkList2, count);
        PrepareList(WorkList3, count);

        System.Collections.Generic.List<float> dyDt = WorkList0;
        System.Collections.Generic.List<float> dym = WorkList1;
        System.Collections.Generic.List<float> dyt = WorkList2;
        System.Collections.Generic.List<float> yt = WorkList3;

        _ = system.ComputeDerivatives(0.0f, null, dyDt);
        for (var i = 0; i < count; i++)
        {
            yt[i] = Y0[i] + (dt2 * dyDt[i]);
        }

        _ = system.ComputeDerivatives(dt2, yt, dyt);
        for (var i = 0; i < count; i++)
        {
            yt[i] = Y0[i] + (dt2 * dyt[i]);
        }

        _ = system.ComputeDerivatives(dt2, yt, dym);
        for (var i = 0; i < count; i++)
        {
            yt[i] = Y0[i] + (dt * dym[i]);
            dym[i] += dyt[i];
        }

        _ = system.ComputeDerivatives(dt, yt, dyt);

        PrepareList(Y1, count);
        for (var i = 0; i < count; i++)
        {
            Y1[i] = Y0[i] + (dt6 * (dyDt[i] + dyt[i] + (2.0f * dym[i])));
        }

        _ = system.SetState(Y1);
    }

    public static void RungeKutta5Integrate(OrdinaryDifferentialEquationsSystem system, float dt)
    {
        ArgumentNullException.ThrowIfNull(system);

        const float a2 = .2F;
        const float a3 = .3F;
        const float a4 = .6F;
        const float a5 = 1F;
        const float a6 = .875F;
        const float b21 = .2F;
        const float b31 = 3F / 40F;
        const float b32 = 9F / 40F;
        const float b41 = .3F;
        const float b42 = -.9F;
        const float b43 = 1.2F;
        const float b51 = -11F / 54F;
        const float b52 = 2.5F;
        const float b53 = -70F / 27F;
        const float b54 = 35F / 27F;
        const float b61 = 1_631F / 55_296F;
        const float b62 = 175F / 512F;
        const float b63 = 575F / 13_824F;
        const float b64 = 44_275F / 110_592F;
        const float b65 = 253F / 4_096F;
        const float c1 = 37F / 378F;
        const float c3 = 250F / 621F;
        const float c4 = 125F / 594F;
        const float c6 = 512F / 1_771F;

        Y0.Clear();
        Y0.AddRange(system.State);
        var length = Y0.Count;

        PrepareList(WorkList0, length);
        PrepareList(WorkList1, length);
        PrepareList(WorkList2, length);
        PrepareList(WorkList3, length);
        PrepareList(WorkList4, length);
        PrepareList(WorkList5, length);
        PrepareList(WorkList6, length);

        System.Collections.Generic.List<float> dyDt = WorkList0;
        System.Collections.Generic.List<float> ak2 = WorkList1;
        System.Collections.Generic.List<float> ak3 = WorkList2;
        System.Collections.Generic.List<float> ak4 = WorkList3;
        System.Collections.Generic.List<float> ak5 = WorkList4;
        System.Collections.Generic.List<float> ak6 = WorkList5;
        System.Collections.Generic.List<float> yTemp = WorkList6;

        _ = system.ComputeDerivatives(0.0f, null, dyDt);
        for (var i = 0; i < length; i++)
        {
            yTemp[i] = Y0[i] + (b21 * dt * dyDt[i]);
        }

        _ = system.ComputeDerivatives(a2 * dt, yTemp, ak2);
        for (var i = 0; i < length; i++)
        {
            yTemp[i] = Y0[i] + (dt * ((b31 * dyDt[i]) + (b32 * ak2[i])));
        }

        _ = system.ComputeDerivatives(a3 * dt, yTemp, ak3);
        for (var i = 0; i < length; i++)
        {
            yTemp[i] = Y0[i] + (dt * ((b41 * dyDt[i]) + (b42 * ak2[i]) + (b43 * ak3[i])));
        }

        _ = system.ComputeDerivatives(a4 * dt, yTemp, ak4);
        for (var i = 0; i < length; i++)
        {
            yTemp[i] = Y0[i] + (dt * ((b51 * dyDt[i]) + (b52 * ak2[i]) + (b53 * ak3[i]) + (b54 * ak4[i])));
        }

        _ = system.ComputeDerivatives(a5 * dt, yTemp, ak5);
        for (var i = 0; i < length; i++)
        {
            yTemp[i] =
                Y0[i] + (dt * ((b61 * dyDt[i]) + (b62 * ak2[i]) + (b63 * ak3[i]) + (b64 * ak4[i]) + (b65 * ak5[i])));
        }

        _ = system.ComputeDerivatives(a6 * dt, yTemp, ak6);

        PrepareList(Y1, length);
        for (var i = 0; i < length; i++)
        {
            Y1[i] = Y0[i] + (dt * ((c1 * dyDt[i]) + (c3 * ak3[i]) + (c4 * ak4[i]) + (c6 * ak6[i])));
        }

        _ = system.SetState(Y1);
    }

    private static void PrepareList(System.Collections.Generic.List<float> list, int size)
    {
        if (list.Count == size)
        {
            return;
        }

        list.Clear();
        for (var i = 0; i < size; i++)
        {
            list.Add(0f);
        }
    }
}
