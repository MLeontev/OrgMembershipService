namespace OrgMembershipService.Infrastructure.Database.Seeding;

internal static class SeedData
{
    public static readonly (string Code, string Name, string? Description)[] Permissions =
    [
        ("ORG_CREATE", "Создание организации", null),
        ("ORG_READ", "Просмотр организации", null),
        ("ORG_UPDATE", "Редактирование организации", null),
        ("ORG_DELETE", "Удаление организации", null),
        ("ORG_OWNER_CHANGE", "Смена владельца организации", null),

        ("MEMBERS_LIST", "Просмотр участников", null),
        ("MEMBERS_READ", "Просмотр профиля участника", null),
        ("MEMBERS_INVITE_CREATE", "Создание приглашений", null),
        ("MEMBERS_INVITE_LIST", "Просмотр приглашений", null),
        ("MEMBERS_INVITE_REVOKE", "Отзыв приглашений", null),
        ("MEMBERS_UPDATE", "Редактирование организационного профиля участника", null),
        ("MEMBERS_DEACTIVATE", "Временная деактивация участника", null),
        ("MEMBERS_REMOVE", "Удаление участника", null),

        ("ROLES_LIST", "Просмотр ролей", null),
        ("ROLES_CREATE", "Создание ролей", null),
        ("ROLES_UPDATE", "Редактирование ролей", null),
        ("ROLES_DELETE", "Удаление ролей", null),
        ("ROLES_ASSIGN", "Назначение ролей", null),
        ("ROLES_REVOKE", "Снятие ролей", null),

        ("RESOURCES_LIST", "Просмотр ресурсов", null),
        ("RESOURCES_READ", "Просмотр ресурса", null),
        ("RESOURCES_CREATE", "Создание ресурса", null),
        ("RESOURCES_UPDATE", "Редактирование ресурса", null),
        ("RESOURCES_DELETE", "Удаление ресурса", null),
        ("RESOURCES_STATUS_CHANGE", "Изменение статуса ресурса", null),
        ("RESOURCES_RULES_MANAGE", "Управление правилами ресурса", null),

        ("BOOKINGS_SEARCH", "Поиск доступных ресурсов", null),
        ("BOOKINGS_CREATE", "Создание бронирования", null),
        ("BOOKINGS_READ", "Просмотр бронирований", null),
        ("BOOKINGS_UPDATE_OWN", "Изменение своих бронирований", null),
        ("BOOKINGS_CANCEL_OWN", "Отмена своих бронирований", null),
        ("BOOKINGS_MANAGE_ANY", "Управление любыми бронированиями", null),

        ("POLICIES_LIST", "Просмотр политик бронирования", null),
        ("POLICIES_MANAGE", "Управление политиками бронирования", null)
    ];

    public static readonly (string Code, string Name, string? Description, int Priority)[] DefaultRoles =
    [
        ("ORG_OWNER", "Владелец организации", "Полный доступ в организации", 1000),
        ("ORG_ADMIN", "Администратор организации", "Управление участниками и ролями", 900),
        ("RESOURCE_ADMIN", "Администратор ресурсов", "Управление ресурсами и их доступностью", 800),
        ("EMPLOYEE", "Сотрудник", "Бронирование ресурсов и управление своими бронированиями", 100),
        ("VIEWER", "Наблюдатель", "Просмотр ресурсов и занятости", 10)
    ];

    public static readonly Dictionary<string, string[]> RolePermissionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ORG_OWNER"] = Permissions.Select(x => x.Code).ToArray(),

        ["ORG_ADMIN"] =
        [
            "ORG_READ", "ORG_UPDATE",
            "MEMBERS_LIST", "MEMBERS_READ", "MEMBERS_INVITE_CREATE", "MEMBERS_INVITE_LIST", "MEMBERS_INVITE_REVOKE",
            "MEMBERS_UPDATE", "MEMBERS_DEACTIVATE", "MEMBERS_REMOVE",
            "ROLES_LIST", "ROLES_CREATE", "ROLES_UPDATE", "ROLES_DELETE", "ROLES_ASSIGN", "ROLES_REVOKE",
            "RESOURCES_LIST", "RESOURCES_READ",
            "BOOKINGS_SEARCH", "BOOKINGS_READ",
            "POLICIES_LIST", "POLICIES_MANAGE"
        ],

        ["RESOURCE_ADMIN"] =
        [
            "RESOURCES_LIST", "RESOURCES_READ", "RESOURCES_CREATE", "RESOURCES_UPDATE", "RESOURCES_DELETE",
            "RESOURCES_STATUS_CHANGE", "RESOURCES_RULES_MANAGE",
            "BOOKINGS_SEARCH", "BOOKINGS_READ", "BOOKINGS_CREATE", "BOOKINGS_UPDATE_OWN", "BOOKINGS_CANCEL_OWN",
            "BOOKINGS_MANAGE_ANY",
            "POLICIES_LIST"
        ],


        ["EMPLOYEE"] =
        [
            "RESOURCES_LIST", "RESOURCES_READ",
            "BOOKINGS_SEARCH", "BOOKINGS_READ", "BOOKINGS_CREATE", "BOOKINGS_UPDATE_OWN", "BOOKINGS_CANCEL_OWN",
            "POLICIES_LIST"
        ],

        ["VIEWER"] =
        [
            "RESOURCES_LIST", "RESOURCES_READ",
            "BOOKINGS_SEARCH", "BOOKINGS_READ",
            "POLICIES_LIST"
        ]
    };
}