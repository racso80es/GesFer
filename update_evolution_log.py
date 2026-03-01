import sys
import datetime

date_str = datetime.datetime.now().strftime("%Y-%m-%d")
log_entry = f"\\n[{date_str}] [REFACTORIZACIÓN DE TESTS UNITARIOS EN PRODUCTO (Puros Mocks para ArticleFamilies, User y TaxTypes)] [Se eliminó la dependencia de UseInMemoryDatabase en Handlers de Producto, usando MockQueryable.Moq. Aumentando la pureza y aislamiento de los Unit Tests] [ESTADO S+]\\n"

with open("docs/EVOLUTION_LOG.md", "a") as f:
    f.write(log_entry)
