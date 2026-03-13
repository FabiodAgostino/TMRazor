import json

sys_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

records = [
    {
        "user": "What does SellAgent.Disable do?",
        "assistant": "SellAgent.Disable is a Razor Enhanced Method.\nDescription: Disables automatic selling.\nReturn: Void\nParameters: N/A\nExample:\n```python\nSellAgent.Disable()\n```"
    },
    {
        "user": "Cosa fa SellAgent.Enable?",
        "assistant": "SellAgent.Enable è un Metodo di Razor Enhanced.\nDescrizione: Abilita la vendita automatica ai vendor NPC.\nRitorno: Void\nParametri: N/A\nEsempio:\n```python\nSellAgent.Enable()\n```"
    },
    {
        "user": "¿Qué hace SellAgent.Status?",
        "assistant": "SellAgent.Status es un Método de Razor Enhanced.\nDescripción: Verifica el estado del SellAgent.\nRetorno: Boolean\nParámetros: N/A\nEjemplo:\n```python\nresult = SellAgent.Status()\n```"
    },
    {
        "user": "Was macht Organizer.ChangeList?",
        "assistant": "Organizer.ChangeList ist eine Methode von Razor Enhanced.\nBeschreibung: Ändert die aktive Konfiguration des Organizers.\nRückgabe: Void\nParameter: listName: Nome della lista.\nBeispiel:\n```python\nOrganizer.ChangeList(listName)\n```"
    },
    {
        "user": "Organizer.FStart 是做什么的？",
        "assistant": "Organizer.FStart 是 Razor Enhanced 的方法。\n描述：开始物品整理。\n返回：Void\n参数：N/A\n示例:\n```python\nOrganizer.FStart()\n```"
    },
    {
        "user": "Что делает Organizer.FStop?",
        "assistant": "Organizer.FStop это Метод в Razor Enhanced.\nОписание: Останавливает перекладывание предметов.\nВозврат: Void\nПараметры: N/A\nПример:\n```python\nOrganizer.FStop()\n```"
    },
    {
        "user": "What does Organizer.RunOnce do?",
        "assistant": "Organizer.RunOnce is a Razor Enhanced Method.\nDescription: Runs the organizer a single time.\nReturn: Void\nParameters: organizerName, sourceBag [Int32], destBag [Int32], dragDelay [Int32]\nExample:\n```python\nOrganizer.RunOnce(organizerName, sourceBag, destBag, dragDelay)\n```"
    },
    {
        "user": "Cosa fa Organizer.Status?",
        "assistant": "Organizer.Status è un Metodo di Razor Enhanced.\nDescrizione: Ritorna True se l'Organizer è in azione.\nRitorno: Boolean\nParametri: N/A\nEsempio:\n```python\nresult = Organizer.Status()\n```"
    },
    {
        "user": "¿Qué hace Items.Filter.CheckIgnoreObject?",
        "assistant": "Items.Filter.CheckIgnoreObject es una Propiedad de Razor Enhanced.\nDescripción: Excluye los objetos presentes en la lista global ignorados (por defecto: False).\nRetorno: Boolean\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Items.Filter.CheckIgnoreObject\n```"
    },
    {
        "user": "Was macht Items.Filter.Enabled?",
        "assistant": "Items.Filter.Enabled ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Aktiviert oder deaktiviert die Anwendung des Filters (Standard: True).\nRückgabe: Boolean\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Items.Filter.Enabled\n```"
    },
    {
        "user": "Items.Filter.Graphics 是做什么的？",
        "assistant": "Items.Filter.Graphics 是 Razor Enhanced 的属性。\n描述：将搜索限制为特定 Graphic ID (ItemID) 的列表。支持 Add() 和 AddRange()。\n返回：List[Int32]\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Items.Filter.Graphics\n```"
    },
    {
        "user": "Что делает Items.Filter.Hues?",
        "assistant": "Items.Filter.Hues это Свойство в Razor Enhanced.\nОписание: Ограничивает поиск списком определенных цветов.\nВозврат: List[Int32]\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Items.Filter.Hues\n```"
    },
    {
        "user": "What does Items.Filter.IsContainer do?",
        "assistant": "Items.Filter.IsContainer is a Razor Enhanced Property.\nDescription: Filters only objects that act as containers (-1 to ignore).\nReturn: Int32\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Items.Filter.IsContainer\n```"
    },
    {
        "user": "Cosa fa Items.Filter.IsCorpse?",
        "assistant": "Items.Filter.IsCorpse è una Proprietà di Razor Enhanced.\nDescrizione: Filtra esclusivamente i cadaveri presenti sul suolo.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Items.Filter.IsCorpse\n```"
    },
    {
        "user": "¿Qué hace Items.Filter.IsDoor?",
        "assistant": "Items.Filter.IsDoor es una Propiedad de Razor Enhanced.\nDescripción: Filtra los objetos clasificados estructuralmente como puertas.\nRetorno: Int32\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Items.Filter.IsDoor\n```"
    },
    {
        "user": "Was macht Items.Filter.Layers?",
        "assistant": "Items.Filter.Layers ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Beschränkt die Suche auf Gegenstände, die auf bestimmten anatomischen Schichten ausgerüstet sind (z. B. RightHand).\nRückgabe: List\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Items.Filter.Layers\n```"
    },
    {
        "user": "Items.Filter.Movable 是做什么的？",
        "assistant": "Items.Filter.Movable 是 Razor Enhanced 的属性。\n描述：根据服务器设置的移动标志过滤物品。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Items.Filter.Movable\n```"
    },
    {
        "user": "Что делает Items.Filter.Multi?",
        "assistant": "Items.Filter.Multi это Свойство в Razor Enhanced.\nОписание: Ограничивает поиск сложными объектами (Multi), такими как дома или корабли.\nВозврат: Int32\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Items.Filter.Multi\n```"
    },
    {
        "user": "What does Items.Filter.Name do?",
        "assistant": "Items.Filter.Name is a Razor Enhanced Property.\nDescription: Matches the textual name of the object.\nReturn: String\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Items.Filter.Name\n```"
    },
    {
        "user": "Cosa fa Items.Filter.OnGround?",
        "assistant": "Items.Filter.OnGround è una Proprietà di Razor Enhanced.\nDescrizione: Filtra solo gli oggetti non contenuti in inventari, liberi sulla mappa.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Items.Filter.OnGround\n```"
    },
    {
        "user": "¿Qué hace Items.Filter.RangeMax?",
        "assistant": "Items.Filter.RangeMax es una Propiedad de Razor Enhanced.\nDescripción: Distancia radial máxima en tiles con respecto a la posición del jugador.\nRetorno: Double\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Items.Filter.RangeMax\n```"
    },
    {
        "user": "Was macht Items.Filter.RangeMin?",
        "assistant": "Items.Filter.RangeMin ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Minimale radiale Entfernung zum Spieler.\nRückgabe: Double\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Items.Filter.RangeMin\n```"
    },
    {
        "user": "Items.Filter.Serials 是做什么的？",
        "assistant": "Items.Filter.Serials 是 Razor Enhanced 的属性。\n描述：将搜索限制为精确的序列标识符池。\n返回：List[Int32]\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Items.Filter.Serials\n```"
    },
    {
        "user": "Что делает Items.Filter.ZRangeMax?",
        "assistant": "Items.Filter.ZRangeMax это Свойство в Razor Enhanced.\nОписание: Верхний предел координаты Z для трехмерной фильтрации.\nВозврат: Double\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Items.Filter.ZRangeMax\n```"
    },
    {
        "user": "What does Items.Filter.ZRangeMin do?",
        "assistant": "Items.Filter.ZRangeMin is a Razor Enhanced Property.\nDescription: Lower limit of the Z coordinate.\nReturn: Double\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Items.Filter.ZRangeMin\n```"
    },
    {
        "user": "Cosa fa Items.ApplyFilter?",
        "assistant": "Items.ApplyFilter è un Metodo di Razor Enhanced.\nDescrizione: Risolve il filtro interrogando la memoria e restituisce le istanze conformi.\nRitorno: List[Item]\nParametri: filter [Items.Filter]\nEsempio:\n```python\nresult = Items.ApplyFilter(filter)\n```"
    },
    {
        "user": "¿Qué hace Items.BackpackCount?",
        "assistant": "Items.BackpackCount es un Método de Razor Enhanced.\nDescripción: Calcula la suma de las cantidades de un objeto específico en la mochila.\nRetorno: Int32\nParámetros: itemid [Int32], color [Int32]\nEjemplo:\n```python\nresult = Items.BackpackCount(itemid, color)\n```"
    },
    {
        "user": "Was macht Items.ChangeDyeingTubColor?",
        "assistant": "Items.ChangeDyeingTubColor ist eine Methode von Razor Enhanced.\nBeschreibung: Sendet das Paket, um die Farbe eines Färbebeckens zu ändern.\nRückgabe: Void\nParameter: dyes [Item], dyeingTub [Item], color [Int32]\nBeispiel:\n```python\nItems.ChangeDyeingTubColor(dyes, dyeingTub, color)\n```"
    },
    {
        "user": "Items.Close 是做什么的？",
        "assistant": "Items.Close 是 Razor Enhanced 的方法。\n描述：关闭打开的容器或尸体的图形界面。\n返回：Void\n参数：serial [Item/Int32]\n示例:\n```python\nItems.Close(serial)\n```"
    },
    {
        "user": "Что делает Items.ContainerCount?",
        "assistant": "Items.ContainerCount это Метод в Razor Enhanced.\nОписание: Рекурсивно подсчитывает предметы внутри корневого контейнера.\nВозврат: Int32\nПараметры: container [Item/Int32], itemid [Int32], color [Int32], recursive\nПример:\n```python\nresult = Items.ContainerCount(container, itemid, color, recursive)\n```"
    },
    {
        "user": "What does Items.ContextExist do?",
        "assistant": "Items.ContextExist is a Razor Enhanced Method.\nDescription: Queries the existence of an option in the object's context menu.\nReturn: Int32\nParameters: serial [Item/Int32], name\nExample:\n```python\nresult = Items.ContextExist(serial, name)\n```"
    },
    {
        "user": "Cosa fa Items.DropFromHand?",
        "assistant": "Items.DropFromHand è un Metodo di Razor Enhanced.\nDescrizione: Rilascia un oggetto sollevato virtualmente nel cursore in un contenitore.\nRitorno: Void\nParametri: item [Item], container [Item]\nEsempio:\n```python\nItems.DropFromHand(item, container)\n```"
    },
    {
        "user": "¿Qué hace Items.DropItemGroundSelf?",
        "assistant": "Items.DropItemGroundSelf es un Método de Razor Enhanced.\nDescripción: Mueve un objeto del inventario al suelo a los pies del jugador.\nRetorno: Void\nParámetros: item [Item/Int32], amount [Int32], direction [Int32]\nEjemplo:\n```python\nItems.DropItemGroundSelf(item, amount, direction)\n```"
    },
    {
        "user": "Was macht Items.FindAllByID?",
        "assistant": "Items.FindAllByID ist eine Methode von Razor Enhanced.\nBeschreibung: Führt eine tiefgreifende Suche durch und gibt alle Vorkommen einer ID zurück.\nRückgabe: List[Item]\nParameter: itemids [List/Int32], color [Int32], container [Int32], range [Int32], considerIgnoreList\nBeispiel:\n```python\nresult = Items.FindAllByID(itemids, color, container, range, considerIgnoreList)\n```"
    },
    {
        "user": "Items.FindByID 是做什么的？",
        "assistant": "Items.FindByID 是 Razor Enhanced 的方法。\n描述：返回与基本参数匹配的找到的单个实例。\n返回：Item\n参数：itemid [List/Int32], color [Int32], container [Int32], recursive, considerIgnoreList\n示例:\n```python\nresult = Items.FindByID(itemid, color, container, recursive, considerIgnoreList)\n```"
    },
    {
        "user": "Что делает Items.FindByName?",
        "assistant": "Items.FindByName это Метод в Razor Enhanced.\nОписание: Быстрый поиск на основе анализа строки имени.\nВозврат: Item\nПараметры: itemName, color [Int32], container [Int32], range [Int32], considerIgnoreList\nПример:\n```python\nresult = Items.FindByName(itemName, color, container, range, considerIgnoreList)\n```"
    },
    {
        "user": "What does Items.FindBySerial do?",
        "assistant": "Items.FindBySerial is a Razor Enhanced Method.\nDescription: Instantiates an object starting from its network serial.\nReturn: Item\nParameters: serial [Int32]\nExample:\n```python\nresult = Items.FindBySerial(serial)\n```"
    },
    {
        "user": "Cosa fa Items.GetImage?",
        "assistant": "Items.GetImage è un Metodo di Razor Enhanced.\nDescrizione: Estrae e applica la tonalità alla risorsa grafica nativa del client.\nRitorno: Bitmap\nParametri: itemID [Int32], hue [Int32]\nEsempio:\n```python\nresult = Items.GetImage(itemID, hue)\n```"
    },
    {
        "user": "¿Qué hace Items.GetPropStringByIndex?",
        "assistant": "Items.GetPropStringByIndex es un Método de Razor Enhanced.\nDescripción: Extrae una línea específica del tooltip formateado del objeto.\nRetorno: String\nParámetros: serial [Item/Int32], index [Int32]\nEjemplo:\n```python\nresult = Items.GetPropStringByIndex(serial, index)\n```"
    },
    {
        "user": "Was macht Items.GetPropStringList?",
        "assistant": "Items.GetPropStringList ist eine Methode von Razor Enhanced.\nBeschreibung: Gibt das gesamte Array der Eigenschaftszeichenfolgen zurück.\nRückgabe: List\nParameter: serial [Item/Int32]\nBeispiel:\n```python\nresult = Items.GetPropStringList(serial)\n```"
    },
    {
        "user": "Items.GetPropValue 是做什么的？",
        "assistant": "Items.GetPropValue 是 Razor Enhanced 的方法。\n描述：解析与文本属性关联的数字浮点值。\n返回：Single\n参数：serial [Item/Int32], name\n示例:\n```python\nresult = Items.GetPropValue(serial, name)\n```"
    },
    {
        "user": "Что делает Items.GetPropValueString?",
        "assistant": "Items.GetPropValueString это Метод в Razor Enhanced.\nОписание: Ищет и возвращает необработанное текстовое значение свойства.\nВозврат: String\nПараметры: serial [Int32], name\nПример:\n```python\nresult = Items.GetPropValueString(serial, name)\n```"
    },
    {
        "user": "What does Items.GetProperties do?",
        "assistant": "Items.GetProperties is a Razor Enhanced Method.\nDescription: Forces the request to the server for updated tooltips if not in cache.\nReturn: List[Property]\nParameters: itemserial [Int32], delay [Int32]\nExample:\n```python\nresult = Items.GetProperties(itemserial, delay)\n```"
    },
    {
        "user": "Cosa fa Items.GetWeaponAbility?",
        "assistant": "Items.GetWeaponAbility è un Metodo di Razor Enhanced.\nDescrizione: Consulta la tabella interna per restituire le mosse speciali dell'arma.\nRitorno: Tuple\nParametri: itemId [Int32]\nEsempio:\n```python\nresult = Items.GetWeaponAbility(itemId)\n```"
    },
    {
        "user": "¿Qué hace Items.Hide?",
        "assistant": "Items.Hide es un Método de Razor Enhanced.\nDescripción: Altera el renderizado local haciendo que el objeto sea invisible para el cliente.\nRetorno: Void\nParámetros: serial [Item/Int32]\nEjemplo:\n```python\nItems.Hide(serial)\n```"
    },
    {
        "user": "Was macht Items.IgnoreTypes?",
        "assistant": "Items.IgnoreTypes ist eine Methode von Razor Enhanced.\nBeschreibung: Verhindert die visuelle und logische Verarbeitung bestimmter Grafik-IDs.\nRückgabe: Void\nParameter: itemIdList [PythonList]\nBeispiel:\n```python\nItems.IgnoreTypes(itemIdList)\n```"
    },
    {
        "user": "Items.Lift 是做什么的？",
        "assistant": "Items.Lift 是 Razor Enhanced 的方法。\n描述：创建逻辑拖动事件，将对象与光标关联。\n返回：Void\n参数：item [Item], amount [Int32]\n示例:\n```python\nItems.Lift(item, amount)\n```"
    },
    {
        "user": "Что делает Items.Message?",
        "assistant": "Items.Message это Метод в Razor Enhanced.\nОписание: Накладывает плавающий текст на объект, видимый только локально.\nВозврат: Void\nПараметры: item [Item/Int32], hue [Int32], message\nПример:\n```python\nItems.Message(item, hue, message)\n```"
    },
    {
        "user": "What does Items.Move do?",
        "assistant": "Items.Move is a Razor Enhanced Method.\nDescription: Generates the full move packet towards inventories or mobs.\nReturn: Void\nParameters: source [Item/Int32], destination [Item/Mobile/Int32], amount [Int32], x [Int32], y [Int32]\nExample:\n```python\nItems.Move(source, destination, amount, x, y)\n```"
    },
    {
        "user": "Cosa fa Items.MoveOnGround?",
        "assistant": "Items.MoveOnGround è un Metodo di Razor Enhanced.\nDescrizione: Sposta l'oggetto in specifiche coordinate spaziali XYZ tridimensionali.\nRitorno: Void\nParametri: source [Item/Int32], amount [Int32], x [Int32], y [Int32], z [Int32]\nEsempio:\n```python\nItems.MoveOnGround(source, amount, x, y, z)\n```"
    },
    {
        "user": "¿Qué hace Items.OpenAt?",
        "assistant": "Items.OpenAt es un Método de Razor Enhanced.\nDescripción: Fuerza la apertura gráfica en coordenadas absolutas de pantalla.\nRetorno: Void\nParámetros: serial [Item/Int32], x [Int32], y [Int32]\nEjemplo:\n```python\nItems.OpenAt(serial, x, y)\n```"
    }
]

with open('dataset_razor_API.jsonl', 'a', encoding='utf-8') as f:
    for record in records:
        entry = {
            "messages": [
                {"role": "system", "content": sys_prompt},
                {"role": "user", "content": record["user"]},
                {"role": "assistant", "content": record["assistant"]}
            ]
        }
        f.write(json.dumps(entry, ensure_ascii=False) + '\\n')
