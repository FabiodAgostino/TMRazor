import json

sys_prompt = "You are a Razor Enhanced scripting assistant for Ultima Online. You write correct IronPython scripts using only the official Razor Enhanced API. Never invent or guess methods, properties, or classes. If a task cannot be done with the available API, say so explicitly. Respond in the same language as the user."

records = [
    {
        "user": "What does Mobile.Hue do?",
        "assistant": "Mobile.Hue is a Razor Enhanced Property.\nDescription: Homologous property to Color.\nReturn: UInt16\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobile.Hue\n```"
    },
    {
        "user": "Spiegami Mobile.InParty.",
        "assistant": "Mobile.InParty è una Proprietà di Razor Enhanced.\nDescrizione: Riconoscimento immediato per evitare fuoco amico su membri alleati condivisi.\nRitorno: Boolean\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobile.InParty\n```"
    },
    {
        "user": "¿Para qué sirve Mobile.IsGhost?",
        "assistant": "Mobile.IsGhost es una Propiedad de Razor Enhanced.\nDescripción: Valida la pertenencia al estado de muerte mediante la inspección del ID gráfico.\nRetorno: Boolean\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobile.IsGhost\n```"
    },
    {
        "user": "Was macht Mobile.IsHuman?",
        "assistant": "Mobile.IsHuman ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Boolesche Auswertung von bodyIDs, die mit menschlicher Kleidung kompatibel sind (183, 400, usw.).\nRückgabe: Boolean\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobile.IsHuman\n```"
    },
    {
        "user": "解释一下 Mobile.ItemID。",
        "assistant": "Mobile.ItemID 是 Razor Enhanced 的属性。\n描述：用于向后兼容检查的技术参数。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobile.ItemID\n```"
    },
    {
        "user": "Что делает Mobile.Karma?",
        "assistant": "Mobile.Karma это Свойство в Razor Enhanced.\nОписание: Этическая переменная, оцениваемая по имени (-5 злой, +5 добродетельный).\nВозврат: Int32\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobile.Karma\n```"
    },
    {
        "user": "Could you explain Mobile.KarmaTitle?",
        "assistant": "Mobile.KarmaTitle is a Razor Enhanced Property.\nDescription: Preserves the original prefix/suffix received without processing.\nReturn: String\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobile.KarmaTitle\n```"
    },
    {
        "user": "A cosa serve Mobile.Mana?",
        "assistant": "Mobile.Mana è una Proprietà di Razor Enhanced.\nDescrizione: Carica magica corrente (la risoluzione dipende dai privilegi di rete).\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobile.Mana\n```"
    },
    {
        "user": "¿Cuál es la función de Mobile.ManaMax?",
        "assistant": "Mobile.ManaMax es una Propiedad de Razor Enhanced.\nDescripción: Umbral límite del maná asociado a la entidad.\nRetorno: Int32\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobile.ManaMax\n```"
    },
    {
        "user": "Kannst du Mobile.Map erklären?",
        "assistant": "Mobile.Map ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Identifikator der territorialen Domäne (Facet), in der die Verfolgung stattfindet.\nRückgabe: Int32\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobile.Map\n```"
    },
    {
        "user": "Mobile.MobileID 的功能是什么？",
        "assistant": "Mobile.MobileID 是 Razor Enhanced 的属性。\n描述：怪物或玩家创建模板的基础 ID。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobile.MobileID\n```"
    },
    {
        "user": "Объясни работу Mobile.Mount.",
        "assistant": "Mobile.Mount это Свойство в Razor Enhanced.\nОписание: Перекрестная ссылка для изоляции серийного номера интерактивного скакуна.\nВозврат: Item\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobile.Mount\n```"
    },
    {
        "user": "What does Mobile.Name do?",
        "assistant": "Mobile.Name is a Razor Enhanced Property.\nDescription: Base value of the name, split from guild title and lord prefix.\nReturn: String\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobile.Name\n```"
    },
    {
        "user": "Descrivimi Mobile.Notoriety.",
        "assistant": "Mobile.Notoriety è una Proprietà di Razor Enhanced.\nDescrizione: Indice cardine (1-7) per definire l'ostilità logica riconosciuta dal client di base.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobile.Notoriety\n```"
    },
    {
        "user": "Explica Mobile.Paralized.",
        "assistant": "Mobile.Paralized es una Propiedad de Razor Enhanced.\nDescripción: Señala la presencia de freeze dictado por el servidor (aturdimientos o hechizos).\nRetorno: Boolean\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobile.Paralized\n```"
    },
    {
        "user": "Wofür wird Mobile.Poisoned verwendet?",
        "assistant": "Mobile.Poisoned ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Flag, das vom Zustand der Lebensanzeige abgeleitet wird (grüner/oranger Balken).\nRückgabe: Boolean\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobile.Poisoned\n```"
    },
    {
        "user": "请解释 Mobile.Position。",
        "assistant": "Mobile.Position 是 Razor Enhanced 的属性。\n描述：用于当前几何位置表示的复杂对象。\n返回：Point3D\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobile.Position\n```"
    },
    {
        "user": "Для чего используется Mobile.Properties?",
        "assistant": "Mobile.Properties это Свойство в Razor Enhanced.\nОписание: Полный текстовый анализ, структурированный по строкам и производным значениям.\nВозврат: List[Property]\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobile.Properties\n```"
    },
    {
        "user": "Explain the purpose of Mobile.PropsUpdated.",
        "assistant": "Mobile.PropsUpdated is a Razor Enhanced Property.\nDescription: Verifies the alignment of the cache with the most recent packets.\nReturn: Boolean\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobile.PropsUpdated\n```"
    },
    {
        "user": "Qual è lo scopo di Mobile.Quiver?",
        "assistant": "Mobile.Quiver è una Proprietà di Razor Enhanced.\nDescrizione: Individua il nodo inventario dedicato ai proiettili equipaggiato sull'entità.\nRitorno: Item\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobile.Quiver\n```"
    },
    {
        "user": "¿Qué indica Mobile.Serial?",
        "assistant": "Mobile.Serial es una Propiedad de Razor Enhanced.\nDescripción: Piedra angular para cualquier iteración segura entre script y memoria.\nRetorno: Int32\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobile.Serial\n```"
    },
    {
        "user": "Was ist der Zweck von Mobile.Stam?",
        "assistant": "Mobile.Stam ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Widerstand gegen Anstrengung und aktuelle Angriffsfrequenz.\nRückgabe: Int32\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobile.Stam\n```"
    },
    {
        "user": "Mobile.StamMax 是什么意思？",
        "assistant": "Mobile.StamMax 是 Razor Enhanced 的属性。\n描述：最大耐力池。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Mobile.StamMax\n```"
    },
    {
        "user": "Что означает Mobile.Visible?",
        "assistant": "Mobile.Visible это Свойство в Razor Enhanced.\nОписание: Подтверждает существование в видимом спектре, противодействуя скрытности или невидимости.\nВозврат: Boolean\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Mobile.Visible\n```"
    },
    {
        "user": "What does Mobile.WarMode represent?",
        "assistant": "Mobile.WarMode is a Razor Enhanced Property.\nDescription: Reflects the combat stance, which modifies collisions and push interactions.\nReturn: Boolean\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobile.WarMode\n```"
    },
    {
        "user": "Spiegami Mobile.YellowHits.",
        "assistant": "Mobile.YellowHits è una Proprietà di Razor Enhanced.\nDescrizione: Rileva alterazioni della barra HP indicative di debuff che prevengono le cure (Mortal Strike).\nRitorno: Boolean\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobile.YellowHits\n```"
    },
    {
        "user": "¿Qué hace Mobile.DistanceTo?",
        "assistant": "Mobile.DistanceTo es un Método de Razor Enhanced.\nDescripción: Realiza el cálculo euclidiano reducido entre el vector actual y la entidad objetivo.\nRetorno: Int32\nParámetros: other_mobile\nEjemplo:\n```python\nresult = Mobile.DistanceTo(other_mobile)\n```"
    },
    {
        "user": "Was macht Mobile.Equals?",
        "assistant": "Mobile.Equals ist eine Methode von Razor Enhanced.\nBeschreibung: Vergleicht Basisreferenzen auf C#-Ebene, um Duplikate zu identifizieren.\nRückgabe: Boolean\nParameter: obj\nBeispiel:\n```python\nresult = Mobile.Equals(obj)\n```"
    },
    {
        "user": "Mobile.GetItemOnLayer 有什么用？",
        "assistant": "Mobile.GetItemOnLayer 是 Razor Enhanced 的方法。\n描述：返回指向在已定义插槽中装备的对象的指针。\n返回：Item\n参数：layer\n示例:\n```python\nresult = Mobile.GetItemOnLayer(layer)\n```"
    },
    {
        "user": "Для чего нужен Mobile.UpdateKarma?",
        "assistant": "Mobile.UpdateKarma это Метод в Razor Enhanced.\nОписание: Принудительно пересчитывает и обновляет этические титулы, блокируя асинхронное выполнение для ожидания ответа.\nВозврат: Boolean\nПараметры: N/A\nПример:\n```python\nresult = Mobile.UpdateKarma()\n```"
    },
    {
        "user": "Explain Mobiles.TrackingInfo.lastUpdate.",
        "assistant": "Mobiles.TrackingInfo.lastUpdate is a Razor Enhanced Property.\nDescription: Native timestamp of the last spatial radar reception.\nReturn: DateTime\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Mobiles.TrackingInfo.lastUpdate\n```"
    },
    {
        "user": "A cosa serve Mobiles.TrackingInfo.serial?",
        "assistant": "Mobiles.TrackingInfo.serial è una Proprietà di Razor Enhanced.\nDescrizione: Identificativo del bersaglio rilevato fuori dallo schermo.\nRitorno: UInt32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Mobiles.TrackingInfo.serial\n```"
    },
    {
        "user": "¿Cuál es la función de Mobiles.TrackingInfo.x?",
        "assistant": "Mobiles.TrackingInfo.x es una Propiedad de Razor Enhanced.\nDescripción: Coordenada X cruda estimada por la habilidad.\nRetorno: UInt16\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Mobiles.TrackingInfo.x\n```"
    },
    {
        "user": "Kannst du Mobiles.TrackingInfo.y erklären?",
        "assistant": "Mobiles.TrackingInfo.y ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Rohe Y-Koordinate.\nRückgabe: UInt16\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Mobiles.TrackingInfo.y\n```"
    },
    {
        "user": "解释一下 Player.AR。",
        "assistant": "Player.AR 是 Razor Enhanced 的属性。\n描述：累积物理护甲等级的测量值。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Player.AR\n```"
    },
    {
        "user": "Что делает Player.Backpack?",
        "assistant": "Player.Backpack это Свойство в Razor Enhanced.\nОписание: Корневой узел для операций с лутом и личным инвентарем.\nВозврат: Item\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Player.Backpack\n```"
    },
    {
        "user": "What does Player.Bank do?",
        "assistant": "Player.Bank is a Razor Enhanced Property.\nDescription: Secure reference to the safe instance managed by the server.\nReturn: Item\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Player.Bank\n```"
    },
    {
        "user": "Descrivimi Player.Body.",
        "assistant": "Player.Body è una Proprietà di Razor Enhanced.\nDescrizione: Forma attuale, riflettendo incantesimi di polimorfismo o forme necromantiche.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Player.Body\n```"
    },
    {
        "user": "Explica Player.Buffs.",
        "assistant": "Player.Buffs es una Propiedad de Razor Enhanced.\nDescripción: Vector textual en tiempo real de todas las auras, penalizaciones y alteraciones lógicas.\nRetorno: List\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Player.Buffs\n```"
    },
    {
        "user": "Wofür wird Player.BuffsInfo verwendet?",
        "assistant": "Player.BuffsInfo ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Strukturiertes technisches Detail für jeden Buff, einschließlich der verbleibenden Dauer in C# skaliert.\nRückgabe: List\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Player.BuffsInfo\n```"
    },
    {
        "user": "请解释 Player.ColdResistance。",
        "assistant": "Player.ColdResistance 是 Razor Enhanced 的属性。\n描述：对抗寒冷伤害的原始减免量。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Player.ColdResistance\n```"
    },
    {
        "user": "Для чего используется Player.Connected?",
        "assistant": "Player.Connected это Свойство в Razor Enhanced.\nОписание: Состояние целостности потока сокетов между клиентом и игровым сервером.\nВозврат: Boolean\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Player.Connected\n```"
    },
    {
        "user": "Explain the purpose of Player.Corpses.",
        "assistant": "Player.Corpses is a Razor Enhanced Property.\nDescription: List of positions of the player's previous deaths for quick recovery.\nReturn: HashSet[Item]\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Player.Corpses\n```"
    },
    {
        "user": "A cosa serve Player.DamageChanceIncrease?",
        "assistant": "Player.DamageChanceIncrease è una Proprietà di Razor Enhanced.\nDescrizione: Incremento percentuale DCI calcolato sull'equipaggiamento indossato.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Player.DamageChanceIncrease\n```"
    },
    {
        "user": "¿Qué indica Player.DefenseChanceIncrease?",
        "assistant": "Player.DefenseChanceIncrease es una Propiedad de Razor Enhanced.\nDescripción: Modificador defensivo HCI total.\nRetorno: Int32\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Player.DefenseChanceIncrease\n```"
    },
    {
        "user": "Was ist der Zweck von Player.Dex?",
        "assistant": "Player.Dex ist eine Eigenschaft von Razor Enhanced.\nBeschreibung: Nominaler Geschicklichkeitswert.\nRückgabe: Int32\nParameter: N/A\nBeispiel:\n```python\n# Dies ist eine Eigenschaft, keine Methode\nresult = Player.Dex\n```"
    },
    {
        "user": "Player.DexterityIncrease 是什么意思？",
        "assistant": "Player.DexterityIncrease 是 Razor Enhanced 的属性。\n描述：由戒指和神器提供的敏捷加成的总和。\n返回：Int32\n参数：N/A\n示例:\n```python\n# 这是一个属性，不是一个方法\nresult = Player.DexterityIncrease\n```"
    },
    {
        "user": "Что означает Player.Direction?",
        "assistant": "Player.Direction это Свойство в Razor Enhanced.\nОписание: Текст, кодирующий фронтальное направление аватара (North, South, и т.д.).\nВозврат: String\nПараметры: N/A\nПример:\n```python\n# Это свойство, а не метод\nresult = Player.Direction\n```"
    },
    {
        "user": "What does Player.EnergyResistance represent?",
        "assistant": "Player.EnergyResistance is a Razor Enhanced Property.\nDescription: Absolute tolerance to energy or magnetic damage.\nReturn: Int32\nParameters: N/A\nExample:\n```python\n# This is a property, not a method\nresult = Player.EnergyResistance\n```"
    },
    {
        "user": "Spiegami Player.EnhancePotions.",
        "assistant": "Player.EnhancePotions è una Proprietà di Razor Enhanced.\nDescrizione: Modificatore percentuale per il fattore di moltiplicazione delle cure chimiche.\nRitorno: Int32\nParametri: N/A\nEsempio:\n```python\n# Questa è una proprietà, non un metodo\nresult = Player.EnhancePotions\n```"
    },
    {
        "user": "¿Qué hace Player.Fame?",
        "assistant": "Player.Fame es una Propiedad de Razor Enhanced.\nDescripción: Reputación social deducida por mecánicas de muerte de NPC.\nRetorno: Int32\nParámetros: N/A\nEjemplo:\n```python\n# Esta es una propiedad, no un método\nresult = Player.Fame\n```"
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
        f.write(json.dumps(json_obj, ensure_ascii=False) + '\\n')

print(f"Successfully appended {len(records)} entries to {file_path}")
