import sys

def main():
    filepath = sys.argv[1]
    with open(filepath, 'r') as f:
        content = f.read()

    # The properties in ApplicationDbContext are not virtual, and we need a parameterless constructor.
    # The memory instruction says: "All `DbSet<T>` properties in `src/Product/Back/Infrastructure/Data/ProductDbContext.cs` are `virtual` to support mocking frameworks (Moq), and the class includes a parameterless constructor."
    # Wait, the file is `ApplicationDbContext.cs` but the class is `ApplicationDbContext`. Let me add virtual and parameterless constructor.

    lines = content.split('\n')
    new_lines = []
    in_class = False
    added_constructor = False

    for line in lines:
        if 'public class ApplicationDbContext : DbContext' in line:
            in_class = True

        if in_class and 'public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)' in line and not added_constructor:
            # Add parameterless constructor
            new_lines.append('    protected ApplicationDbContext() {}')
            new_lines.append('')
            added_constructor = True

        if 'public DbSet<' in line and 'virtual' not in line:
            line = line.replace('public DbSet<', 'public virtual DbSet<')

        new_lines.append(line)

    with open(filepath, 'w') as f:
        f.write('\n'.join(new_lines))

if __name__ == "__main__":
    main()
