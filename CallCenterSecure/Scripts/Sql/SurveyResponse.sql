CREATE TABLE [dbo].[SurveyFormResponse](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyFormId] [int] NOT NULL,
    [SurveyCustomerDataId] [int] NULL,
    [RespondentName] [nvarchar](100) NULL,
    [RespondentMobile] [nvarchar](50) NULL,
    [SubmittedBy] [nvarchar](100) NOT NULL,
    [SubmittedDate] [datetime] NOT NULL,
    CONSTRAINT [PK_SurveyFormResponse] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyFormResponse_SurveyForm] FOREIGN KEY ([SurveyFormId]) REFERENCES [dbo].[SurveyForm]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SurveyFormResponse_SurveyCustomerData] FOREIGN KEY ([SurveyCustomerDataId]) REFERENCES [dbo].[SurveyCustomerData]([Id])
);
GO

CREATE TABLE [dbo].[SurveyFormAnswer](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyFormResponseId] [int] NOT NULL,
    [SurveyQuestionId] [int] NOT NULL,
    [AnswerText] [nvarchar](2000) NULL,
    [SelectedOption] [nvarchar](500) NULL,
    [SelectedOptionsCsv] [nvarchar](1000) NULL,
    [FileName] [nvarchar](255) NULL,
    [FilePath] [nvarchar](500) NULL,
    CONSTRAINT [PK_SurveyFormAnswer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyFormAnswer_SurveyFormResponse] FOREIGN KEY ([SurveyFormResponseId]) REFERENCES [dbo].[SurveyFormResponse]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SurveyFormAnswer_SurveyQuestion] FOREIGN KEY ([SurveyQuestionId]) REFERENCES [dbo].[SurveyQuestion]([Id])
);
GO

CREATE TABLE [dbo].[SurveyFormGridAnswer](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [SurveyFormAnswerId] [int] NOT NULL,
    [RowText] [nvarchar](500) NOT NULL,
    [SelectedColumnText] [nvarchar](500) NULL,
    [SelectedColumnTextsCsv] [nvarchar](1000) NULL,
    CONSTRAINT [PK_SurveyFormGridAnswer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SurveyFormGridAnswer_SurveyFormAnswer] FOREIGN KEY ([SurveyFormAnswerId]) REFERENCES [dbo].[SurveyFormAnswer]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SurveyFormResponse_SurveyFormId] ON [dbo].[SurveyFormResponse]([SurveyFormId]);
CREATE INDEX [IX_SurveyFormResponse_SurveyCustomerDataId] ON [dbo].[SurveyFormResponse]([SurveyCustomerDataId]);
CREATE INDEX [IX_SurveyFormAnswer_SurveyFormResponseId] ON [dbo].[SurveyFormAnswer]([SurveyFormResponseId]);
CREATE INDEX [IX_SurveyFormAnswer_SurveyQuestionId] ON [dbo].[SurveyFormAnswer]([SurveyQuestionId]);
CREATE INDEX [IX_SurveyFormGridAnswer_SurveyFormAnswerId] ON [dbo].[SurveyFormGridAnswer]([SurveyFormAnswerId]);
GO
