import json
import os

sys_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

records = [
    {
        "user": "What does Item.IsChildOf do?",
        "assistant": "Item.IsChildOf is a Razor Enhanced Method.\nDescription: Determines logical inclusion by checking the branches of the hierarchical tree.\nReturn: Boolean\nParameters: container, maxDepth\nExample:\n```python\nresult = Item.IsChildOf(container, maxDepth)\n```"
    },
    {
        "user": "Cosa fa Mobiles.Filter.Blessed?",
        "assistant": "Mobiles.Filter.Blessed è una Proprietà di Razor Enhanced.\nDescrizione: Rileva lo stato di invulnerabilità conferito dal server.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobiles.Filter.Blessed\n```"
    },
    {
        "user": "¿Qué hace Mobiles.Filter.Bodies?",
        "assistant": "Mobiles.Filter.Bodies es una Propiedad de Razor Enhanced.\nDescripción: Limita a los Graphic ID específicos del cuerpo (ej. demonios o humanos).\nRetorno: List[Int32]\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobiles.Filter.Bodies\n```"
    },
    {
        "user": "Was macht Mobiles.Filter.CheckIgnoreObject?",
        "assistant": "Mobiles.Filter.CheckIgnoreObject ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Ignoriert die in der Blacklist eingetragenen Seriennummern.\nRückgabe: Boolean\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobiles.Filter.CheckIgnoreObject\n```"
    },
    {
        "user": "Mobiles.Filter.CheckLineOfSight 是做什么的？",
        "assistant": "Mobiles.Filter.CheckLineOfSight 是 Razor Enhanced 的属性。\n描述：查询几何引擎以确认没有视觉障碍。\n返回：Boolean\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobiles.Filter.CheckLineOfSight\n```"
    },
    {
        "user": "Что делает Mobiles.Filter.Enabled?",
        "assistant": "Mobiles.Filter.Enabled это Свойство в Razor Enhanced.\nОписание: Переключатель для применения фильтра к глобальным процедурам.\nВозврат: Boolean\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobiles.Filter.Enabled\n```"
    },
    {
        "user": "Could you explain Mobiles.Filter.Female?",
        "assistant": "Mobiles.Filter.Female is a Razor Enhanced Property.\nDescription: Anatomical parameter specific to the body.\nReturn: Int32\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobiles.Filter.Female\n```"
    },
    {
        "user": "Spiegami Mobiles.Filter.Friend.",
        "assistant": "Mobiles.Filter.Friend è una Proprietà di Razor Enhanced.\nDescrizione: Include esclusivamente entità presenti nella trust-list locale.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobiles.Filter.Friend\n```"
    },
    {
        "user": "¿Para qué sirve Mobiles.Filter.Graphics?",
        "assistant": "Mobiles.Filter.Graphics es una Propiedad de Razor Enhanced.\nDescripción: Alias funcional para los Bodies ID.\nRetorno: List[Int32]\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobiles.Filter.Graphics\n```"
    },
    {
        "user": "Kannst du Mobiles.Filter.Hues erklären?",
        "assistant": "Mobiles.Filter.Hues ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Nützlich, um chromatisch generierte Proben zu identifizieren, die von der Basis abweichen.\nRückgabe: List[Int32]\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobiles.Filter.Hues\n```"
    },
    {
        "user": "请解释一下 Mobiles.Filter.IgnorePets。",
        "assistant": "Mobiles.Filter.IgnorePets 是 Razor Enhanced 的属性。\n描述：将自己或他人的召唤物从敌对目标池中移除。\n返回：Boolean\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobiles.Filter.IgnorePets\n```"
    },
    {
        "user": "Объясни Mobiles.Filter.IsGhost.",
        "assistant": "Mobiles.Filter.IsGhost это Свойство в Razor Enhanced.\nОписание: Идентифицирует ID тел, соответствующие состояниям смерти, для воскрешения союзников.\nВозврат: Int32\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobiles.Filter.IsGhost\n```"
    },
    {
        "user": "What is Mobiles.Filter.IsHuman used for?",
        "assistant": "Mobiles.Filter.IsHuman is a Razor Enhanced Property.\nDescription: Limits to humanoid rendering classes.\nReturn: Int32\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobiles.Filter.IsHuman\n```"
    },
    {
        "user": "Qual è lo scopo di Mobiles.Filter.Name?",
        "assistant": "Mobiles.Filter.Name è una Proprietà di Razor Enhanced.\nDescrizione: Matching testuale per la ricerca rapida di NPC nominati.\nRitorno: String\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobiles.Filter.Name\n```"
    },
    {
        "user": "Explica Mobiles.Filter.Notorieties.",
        "assistant": "Mobiles.Filter.Notorieties es una Propiedad de Razor Enhanced.\nDescripción: Sistema de clasificación esencial (1-7) de las reglas de combate.\nRetorno: List\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobiles.Filter.Notorieties\n```"
    },
    {
        "user": "Wofür wird Mobiles.Filter.Paralized verwendet?",
        "assistant": "Mobiles.Filter.Paralized ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Identifiziert Entitäten, die bewegungseinschränkenden Effekten unterliegen.\nRückgabe: Int32\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobiles.Filter.Paralized\n```"
    },
    {
        "user": "Mobiles.Filter.Poisoned 有什么用？",
        "assistant": "Mobiles.Filter.Poisoned 是 Razor Enhanced 的属性。\n描述：通过分析生命值的颜色标题来检测中毒状态。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobiles.Filter.Poisoned\n```"
    },
    {
        "user": "Для чего используется Mobiles.Filter.RangeMax?",
        "assistant": "Mobiles.Filter.RangeMax это Свойство в Razor Enhanced.\nОписание: Максимальный геометрический радиус, в котором сущности считаются действительными.\nВозврат: Double\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobiles.Filter.RangeMax\n```"
    },
    {
        "user": "Explain the purpose of Mobiles.Filter.RangeMin.",
        "assistant": "Mobiles.Filter.RangeMin is a Razor Enhanced Property.\nDescription: Minimum distance offset, useful for ignoring adjacent enemies (melee range).\nReturn: Double\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobiles.Filter.RangeMin\n```"
    },
    {
        "user": "A cosa serve Mobiles.Filter.Serials?",
        "assistant": "Mobiles.Filter.Serials è una Proprietà di Razor Enhanced.\nDescrizione: Hardcoding di identificatori di rete specifici.\nRitorno: List[Int32]\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobiles.Filter.Serials\n```"
    },
    {
        "user": "¿Cuál es la función de Mobiles.Filter.Warmode?",
        "assistant": "Mobiles.Filter.Warmode es una Propiedad de Razor Enhanced.\nDescripción: Discrimina entidades en postura pacífica de aquellas en estado de alerta o combate.\nRetorno: Int32\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobiles.Filter.Warmode\n```"
    },
    {
        "user": "Was ist der Zweck von Mobiles.Filter.ZLevelMax?",
        "assistant": "Mobiles.Filter.ZLevelMax ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Oberer Schnitt der vertikalen Koordinaten für die dreidimensionale Raumgestaltung.\nRückgabe: Double\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobiles.Filter.ZLevelMax\n```"
    },
    {
        "user": "解释一下 Mobiles.Filter.ZLevelMin。",
        "assistant": "Mobiles.Filter.ZLevelMin 是 Razor Enhanced 的属性。\n描述：Z坐标的下限。\n返回：Double\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobiles.Filter.ZLevelMin\n```"
    },
    {
        "user": "Что делает Mobiles.ApplyFilter?",
        "assistant": "Mobiles.ApplyFilter это Метод в Razor Enhanced.\nОписание: Генерирует разрешенный набор сущностей в точный момент времени.\nВозврат: List[Mobile]\nПараметры: filter\nПример:\n```python\nresult = Mobiles.ApplyFilter(filter)\n```"
    },
    {
        "user": "How does Mobiles.ContextExist work?",
        "assistant": "Mobiles.ContextExist is a Razor Enhanced Method.\nDescription: Asynchronously requests and examines context menu nodes.\nReturn: Int32\nParameters: mob, name, showContext\nExample:\n```python\nresult = Mobiles.ContextExist(mob, name, showContext)\n```"
    },
    {
        "user": "Descrivimi Mobiles.FindBySerial.",
        "assistant": "Mobiles.FindBySerial è un Metodo di Razor Enhanced.\nDescrizione: Interroga direttamente l'hash table locale per recuperare l'istanza univoca.\nRitorno: Mobile\nParametri: serial\nEsempio:\n```python\nresult = Mobiles.FindBySerial(serial)\n```"
    },
    {
        "user": "¿Qué hace Mobiles.FindMobile?",
        "assistant": "Mobiles.FindMobile es un Método de Razor Enhanced.\nDescripción: Método rápido y no asignativo para la generación rápida de objetivos.\nRetorno: Mobile\nParámetros: graphic, notoriety, rangemax, selector, highlight\nEjemplo:\n```python\nresult = Mobiles.FindMobile(graphic, notoriety, rangemax, selector, highlight)\n```"
    },
    {
        "user": "Was macht Mobiles.GetPropStringByIndex?",
        "assistant": "Mobiles.GetPropStringByIndex ist eine Methode von Razor Enhanced.\nBeschreibung: Navigiert durch das Array von Strings, das über den cliloc Tooltip für die Entität gesendet wird.\nRückgabe: String\nParameter: serial, index\nBeispiel:\n```python\nresult = Mobiles.GetPropStringByIndex(serial, index)\n```"
    },
    {
        "user": "Mobiles.GetPropStringList 的功能是什么？",
        "assistant": "Mobiles.GetPropStringList 是 Razor Enhanced 的方法。\n描述：返回目标描述性元数据的完整转储。\n返回：List\n参数：serial\n示例:\n```python\nresult = Mobiles.GetPropStringList(serial)\n```"
    },
    {
        "user": "Объясни работу Mobiles.GetPropValue.",
        "assistant": "Mobiles.GetPropValue это Метод в Razor Enhanced.\nОписание: Переводит текстовые метки в значения float для логических оценок.\nВозврат: Single\nПараметры: serial, name\nПример:\n```python\nresult = Mobiles.GetPropValue(serial, name)\n```"
    },
    {
        "user": "What is Mobiles.GetTargetingFilter used for?",
        "assistant": "Mobiles.GetTargetingFilter is a Razor Enhanced Method.\nDescription: Recalls a complex filter object preconfigured by the user from the GUI.\nReturn: Mobiles.Filter\nParameters: target_name\nExample:\n```python\nresult = Mobiles.GetTargetingFilter(target_name)\n```"
    },
    {
        "user": "A cosa serve Mobiles.GetTrackingInfo?",
        "assistant": "Mobiles.GetTrackingInfo è un Metodo di Razor Enhanced.\nDescrizione: Estrae i dati inviati dalla skill Tracking decodificando i pacchetti radar di direzione.\nRitorno: Mobiles.TrackingInfo\nParametri: N/A\nEsempio:\n```python\nresult = Mobiles.GetTrackingInfo()\n```"
    },
    {
        "user": "¿Para qué sirve Mobiles.Message?",
        "assistant": "Mobiles.Message es un Método de Razor Enhanced.\nDescripción: Genera un mensaje de texto flotante anclado al offset gráfico de la entidad.\nRetorno: Void\nParámetros: mobile, hue, message, wait\nEjemplo:\n```python\nMobiles.Message(mobile, hue, message, wait)\n```"
    },
    {
        "user": "Kannst du Mobiles.Select erklären?",
        "assistant": "Mobiles.Select ist eine Methode von Razor Enhanced.\nBeschreibung: Verarbeitet eine begrenzte Teilmenge durch Anwenden eines zusätzlichen Namensfilters.\nRückgabe: Mobile\nParameter: mobiles, selector\nBeispiel:\n```python\nresult = Mobiles.Select(mobiles, selector)\n```"
    },
    {
        "user": "请解释 Mobiles.SingleClick。",
        "assistant": "Mobiles.SingleClick 是 Razor Enhanced 的方法。\n描述：通过路由主要检查数据包来强制更新基础状态。\n返回：Void\n参数：mobile\n示例:\n```python\nMobiles.SingleClick(mobile)\n```"
    },
    {
        "user": "Для чего нужен Mobiles.UseMobile?",
        "assistant": "Mobiles.UseMobile это Метод в Razor Enhanced.\nОписание: Вызывает прямое взаимодействие (открытие вендора, базовая атака на фришарде, посадка на ездовое животное).\nВозврат: Void\nПараметры: mobile\nПример:\n```python\nMobiles.UseMobile(mobile)\n```"
    },
    {
        "user": "Explain Mobiles.WaitForProps.",
        "assistant": "Mobiles.WaitForProps is a Razor Enhanced Method.\nDescription: Freezes execution waiting for the properties packet for the specified serial to arrive.\nReturn: Boolean\nParameters: m, delay\nExample:\n```python\nresult = Mobiles.WaitForProps(m, delay)\n```"
    },
    {
        "user": "Qual è lo scopo di Mobiles.WaitForStats?",
        "assistant": "Mobiles.WaitForStats è un Metodo di Razor Enhanced.\nDescrizione: Sospende il thread fintanto che l'aggiornamento strutturale (salute/mana) non viene confermato.\nRitorno: Boolean\nParametri: m, delay\nEsempio:\n```python\nresult = Mobiles.WaitForStats(m, delay)\n```"
    },
    {
        "user": "¿Qué indica Mobile.Backpack?",
        "assistant": "Mobile.Backpack es una Propiedad de Razor Enhanced.\nDescripción: Enlace relacional al objeto contenedor asociado a la entidad.\nRetorno: Item\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobile.Backpack\n```"
    },
    {
        "user": "Was macht Mobile.CanRename?",
        "assistant": "Mobile.CanRename ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Bestimmt, ob das Protokoll das Privileg zur Namensänderung gewährt (z. B. Familiar oder Pet).\nRückgabe: Boolean\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobile.CanRename\n```"
    },
    {
        "user": "Mobile.Color 是什么意思？",
        "assistant": "Mobile.Color 是 Razor Enhanced 的属性。\n描述：身体基础色素沉着的值。\n返回：UInt16\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobile.Color\n```"
    },
    {
        "user": "Что делает Mobile.Contains?",
        "assistant": "Mobile.Contains это Свойство в Razor Enhanced.\nОписание: Разрешает все дочерние узлы, представляя надетую экипировку (Paperdoll).\nВозврат: List[Item]\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobile.Contains\n```"
    },
    {
        "user": "What does Mobile.Deleted represent?",
        "assistant": "Mobile.Deleted is a Razor Enhanced Property.\nDescription: Verifies if the server has communicated the cancellation of existence in memory.\nReturn: Boolean\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobile.Deleted\n```"
    },
    {
        "user": "Spiegami Mobile.Direction.",
        "assistant": "Mobile.Direction è una Proprietà di Razor Enhanced.\nDescrizione: Codifica la direttrice del movimento spaziale in 8 angolazioni canoniche.\nRitorno: String\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobile.Direction\n```"
    },
    {
        "user": "¿Qué es Mobile.Fame?",
        "assistant": "Mobile.Fame es una Propiedad de Razor Enhanced.\nDescripción: Estimación heurística deducida del título nobiliario comunicado en broadcast.\nRetorno: Int32\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobile.Fame\n```"
    },
    {
        "user": "Wofür steht Mobile.Female?",
        "assistant": "Mobile.Female ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Gibt das Geschlecht des Körpers an, nützlich für die Kompatibilität der Ausrüstung.\nRückgabe: Boolean\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobile.Female\n```"
    },
    {
        "user": "解释一下 Mobile.Flying。",
        "assistant": "Mobile.Flying 是 Razor Enhanced 的属性。\n描述：与绕过地质障碍相关的高级机动性参数。\n返回：Boolean\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobile.Flying\n```"
    },
    {
        "user": "Что возвращает Mobile.Graphics?",
        "assistant": "Mobile.Graphics это Свойство в Razor Enhanced.\nОписание: ID рендеринга полигональной или 2D модели (MobileID).\nВозврат: UInt16\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobile.Graphics\n```"
    },
    {
        "user": "What is Mobile.Hits?",
        "assistant": "Mobile.Hits is a Razor Enhanced Property.\nDescription: Current vital parameter. For non-allied entities, it is often expressed as a percentage.\nReturn: Int32\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobile.Hits\n```"
    },
    {
        "user": "A cosa serve Mobile.HitsMax?",
        "assistant": "Mobile.HitsMax è una Proprietà di Razor Enhanced.\nDescrizione: Calibratore per convertire la proporzione della salute in valori assoluti se conosciuto.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobile.HitsMax\n```"
    },
    {
        "user": "¿Para qué sirve Mobile.Hue?",
        "assistant": "Mobile.Hue es una Propiedad de Razor Enhanced.\nDescripción: Propiedad homóloga a Color.\nRetorno: UInt16\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobile.Hue\n```"
    }
]

file_path = 'dataset_razor_API.jsonl'
with open(file_path, 'a', encoding='utf-8') as f:
    for record in records:
        json_obj = {
            "messages": [
                {"role": "system", "content": sys_prompt},
                {"role": "user", "content": record["user"]},
                {"role": "assistant", "content": record["assistant"]}
            ]
        }
        f.write(json.dumps(json_obj, ensure_ascii=False) + '\n')

print(f"Successfully appended {len(records)} entries to {file_path}")