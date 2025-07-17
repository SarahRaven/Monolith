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


using Content.Shared.Starlight.ItemSwitch;
using Content.Shared.Interaction.Events;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Toggleable;
using Content.Shared.Verbs;
using Robust.Client.GameObjects;

namespace Content.Client._Starlight.ItemSwitch;

public sealed class ItemSwitchSystem : SharedItemSwitchSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemSwitchComponent, AfterAutoHandleStateEvent>(OnChanged);
    }

    private void OnChanged(Entity<ItemSwitchComponent> ent, ref AfterAutoHandleStateEvent args) => UpdateVisuals(ent, ent.Comp.State);

    protected override void UpdateVisuals(Entity<ItemSwitchComponent> ent, string key)
    {
        base.UpdateVisuals(ent, key);
        if (TryComp(ent, out SpriteComponent? sprite) && ent.Comp.States.TryGetValue(key, out var state))
            if (state.Sprite != null)
                sprite.LayerSetSprite(0, state.Sprite);
    }
}
