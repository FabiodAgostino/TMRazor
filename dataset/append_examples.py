import json
import os

examples = [
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Приготовьте 10 зелий с помощью Алхимии, дождитесь завершения каждого действия и сообщите об общем количестве успехов."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Gumps.SendAction, Journal.Search, Misc.Pause. Logic: Loop 10 times, trigger craft, check journal for success message.</think>\nimport Gumps, Journal, Misc\n\nsuccess_count = 0\nfor i in range(10):\n    Journal.Clear()\n    # Assuming alchemy gump is open and button 2 crafts the potion\n    Gumps.SendAction(Gumps.CurrentGump(), 2)\n    Misc.Pause(2000)\n    if Journal.Search('You create'):\n        success_count += 1\n\nMisc.SendMessage('Приготовление завершено. Успехов: {0}'.format(success_count), 66)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "將物品從箱子 (0x40001234) 一個接一個地移動到我的背包中，每個物品之間稍作延遲，直到箱子變空。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: Items.FindBySerial, Items.Move, Player.Backpack. Logic: Get container, iterate over Contains list, move each item with pause.</think>\nimport Items, Player, Misc\n\nchest_serial = 0x40001234\nchest = Items.FindBySerial(chest_serial)\n\nif chest:\n    Items.UseItem(chest)\n    Misc.Pause(1000)\n    for item in chest.Contains:\n        Items.Move(item, Player.Backpack, 0)\n        Misc.Pause(600)\n    Misc.SendMessage('箱子已清空。', 66)\nelse:\n    Misc.SendMessage('未找到箱子。', 33)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Use Hiding and Stealth to sneak through an area, checking at each step whether any enemy is nearby."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Player.UseSkill, Mobiles.ApplyFilter, Misc.Pause. Logic: Hide, then move in steps using Stealth skill while checking for hostiles.</think>\nimport Player, Mobiles, Misc\n\nif not Player.Buffs.Contains('Hiding'):\n    Player.UseSkill('Hiding')\n    Misc.Pause(3000)\n\nfil = Mobiles.Filter()\nfil.RangeMax = 8\nfil.Notoriety = [3, 4, 5, 6]\n\nfor i in range(10):\n    Player.UseSkill('Stealth')\n    Misc.Pause(2000)\n    enemies = Mobiles.ApplyFilter(fil)\n    if enemies:\n        Misc.SendMessage('Enemy detected! Stopping stealth move.', 33)\n        break\n    Misc.SendMessage('Step {0} clear...'.format(i+1), 55)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Lancia Wither su tutti i nemici nel raggio d'azione e attendi che ogni lancio vada a segno prima di lanciare di nuovo."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Spells.CastNecro, Journal.Search, Misc.Pause. Logic: Cast Wither, wait for journal confirmation of completion.</think>\nimport Spells, Journal, Misc\n\nwhile True:\n    Journal.Clear()\n    Spells.CastNecro('Wither')\n    # Necromancy spells often take time to land\n    Misc.Pause(2000)\n    if Journal.Search('The spell fizzles'):\n        continue\n    Misc.Pause(1000) # Cooldown"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Lee los nombres de los azulejos estáticos (static tiles) en mi posición actual e imprime todos los objetos presentes en el suelo."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Statics.GetStaticsTileInfo, Player.Position, Player.Map. Logic: Access map data for current coordinates.</think>\nimport Statics, Player, Misc\n\nx, y = Player.Position.X, Player.Position.Y\nm = Player.Map\n\ntiles = Statics.GetStaticsTileInfo(x, y, m)\nif tiles:\n    for t in tiles:\n        Misc.SendMessage('Static object: {0} (StaticID: {1})'.format(t.Name, t.StaticID), 66)\nelse:\n    Misc.SendMessage('No hay objetos estáticos aquí.', 33)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Öffnet das Kontextmenü eines Stallmeisters, liest die verfügbaren Optionen und wählt diejenige aus, um mein Haustier zurückzuholen."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Mobiles.ApplyFilter, ContextMenu.Open, ContextMenu.Click. Logic: Find stablemaster, open menu, select 'Retrieve'.</think>\nimport Mobiles, ContextMenu, Misc\n\nfil = Mobiles.Filter()\nfil.IsHuman = True\nfil.RangeMax = 3\nnearby = Mobiles.ApplyFilter(fil)\n\nfor mob in nearby:\n    if 'Stable' in mob.Name or 'Master' in mob.Name:\n        ContextMenu.Open(mob.Serial)\n        Misc.Pause(1000)\n        ContextMenu.Click(mob.Serial, 'Retrieve')\n        Misc.SendMessage('Haustier zurückgefordert von ' + mob.Name, 66)\n        break"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Контролирует здоровье всех членов группы и автоматически применяет Greater Heal к тому, у кого меньше всего HP."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Party.Members, Spells.Cast, Target.WaitForTarget, Target.TargetExecute. Logic: Find member with lowest HP, cast heal on them.</think>\nimport Party, Spells, Target, Misc\n\nweakest = None\nmin_hp = 1000\n\nfor m in Party.Members:\n    if m.Hits < min_hp and m.Hits > 0:\n        min_hp = m.Hits\n        weakest = m\n\nif weakest and min_hp < 60:\n    Misc.SendMessage('Лечу {0} ({1} HP)'.format(weakest.Name, min_hp), 55)\n    Spells.Cast('Greater Heal')\n    if Target.WaitForTarget(2000):\n        Target.TargetExecute(weakest.Serial)\nelse:\n    Misc.SendMessage('Все члены группы в порядке.', 66)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "命令我的寵物攻擊最近的敵人，用繃帶保持它的健康，如果它死了就對它施放復活術。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: Mobiles.ApplyFilter, Misc.PetAggregation, Items.UseItemByID, Spells.Cast. Logic: Check pet health, use bandage if low, cast res if ghost, command attack.</think>\nimport Mobiles, Items, Spells, Target, Misc\n\n# Assuming pet serial is known or using last target\npet = Mobiles.FindBySerial(0x00012345)\nif pet:\n    if pet.IsGhost:\n        Spells.Cast('Resurrection')\n        if Target.WaitForTarget(2000): Target.TargetExecute(pet)\n    elif pet.Hits < (pet.HitsMax * 0.5):\n        Items.UseItemByID(0x0E21, -1)\n        if Target.WaitForTarget(2000): Target.TargetExecute(pet)\n    \n    enemies = Mobiles.ApplyFilter(Mobiles.Filter())\n    if enemies:\n        Misc.PetAggregation(pet.Serial, enemies[0].Serial)\n        Misc.SendMessage('寵物正在攻擊：' + enemies[0].Name, 33)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Automate a full crafting resupply cycle: recall to a vendor, buy what is needed, then recall back."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Magery.Recall, BuyAgent, Items.FindByID. Logic: Recall to shop, start buy agent, wait, then recall back to base.</think>\nimport Magery, BuyAgent, Misc\n\n# Shop Rune Serial\nMagery.Recall(0x40008899)\nMisc.Pause(5000)\n\nBuyAgent.Enable()\nMisc.SendMessage('Buying supplies...', 55)\nMisc.Pause(3000)\n\n# Home Rune Serial\nMagery.Recall(0x40009900)\nMisc.SendMessage('Resupply complete.', 66)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Esegue la pulizia di una stanza del dungeon: trova tutti i nemici, attacca ciascuno con una rotazione magica."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Mobiles.ApplyFilter, Spells.Cast, Target.WaitForTarget, Target.TargetExecute. Logic: Loop through nearby hostiles and cast offensive spells.</think>\nimport Mobiles, Spells, Target, Misc\n\nfil = Mobiles.Filter()\nfil.RangeMax = 10\nfil.Notoriety = [3, 4, 5, 6]\n\nenemies = Mobiles.ApplyFilter(fil)\nfor en in enemies:\n    Misc.SendMessage('Attaccando ' + en.Name, 33)\n    Spells.Cast('Energy Bolt')\n    if Target.WaitForTarget(3000): Target.TargetExecute(en)\n    Misc.Pause(2000)\n    if en.Hits <= 0: continue"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Ejecuta un bucle de exploración sigilosa: activa Ocultarse, muévete por un área, usa Detectar oculto."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Player.UseSkill, Misc.Pause. Logic: Cycle through stealth skills for scouting.</think>\nimport Player, Misc\n\nwhile True:\n    if not Player.Buffs.Contains('Hiding'):\n        Player.UseSkill('Hiding')\n        Misc.Pause(3000)\n    \n    Player.UseSkill('Stealth')\n    Misc.Pause(2000)\n    \n    Player.UseSkill('Detecting Hidden')\n    Target.Self()\n    Misc.Pause(2000)\n    Misc.SendMessage('Escaneo de área completado.', 66)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Verwalten Sie eine komplette Bergbausitzung: Erz abbauen, es an einer nahe gelegenen Schmiede schmelzen."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Items.FindByID, Items.UseItemByID, Target.WaitForTarget, Target.TargetExecute. Logic: Use shovel on ground, then use ore on forge.</think>\nimport Items, Target, Misc\n\n# Mining\nItems.UseItemByID(0x0F39, -1) # Shovel\nif Target.WaitForTarget(2000): Target.TargetExecuteRelative(Player.Serial, 1)\nMisc.Pause(3000)\n\n# Smelting\nore = Items.FindByID(0x19B9, -1, Player.Backpack.Serial)\nforge = Items.FindByID(0x0FB1, -1, -1) # Forge nearby\nif ore and forge:\n    Items.Move(ore, forge.Serial, 0)\n    Misc.SendMessage('Erz geschmolzen.', 66)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Действует как умный целитель: сначала лечит яд, использует бинты при умеренных ранах."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Player.Poisoned, Spells.Cast, Items.UseItemByID, Target.Self. Logic: Priority: Cure > Bandage.</think>\nimport Player, Spells, Items, Target, Misc\n\nif Player.Poisoned:\n    Misc.SendMessage('Яд обнаружен! Лечу...', 33)\n    Spells.Cast('Cure')\n    if Target.WaitForTarget(2000): Target.Self()\nelif Player.Hits < (Player.HitsMax - 20):\n    Items.UseItemByID(0x0E21, -1) # Bandage\n    if Target.WaitForTarget(2000): Target.Self()\n    Misc.SendMessage('Применяю бинт.', 55)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "自動化銘文訓練：施放法術、冥想、刻寫捲軸。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: Spells.Cast, Player.UseSkill, Gumps.SendAction. Logic: Cycle between mana-consuming spell, meditation, and crafting scrolls.</think>\nimport Spells, Player, Gumps, Misc\n\nwhile Player.Mana > 20:\n    # Crafting loop\n    Gumps.SendAction(0x12345678, 2) # Inscription Gump\n    Misc.Pause(3000)\n\nif Player.Mana < 10:\n    Player.UseSkill('Meditation')\n    while Player.Mana < Player.ManaMax:\n        Misc.Pause(1000)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "PvP escape macro: drink invisibility potion, stealth, pathfind to a safe spot."},
            {"role": "assistant", "content": "<think>User speaks English. APIs: Items.UseItemByID, Player.UseSkill, Player.Pathfind. Logic: Instant invis, stealth activation, and automated movement to exit.</think>\nimport Items, Player, Misc\n\n# Invisibility Potion 0x0F0E color 0x0000?\n# Better to find by name or specific ID\nItems.UseItemByID(0x0F0E, -1)\nMisc.Pause(500)\nPlayer.UseSkill('Stealth')\nMisc.Pause(500)\nPlayer.Pathfind(1234, 567, 0) # Safe coordinates\nMisc.SendMessage('Escaping...', 33)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Rituale di Tessitura Magica (Spellweaving): cerchio arcano, Arcane Empowerment, Wildfire."},
            {"role": "assistant", "content": "<think>User speaks Italian. APIs: Spells.CastSpellweaving. Logic: Chain spellweaving buffs and offensive spells.</think>\nimport Spells, Misc\n\nSpells.CastSpellweaving('Arcane Circle')\nMisc.Pause(3000)\nSpells.CastSpellweaving('Arcane Empowerment')\nMisc.Pause(2000)\nSpells.CastSpellweaving('Wildfire')\nMisc.SendMessage('Rituale completato!', 66)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Rotación de combate de Nigromancia: Corpse Skin, Poison Strike, Strangle."},
            {"role": "assistant", "content": "<think>User speaks Spanish. APIs: Spells.CastNecro, Target.WaitForTarget, Target.LastTarget. Logic: Sequence of offensive necromancy spells on current target.</think>\nimport Spells, Target, Misc\n\ntarget = Target.GetLast()\nif target:\n    Spells.CastNecro('Corpse Skin')\n    if Target.WaitForTarget(2000): Target.TargetExecute(target)\n    Misc.Pause(1500)\n    Spells.CastNecro('Poison Strike')\n    if Target.WaitForTarget(2000): Target.TargetExecute(target)\n    Misc.Pause(1500)\n    Spells.CastNecro('Strangle')\n    if Target.WaitForTarget(2000): Target.TargetExecute(target)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Dynamischer Beutefilter: Öffnen Sie jede Leiche, behalten Sie nur Gegenstände mit hohem Eigenschaftswert."},
            {"role": "assistant", "content": "<think>User speaks German. APIs: Items.Filter, Items.ApplyFilter, item.Properties. Logic: Find corpses, open, scan properties for 'Damage Increase' etc.</think>\nimport Items, Player, Misc\n\nfil = Items.Filter()\nfil.Graphics = [0x2006] # Corpse\ncorpses = Items.ApplyFilter(fil)\n\nfor c in corpses:\n    Items.UseItem(c)\n    Misc.Pause(600)\n    for item in c.Contains:\n        for prop in item.Properties:\n            if 'Damage Increase' in str(prop):\n                Items.Move(item, Player.Backpack, 0)\n                Misc.Pause(600)"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "Сеанс шитья: разрежьте ткань на рулоны, сделайте предметы."},
            {"role": "assistant", "content": "<think>User speaks Russian. APIs: Items.FindByID, Items.UseItemByID, Target.WaitForTarget, Target.TargetExecute, Gumps.SendAction. Logic: Cut cloth, then use sewing kit.</think>\nimport Items, Target, Gumps, Misc\n\ncloth = Items.FindByID(0x1766, -1, Player.Backpack.Serial)\nscissors = Items.FindByID(0x0F9F, -1, Player.Backpack.Serial)\n\nif cloth and scissors:\n    Items.UseItem(scissors)\n    if Target.WaitForTarget(2000): Target.TargetExecute(cloth)\n    Misc.Pause(1000)\n\n# Start crafting with sewing kit\nkit = Items.FindByID(0x0F9D, -1, Player.Backpack.Serial)\nif kit:\n    Items.UseItem(kit)\n    Misc.Pause(1000)\n    Gumps.SendAction(Gumps.CurrentGump(), 10) # Craft specific item"}
        ]
    },
    {
        "messages": [
            {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
            {"role": "user", "content": "資源交易：從一家商販低價購買，然後賣給另一家。"},
            {"role": "assistant", "content": "<think>User speaks Chinese. APIs: BuyAgent, SellAgent, Magery.Recall. Logic: Travel between two shops using agents to automate profit.</think>\nimport BuyAgent, SellAgent, Magery, Misc\n\n# Shop 1: Buy\nMagery.Recall(0x40001111)\nMisc.Pause(5000)\nBuyAgent.Enable()\nMisc.Pause(2000)\n\n# Shop 2: Sell\nMagery.Recall(0x40002222)\nMisc.Pause(5000)\nSellAgent.Enable()\nMisc.SendMessage('交易完成。', 66)"}
        ]
    }
]

file_path = 'dataset.jsonl'
with open(file_path, 'a', encoding='utf-8') as f:
    for ex in examples:
        f.write(json.dumps(ex, ensure_ascii=False) + '\n')

print(f"Successfully appended {len(examples)} examples.")
