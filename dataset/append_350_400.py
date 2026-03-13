import json

system_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

data = [
    # 350 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "What does Player.LeaveParty do in Razor Enhanced?"}, {"role": "assistant", "content": "Player.LeaveParty is a Method of Razor Enhanced.\nDescription: Generates the asynchronous command for voluntary abandonment without going through the Gump.\nReturn: Void\nParameters: force\nExample:\n```python\nPlayer.LeaveParty(force)\n```"}]},
    # 351 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Puoi spiegarmi a cosa serve Player.MapSay?"}, {"role": "assistant", "content": "Player.MapSay è un Metodo di Razor Enhanced.\nDescrizione: Interagisce testualmente in particolari broadcast di mappa.\nRitorno: Void\nParametri: msg\nEsempio:\n```python\nPlayer.MapSay(msg)\n```"}]},
    # 352 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Cómo funciona Player.OpenPaperDoll en Razor Enhanced?"}, {"role": "assistant", "content": "Player.OpenPaperDoll es un Método de Razor Enhanced.\nDescripción: Señala la intención visual de mostrar el modelo de avatar 2D, provocando latencia de actualización.\nRetorno: Void\nParámetros: N/A\nEjemplo:\n```python\nPlayer.OpenPaperDoll()\n```"}]},
    # 353 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Erkläre mir, was Player.PartyAccept bewirkt."}, {"role": "assistant", "content": "Player.PartyAccept ist eine Methode von Razor Enhanced.\nBeschreibung: Antwortet automatisch auf UI-Popups und fängt die Einladung asynchron ab.\nRückgabe: Boolean\nParameter: from_serial, force\nBeispiel:\n```python\nresult = Player.PartyAccept(from_serial, force)\n```"}]},
    # 354 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "请解释 Razor Enhanced 中 Player.PartyCanLoot 的作用。"}, {"role": "assistant", "content": "Player.PartyCanLoot 是 Razor Enhanced 的一个方法 (Method)。\n描述： 以编程方式更改团队成员之间的防盗保护标志。\n返回： Void\n参数： CanLoot\n示例:\n```python\nPlayer.PartyCanLoot(CanLoot)\n```"}]},
    # 355 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Что делает Player.PartyInvite в Razor Enhanced?"}, {"role": "assistant", "content": "Player.PartyInvite — это Метод в Razor Enhanced.\nОписание: Запускает ожидание ввода пользователя (прицел) для логического динамического добавления.\nВозврат: Void\nПараметры: N/A\nПример:\n```python\nPlayer.PartyInvite()\n```"}]},
    # 356 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Could you describe the function of Player.PathFindTo?"}, {"role": "assistant", "content": "Player.PathFindTo is a Method of Razor Enhanced.\nDescription: Delegates the heuristic calculation of the path to the client's middleware (if supported).\nReturn: Void\nParameters: x, y, z\nExample:\n```python\nPlayer.PathFindTo(x, y, z)\n```"}]},
    # 357 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Che funzione ha Player.QuestButton in Razor?"}, {"role": "assistant", "content": "Player.QuestButton è un Metodo di Razor Enhanced.\nDescrizione: Accede ai log asincroni della storyline aggirando la grafica paperdoll.\nRitorno: Void\nParametri: N/A\nEsempio:\n```python\nPlayer.QuestButton()\n```"}]},
    # 358 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Para qué sirve Player.Run en los scripts?"}, {"role": "assistant", "content": "Player.Run es un Método de Razor Enhanced.\nDescripción: Traduce el comando vectorial en pasos forzados calculando dinámicamente las coordenadas con desplazamiento.\nRetorno: Boolean\nParámetros: direction\nEjemplo:\n```python\nresult = Player.Run(direction)\n```"}]},
    # 359 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Was ist der Zweck von Player.SetSkillStatus?"}, {"role": "assistant", "content": "Player.SetSkillStatus ist eine Methode von Razor Enhanced.\nBeschreibung: Manipuliert die Benutzeroberfläche, um Verschlechterungen zu verhindern oder das Wachstum bestimmter Fähigkeiten zu sperren.\nRückgabe: Void\nParameter: skillname, status\nBeispiel:\n```python\nPlayer.SetSkillStatus(skillname, status)\n```"}]},
    # 360 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Player.SetStatStatus 有什么用？"}, {"role": "assistant", "content": "Player.SetStatStatus 是 Razor Enhanced 的一个方法 (Method)。\n描述： 逻辑上管理 STR/DEX/INT 基础点数的有限总分配。\n返回： Void\n参数： statname, status\n示例:\n```python\nPlayer.SetStatStatus(statname, status)\n```"}]},
    # 361 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Для чего используется функция Player.SetStaticMount?"}, {"role": "assistant", "content": "Player.SetStaticMount — это Метод в Razor Enhanced.\nОписание: Принудительно вставляет или исправляет идентификатор в фильтре ездовых животных Razor.\nВозврат: Void\nПараметры: serial\nПример:\n```python\nPlayer.SetStaticMount(serial)\n```"}]},
    # 362 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "What is Player.SetWarMode used for?"}, {"role": "assistant", "content": "Player.SetWarMode is a Method of Razor Enhanced.\nDescription: Reactively sets the framework to allow target aggression.\nReturn: Void\nParameters: warflag\nExample:\n```python\nPlayer.SetWarMode(warflag)\n```"}]},
    # 363 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Mi descrivi Player.SpellIsEnabled?"}, {"role": "assistant", "content": "Player.SpellIsEnabled è un Metodo di Razor Enhanced.\nDescrizione: Controlla validazioni di debuff per l'accertamento di spell continui (es. focus).\nRitorno: Boolean\nParametri: spellname\nEsempio:\n```python\nresult = Player.SpellIsEnabled(spellname)\n```"}]},
    # 364 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Puedes explicarme Player.SumAttribute?"}, {"role": "assistant", "content": "Player.SumAttribute es un Método de Razor Enhanced.\nDescripción: Operación costosa: iteración completa sobre nodos hijos del inventario para calcular el bono global (LRC, LMC).\nRetorno: Single\nParámetros: attributename\nEjemplo:\n```python\nresult = Player.SumAttribute(attributename)\n```"}]},
    # 365 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Welche Funktion hat Player.ToggleAlwaysRun in Razor Enhanced?"}, {"role": "assistant", "content": "Player.ToggleAlwaysRun ist eine Methode von Razor Enhanced.\nBeschreibung: Schaltet die logische Laufeinstellung um und umgeht dabei die physischen OSI-Tastatureingaben.\nRückgabe: Void\nParameter: N/A\nBeispiel:\n```python\nPlayer.ToggleAlwaysRun()\n```"}]},
    # 366 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Razor Enhanced 里的 Player.TrackingArrow 是做什么的？"}, {"role": "assistant", "content": "Player.TrackingArrow 是 Razor Enhanced 的一个方法 (Method)。\n描述： 在屏幕上叠加渲染一个客户端箭头，模拟游侠追踪探测。\n返回： Void\n参数： x, y, display, target\n示例:\n```python\nPlayer.TrackingArrow(x, y, display, target)\n```"}]},
    # 367 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Объясни, как работает Player.UnEquipItemByLayer."}, {"role": "assistant", "content": "Player.UnEquipItemByLayer — это Метод в Razor Enhanced.\nОписание: Гарантированное снятие экипировки с ожиданием задержки для предотвращения застревания предметов в неопределенности.\nВозврат: Void\nПараметры: layer, wait\nПример:\n```python\nPlayer.UnEquipItemByLayer(layer, wait)\n```"}]},
    # 368 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Explain the Player.UnEquipUO3D method."}, {"role": "assistant", "content": "Player.UnEquipUO3D is a Method of Razor Enhanced.\nDescription: Optimized unequip modulation for the asynchronous 3D client.\nReturn: Void\nParameters: _layers\nExample:\n```python\nPlayer.UnEquipUO3D(_layers)\n```"}]},
    # 369 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Cosa fa l'API Player.UpdateKarma?"}, {"role": "assistant", "content": "Player.UpdateKarma è un Metodo di Razor Enhanced.\nDescrizione: Forza una query latente di rete per ricalibrare fama nominale.\nRitorno: Boolean\nParametri: N/A\nEsempio:\n```python\nresult = Player.UpdateKarma()\n```"}]},
    # 370 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Qué hace la función Player.UseSkill?"}, {"role": "assistant", "content": "Player.UseSkill es un Método de Razor Enhanced.\nDescripción: Método central para activar la habilidad uniéndola inmediatamente al objetivo automático predeterminado.\nRetorno: Void\nParámetros: skillname, target, wait\nEjemplo:\n```python\nPlayer.UseSkill(skillname, target, wait)\n```"}]},
    # 371 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Was macht Player.UseSkillOnly?"}, {"role": "assistant", "content": "Player.UseSkillOnly ist eine Methode von Razor Enhanced.\nBeschreibung: Aktiviert passiv Meditation oder Tracking, indem dem System ein erzwungenes Ziel entzogen wird.\nRückgabe: Void\nParameter: skillname, wait\nBeispiel:\n```python\nPlayer.UseSkillOnly(skillname, wait)\n```"}]},
    # 372 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Player.Walk 的功能是什么？"}, {"role": "assistant", "content": "Player.Walk 是 Razor Enhanced 的一个方法 (Method)。\n描述： 生成平滑插值的基础移动，忽略狂暴状态。\n返回： Boolean\n参数： direction\n示例:\n```python\nresult = Player.Walk(direction)\n```"}]},
    # 373 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Что делает функция Player.WeaponClearSA?"}, {"role": "assistant", "content": "Player.WeaponClearSA — это Метод в Razor Enhanced.\nОписание: Удаляет состояние ожидания для макросов оружия, предотвращая неожиданные атаки.\nВозврат: Void\nПараметры: N/A\nПример:\n```python\nPlayer.WeaponClearSA()\n```"}]},
    # 374 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "How does Player.WeaponDisarmSA work?"}, {"role": "assistant", "content": "Player.WeaponDisarmSA is a Method of Razor Enhanced.\nDescription: Sets the switch to disarm the enemy.\nReturn: Void\nParameters: N/A\nExample:\n```python\nPlayer.WeaponDisarmSA()\n```"}]},
    # 375 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Qual è il compito di Player.WeaponPrimarySA?"}, {"role": "assistant", "content": "Player.WeaponPrimarySA è un Metodo di Razor Enhanced.\nDescrizione: Assicura l'uso del primo modificatore speciale legato all'arma impugnata (es. Paralyzing Blow).\nRitorno: Void\nParametri: N/A\nEsempio:\n```python\nPlayer.WeaponPrimarySA()\n```"}]},
    # 376 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Para qué sirve Player.WeaponSecondarySA?"}, {"role": "assistant", "content": "Player.WeaponSecondarySA es un Método de Razor Enhanced.\nDescripción: Modula el segundo movimiento disponible al soltar el golpe (ej. Crushing Blow).\nRetorno: Void\nParámetros: N/A\nEjemplo:\n```python\nPlayer.WeaponSecondarySA()\n```"}]},
    # 377 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Erkläre mir Player.WeaponStunSA."}, {"role": "assistant", "content": "Player.WeaponStunSA ist eine Methode von Razor Enhanced.\nBeschreibung: Aktiviert unbewaffnete asynchrone Betäubung in klassischen PvP-Interaktionen.\nRückgabe: Void\nParameter: N/A\nBeispiel:\n```python\nPlayer.WeaponStunSA()\n```"}]},
    # 378 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Player.Zone 是怎么工作的？"}, {"role": "assistant", "content": "Player.Zone 是 Razor Enhanced 的一个方法 (Method)。\n描述： 读取几何路径以返回基础分类，如“城镇”或“地牢”。\n返回： String\n参数： N/A\n示例:\n```python\nresult = Player.Zone()\n```"}]},
    # 379 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Что выполняет CUO.CloseGump?"}, {"role": "assistant", "content": "CUO.CloseGump — это Метод в Razor Enhanced.\nОписание: Принудительно удаляет графический оверлей, перехватывая рендеринг ClassicUO.\nВозврат: Void\nПараметры: serial\nПример:\n```python\nCUO.CloseGump(serial)\n```"}]},
    # 380 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "What is the purpose of CUO.CloseMobileHealthBar?"}, {"role": "assistant", "content": "CUO.CloseMobileHealthBar is a Method of Razor Enhanced.\nDescription: Removes HP bars pulled out during combat, reducing clutter.\nReturn: Void\nParameters: mobileserial\nExample:\n```python\nCUO.CloseMobileHealthBar(mobileserial)\n```"}]},
    # 381 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Spiegami a cosa serve CUO.CloseMyStatusBar."}, {"role": "assistant", "content": "CUO.CloseMyStatusBar è un Metodo di Razor Enhanced.\nDescrizione: Ripulisce lo spazio rimuovendo la rappresentazione visiva dei parametri dell'utente.\nRitorno: Void\nParametri: N/A\nEsempio:\n```python\nCUO.CloseMyStatusBar()\n```"}]},
    # 382 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Cómo se usa CUO.CloseTMap?"}, {"role": "assistant", "content": "CUO.CloseTMap es un Método de Razor Enhanced.\nDescripción: Simula una acción codificada de clic para deconstruir la interfaz de mapas del tesoro.\nRetorno: Boolean\nParámetros: N/A\nEjemplo:\n```python\nresult = CUO.CloseTMap()\n```"}]},
    # 383 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Was macht CUO.FollowMobile?"}, {"role": "assistant", "content": "CUO.FollowMobile ist eine Methode von Razor Enhanced.\nBeschreibung: Automatisiert die Follow-Routine des Clients, indem sie sich stabil an das mobile Ziel klammert.\nRückgabe: Void\nParameter: mobileserial\nBeispiel:\n```python\nCUO.FollowMobile(mobileserial)\n```"}]},
    # 384 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "请解释 CUO.FollowOff 的作用。"}, {"role": "assistant", "content": "CUO.FollowOff 是 Razor Enhanced 的一个方法 (Method)。\n描述： 解除之前锚定实体的跟随状态。\n返回： Void\n参数： N/A\n示例:\n```python\nCUO.FollowOff()\n```"}]},
    # 385 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Объясни назначение функции CUO.Following."}, {"role": "assistant", "content": "CUO.Following — это Метод в Razor Enhanced.\nОписание: Запрашивает логический статус привязки, возвращая логическое значение и ID цели.\nВозврат: Tuple\nПараметры: N/A\nПример:\n```python\nresult = CUO.Following()\n```"}]},
    # 386 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Could you explain what CUO.FreeView does?"}, {"role": "assistant", "content": "CUO.FreeView is a Method of Razor Enhanced.\nDescription: Unlocks the centered rendering limit on the character, allowing aerial inspections of the map.\nReturn: Void\nParameters: free\nExample:\n```python\nCUO.FreeView(free)\n```"}]},
    # 387 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Cosa fa esattamente CUO.GetSetting?"}, {"role": "assistant", "content": "CUO.GetSetting è un Metodo di Razor Enhanced.\nDescrizione: Legge la gerarchia JSON interna del client ClassicUO dinamicamente.\nRitorno: String\nParametri: settingName\nEsempio:\n```python\nresult = CUO.GetSetting(settingName)\n```"}]},
    # 388 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Qué función cumple CUO.GoToMarker?"}, {"role": "assistant", "content": "CUO.GoToMarker es un Método de Razor Enhanced.\nDescripción: Desplaza radicalmente el área visualizada si el módulo de radar del cliente está instanciado.\nRetorno: Void\nParámetros: x, y\nEjemplo:\n```python\nCUO.GoToMarker(x, y)\n```"}]},
    # 389 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Wofür wird CUO.LoadMarkers verwendet?"}, {"role": "assistant", "content": "CUO.LoadMarkers ist eine Methode von Razor Enhanced.\nBeschreibung: Berechnet und instanziiert externe räumliche Elemente auf der globalen Karte neu.\nRückgabe: Void\nParameter: N/A\nBeispiel:\n```python\nCUO.LoadMarkers()\n```"}]},
    # 390 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "CUO.MoveGump 是做什么的？"}, {"role": "assistant", "content": "CUO.MoveGump 是 Razor Enhanced 的一个方法 (Method)。\n描述： 通过强制重新定位图形窗口来防止战术遮挡。\n返回： Void\n参数： serial, x, y\n示例:\n```python\nCUO.MoveGump(serial, x, y)\n```"}]},
    # 391 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Что выполняет CUO.OpenContainerAt?"}, {"role": "assistant", "content": "CUO.OpenContainerAt — это Метод в Razor Enhanced.\nОписание: Асинхронно отрисовывает сложные инвентари в заданных зонах монитора.\nВозврат: Void\nПараметры: bag, x, y\nПример:\n```python\nCUO.OpenContainerAt(bag, x, y)\n```"}]},
    # 392 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "What is the function of CUO.OpenMobileHealthBar?"}, {"role": "assistant", "content": "CUO.OpenMobileHealthBar is a Method of Razor Enhanced.\nDescription: Builds stable visual indicators for boss fight monitoring without manual dragging.\nReturn: Void\nParameters: mobileserial, x, y, custom\nExample:\n```python\nCUO.OpenMobileHealthBar(mobileserial, x, y, custom)\n```"}]},
    # 393 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Mi spieghi a cosa serve CUO.OpenMyStatusBar?"}, {"role": "assistant", "content": "CUO.OpenMyStatusBar è un Metodo di Razor Enhanced.\nDescrizione: Pone lo stato minimale in corner per minimizzare i movimenti oculari.\nRitorno: Void\nParametri: x, y\nEsempio:\n```python\nCUO.OpenMyStatusBar(x, y)\n```"}]},
    # 394 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Cómo funciona CUO.PlayMacro en Razor?"}, {"role": "assistant", "content": "CUO.PlayMacro es un Método de Razor Enhanced.\nDescripción: Invoca subrutinas heredadas compiladas dentro de UOSteam/ClassicUO.\nRetorno: Void\nParámetros: macroName\nEjemplo:\n```python\nCUO.PlayMacro(macroName)\n```"}]},
    # 395 DE
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Was bewirkt CUO.ProfilePropertySet?"}, {"role": "assistant", "content": "CUO.ProfilePropertySet ist eine Methode von Razor Enhanced.\nBeschreibung: Überschreibt den Hardcode von Video-, Eingabe- oder Audioeinstellungen im laufenden Betrieb.\nRückgabe: Void\nParameter: propertyName, enable\nBeispiel:\n```python\nCUO.ProfilePropertySet(propertyName, enable)\n```"}]},
    # 396 ZH
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "请说明 CUO.SetGumpOpenLocation 的作用。"}, {"role": "assistant", "content": "CUO.SetGumpOpenLocation 是 Razor Enhanced 的一个方法 (Method)。\n描述： 预先计算传入 Gump 窗口的生成范围，防止逻辑重叠。\n返回： Void\n参数： gumpserial, x, y\n示例:\n```python\nCUO.SetGumpOpenLocation(gumpserial, x, y)\n```"}]},
    # 397 RU
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Для чего нужна функция Gumps.AddAlphaRegion?"}, {"role": "assistant", "content": "Gumps.AddAlphaRegion — это Метод в Razor Enhanced.\nОписание: Создает полупрозрачные слои для пользовательских Gump.\nВозврат: Void\nПараметры: gd, x, y, width, height\nПример:\n```python\nGumps.AddAlphaRegion(gd, x, y, width, height)\n```"}]},
    # 398 EN
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Can you describe Gumps.AddBackground?"}, {"role": "assistant", "content": "Gumps.AddBackground is a Method of Razor Enhanced.\nDescription: Compiles the base background texture by analyzing native layout arrays.\nReturn: Void\nParameters: gd, x, y, width, height, gumpId\nExample:\n```python\nGumps.AddBackground(gd, x, y, width, height, gumpId)\n```"}]},
    # 399 IT
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "Che cosa fa Gumps.AddButton?"}, {"role": "assistant", "content": "Gumps.AddButton è un Metodo di Razor Enhanced.\nDescrizione: Traccia hotspot logici per il ritorno asincrono ai server.\nRitorno: Void\nParametri: gd, x, y, normalID, pressedID, buttonID, type, param\nEsempio:\n```python\nGumps.AddButton(gd, x, y, normalID, pressedID, buttonID, type, param)\n```"}]},
    # 400 ES
    {"messages": [{"role": "system", "content": system_prompt}, {"role": "user", "content": "¿Qué hace Gumps.AddCheck?"}, {"role": "assistant", "content": "Gumps.AddCheck es un Método de Razor Enhanced.\nDescripción: Modela los parámetros booleanos necesarios para la elaboración de UI complejas.\nRetorno: Void\nParámetros: gd, x, y, inactiveID, activeID, initialState, switchID\nEjemplo:\n```python\nGumps.AddCheck(gd, x, y, inactiveID, activeID, initialState, switchID)\n```"}]}
]

with open("dataset_razor_API.jsonl", "a", encoding="utf-8") as f:
    for item in data:
        f.write(json.dumps(item, ensure_ascii=False) + "\n")

print("Appended 51 lines successfully.")
