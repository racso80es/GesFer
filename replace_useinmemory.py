import sys

def main():
    filepath = sys.argv[1]
    with open(filepath, 'r') as f:
        content = f.read()

    # We need to mock IApplicationDbContext or pass options to ApplicationDbContext.
    # The error "Could not find a parameterless constructor" happens because ApplicationDbContext has a constructor with DbContextOptions.
    new_content = content.replace('new Mock<ApplicationDbContext>()', 'new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>())')

    with open(filepath, 'w') as f:
        f.write(new_content)

if __name__ == "__main__":
    main()
