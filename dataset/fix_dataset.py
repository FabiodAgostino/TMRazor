import json
import re

def fix_dataset():
    with open('dataset_razor_API.jsonl', 'r', encoding='utf-8') as f:
        content = f.read()

    # The file might have literal '\n' separating objects, or they might just be smashed together
    # like {"messages":...}\n{"messages":...} where \n is a literal backslash n, 
    # or actual newlines but with some objects containing \n.
    
    # A robust way is to use regex to find all objects starting with {"messages":
    # But we need to make sure we extract valid JSON strings.
    # Actually, if we just split by '{"messages"' we can reconstruct them.
    
    objects = []
    parts = content.split('{"messages"')
    for p in parts[1:]:
        s = '{"messages"' + p
        # sometimes they might have trailing characters like literal '\n' or newlines
        s = s.strip()
        # strip literal \n if it was appended as string
        if s.endswith('\\n'):
            s = s[:-2]
        
        try:
            obj = json.loads(s)
            objects.append(obj)
        except json.JSONDecodeError as e:
            print(f"Failed to parse object: {s[:50]}... Error: {e}")

    print(f"Successfully extracted {len(objects)} JSON objects.")
    
    # Let's count how many we actually need and if there are duplicates
    unique_objects = []
    seen = set()
    for obj in objects:
        key = obj['messages'][1]['content'] # user prompt
        if key not in seen:
            seen.add(key)
            unique_objects.append(obj)
            
    print(f"Unique objects: {len(unique_objects)}")

    with open('dataset_razor_API_fixed.jsonl', 'w', encoding='utf-8') as f:
        for obj in unique_objects:
            f.write(json.dumps(obj, ensure_ascii=False) + '\n')

fix_dataset()
