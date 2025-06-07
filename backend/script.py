import os
import re
from pathlib import Path

# CONFIGURE HERE
ROOT_NAMESPACE = ""  # Root C# namespace
SOURCE_DIR = "src"        # Folder where .cs files are located

def to_namespace(file_path, source_root):
    relative = file_path.relative_to(source_root).parent
    parts = [ROOT_NAMESPACE] + list(relative.parts)
    return ".".join(parts)[1:]

def update_namespace_in_file(file_path, new_namespace):
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    pattern = re.compile(r'^\s*namespace\s+[\w\.]+')
    updated = False

    for i, line in enumerate(lines):
        if pattern.match(line):
            lines[i] = f"namespace {new_namespace};\n"
            updated = True
            break

    if updated:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.writelines(lines)
        print(f"Updated: {file_path}")
    else:
        print(f"Skipped (no namespace): {file_path}")

def process_directory(source_dir):
    source_path = Path(source_dir)

    files = source_path.rglob("*.cs")
    # print(f"Found {len(list(files))} .cs files in {source_path}")

    for file_path in files:
        if file_path.name.endswith(".Designer.cs") or file_path.name.endswith(".g.cs"):
            continue  # Skip generated files
        new_namespace = to_namespace(file_path, source_path)
        update_namespace_in_file(file_path, new_namespace)

if __name__ == "__main__":
    process_directory(SOURCE_DIR)
