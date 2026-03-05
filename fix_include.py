import os

# Ah! GetArticleFamilyByIdCommandHandler uses `.Include(af => af.TaxType)`
# And our Setup doesn't explicitly link TaxType object inside the test data!
# We need to manually set `TaxType = new TaxType { ... }` in the arranged object!
# EF InMemory handled the relation automatically, but pure Mocks don't do relation fixup unless told!

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs"
with open(filepath, "r") as f:
    content = f.read()

new_content = content.replace("TaxTypeId = taxTypeId,", "TaxTypeId = taxTypeId,\n            TaxType = _taxTypes[0],")

with open(filepath, "w") as f:
    f.write(new_content)

# And what about SetupServiceTests?
filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs"
with open(filepath, "r") as f:
    content = f.read()

# SetupServiceTests does `_dbContext.Database.MigrateAsync()`.
# We can mock `Database.MigrateAsync()`:
# Wait, Database property is part of `DatabaseFacade`.
# Or we can just use `DbContextOptionsBuilder` for a REAL in-memory DB context for this test!
# As it's testing the overall Setup mechanism which requires DbContext for Seeders, using `UseInMemoryDatabase` is entirely fine and correct for SetupServiceTests (it tests DB provisioning!).
# I will rollback `SetupServiceTests.cs` to its original state before any of my modifications.
