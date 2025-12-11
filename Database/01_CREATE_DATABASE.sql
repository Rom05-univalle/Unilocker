CREATE DATABASE UnilockerDBV1;


USE UnilockerDBV1;
GO

-- ============================================================================
-- 1. LOCATION HIERARCHY
-- ============================================================================

-- BRANCH (University campus)
CREATE TABLE Branch (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200),
    Code NVARCHAR(10) UNIQUE,
    
    -- Standard CRUD fields
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedUpdatedBy INT
);

-- BLOCK (Building within a branch)
CREATE TABLE Block (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Address NVARCHAR(200), -- Block-specific address
    BranchId INT NOT NULL,
    
    -- Standard CRUD fields
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedUpdatedBy INT,
    
    CONSTRAINT FK_Block_Branch FOREIGN KEY (BranchId) 
        REFERENCES Branch(Id) ON DELETE CASCADE
);

-- CLASSROOM (Computer lab or room)
CREATE TABLE Classroom (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Capacity INT,
    BlockId INT NOT NULL,
    
    -- Standard CRUD fields
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedUpdatedBy INT,
    
    CONSTRAINT FK_Classroom_Block FOREIGN KEY (BlockId) 
        REFERENCES Block(Id) ON DELETE CASCADE
);

-- COMPUTER (Physical equipment)
CREATE TABLE Computer (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    UUID UNIQUEIDENTIFIER NOT NULL UNIQUE DEFAULT NEWID(),
    SerialNumber NVARCHAR(100),
    Brand NVARCHAR(50),
    Model NVARCHAR(50),
    ClassroomId INT NOT NULL,
    
    -- Standard CRUD fields
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedUpdatedBy INT,
    
    CONSTRAINT FK_Computer_Classroom FOREIGN KEY (ClassroomId) 
        REFERENCES Classroom(Id) ON DELETE CASCADE
);

-- ============================================================================
-- 2. USERS AND ROLES
-- ============================================================================

-- ROLE (User profiles)
CREATE TABLE Role (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    
    -- Standard CRUD fields
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedUpdatedBy INT
);

-- USER (People who use the system)
CREATE TABLE [User] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(150) NOT NULL,
	LastName NVARCHAR(150) NOT NULL,
	SecondLastName NVARCHAR(150),
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    PasswordHash NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(20),
    RoleId INT NOT NULL,
    
    -- Access control
    LastAccess DATETIME2,
    FailedLoginAttempts INT DEFAULT 0,
    IsBlocked BIT DEFAULT 0,
    
    -- Standard CRUD fields
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedUpdatedBy INT,
    
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId) 
        REFERENCES Role(Id)
);

-- Índices únicos filtrados: Solo aplican a usuarios activos (Status=1)
-- Esto permite reutilizar Username/Email de usuarios eliminados (Status=0)
CREATE UNIQUE NONCLUSTERED INDEX UQ_User_Username_Active
ON [User] (Username)
WHERE Status = 1;

CREATE UNIQUE NONCLUSTERED INDEX UQ_User_Email_Active
ON [User] (Email)
WHERE Status = 1 AND Email IS NOT NULL;

-- ============================================================================
-- 3. SESSIONS AND REPORTS
-- ============================================================================

-- SESSION (Computer usage by user)
CREATE TABLE Session (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ComputerId INT NOT NULL,
    StartDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    EndDateTime DATETIME2,
    
    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    EndMethod NVARCHAR(20),
    
    -- Heartbeat (pulse to detect disconnections)
    LastHeartbeat DATETIME2,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    
    CONSTRAINT FK_Session_User FOREIGN KEY (UserId) 
        REFERENCES [User](Id),
    CONSTRAINT FK_Session_Computer FOREIGN KEY (ComputerId) 
        REFERENCES Computer(Id),
    CONSTRAINT CK_Session_EndMethod CHECK (
        EndMethod IN ('Normal', 'Forced', 'Timeout', 'Administrative')
    )
);

-- PROBLEM TYPE (Catalog of common issues)
CREATE TABLE ProblemType (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(200),
    
    -- Standard CRUD fields
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    CreatedUpdatedBy INT
);

-- REPORT (Incidents reported by users)
CREATE TABLE Report (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId INT NOT NULL,
    ProblemTypeId INT NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    ReportDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Report management
    ReportStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    ResolutionDate DATETIME2,
    
    -- Audit
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    
    CONSTRAINT FK_Report_Session FOREIGN KEY (SessionId) 
        REFERENCES Session(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Report_ProblemType FOREIGN KEY (ProblemTypeId) 
        REFERENCES ProblemType(Id),
    CONSTRAINT CK_Report_Status CHECK (
        ReportStatus IN ('Pending', 'InReview', 'Resolved', 'Rejected')
    )
);

-- ============================================================================
-- 4. GLOBAL AUDIT
-- ============================================================================

-- AUDIT LOG (Complete change log)
CREATE TABLE AuditLog (
    Id BIGINT PRIMARY KEY IDENTITY(1,1),
    AffectedTable NVARCHAR(50) NOT NULL,
    RecordId INT NOT NULL,
    ActionType NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    ResponsibleUserId INT,
    ActionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ChangeDetails NVARCHAR(MAX), -- JSON with before/after values
    IpAddress NVARCHAR(45),
    
    CONSTRAINT FK_AuditLog_User FOREIGN KEY (ResponsibleUserId) 
        REFERENCES [User](Id)
);
