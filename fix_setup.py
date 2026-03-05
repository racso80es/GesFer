import os

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs"

with open(filepath, "r") as f:
    content = f.read()

# Let's check GetArticleFamilyByIdTests
# Why is TaxTypeName null?
# `var taxType = await _context.TaxTypes.FindAsync(new object[] { entity.TaxTypeId }, cancellationToken);`
# The FindAsync mock setup:
# `return _taxTypes.SingleOrDefault(t => t.Id == id);`
# Is the item correctly in _taxTypes?
# `_taxTypes.Add(new TaxType { Id = taxTypeId, CompanyId = companyId, Code = "T1", Name = "Tax 1", Value = 10, CreatedAt = DateTime.UtcNow, IsActive = true });`
# Yes, it's added. But wait! SetupMocks() might be missing the `FindAsync` setup inside the method!
# Let's verify what's inside `SetupMocks()`.

if "mockTaxTypes.Setup(x => x.FindAsync" in content:
    print("FindAsync is mocked in content")

with open(filepath, "w") as f:
    f.write(content.replace("""var mockTaxTypes = _taxTypes.BuildMockDbSet();

        mockTaxTypes.Setup(m => m.Add(It.IsAny<TaxType>())).Callback<TaxType>((s) => _taxTypes.Add(s));""", """var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _taxTypes.SingleOrDefault(t => t.Id == id);
                return null;
            });
        mockTaxTypes.Setup(m => m.Add(It.IsAny<TaxType>())).Callback<TaxType>((s) => _taxTypes.Add(s));"""))
