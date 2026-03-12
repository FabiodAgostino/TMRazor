with open(r'C:\Users\fabio.dagostino\Desktop\dataset\dataset.jsonl', 'rb') as f:
    lines = f.readlines()
    line4 = lines[3]
    print("Line 4 bytes:")
    print(line4)
    print("\nLine 4 as Latin-1:")
    print(line4.decode('latin-1'))
    print("\nLine 4 as UTF-8:")
    print(line4.decode('utf-8'))
