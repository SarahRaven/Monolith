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
// - Any use of the Software in accordance with the rights granted above must attribute the Starlight project as its source, hosted on the following GitHub repository: https://github.com/ss14Starlight/space-    station-14
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Linq;
using Content.Shared.Bed.Sleep;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Content.Shared.IdentityManagement;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared._Starlight.Combat.OnHit;

public abstract class SharedOnHitSystem : EntitySystem
{
    [Dependency] protected readonly INetManager _net = default!;
    [Dependency] protected readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly ReactiveSystem _reactiveSystem = default!;
    [Dependency] protected readonly SharedSolutionContainerSystem _solutionContainers = default!;
    [Dependency] protected readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] protected readonly SharedCuffableSystem _cuffs = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<InjectOnHitComponent, MeleeHitEvent>(OnInjectOnMeleeHit);
        SubscribeLocalEvent<CuffsOnHitComponent, MeleeHitEvent>(OnCuffsOnMeleeHit);

        base.Initialize();
    }

    private void OnCuffsOnMeleeHit(Entity<CuffsOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit
         || !args.HitEntities.Any())
            return;

        var ev = new InjectOnHitAttemptEvent();
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!TryComp<CuffableComponent>(target, out var cuffable) || cuffable.Container.Count != 0)
                continue;
            var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.Duration, new CuffsOnHitDoAfter(), ent, target)
            {
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
                BreakOnDamage = true,
                NeedHand = true,
                DistanceThreshold = 1f
            };

            if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
                continue;
            _color.RaiseEffect(Color.FromHex("#601653"), new List<EntityUid>(1) { target }, Filter.Pvs(target, entityManager: EntityManager));
        }
    }

    private void OnInjectOnMeleeHit(Entity<InjectOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit
            || !args.HitEntities.Any())
            return;

        var ev = new InjectOnHitAttemptEvent();
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
            return;

        foreach (var target in args.HitEntities)
        {
            if (_solutionContainers.TryGetInjectableSolution(target, out var targetSoln, out var targetSolution))
            {
                var solution = new Solution(ent.Comp.Reagents);

                foreach (var reagent in ent.Comp.Reagents)
                    if (ent.Comp.ReagentLimit != null && _solutionContainers.GetTotalPrototypeQuantity(target, reagent.Reagent.ToString()) >= FixedPoint2.New(ent.Comp.ReagentLimit.Value))
                        return;

                _reactiveSystem.DoEntityReaction(target, solution, ReactionMethod.Injection);
                _solutionContainers.TryAddSolution(targetSoln.Value, solution);
                _color.RaiseEffect(Color.FromHex("#0000FF"), new List<EntityUid>(1) { target }, Filter.Pvs(target, entityManager: EntityManager));
            }
            if (ent.Comp.Sound is not null && _net.IsServer)
                _audio.PlayPvs(ent.Comp.Sound, target);
        }
    }

    public override void Update(float frameTime)
    {
    }
}

