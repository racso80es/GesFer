import sys

filepath = 'src/Product/Back/tests/GesFer.Product.UnitTests/Services/SetupServiceTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

# I want to print result.Errors in the test to see what's actually failing.
content = content.replace('result.Success.Should().BeTrue();', 'if (!result.Success) throw new Exception(string.Join(", ", result.Errors));\n        result.Success.Should().BeTrue();')

with open(filepath, 'w') as f:
    f.write(content)
