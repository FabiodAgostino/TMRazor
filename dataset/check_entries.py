import json
import re

try:
    with open('dataset_razor_API.jsonl', 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Count how many objects exist
    matches = len(re.findall(r'\{"messages":', content))
    print(f"Total 'messages' objects found: {matches}")
    
    # Let's count how many actual newlines
    lines = content.split('\n')
    print(f"Total lines: {len(lines)}")
    
except Exception as e:
    print(f"Error: {e}")
