import sys

def main():
    filepath = sys.argv[1]
    with open(filepath, 'r') as f:
        content = f.read()

    # Add using Microsoft.EntityFrameworkCore;
    if 'using Microsoft.EntityFrameworkCore;' not in content:
        content = content.replace('using Microsoft.EntityFrameworkCore;\n', '')
        content = 'using Microsoft.EntityFrameworkCore;\n' + content

    with open(filepath, 'w') as f:
        f.write(content)

if __name__ == "__main__":
    main()
