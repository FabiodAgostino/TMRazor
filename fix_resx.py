import xml.etree.ElementTree as ET
import os

def deduplicate_resx(file_path):
    if not os.path.exists(file_path):
        print(f"File not found: {file_path}")
        return
    print(f"Deduplicating {file_path}...")
    # Read the file content
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Parse XML
    try:
        root = ET.fromstring(content)
    except ET.ParseError as e:
        print(f"Error parsing {file_path}: {e}")
        # Try to fix truncated/multiple root issues if present
        if "</root>" in content:
            content = content[:content.find("</root>") + 7]
            try:
                root = ET.fromstring(content)
            except:
                print("Could not recover XML.")
                return
        else:
            return

    seen_keys = set()
    to_remove = []

    # Iterate items
    for data in root.findall('data'):
        name = data.get('name')
        if name in seen_keys:
            # print(f"  Removing duplicate: {name}")
            to_remove.append(data)
        else:
            seen_keys.add(name)

    for data in to_remove:
        root.remove(data)

    # Write back
    tree = ET.ElementTree(root)
    if hasattr(ET, 'indent'):
        ET.indent(tree, space="  ", level=0)
    tree.write(file_path, encoding='utf-8', xml_declaration=True)
    print(f"Finished {file_path}. Removed {len(to_remove)} duplicates.")

# Correct paths
base_path = r"C:\Users\fabio.dagostino\Documents\GitHub\TMRazor\TMRazorImproved\TMRazorImproved.Shared\Resources"
deduplicate_resx(os.path.join(base_path, "Strings.resx"))
deduplicate_resx(os.path.join(base_path, "Strings.it.resx"))
