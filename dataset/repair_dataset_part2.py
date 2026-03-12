import json
import re

with open('dataset.jsonl', 'r', encoding='utf-8') as f:
    content = f.read()

parts = content.split('{"messages":')

for part in parts:
    if not part.strip():
        continue
    obj_str = '{"messages":' + part
    obj_str = obj_str.strip()
    obj_str = obj_str.replace('\n', '\\n')
    
    try:
        obj = json.loads(obj_str)
    except Exception as e:
        print(f"Error parsing: {e}")
        # Find where it failed
        # e.g., "Extra data: line 1 column 1174 (char 1173)"
        print("STRING DUMP:")
        print(obj_str)
        break
