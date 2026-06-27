CREATE TABLE [dbo].[SurveyForm](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyTemplateId] [int] NOT NULL,
    [Title] [nvarchar](200) NOT NULL,
    [Category] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [CreatedBy] [nvarchar](100) NULL,
    [CreatedDate] [datetime] NOT NULL,
    [ModifiedBy] [nvarchar](100) NULL,
    [ModifiedDate] [datetime] NULL,
    [IsActive] [bit] NOT NULL,
    CONSTRAINT [PK_SurveyForm] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyForm_SurveyTemplateType] FOREIGN KEY ([SurveyTemplateId])
        REFERENCES [dbo].[SurveyTemplateType]([Id])
);
GO

CREATE TABLE [dbo].[SurveyQuestion](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyFormId] [int] NOT NULL,
    [QuestionText] [nvarchar](1000) NOT NULL,
    [QuestionType] [nvarchar](50) NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    [IsRequired] [bit] NOT NULL,
    [CreatedDate] [datetime] NOT NULL,
    [MinValue] [int] NULL,
    [MaxValue] [int] NULL,
    [MinLabel] [nvarchar](100) NULL,
    [MaxLabel] [nvarchar](100) NULL,
    CONSTRAINT [PK_SurveyQuestion] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyQuestion_SurveyForm] FOREIGN KEY ([SurveyFormId])
        REFERENCES [dbo].[SurveyForm]([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[SurveyQuestionOption](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyQuestionId] [int] NOT NULL,
    [OptionText] [nvarchar](500) NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    CONSTRAINT [PK_SurveyQuestionOption] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyQuestionOption_SurveyQuestion] FOREIGN KEY ([SurveyQuestionId])
        REFERENCES [dbo].[SurveyQuestion]([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[SurveyGridRow](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyQuestionId] [int] NOT NULL,
    [RowText] [nvarchar](500) NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    CONSTRAINT [PK_SurveyGridRow] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyGridRow_SurveyQuestion] FOREIGN KEY ([SurveyQuestionId])
        REFERENCES [dbo].[SurveyQuestion]([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [dbo].[SurveyGridColumn](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyQuestionId] [int] NOT NULL,
    [ColumnText] [nvarchar](500) NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    CONSTRAINT [PK_SurveyGridColumn] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyGridColumn_SurveyQuestion] FOREIGN KEY ([SurveyQuestionId])
        REFERENCES [dbo].[SurveyQuestion]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SurveyForm_SurveyTemplateId] ON [dbo].[SurveyForm]([SurveyTemplateId]);
CREATE INDEX [IX_SurveyQuestion_SurveyFormId] ON [dbo].[SurveyQuestion]([SurveyFormId]);
CREATE INDEX [IX_SurveyQuestionOption_SurveyQuestionId] ON [dbo].[SurveyQuestionOption]([SurveyQuestionId]);
CREATE INDEX [IX_SurveyGridRow_SurveyQuestionId] ON [dbo].[SurveyGridRow]([SurveyQuestionId]);
CREATE INDEX [IX_SurveyGridColumn_SurveyQuestionId] ON [dbo].[SurveyGridColumn]([SurveyQuestionId]);
GO
