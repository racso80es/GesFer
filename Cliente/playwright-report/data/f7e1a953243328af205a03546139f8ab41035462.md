# Page snapshot

```yaml
- generic [active] [ref=e1]:
  - generic [ref=e3]:
    - generic [ref=e4]:
      - img [ref=e6]
      - heading "Acceso Administrativo" [level=3] [ref=e8]
      - paragraph [ref=e9]: Ingresa tus credenciales administrativas para acceder al panel de administración
    - generic [ref=e11]:
      - generic [ref=e12]:
        - text: Usuario Administrativo
        - generic [ref=e13]:
          - img [ref=e14]
          - textbox "Usuario Administrativo" [ref=e17]:
            - /placeholder: admin
            - text: admin
      - generic [ref=e18]:
        - text: Contraseña
        - generic [ref=e19]:
          - img [ref=e20]
          - textbox "Contraseña" [ref=e23]:
            - /placeholder: ••••••••
            - text: admin123
      - generic [ref=e24]:
        - img [ref=e25]
        - generic [ref=e27]: Credenciales administrativas inválidas
      - button "Acceder al Panel Administrativo" [ref=e28] [cursor=pointer]:
        - img [ref=e29]
        - text: Acceder al Panel Administrativo
  - generic [ref=e31]:
    - img [ref=e33]
    - button "Open Tanstack query devtools" [ref=e81] [cursor=pointer]:
      - img [ref=e82]
  - alert [ref=e130]
```