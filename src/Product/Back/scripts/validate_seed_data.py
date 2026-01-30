#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script de validación de datos de seed
Verifica que:
1. Todos los datos de master-data.json están en test-data.json
2. Los tests utilizan datos existentes en test-data.json
"""

import json
import os
import sys
from pathlib import Path

def load_json_file(filepath):
    """Carga un archivo JSON y retorna el objeto"""
    with open(filepath, 'r', encoding='utf-8') as f:
        return json.load(f)

def main():
    # Rutas de los archivos
    script_dir = Path(__file__).parent
    api_dir = script_dir.parent
    seeds_path = api_dir / "src" / "Infrastructure" / "Data" / "Seeds"
    master_data_path = seeds_path / "master-data.json"
    test_data_path = seeds_path / "test-data.json"
    
    if not master_data_path.exists():
        print(f"ERROR: No se encuentra master-data.json en {master_data_path}")
        sys.exit(1)
    
    if not test_data_path.exists():
        print(f"ERROR: No se encuentra test-data.json en {test_data_path}")
        sys.exit(1)
    
    print("=== Validación de Datos de Seed ===")
    print()
    print("Cargando archivos JSON...")
    master_data = load_json_file(master_data_path)
    test_data = load_json_file(test_data_path)
    
    errors = []
    warnings = []
    
    print()
    print("=== 1. Validación: Datos Maestros en test-data.json ===")
    print()
    
    # Validar Languages
    print("Validando Languages...")
    master_languages = {lang["id"]: lang for lang in master_data.get("languages", [])}
    test_languages = {lang["id"]: lang for lang in test_data.get("languages", [])}
    
    missing_languages = [lang_id for lang_id in master_languages.keys() if lang_id not in test_languages]
    if missing_languages:
        errors.extend([f"Language con ID {lang_id} no está en test-data.json" for lang_id in missing_languages])
        print(f"  ✗ Faltan {len(missing_languages)} Languages en test-data.json")
    else:
        print("  ✓ Todos los Languages de master-data.json están en test-data.json")
    
    # Validar Permissions
    print("Validando Permissions...")
    master_permissions = {perm["id"]: perm for perm in master_data.get("permissions", [])}
    test_permissions = {perm["id"]: perm for perm in test_data.get("permissions", [])}
    
    missing_permissions = [perm_id for perm_id in master_permissions.keys() if perm_id not in test_permissions]
    if missing_permissions:
        errors.extend([f"Permission con ID {perm_id} ({master_permissions[perm_id]['key']}) no está en test-data.json" 
                      for perm_id in missing_permissions])
        print(f"  ✗ Faltan {len(missing_permissions)} Permissions en test-data.json")
    else:
        print("  ✓ Todos los Permissions de master-data.json están en test-data.json")
    
    # Validar Groups
    print("Validando Groups...")
    master_groups = {group["id"]: group for group in master_data.get("groups", [])}
    test_groups = {group["id"]: group for group in test_data.get("groups", [])}
    
    missing_groups = [group_id for group_id in master_groups.keys() if group_id not in test_groups]
    if missing_groups:
        errors.extend([f"Group con ID {group_id} ({master_groups[group_id]['name']}) no está en test-data.json" 
                      for group_id in missing_groups])
        print(f"  ✗ Faltan {len(missing_groups)} Groups en test-data.json")
    else:
        print("  ✓ Todos los Groups de master-data.json están en test-data.json")
    
    # Validar GroupPermissions
    print("Validando GroupPermissions...")
    master_group_permissions = {}
    for gp in master_data.get("groupPermissions", []):
        key = f"{gp['groupId']}-{gp['permissionId']}"
        master_group_permissions[key] = gp
    
    test_group_permissions = {}
    for gp in test_data.get("groupPermissions", []):
        key = f"{gp['groupId']}-{gp['permissionId']}"
        test_group_permissions[key] = gp
    
    missing_group_permissions = [key for key in master_group_permissions.keys() if key not in test_group_permissions]
    if missing_group_permissions:
        errors.extend([f"GroupPermission (GroupId: {master_group_permissions[key]['groupId']}, PermissionId: {master_group_permissions[key]['permissionId']}) no está en test-data.json" 
                      for key in missing_group_permissions])
        print(f"  ✗ Faltan {len(missing_group_permissions)} GroupPermissions en test-data.json")
    else:
        print("  ✓ Todos los GroupPermissions de master-data.json están en test-data.json")
    
    # Validar AdminUsers
    print("Validando AdminUsers...")
    master_admin_users = {au["username"]: au for au in master_data.get("adminUsers", [])}
    test_admin_users = {au["username"]: au for au in test_data.get("adminUsers", [])}
    
    missing_admin_users = [username for username in master_admin_users.keys() if username not in test_admin_users]
    if missing_admin_users:
        errors.extend([f"AdminUser con username '{username}' no está en test-data.json" for username in missing_admin_users])
        print(f"  ✗ Faltan {len(missing_admin_users)} AdminUsers en test-data.json")
    else:
        print("  ✓ Todos los AdminUsers de master-data.json están en test-data.json")
    
    print()
    print("=== 2. Validación: Datos usados en Tests ===")
    print()
    
    # Validar datos específicos usados en tests
    test_user_ids = [
        "99999999-9999-9999-9999-999999999999",  # admin
        "99999999-9999-9999-9999-999999999998",  # gestor
        "99999999-9999-9999-9999-999999999997",  # usuario_test_update
        "99999999-9999-9999-9999-999999999996",  # usuario_test_password
    ]
    
    test_company_ids = [
        "11111111-1111-1111-1111-111111111111",  # Empresa Demo
        "11111111-1111-1111-1111-111111111112",  # Empresa Test Update
    ]
    
    test_group_ids = [
        "22222222-2222-2222-2222-222222222222",  # Administradores
        "22222222-2222-2222-2222-222222222225",  # Grupo Test Update
    ]
    
    test_admin_user_ids = [
        "aaaaaaaa-0000-0000-0000-000000000000",  # admin (AdminUser)
    ]
    
    test_strings = {
        "users": ["admin", "gestor"],
        "companies": ["Empresa Demo", "Empresa Test Update"],
        "groups": ["Administradores", "Gestores", "Consultores", "Grupo Test Update"],
        "permissions": ["users.read", "users.write", "articles.read"],
    }
    
    # Validar IDs de usuarios
    print("Validando IDs de usuarios usados en tests...")
    test_users = {user["id"]: user for user in test_data.get("users", [])}
    for user_id in test_user_ids:
        if user_id in test_users:
            print(f"  ✓ Usuario ID {user_id} encontrado")
        else:
            errors.append(f"Usuario con ID {user_id} no está en test-data.json")
            print(f"  ✗ Usuario ID {user_id} no encontrado")
    
    # Validar nombres de empresa
    print("Validando nombres de empresa usados en tests...")
    test_companies = {company["name"]: company for company in test_data.get("companies", [])}
    for company_name in test_strings["companies"]:
        if company_name in test_companies:
            print(f"  ✓ Empresa '{company_name}' encontrada")
        else:
            errors.append(f"Empresa '{company_name}' no está en test-data.json")
            print(f"  ✗ Empresa '{company_name}' no encontrada")
    
    # Validar nombres de grupo
    print("Validando nombres de grupo usados en tests...")
    test_groups_by_name = {group["name"]: group for group in test_data.get("groups", [])}
    for group_name in test_strings["groups"]:
        if group_name in test_groups_by_name:
            print(f"  ✓ Grupo '{group_name}' encontrado")
        else:
            errors.append(f"Grupo '{group_name}' no está en test-data.json")
            print(f"  ✗ Grupo '{group_name}' no encontrado")
    
    # Validar permisos
    print("Validando permisos usados en tests...")
    test_permissions_by_key = {perm["key"]: perm for perm in test_data.get("permissions", [])}
    for permission_key in test_strings["permissions"]:
        if permission_key in test_permissions_by_key:
            print(f"  ✓ Permiso '{permission_key}' encontrado")
        else:
            errors.append(f"Permiso '{permission_key}' no está en test-data.json")
            print(f"  ✗ Permiso '{permission_key}' no encontrado")
    
    # Validar IDs de grupos
    print("Validando IDs de grupos usados en tests...")
    for group_id in test_group_ids:
        if group_id in test_groups:
            print(f"  ✓ Grupo ID {group_id} encontrado")
        else:
            errors.append(f"Grupo con ID {group_id} no está en test-data.json")
            print(f"  ✗ Grupo ID {group_id} no encontrado")
    
    # Validar IDs de empresas
    print("Validando IDs de empresas usados en tests...")
    test_companies_by_id = {company["id"]: company for company in test_data.get("companies", [])}
    for company_id in test_company_ids:
        if company_id in test_companies_by_id:
            print(f"  ✓ Empresa ID {company_id} encontrada")
        else:
            errors.append(f"Empresa con ID {company_id} no está en test-data.json")
            print(f"  ✗ Empresa ID {company_id} no encontrada")
    
    # Validar AdminUsers
    print("Validando AdminUsers usados en tests...")
    test_admin_users_by_id = {au["id"]: au for au in test_data.get("adminUsers", [])}
    for admin_user_id in test_admin_user_ids:
        if admin_user_id in test_admin_users_by_id:
            print(f"  ✓ AdminUser ID {admin_user_id} encontrado")
        else:
            errors.append(f"AdminUser con ID {admin_user_id} no está en test-data.json")
            print(f"  ✗ AdminUser ID {admin_user_id} no encontrado")
    
    # Verificar que el usuario admin tiene el grupo Administradores asignado
    print("Validando asignación de grupos a usuarios...")
    admin_user_id = "99999999-9999-9999-9999-999999999999"
    admin_group_id = "22222222-2222-2222-2222-222222222222"
    admin_has_group = False
    
    for ug in test_data.get("userGroups", []):
        if ug["userId"] == admin_user_id and ug["groupId"] == admin_group_id:
            admin_has_group = True
            break
    
    if admin_has_group:
        print("  ✓ Usuario admin tiene grupo Administradores asignado")
    else:
        warnings.append(f"Usuario admin ({admin_user_id}) no tiene grupo Administradores ({admin_group_id}) asignado en userGroups")
        print("  ⚠ Usuario admin no tiene grupo Administradores asignado")
    
    print()
    print("=== Resumen ===")
    print()
    
    if not errors and not warnings:
        print("✓ Validación exitosa: Todos los datos están correctamente sincronizados")
        sys.exit(0)
    else:
        if errors:
            print(f"✗ Se encontraron {len(errors)} error(es):")
            for error in errors:
                print(f"  - {error}")
        
        if warnings:
            print()
            print(f"⚠ Se encontraron {len(warnings)} advertencia(s):")
            for warning in warnings:
                print(f"  - {warning}")
        
        sys.exit(1)

if __name__ == "__main__":
    main()
