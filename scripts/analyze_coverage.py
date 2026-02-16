import os
import xml.etree.ElementTree as ET

def analyze_coverage():
    total_covered = 0
    total_valid = 0
    files = []

    for root, dirs, files_in_dir in os.walk("."):
        for file in files_in_dir:
            if file == "coverage.cobertura.xml":
                files.append(os.path.join(root, file))

    print(f"Found {len(files)} coverage files.")

    for file_path in files:
        try:
            tree = ET.parse(file_path)
            root = tree.getroot()
            covered = float(root.attrib.get('lines-covered', 0))
            valid = float(root.attrib.get('lines-valid', 0))

            print(f"{file_path}: Covered={covered}, Valid={valid}")

            total_covered += covered
            total_valid += valid
        except Exception as e:
            print(f"Error parsing {file_path}: {e}")

    if total_valid > 0:
        percentage = (total_covered / total_valid) * 100
        print(f"Total Coverage: {percentage:.2f}%")
    else:
        print("Total Coverage: 0.00% (No valid lines found)")

if __name__ == "__main__":
    analyze_coverage()
