import sys

# Ah! In my previous refactor to add the parameterless constructor to ApplicationDbContext I changed its original behavior?
# Wait, "No database provider has been configured for this DbContext."
# EF Core InMemoryDatabase uses the parameterless constructor if we pass options to base(options)?
# Let's check `ApplicationDbContext.cs`.
