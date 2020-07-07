/* ================ Recert Cycle ========================================================================================= */
CREATE TABLE [RecertCycleTable] (
	RecertCycleId integer IDENTITY(1,1),
	RecertCycleTitle nvarchar(255) NOT NULL,
	RecertStartedDate datetime2 NOT NULL,
	RecertEndedDate datetime2 NULL,
	RecertEnabled bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_RECERTCYCLETABLE] PRIMARY KEY CLUSTERED
  (
  [RecertCycleId] ASC
  ) WITH (IGNORE_DUP_KEY = OFF)
)

/* ---------------- Recert Cycle Temporal Setup ---------------- */
ALTER TABLE RecertCycleTable ADD
	SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT RCT_SysStart DEFAULT SYSUTCDATETIME(),
	SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN CONSTRAINT RCT_SysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59'),
	PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
GO
ALTER TABLE RecertCycleTable SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RecertCycleTableHistory));
GO



/* ================ Role User ============================================================================================ */
CREATE TABLE [RoleTable] (
	RoleId nvarchar(225) NOT NULL UNIQUE,
	RoleName nvarchar(255) NOT NULL,
	RoleDescription nvarchar(max),
	RoleOwner_RoleId nvarchar(225) NOT NULL,
  CONSTRAINT [PK_ROLETABLE] PRIMARY KEY CLUSTERED
  (
  [RoleId] ASC
  ) WITH (IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [UserTable] (
	UserId nvarchar(225) NOT NULL UNIQUE,
	UserFullName nvarchar(255) NOT NULL,
	RoleId nvarchar(225) NOT NULL,
	LastCertifiedBy nvarchar(225),
	LastCertifiedDate datetime2,
  CONSTRAINT [PK_USERTABLE] PRIMARY KEY CLUSTERED
  (
  [UserId] ASC
  ) WITH (IGNORE_DUP_KEY = OFF)
)
GO


/* ---------------- Role User Constaints ---------------- */
ALTER TABLE [UserTable] WITH CHECK ADD CONSTRAINT [UserTable_fk0] FOREIGN KEY ([RoleId]) REFERENCES [RoleTable]([RoleId])
ON UPDATE CASCADE
GO
ALTER TABLE [UserTable] CHECK CONSTRAINT [UserTable_fk0]
GO

/* ---------------- Role User Temporal Setup ---------------- */
ALTER TABLE RoleTable ADD
	SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT RT_SysStart DEFAULT SYSUTCDATETIME(),
	SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN CONSTRAINT RT_SysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59'),
	PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
GO
ALTER TABLE RoleTable SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RoleTableHistory));
GO

ALTER TABLE UserTable ADD
	SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT UT_SysStart DEFAULT SYSUTCDATETIME(),
	SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN CONSTRAINT UT_SysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59'),
	PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
GO
ALTER TABLE UserTable SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.UserTableHistory));
GO


/* ================ Service Priv ============================================================================================ */
CREATE TABLE [ServiceTable] (
	ServiceId nvarchar(255) NOT NULL UNIQUE,
	ServiceName nvarchar(255) NOT NULL,
	ServiceDescription nvarchar(max),
	ServiceOwner_RoleId nvarchar(225) NOT NULL,

  CONSTRAINT [PK_SERVICETABLE] PRIMARY KEY CLUSTERED
  (
  [ServiceId] ASC
  ) WITH (IGNORE_DUP_KEY = OFF)
)
GO

CREATE TABLE [PrivTable] (
	PrivId nvarchar(255) NOT NULL UNIQUE,
	ServiceId nvarchar(255) NOT NULL,
	ServicePrivSummary nvarchar(max) NOT NULL,
	PermissionGroup nvarchar(255) NOT NULL,
	CredentialStorageMethod nvarchar(255),
  CONSTRAINT [PK_PRIVTABLE] PRIMARY KEY CLUSTERED
  (
  [PrivId] ASC
  ) WITH (IGNORE_DUP_KEY = OFF)
)
GO

/* ---------------- Service Priv Constaints ---------------- */
ALTER TABLE [PrivTable] WITH CHECK ADD CONSTRAINT [PrivTable_fk0] FOREIGN KEY ([ServiceId]) REFERENCES [ServiceTable]([ServiceId])
ON UPDATE CASCADE
GO
ALTER TABLE [PrivTable] CHECK CONSTRAINT [PrivTable_fk0]
GO

