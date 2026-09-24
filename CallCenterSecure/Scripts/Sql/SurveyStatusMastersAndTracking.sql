IF OBJECT_ID('dbo.SurveyCallStatusMaster', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SurveyCallStatusMaster](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_SurveyCallStatusMaster_IsActive] DEFAULT (1),
        CONSTRAINT [PK_SurveyCallStatusMaster] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_SurveyCallStatusMaster_Name] UNIQUE ([Name])
    );

    INSERT INTO [dbo].[SurveyCallStatusMaster] ([Name], [IsActive])
    VALUES
        ('Connected', 1),
        ('No Answer', 1),
        ('Busy', 1),
        ('Wrong Number', 1),
        ('Out of Service', 1),
        ('Declined', 1),
        ('Call Back Late', 1);
END;
GO

IF OBJECT_ID('dbo.SurveyFormStatusMaster', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SurveyFormStatusMaster](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(50) NOT NULL,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_SurveyFormStatusMaster_IsActive] DEFAULT (1),
        CONSTRAINT [PK_SurveyFormStatusMaster] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_SurveyFormStatusMaster_Name] UNIQUE ([Name])
    );

    INSERT INTO [dbo].[SurveyFormStatusMaster] ([Name], [IsActive])
    VALUES
        ('Not Started', 1),
        ('Partially Completed', 1),
        ('Completed', 1);
END;
GO

IF OBJECT_ID('dbo.SurveyCustomerFormTracking', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SurveyCustomerFormTracking](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [SurveyCustomerDataId] INT NOT NULL,
        [SurveyTemplateTypeId] INT NOT NULL,
        [SurveyFormId] INT NOT NULL,
        [CallStatusId] INT NULL,
        [FormStatusId] INT NULL,
        [CallRemarks] NVARCHAR(1000) NULL,
        [ModifiedBy] NVARCHAR(100) NULL,
        [ModifiedDate] DATETIME NULL,
        CONSTRAINT [PK_SurveyCustomerFormTracking] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SurveyCustomerFormTracking_Customer] FOREIGN KEY ([SurveyCustomerDataId]) REFERENCES [dbo].[SurveyCustomerData]([Id]),
        CONSTRAINT [FK_SurveyCustomerFormTracking_Template] FOREIGN KEY ([SurveyTemplateTypeId]) REFERENCES [dbo].[SurveyTemplateType]([Id]),
        CONSTRAINT [FK_SurveyCustomerFormTracking_Form] FOREIGN KEY ([SurveyFormId]) REFERENCES [dbo].[SurveyForm]([Id]),
        CONSTRAINT [FK_SurveyCustomerFormTracking_CallStatus] FOREIGN KEY ([CallStatusId]) REFERENCES [dbo].[SurveyCallStatusMaster]([Id]),
        CONSTRAINT [FK_SurveyCustomerFormTracking_FormStatus] FOREIGN KEY ([FormStatusId]) REFERENCES [dbo].[SurveyFormStatusMaster]([Id]),
        CONSTRAINT [UQ_SurveyCustomerFormTracking_Context] UNIQUE ([SurveyCustomerDataId], [SurveyTemplateTypeId], [SurveyFormId])
    );

    CREATE INDEX [IX_SurveyCustomerFormTracking_CallStatusId]
        ON [dbo].[SurveyCustomerFormTracking]([CallStatusId]);
    CREATE INDEX [IX_SurveyCustomerFormTracking_FormStatusId]
        ON [dbo].[SurveyCustomerFormTracking]([FormStatusId]);
END;
GO