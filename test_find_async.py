import os
import re

filepath = "src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs"
with open(filepath, "r") as f:
    content = f.read()

# SetupMocks inside GetArticleFamilyByIdTests has `mockTaxTypes.Setup(x => x.FindAsync` twice if we are not careful?
# Wait, actually `SetupMocks` was replaced earlier with the "robust_setup" which ONLY had Returns for Object, it removed `FindAsync` setup!

content = content.replace("private void SetupMocks()\n    {\n", "private void SetupMocks()\n    {\n")

# Re-inject the find async logic into SetupMocks() for GetArticleFamilyByIdTests
new_setup = """private void SetupMocks()
    {

        var mockCompanies = _companies.BuildMockDbSet();

        var mockTaxTypes = _taxTypes.BuildMockDbSet();
        mockTaxTypes.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _taxTypes.SingleOrDefault(t => t.Id == id);
                return null;
            });

        var mockArticleFamilies = _articleFamilies.BuildMockDbSet();
        mockArticleFamilies.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken token) =>
            {
                if (ids != null && ids.Length > 0 && ids[0] is Guid id)
                    return _articleFamilies.SingleOrDefault(t => t.Id == id);
                return null;
            });

        _contextMock.Setup(c => c.Companies).Returns(mockCompanies.Object);
        _contextMock.Setup(c => c.ArticleFamilies).Returns(mockArticleFamilies.Object);
        _contextMock.Setup(c => c.TaxTypes).Returns(mockTaxTypes.Object);
    }"""

# Remove existing SetupMocks function
content = re.sub(r"private void SetupMocks\(\)\s*\{.*?\}\s*\}", new_setup + "\n}", content, flags=re.DOTALL)

with open(filepath, "w") as f:
    f.write(content)
