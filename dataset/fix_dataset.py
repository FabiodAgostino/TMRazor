import json
import re

def detect_language(text):
    if re.search(r'[\u4e00-\u9fff]', text): return 'Chinese'
    if re.search(r'[\u0400-\u04ff]', text): return 'Russian'
    text_lower = text.lower()
    
    def has_word(words, text):
        for word in words:
            if re.search(r'\b' + re.escape(word) + r'\b', text): return True
        return False

    if has_word(['bevi', 'cura', 'vita', 'sotto', 'scende', 'gioco', 'oggetto', 'leggo', 'controlla', 'se', 'apri', 'trova', 'usa', 'scrivi', 'crea', 'bende', 'invia'], text_lower):
        return 'Italian'
    if has_word(['trinke', 'stärketrank', 'weniger', 'warne', 'backpackcount', 'anzahl', 'meinem', 'rucksack', 'schreibe', 'erstelle', 'überprüfe', 'benutze', 'wirf', 'zähler', 'zählen', 'zähle'], text_lower):
        return 'German'
    if has_word(['si', 'el', 'peso', 'suelta', 'tengo', 'lánzalo', 'muestrame', 'posición', 'actual', 'escribe', 'comprueba', 'lanza', 'abre', 'encuentra', 'vida'], text_lower):
        return 'Spanish'
    
    return 'English'

def fix_mojibake(text):
    # Try hybrid encoding (cp1252 with latin-1 fallback for undefined bytes like 0x81)
    b = bytearray()
    for c in text:
        try:
            b.extend(c.encode('cp1252'))
        except:
            if ord(c) < 256:
                b.append(ord(c))
            else:
                return text # if we can't map it to a single byte, it's not our specific mojibake
    try:
        repaired = b.decode('utf-8')
        if repaired != text:
            return repaired
    except:
        pass
    return text

def extract_apis(text):
    apis = re.findall(r'\b[A-Z][a-zA-Z0-9_]*\.[A-Z][a-zA-Z0-9_]*\b', text)
    return sorted(list(set(apis)))

def extract_ids(text):
    return sorted(list(set(re.findall(r'0x[0-9A-Fa-f]+', text))))

replacements = {
    'Gumps.LastGumpID': 'Gumps.CurrentGump',
    'Items.FindCount(0x0E21, -1, Player.Backpack.Serial, True)': 'Items.ContainerCount(Player.Backpack.Serial, 0x0E21, -1, True)',
    'Items.FindCount': 'Items.ContainerCount',
    'Misc.SendPrompt': 'Misc.ResponsePrompt',
    'Spells.CastNecromancy': 'Spells.CastNecro',
    'Player.HasBuff': 'Player.BuffsExist',
    'Items.MoveToGround': 'Items.MoveOnGround',
    'Player.WarMode = True': 'Player.SetWarMode(True)',
    'Player.WarMode = False': 'Player.SetWarMode(False)',
}

input_path = 'dataset.jsonl'
output_path = 'dataset.jsonl'
processed_lines = []

with open(input_path, 'rb') as f:
    for line in f:
        if not line.strip(): continue
        try:
            content = line.decode('utf-8')
        except UnicodeDecodeError:
            content = line.decode('latin-1')
        
        try:
            data = json.loads(content)
        except:
            continue
            
        messages = data.get('messages', [])
        
        user_content = ''
        for msg in messages:
            msg['content'] = fix_mojibake(msg['content'])
            if msg['role'] == 'user':
                user_content = msg['content']
                
        for msg in messages:
            if msg['role'] == 'assistant':
                code = msg['content']
                if '</think>' in code:
                    code = code.split('</think>')[-1].strip()
                else:
                    code = code.strip()
                    
                for old, new in replacements.items():
                    code = code.replace(old, new)
                
                lang = detect_language(user_content)
                apis = extract_apis(code)
                ids = extract_ids(user_content + ' ' + code)
                
                valid_prefixes = ['Misc', 'Player', 'Items', 'Item', 'Mobiles', 'Mobile', 'Target', 'Spells', 'Gumps', 'Journal', 'PathFinding', 'AutoLoot', 'Dress', 'BandageHeal', 'Trade', 'Party', 'Sound', 'Statics']
                clean_apis = []
                for api in apis:
                    if any(api.startswith(p + '.') for p in valid_prefixes):
                        clean_apis.append(api)
                
                think_content = f'User speaks {lang}. APIs: {", ".join(clean_apis)}. IDs: {", ".join(ids)}.'
                msg['content'] = f'<think>{think_content}</think>\n{code}'

        processed_lines.append(json.dumps(data, ensure_ascii=False))

with open(output_path, 'w', encoding='utf-8') as f:
    for line in processed_lines:
        f.write(line + '\n')

print(f'Processed {len(processed_lines)} lines.')