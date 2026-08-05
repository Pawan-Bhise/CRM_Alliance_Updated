IF OBJECT_ID('dbo.SurveyCustomerData', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SurveyCustomerData](
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ClientName] NVARCHAR(200) NOT NULL,
        [Gender] NVARCHAR(20) NULL,
        [CustomerCode] NVARCHAR(50) NOT NULL,
        [MobileNumber1] NVARCHAR(30) NULL,
        [MobileNumber2] NVARCHAR(30) NULL,
        [Region] NVARCHAR(100) NULL,
        [Branch] NVARCHAR(100) NULL,
        [Location] NVARCHAR(100) NULL,
        [LoanProduct] NVARCHAR(100) NULL,
        [Age] INT NULL,
        [BusinessCategory] NVARCHAR(100) NULL,
        [ActivitiesSector] NVARCHAR(200) NULL,
        [LevelOfEducation] NVARCHAR(50) NULL,
        [IncomeLevel] NVARCHAR(100) NULL,
        [HouseholdAssets] NVARCHAR(200) NULL,
        [PovertyScore] INT NULL,
        [SurveyTemplateTypeId] INT NULL,
        CONSTRAINT [PK_SurveyCustomerData] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SurveyCustomerData_SurveyTemplateType] FOREIGN KEY ([SurveyTemplateTypeId])
            REFERENCES [dbo].[SurveyTemplateType] ([Id])
    );

    CREATE INDEX [IX_SurveyCustomerData_SurveyTemplateTypeId]
        ON [dbo].[SurveyCustomerData]([SurveyTemplateTypeId]);
END;
GO
