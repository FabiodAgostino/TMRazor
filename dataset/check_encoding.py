import json

file_path = r'C:\Users\fabio.dagostino\Desktop\dataset\dataset.jsonl'

try:
    with open(file_path, 'r', encoding='utf-8') as f:
        line = f.readline()
        print("UTF-8 read test:")
        print(line[:200])
except Exception as e:
    print(f"UTF-8 read error: {e}")

try:
    with open(file_path, 'r', encoding='latin-1') as f:
        line = f.readline()
        print("\nLatin-1 read test:")
        print(line[:200])
except Exception as e:
    print(f"Latin-1 read error: {e}")
