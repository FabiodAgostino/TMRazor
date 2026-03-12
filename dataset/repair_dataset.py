import json
import re

with open('dataset.jsonl', 'r', encoding='utf-8') as f:
    content = f.read()

# The delimiter is '{"messages": ' or '{"messages":'
# Let's split using a regex to handle spacing, but we know it's EXACTLY '{"messages": [{"role": "system"'
parts = content.split('{"messages":')

valid_jsonl = []
for part in parts:
    if not part.strip():
        continue
    # reconstruct the string
    obj_str = '{"messages":' + part
    # Now obj_str is an almost complete JSON string, but it has real newlines where it shouldn't,
    # and it might end with a real newline separating it from the next object.
    # Actually, the trailing newline might belong to this object or separating it.
    obj_str = obj_str.strip() # Remove leading/trailing real newlines
    
    # Inside obj_str, real newlines need to be replaced with the literal string '\n'
    # EXCEPT we need to be careful. Let's just replace all real newlines with '\\n'
    obj_str = obj_str.replace('\n', '\\n')
    
    # Let's test if it parses
    try:
        obj = json.loads(obj_str)
        # Re-dump to ensure it's a tight JSON string on one line
        valid_jsonl.append(json.dumps(obj, ensure_ascii=False))
    except Exception as e:
        print(f"Error parsing: {obj_str[:50]}... -> {e}")

with open('dataset.jsonl', 'w', encoding='utf-8') as f:
    for line in valid_jsonl:
        f.write(line + '\n')

print(f"Repaired {len(valid_jsonl)} lines.")
