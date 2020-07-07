ALTER TABLE dbo.RolePrivLink SET (SYSTEM_VERSIONING = OFF)
DROP TABLE dbo.RolePrivLink;
DROP TABLE dbo.RolePrivLinkHistory;

ALTER TABLE dbo.PrivTable SET (SYSTEM_VERSIONING = OFF)
DROP TABLE dbo.PrivTable;
DROP TABLE dbo.PrivTableHistory;

ALTER TABLE dbo.ServiceTable SET (SYSTEM_VERSIONING = OFF)
DROP TABLE dbo.ServiceTable;
DROP TABLE dbo.ServiceTableHistory;

ALTER TABLE dbo.UserTable SET (SYSTEM_VERSIONING = OFF)
DROP TABLE dbo.UserTable;
DROP TABLE dbo.UserTableHistory;

ALTER TABLE dbo.RoleTable SET (SYSTEM_VERSIONING = OFF)
DROP TABLE dbo.RoleTable;
DROP TABLE dbo.RoleTableHistory;

ALTER TABLE dbo.RecertCycleTable SET (SYSTEM_VERSIONING = OFF)
DROP TABLE dbo.RecertCycleTable;
DROP TABLE dbo.RecertCycleTableHistory;