// SPDX-FileCopyrightTex: 2025 Rinary1
// SPDX-FileCopyrightTex: 2025 starlight
// SPDX-License-Identifier: MIT

// Modified MIT License, henceforth referred to as the Starlight License
//
// Copyright (c) 2024 Starlight
//
// Permission is hereby granted, free of charge,
// to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// - The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// - Any use of the Software in accordance with the rights granted above must attribute the Starlight project as its source, hosted on the following GitHub repository: https://github.com/ss14Starlight/space-station-14
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//using Content.Shared.Starlight.Antags.Abductor; // Mono: not used (port from Starlight)
//using Content.Shared.Starlight.Medical.Surgery; // Mono: not used (port from Starlight)
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._Starlight.Combat.OnHit;

namespace Content.Server._Starlight.Combat.OnHit;

public sealed partial class OnHitSystem : SharedOnHitSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CuffsOnHitComponent, CuffsOnHitDoAfter>(OnCuffsOnHitDoAfter);
        base.Initialize();
    }
    private void OnCuffsOnHitDoAfter(Entity<CuffsOnHitComponent> ent, ref CuffsOnHitDoAfter args)
    {
        if (!args.Args.Target.HasValue || args.Handled || args.Cancelled) return;

        var user = args.Args.User;
        var target = args.Args.Target.Value;

        if (!TryComp<CuffableComponent>(target, out var cuffable) || cuffable.Container.Count != 0)
            return;

        args.Handled = true;

        var handcuffs = SpawnNextToOrDrop(ent.Comp.HandcuffProtorype, args.User);

        if (!_cuffs.TryAddNewCuffs(target, user, handcuffs, cuffable))
            QueueDel(handcuffs);
    }
}