ALTER TABLE [ServiceTable] WITH CHECK ADD CONSTRAINT [ServiceTable_fk0] FOREIGN KEY ([ServiceOwner_RoleId]) REFERENCES [RoleTable]([RoleId])
ON UPDATE CASCADE
GO
ALTER TABLE [ServiceTable] CHECK CONSTRAINT [ServiceTable_fk0]

/* ---------------- Service Priv Temporal Setup ---------------- */
ALTER TABLE ServiceTable ADD
	SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT ST_SysStart DEFAULT SYSUTCDATETIME(),
	SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN CONSTRAINT ST_SysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59'),
	PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
GO
ALTER TABLE ServiceTable SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.ServiceTableHistory));
GO

ALTER TABLE PrivTable ADD
	SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT SPL_SysStart DEFAULT SYSUTCDATETIME(),
	SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN CONSTRAINT SPL_SysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59'),
	PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
GO
ALTER TABLE PrivTable SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.PrivTableHistory));
GO



/* ================ Role Service Priv ============================================================================================ */
CREATE TABLE [RolePrivLink] (
	RolePrivId integer IDENTITY(1,1),
	RoleId nvarchar(225) NOT NULL,
	RoleOwner_PrivId nvarchar(255),
	RoleOwner_RoleAccessJustification nvarchar(max),
	RoleOwner_RemovalImpact nvarchar(max),
	RoleOwner_IsRevoked bit NOT NULL DEFAULT 0,
	RoleOwner_IsCertified bit NOT NULL DEFAULT 0,
	RoleOwner_DateCertified datetime2,
	ServiceOwner_PrivId nvarchar(255),
	ServiceOwner_RoleAccessJustification nvarchar(max),
	ServiceOwner_RemovalImpact nvarchar(max),
	ServiceOwner_IsRevoked bit NOT NULL DEFAULT 0,
	ServiceOwner_IsCertified bit NOT NULL DEFAULT 0,
	ServiceOwner_DateCertified datetime2,
    RiskImpact integer,
    RiskLikelihood integer,
    RiskNotes nvarchar(max),
    RiskAssessmentDate datetime2,
	RiskIsAssessed bit NOT NULL DEFAULT 0,
  CONSTRAINT [PK_ROLEPRIVLINK] PRIMARY KEY CLUSTERED
  (
  [RolePrivId] ASC
  ) WITH (IGNORE_DUP_KEY = OFF),
  CONSTRAINT [UQ_RolePrivLink01] UNIQUE NONCLUSTERED
  (
      [RoleId], [RoleOwner_PrivId]
  ),
  CONSTRAINT [UQ_RolePrivLink02] UNIQUE NONCLUSTERED
  (
      [RoleId], [ServiceOwner_PrivId]
  )
)
GO

/* ---------------- Role Service Priv Constaints ---------------- */
ALTER TABLE [RolePrivLink] WITH CHECK ADD CONSTRAINT [RolePrivLink_fk0] FOREIGN KEY ([RoleId]) REFERENCES [RoleTable]([RoleId])
ON UPDATE CASCADE
GO
ALTER TABLE [RolePrivLink] CHECK CONSTRAINT [RolePrivLink_fk0]
GO
ALTER TABLE [RolePrivLink] WITH CHECK ADD CONSTRAINT [RolePrivLink_fk1] FOREIGN KEY ([RoleOwner_PrivId]) REFERENCES [PrivTable]([PrivId])
ON UPDATE NO ACTION
GO
ALTER TABLE [RolePrivLink] CHECK CONSTRAINT [RolePrivLink_fk1]
GO
ALTER TABLE [RolePrivLink] WITH CHECK ADD CONSTRAINT [RolePrivLink_fk2] FOREIGN KEY ([ServiceOwner_PrivId]) REFERENCES [PrivTable]([PrivId])
ON UPDATE NO ACTION
GO
ALTER TABLE [RolePrivLink] CHECK CONSTRAINT [RolePrivLink_fk2]
GO



/* ---------------- Role Service Priv Temporal Setup ---------------- */
ALTER TABLE RolePrivLink ADD
	SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT RSPL_SysStart DEFAULT SYSUTCDATETIME(),
	SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN CONSTRAINT RSPL_SysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59'),
	PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);
GO
ALTER TABLE RolePrivLink SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RolePrivLinkHistory));
GO
