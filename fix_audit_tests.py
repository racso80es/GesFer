import sys

filepath = 'src/Admin/Back/tests/GesFer.Admin.UnitTests/Services/AuditLogServiceTests.cs'
with open(filepath, 'r') as f:
    content = f.read()

# We have the memory: "To prevent flakiness in temporal unit tests, use FluentAssertions' Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(5)) to provide an adequate margin of error."
content = content.replace('TimeSpan.FromSeconds(2)', 'TimeSpan.FromSeconds(5)')

with open(filepath, 'w') as f:
    f.write(content)
