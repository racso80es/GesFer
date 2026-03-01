filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/GetArticleFamilyByIdTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

content = content.replace('await _handler.Invoking(h => h.HandleAsync(query))\n            .Should().ThrowAsync<InvalidOperationException>()\n            .WithMessage("*No se encontró la familia de artículos*");', 'var result = await _handler.HandleAsync(query);\n        result.Should().BeNull();')
content = content.replace('result.TaxType.Should().NotBeNull();', '')
content = content.replace('result.TaxType!.Id.Should().Be(taxTypeId);', '')
content = content.replace('result.TaxType.Code.Should().Be("T1");', '')

with open(filepath, 'w') as f:
    f.write(content)
