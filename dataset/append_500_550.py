import json
import os

system_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

data = [
    # 500: EN
    {"role": "user", "content": "What does Misc.OpenPaperdoll do?"},
    {"role": "assistant", "content": "Misc.OpenPaperdoll is a Method of Razor Enhanced.\nDescription: Forces forced equipment inventory loading (OSI only).\nReturn: Void\nParameters: N/A\nExample:\nMisc.OpenPaperdoll()"},
    
    # 501: IT
    {"role": "user", "content": "Come funziona Misc.Pause?"},
    {"role": "assistant", "content": "Misc.Pause è un Metodo di Razor Enhanced.\nDescrizione: Astrazione di Thread.Sleep che non invalida il listener background degli agenti principali.\nRitorno: Void\nParametri: millisec\nEsempio:\nMisc.Pause(millisec)"},

    # 502: ES
    {"role": "user", "content": "¿Para qué sirve Misc.PetRename?"},
    {"role": "assistant", "content": "Misc.PetRename es un Método de Razor Enhanced.\nDescripción: Aprovecha la anulación del protocolo para mutar la etiqueta asociada al familiar sometido.\nRetorno: Void\nParámetros: serial, name\nEjemplo:\nMisc.PetRename(serial, name)"},

    # 503: DE
    {"role": "user", "content": "Erkläre Misc.PlaySound."},
    {"role": "assistant", "content": "Misc.PlaySound ist eine Methode von Razor Enhanced.\nBeschreibung: Falscher Audio-Versand: Erzeugt lokales Rauschen für physische Warnungen auf AFK-Skripten.\nRückgabe: Void\nParameter: sound, x, y, z\nBeispiel:\nMisc.PlaySound(sound, x, y, z)"},

    # 504: ZH
    {"role": "user", "content": "Misc.QueryStringResponse 的作用是什么？"},
    {"role": "assistant", "content": "Misc.QueryStringResponse 是 Razor Enhanced 的一个方法。\n描述: 制定并关闭服务器端法术确认或捐赠的模式菜单。\n返回: Void\n参数: okcancel, response\n示例:\nMisc.QueryStringResponse(okcancel, response)"},

    # 505: RU
    {"role": "user", "content": "Что делает Misc.RazorDirectory?"},
    {"role": "assistant", "content": "Misc.RazorDirectory - это Метод Razor Enhanced.\nОписание: Абсолютная навигация ресурсов. Развязывает зависимости от инъекций плагинов, как в ClassicUO.\nВозврат: String\nПараметры: N/A\nПример:\nresult = Misc.RazorDirectory()"},

    # 506: EN
    {"role": "user", "content": "Explain Misc.ReadSharedValue."},
    {"role": "assistant", "content": "Misc.ReadSharedValue is a Method of Razor Enhanced.\nDescription: Together with SetSharedValue, it forms the core for the IronPython inter-process communication pipeline.\nReturn: Object\nParameters: name\nExample:\nresult = Misc.ReadSharedValue(name)"},

    # 507: IT
    {"role": "user", "content": "Cosa fa Misc.RemoveLineInFile?"},
    {"role": "assistant", "content": "Misc.RemoveLineInFile è un Metodo di Razor Enhanced.\nDescrizione: Modifica distruttiva sicura per logfile attivi.\nRitorno: Boolean\nParametri: fileName, lineOfData\nEsempio:\nresult = Misc.RemoveLineInFile(fileName, lineOfData)"},

    # 508: ES
    {"role": "user", "content": "¿Qué hace Misc.RemoveSharedValue?"},
    {"role": "assistant", "content": "Misc.RemoveSharedValue es un Método de Razor Enhanced.\nDescripción: Desasignación quirúrgica previniendo fugas de memoria en conjuntos hash compartidos.\nRetorno: Void\nParámetros: name\nEjemplo:\nMisc.RemoveSharedValue(name)"},

    # 509: DE
    {"role": "user", "content": "Wofür wird Misc.ResetPrompt verwendet?"},
    {"role": "assistant", "content": "Misc.ResetPrompt ist eine Methode von Razor Enhanced.\nBeschreibung: Verhindert bedingte Schleifen von blockierten serverseitigen Menüs.\nRückgabe: Void\nParameter: N/A\nBeispiel:\nMisc.ResetPrompt()"},

    # 510: ZH
    {"role": "user", "content": "解释 Misc.ResponsePrompt。"},
    {"role": "assistant", "content": "Misc.ResponsePrompt 是 Razor Enhanced 的一个方法。\n描述: 通过发送被动的ok来绕过文本输入窗口。\n返回: Void\n参数: text\n示例:\nMisc.ResponsePrompt(text)"},

    # 511: RU
    {"role": "user", "content": "Объясните Misc.Resync."},
    {"role": "assistant", "content": "Misc.Resync - это Метод Razor Enhanced.\nОписание: Незаменим после перемещения через порталы: заставляет матрично регенерировать соседние сущности, очищая поврежденные кеши.\nВозврат: Void\nПараметры: N/A\nПример:\nMisc.Resync()"},

    # 512: EN
    {"role": "user", "content": "What is Misc.RightMouseClick used for?"},
    {"role": "assistant", "content": "Misc.RightMouseClick is a Method of Razor Enhanced.\nDescription: Affinity operation for UI cancellations or Win32 physical inspections.\nReturn: Void\nParameters: xpos, ypos, clientCoords\nExample:\nMisc.RightMouseClick(xpos, ypos, clientCoords)"},

    # 513: IT
    {"role": "user", "content": "Spiega Misc.ScriptCurrent."},
    {"role": "assistant", "content": "Misc.ScriptCurrent è un Metodo di Razor Enhanced.\nDescrizione: Restituisce la sorgente chiamante, basilare per librerie relative.\nRitorno: String\nParametri: fullpath\nEsempio:\nresult = Misc.ScriptCurrent(fullpath)"},

    # 514: ES
    {"role": "user", "content": "¿Qué es Misc.ScriptIsSuspended?"},
    {"role": "assistant", "content": "Misc.ScriptIsSuspended es un Método de Razor Enhanced.\nDescripción: Control de diagnóstico del grupo de subprocesos.\nRetorno: Boolean\nParámetros: scriptfile\nEjemplo:\nresult = Misc.ScriptIsSuspended(scriptfile)"},

    # 515: DE
    {"role": "user", "content": "Was macht Misc.ScriptResume?"},
    {"role": "assistant", "content": "Misc.ScriptResume ist eine Methode von Razor Enhanced.\nBeschreibung: Taut den logischen Ausführer IronPython auf und weckt die Aufgabe auf.\nRückgabe: Void\nParameter: scriptfile\nBeispiel:\nMisc.ScriptResume(scriptfile)"},

    # 516: ZH
    {"role": "user", "content": "Misc.ScriptRun 有什么功能？"},
    {"role": "assistant", "content": "Misc.ScriptRun 是 Razor Enhanced 的一个方法。\n描述: 实现脚本链接，调用主程序和仆从来处理复杂的队列。\n返回: Void\n参数: scriptfile\n示例:\nMisc.ScriptRun(scriptfile)"},

    # 517: RU
    {"role": "user", "content": "Для чего нужен Misc.ScriptStatus?"},
    {"role": "assistant", "content": "Misc.ScriptStatus - это Метод Razor Enhanced.\nОписание: Общая проверка существования задачи в сетке.\nВозврат: Boolean\nПараметры: scriptfile\nПример:\nresult = Misc.ScriptStatus(scriptfile)"},

    # 518: EN
    {"role": "user", "content": "Explain Misc.ScriptStop."},
    {"role": "assistant", "content": "Misc.ScriptStop is a Method of Razor Enhanced.\nDescription: Lethal and clean interruption of the specified thread.\nReturn: Void\nParameters: scriptfile\nExample:\nMisc.ScriptStop(scriptfile)"},

    # 519: IT
    {"role": "user", "content": "A cosa serve Misc.ScriptStopAll?"},
    {"role": "assistant", "content": "Misc.ScriptStopAll è un Metodo di Razor Enhanced.\nDescrizione: Kill-switch d'emergenza asincrono per l'intera virtual machine script.\nRitorno: Void\nParametri: skipCurrent\nEsempio:\nMisc.ScriptStopAll(skipCurrent)"},

    # 520: ES
    {"role": "user", "content": "Explica Misc.ScriptSuspend."},
    {"role": "assistant", "content": "Misc.ScriptSuspend es un Método de Razor Enhanced.\nDescripción: Congelación del marco de estado del subproceso.\nRetorno: Void\nParámetros: scriptfile\nEjemplo:\nMisc.ScriptSuspend(scriptfile)"},

    # 521: DE
    {"role": "user", "content": "Wie funktioniert Misc.SendMessage?"},
    {"role": "assistant", "content": "Misc.SendMessage ist eine Methode von Razor Enhanced.\nBeschreibung: Injektor für lokale Debug-Logik, der ästhetisch im UI-Client maskiert ist. Unterstützt mehrfache Ableitungen.\nRückgabe: Void\nParameter: msg, color, wait\nBeispiel:\nMisc.SendMessage(msg, color, wait)"},

    # 522: ZH
    {"role": "user", "content": "解释一下 Misc.SendToClient。"},
    {"role": "assistant", "content": "Misc.SendToClient 是 Razor Enhanced 的一个方法。\n描述: 绕过按键（Ctrl，Alt）的桥梁，对于ClassicUO上本机命令的绕过至关重要。\n返回: Void\n参数: keys\n示例:\nMisc.SendToClient(keys)"},

    # 523: RU
    {"role": "user", "content": "Что делает Misc.SetSharedValue?"},
    {"role": "assistant", "content": "Misc.SetSharedValue - это Метод Razor Enhanced.\nОписание: Перезаписывает или выделяет пул обмена для мульти-макро архитектур.\nВозврат: Void\nПараметры: name, value\nПример:\nMisc.SetSharedValue(name, value)"},

    # 524: EN
    {"role": "user", "content": "What does Misc.ShardName do?"},
    {"role": "assistant", "content": "Misc.ShardName is a Method of Razor Enhanced.\nDescription: Detects the geographical context of the server useful for multiple shard conditional logic.\nReturn: String\nParameters: N/A\nExample:\nresult = Misc.ShardName()"},

    # 525: IT
    {"role": "user", "content": "Come funziona Misc.UnIgnoreObject?"},
    {"role": "assistant", "content": "Misc.UnIgnoreObject è un Metodo di Razor Enhanced.\nDescrizione: Ripristina visibilità e iterabilità logica annullando isolamenti di IgnoreObject.\nRitorno: Void\nParametri: serial\nEsempio:\nMisc.UnIgnoreObject(serial)"},

    # 526: ES
    {"role": "user", "content": "¿Para qué sirve Misc.UseContextMenu?"},
    {"role": "assistant", "content": "Misc.UseContextMenu es un Método de Razor Enhanced.\nDescripción: Simula íntegramente los procedimientos de clic de interfaz de usuario en entidades y respuestas de texto en tiempo nulo.\nRetorno: Boolean\nParámetros: serial, choice, delay\nEjemplo:\nresult = Misc.UseContextMenu(serial, choice, delay)"},

    # 527: DE
    {"role": "user", "content": "Was macht Misc.WaitForContext?"},
    {"role": "assistant", "content": "Misc.WaitForContext ist eine Methode von Razor Enhanced.\nBeschreibung: Wartet darauf, dass der Server die Arrays der Optionen zurückgibt (z. B. \"Kaufen\", \"Verkaufen\") und generiert den Kontext.\nRückgabe: List[Misc.Context]\nParameter: serial, delay, showContext\nBeispiel:\nresult = Misc.WaitForContext(serial, delay, showContext)"},

    # 528: ZH
    {"role": "user", "content": "Misc.WaitForMenu 的作用是什么？"},
    {"role": "assistant", "content": "Misc.WaitForMenu 是 Razor Enhanced 的一个方法。\n描述: 针对复古工艺的旧版UI数据包的异步侦听器。\n返回: Boolean\n参数: delay\n示例:\nresult = Misc.WaitForMenu(delay)"},

    # 529: RU
    {"role": "user", "content": "Для чего нужен Misc.WaitForPrompt?"},
    {"role": "assistant", "content": "Misc.WaitForPrompt - это Метод Razor Enhanced.\nОписание: Логическая заморозка, предотвращающая отсутствие полезной нагрузки в подсказке ответа.\nВозврат: Boolean\nПараметры: delay\nПример:\nresult = Misc.WaitForPrompt(delay)"},

    # 530: EN
    {"role": "user", "content": "Explain Misc.WaitForQueryString."},
    {"role": "assistant", "content": "Misc.WaitForQueryString is a Method of Razor Enhanced.\nDescription: Identical use limited to the free-shard vendor text drop-down menu.\nReturn: Boolean\nParameters: delay\nExample:\nresult = Misc.WaitForQueryString(delay)"},

    # 531: IT
    {"role": "user", "content": "Cosa fa PacketLogger.AddBlacklist?"},
    {"role": "assistant", "content": "PacketLogger.AddBlacklist è un Metodo di Razor Enhanced.\nDescrizione: Estrapola in base pre-parsing ed elide in runtime i nodi id per filtrare noise.\nRitorno: Void\nParametri: packetID\nEsempio:\nPacketLogger.AddBlacklist(packetID)"},

    # 532: ES
    {"role": "user", "content": "¿Qué hace PacketLogger.AddTemplate?"},
    {"role": "assistant", "content": "PacketLogger.AddTemplate es un Método de Razor Enhanced.\nDescripción: Inserta definiciones JSON que fuerzan el análisis espacial del registrador en lógicas privadas de OSI.\nRetorno: Void\nParámetros: packetTemplate\nEjemplo:\nPacketLogger.AddTemplate(packetTemplate)"},

    # 533: DE
    {"role": "user", "content": "Wofür wird PacketLogger.AddWhitelist verwendet?"},
    {"role": "assistant", "content": "PacketLogger.AddWhitelist ist eine Methode von Razor Enhanced.\nBeschreibung: Zero-Trust-Ansatz, der nur durch ID diagnostizierte Zeichenfolgen anzeigt.\nRückgabe: Void\nParameter: packetID\nBeispiel:\nPacketLogger.AddWhitelist(packetID)"},

    # 534: ZH
    {"role": "user", "content": "解释 PacketLogger.DiscardAll。"},
    {"role": "assistant", "content": "PacketLogger.DiscardAll 是 Razor Enhanced 的一个方法。\n描述: 将拦截器置于限制性隔离状态（仅显示白名单）。\n返回: Void\n参数: discardAll\n示例:\nPacketLogger.DiscardAll(discardAll)"},

    # 535: RU
    {"role": "user", "content": "Объясните PacketLogger.DiscardShowHeader."},
    {"role": "assistant", "content": "PacketLogger.DiscardShowHeader - это Метод Razor Enhanced.\nОписание: Обеспечивает видимость метаданных, отбрасывая внутренний шестнадцатеричный дамп, что снижает нагрузку на процессор.\nВозврат: Void\nПараметры: showHeader\nПример:\nPacketLogger.DiscardShowHeader(showHeader)"},

    # 536: EN
    {"role": "user", "content": "What does PacketLogger.ListenPacketPath do?"},
    {"role": "assistant", "content": "PacketLogger.ListenPacketPath is a Method of Razor Enhanced.\nDescription: Asynchronously determines whether to divert ClientToServer, ServerToClient, or RazorToServer bytes.\nReturn: Array\nParameters: packetPath, active\nExample:\nresult = PacketLogger.ListenPacketPath(packetPath, active)"},

    # 537: IT
    {"role": "user", "content": "Spiega PacketLogger.RemoveTemplate."},
    {"role": "assistant", "content": "PacketLogger.RemoveTemplate è un Metodo di Razor Enhanced.\nDescrizione: Rimuove lo spatial decoding per ripristinare il dump base.\nRitorno: Void\nParametri: packetID\nEsempio:\nPacketLogger.RemoveTemplate(packetID)"},

    # 538: ES
    {"role": "user", "content": "¿Qué es PacketLogger.Reset?"},
    {"role": "assistant", "content": "PacketLogger.Reset es un Método de Razor Enhanced.\nDescripción: Restaura las variables asíncronas internas del monitor a su estado de fábrica (no ofuscado).\nRetorno: Void\nParámetros: N/A\nEjemplo:\nPacketLogger.Reset()"},

    # 539: DE
    {"role": "user", "content": "Was macht PacketLogger.SendToClient?"},
    {"role": "assistant", "content": "PacketLogger.SendToClient ist eine Methode von Razor Enhanced.\nBeschreibung: Schreibt nativ in die Eingabe. Generiert beispielsweise fiktive visuelle Monster oder extreme clientseitige UIs.\nRückgabe: Void\nParameter: packetData\nBeispiel:\nPacketLogger.SendToClient(packetData)"},

    # 540: ZH
    {"role": "user", "content": "PacketLogger.SendToServer 有什么功能？"},
    {"role": "assistant", "content": "PacketLogger.SendToServer 是 Razor Enhanced 的一个方法。\n描述: 更改输入缓冲区，完全忽略客户端以传输原始TCP有效负载。\n返回: Void\n参数: packetData\n示例:\nPacketLogger.SendToServer(packetData)"},

    # 541: RU
    {"role": "user", "content": "Для чего нужен PacketLogger.SetOutputPath?"},
    {"role": "assistant", "content": "PacketLogger.SetOutputPath - это Метод Razor Enhanced.\nОписание: Извлекает асинхронное место назначения непрерывного дампа файла.\nВозврат: String\nПараметры: outputpath\nПример:\nresult = PacketLogger.SetOutputPath(outputpath)"},

    # 542: EN
    {"role": "user", "content": "Explain PacketLogger.Start."},
    {"role": "assistant", "content": "PacketLogger.Start is a Method of Razor Enhanced.\nDescription: Creates a system lock for I/O and starts byte transcoding to ASCII log.\nReturn: String\nParameters: outputpath, appendLogs\nExample:\nresult = PacketLogger.Start(outputpath, appendLogs)"},

    # 543: IT
    {"role": "user", "content": "A cosa serve PacketLogger.Stop?"},
    {"role": "assistant", "content": "PacketLogger.Stop è un Metodo di Razor Enhanced.\nDescrizione: Sblocca IO lock salvando e disattivando l'aggancio socket.\nRitorno: String\nParametri: N/A\nEsempio:\nresult = PacketLogger.Stop()"},

    # 544: ES
    {"role": "user", "content": "Explica las propiedades de PacketLogger.FieldTemplate."},
    {"role": "assistant", "content": "PacketLogger.FieldTemplate es una Propiedad de Razor Enhanced.\nDescripción: Abstracciones que definen fragmentos de extracción complejos. Ejemplos de FieldType compatibles para la decodificación automática de JSON: BOOL, DUMP, FIELDS, HEX, INT, MODELID, PACKETID, SERIAL, SKIP, SUBPACKET, TEXT, UINT, UTF8.\nRetorno: Misto\nParámetros: N/A\nEjemplo:\n# Esta es una propiedad, no un método\nresult = PacketLogger.FieldTemplate.fields"},

    # 545: DE
    {"role": "user", "content": "Wie funktioniert Sound.AddFilter?"},
    {"role": "assistant", "content": "Sound.AddFilter ist eine Methode von Razor Enhanced.\nBeschreibung: Maskiert störende Audio-Sendungen, um intensives Testen zu erleichtern (z. B. Hunde oder Maschinen).\nRückgabe: Void\nParameter: name, sounds\nBeispiel:\nSound.AddFilter(name, sounds)"},

    # 546: ZH
    {"role": "user", "content": "解释一下 Sound.Log。"},
    {"role": "assistant", "content": "Sound.Log 是 Razor Enhanced 的一个方法。\n描述: 启用跟踪以进行启发式的触发器推导（例如，潜行检测）。\n返回: Void\n参数: activateLogging\n示例:\nSound.Log(activateLogging)"},

    # 547: RU
    {"role": "user", "content": "Что делает Sound.OnFilter?"},
    {"role": "assistant", "content": "Sound.OnFilter - это Метод Razor Enhanced.\nОписание: Низкоуровневое асинхронное управление.\nВозврат: Void\nПараметры: p, args\nПример:\nSound.OnFilter(p, args)"},

    # 548: EN
    {"role": "user", "content": "What does Sound.RemoveFilter do?"},
    {"role": "assistant", "content": "Sound.RemoveFilter is a Method of Razor Enhanced.\nDescription: Restores normal sending to the local audio API client.\nReturn: Void\nParameters: name\nExample:\nSound.RemoveFilter(name)"},

    # 549: IT
    {"role": "user", "content": "Come funziona Sound.WaitForSound?"},
    {"role": "assistant", "content": "Sound.WaitForSound è un Metodo di Razor Enhanced.\nDescrizione: Listener passivo privo di calcolo poligonale basato sui request 0x54 per trigger macro ad alte performance.\nRitorno: Boolean\nParametri: sounds, timeout\nEsempio:\nresult = Sound.WaitForSound(sounds, timeout)"},

    # 550: ES
    {"role": "user", "content": "¿Para qué sirve PathFinding.GetPath?"},
    {"role": "assistant", "content": "PathFinding.GetPath es un Método de Razor Enhanced.\nDescripción: Deriva la secuencia vectorial más corta iterando los nodos del gráfico de obstáculos.\nRetorno: List\nParámetros: dst_x, dst_y, ignoremob\nEjemplo:\nresult = PathFinding.GetPath(dst_x, dst_y, ignoremob)"}
]

output_file = "dataset_razor_API.jsonl"
with open(output_file, 'a', encoding='utf-8') as f:
    for i in range(0, len(data), 2):
        user_msg = data[i]
        assistant_msg = data[i+1]
        
        json_obj = {
            "messages": [
                {"role": "system", "content": system_prompt},
                user_msg,
                assistant_msg
            ]
        }
        f.write(json.dumps(json_obj, ensure_ascii=False) + '\n')

print(f"Successfully appended {len(data)//2} entries to {output_file}")
