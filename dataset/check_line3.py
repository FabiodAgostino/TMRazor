with open(r'C:\Users\fabio.dagostino\Desktop\dataset\dataset.jsonl', 'rb') as f:
    lines = f.readlines()
    line3 = lines[2]
    print("Line 3 bytes:")
    print(line3)
    print("\nLine 3 as Latin-1:")
    print(line3.decode('latin-1'))
    print("\nLine 3 as UTF-8:")
    print(line3.decode('utf-8'))
